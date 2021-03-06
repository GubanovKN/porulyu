using NLog;
using porulyu.BotMain.Common;
using porulyu.BotMain.Services.Payments;
using porulyu.Infrastructure.Cryptography;
using porulyu.Infrastructure.Data;
using porulyu.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace porulyu.BotMain.Services.Main
{
    class OperationsBot
    {
        #region Поля
        private readonly Logger logger;

        private TelegramBotClient Bot;

        private OperationsCheckCar OperationsCheckCar;

        private OperationsCreateMessagesBot OperationsCreateMessagesBot;

        private string SenderName;
        #endregion

        #region Инициализация
        public OperationsBot()
        {
            logger = LogManager.GetCurrentClassLogger();
        }

        public async Task Start()
        {
            try
            {
                Encryption encryption = new Encryption();
                Bot = new TelegramBotClient(encryption.DecryptRSA(Constants.BotMain.Token));

                TelegramBotClient senderUserName = new TelegramBotClient(encryption.DecryptRSA(Constants.BotSender.Token));
                var me = await senderUserName.GetMeAsync();
                SenderName = me.Username;

                OperationsCheckCar = new OperationsCheckCar();

                OperationsCreateMessagesBot = new OperationsCreateMessagesBot();

                Bot.OnMessage += BotOnMessageReceived;
                Bot.OnMessageEdited += BotOnMessageReceived;
                Bot.OnCallbackQuery += BotOnCallbackQueryReceived;
                Bot.OnReceiveError += BotOnReceiveError;

                Bot.StartReceiving(Array.Empty<UpdateType>());
            }
            catch (Exception Ex)
            {
                throw new Exception(Ex.Message, Ex);
            }
        }
        #endregion

        #region События бота
        private async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;
            if (message == null || message.Type != MessageType.Text)
                return;

            try
            {
                Domain.Models.User user = await new OperationsUser().GetUser(message.Chat.Id);

                if (user == null)
                {
                    await new OperationsUser().CreateUser(message.Chat.Id);
                    user = await new OperationsUser().GetUser(message.Chat.Id); ;
                }

                try
                {
                    switch (message.Text)
                    {
                        case "/start":
                            if (!user.UserAgreement)
                            {
                                await SendUserAgreement(message, user);
                            }
                            else
                            {
                                await SendUsage(message, user);
                            }
                            break;

                        default:
                            int PositionDialog = user.PositionDialog / 100;
                            switch (PositionDialog)
                            {
                                case 1:
                                    PositionDialog = (user.PositionDialog - 100) / 10;
                                    switch (PositionDialog)
                                    {
                                        case 1:
                                            PositionDialog = user.PositionDialog - 110;
                                            switch (PositionDialog)
                                            {
                                                case 1:
                                                    await new OperationsFilter().SaveNameFilter(message.Text, user);
                                                    await SendCreateFitler(message, user);
                                                    return;
                                                case 4:
                                                    await new OperationsFilter().SaveYearsFilter(message.Text, user);
                                                    await SendCreateFitler(message, user);
                                                    return;
                                                case 5:
                                                    await new OperationsFilter().SavePricesFilter(message.Text, user);
                                                    await SendCreateFitler(message, user);
                                                    return;
                                            }
                                            break;
                                        case 2:
                                            PositionDialog = user.PositionDialog - 120;
                                            switch (PositionDialog)
                                            {
                                                case 4:
                                                    await new OperationsFilter().SaveEngineCapacityFilter(message.Text, user);
                                                    await SendCreateOptionallyFitler(message, user);
                                                    return;
                                                case 5:
                                                    await new OperationsFilter().SaveMileageFilter(message.Text, user);
                                                    await SendCreateOptionallyFitler(message, user);
                                                    return;
                                            }
                                            break;
                                        case 3:
                                            PositionDialog = user.PositionDialog - 130;
                                            switch (PositionDialog)
                                            {
                                                case 1:
                                                    return;
                                                case 2:
                                                    return;
                                            }
                                            break;
                                    }
                                    await new OperationsFilter().DeleteFilter(user.Filters.Last());

                                    break;
                                case 3:
                                    PositionDialog = user.PositionDialog - 300;

                                    switch (PositionDialog)
                                    {
                                        case 10:
                                            await SendLoadReport(message, user);
                                            await SendFreeReport(message, user);
                                            return;
                                        case 11:
                                            return;
                                        case 22:
                                            return;
                                    }
                                    break;
                            }

                            if (user.UserAgreement)
                            {
                                await SendUsage(message, user);
                            }
                            else
                            {
                                throw new Exception("Пользоваться ботом можно только после принятия пользовательского соглашения!");
                            }
                            break;
                    }
                }
                catch (Exception Ex)
                {
                    await SendError(message, Ex.Message, user);
                }
            }
            catch (Exception Ex)
            {
                logger.Error("Пользователь:" + message.Chat.Id + " Ошибка:" + Ex.Message);
            }
        }
        private async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            try
            {
                var callbackQuery = callbackQueryEventArgs.CallbackQuery;

                Domain.Models.User user = await new OperationsUser().GetUser(callbackQuery.Message.Chat.Id);

                switch (user.PositionDialog / 100)
                {
                    case -2:
                        switch (callbackQuery.Data)
                        {
                            case "Accept":
                                await new OperationsUser().SetUserAgreementUser(user);
                                await SendGift(callbackQuery.Message, user);
                                break;
                            case "OK":
                                await SendUserAgreement(callbackQuery.Message, user);
                                break;
                        }
                        break;
                    case -1:
                        switch (callbackQuery.Data)
                        {
                            case "Activate":
                                await new OperationsUser().SetRateIdUser(user, (await new OperationsRate().GetAllAsync()).Find(p => p.Demo));
                                await SendUsage(callbackQuery.Message, user);
                                break;
                        }
                        break;
                    case 0:
                        switch (callbackQuery.Data)
                        {
                            case "Filters":
                                await SendFilters(callbackQuery.Message, user);
                                break;
                            case "Rates":
                                await SendRates(callbackQuery.Message, user);
                                break;
                            case "CheckingCar":
                                await SendCheckingCar(callbackQuery.Message, user);
                                break;
                            case "Help":
                                await SendHelp(callbackQuery.Message, user);
                                break;
                            case "OK":
                                await SendUserAgreement(callbackQuery.Message, user);
                                break;
                        }
                        break;
                    case 1:
                        if (user.PositionDialog == 100)
                        {
                            switch (callbackQuery.Data)
                            {
                                case "Main":
                                    await SendUsage(callbackQuery.Message, user);
                                    break;
                                case "Create":
                                    if(user.Rate.Id != 1)
                                    {
                                        if (user.DateExpired > DateTime.Now)
                                        {
                                            if (user.Rate.CountFilters > user.Filters.Count)
                                            {
                                                await new OperationsFilter().CreateFilter(user);
                                                await SendCreateFitler(callbackQuery.Message, user);
                                            }
                                            else
                                            {
                                                await SendError(callbackQuery.Message, "Достигнуто максимальное количество фильтров", user);
                                            }
                                        }
                                        else
                                        {
                                            await SendError(callbackQuery.Message, "Срок действия тарифа истёк", user);
                                        }
                                    }
                                    else
                                    {
                                        await SendError(callbackQuery.Message, "Смените тариф для создания фильтра", user);
                                    }
                                    break;
                                case "MyFilters":
                                    await new OperationsUser().SetPositionViewUser(user, 0);
                                    await SendMyFilters(callbackQuery.Message, user);
                                    break;
                                case "OK":
                                    await SendFilters(callbackQuery.Message, user);
                                    break;
                            }
                        }
                        else
                        {
                            await SelectingSubActionFilter(callbackQuery, user);
                        }
                        break;
                    case 2:
                        if (user.PositionDialog == 200)
                        {
                            switch (callbackQuery.Data)
                            {
                                case "Main":
                                    await SendUsage(callbackQuery.Message, user);
                                    break;
                                case "ExtendRate":
                                    await SendExtendRate(callbackQuery.Message, user);
                                    break;
                                case "ChangeRate":
                                    await new OperationsUser().SetPositionViewUser(user, 0);
                                    await SendAllRates(callbackQuery.Message, user);
                                    break;
                            }
                        }
                        else
                        {
                            await SelectingSubActionRates(callbackQuery, user);
                        }
                        break;
                    case 3:
                        if (user.PositionDialog == 300)
                        {
                            switch (callbackQuery.Data)
                            {
                                case "Main":
                                    await SendUsage(callbackQuery.Message, user);
                                    break;
                                case "CheckCar":
                                    await SendCheckCar(callbackQuery.Message, user);
                                    break;
                                case "MyReports":
                                    if (user.Reports.Count() > 0)
                                    {
                                        await new OperationsUser().SetPositionViewUser(user, 0);
                                        await SendMyReports(callbackQuery.Message, user);
                                    }
                                    else
                                    {
                                        await SendError(callbackQuery.Message, "Вы еще не проверяли ни одного автомобиля", user);
                                    }
                                    break;
                                case "OK":
                                    await SendCheckingCar(callbackQuery.Message, user);
                                    break;
                            }
                        }
                        else
                        {
                            await SelectingSubActionChekingCar(callbackQuery, user);
                        }
                        break;
                    case 4:
                        if (user.PositionDialog == 400)
                        {
                            switch (callbackQuery.Data)
                            {
                                case "Main":
                                    await SendUsage(callbackQuery.Message, user);
                                    break;
                                case "TopQuestions":
                                    await SendTopQuestions(callbackQuery.Message, user);
                                    break;
                            }
                        }
                        else
                        {
                            await SelectingSubActionHelp(callbackQuery, user);
                        }
                        break;
                }
            }
            catch (Exception Ex)
            {
                logger.Error(Ex.ToString());
            }
        }

        private void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            logger.Error(receiveErrorEventArgs.ApiRequestException.ErrorCode + ": " + receiveErrorEventArgs.ApiRequestException.Message);
        }
        #endregion

        #region Отправка сообщений

        #region Пользовательское соглашение
        private async Task SendUserAgreement(Message message, Domain.Models.User user)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(OperationsCreateMessagesBot.GetUserAgreementButtons());

            int Id = (await Bot.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: OperationsCreateMessagesBot.GetUserAgreementText(),
                replyMarkup: inlineKeyboard,
                parseMode: ParseMode.Html
            )).MessageId;

            if (user.LastMessageId != 0 && Id != user.LastMessageId)
            {
                await DeleteMessage(message.Chat.Id, user.LastMessageId);
            }

            await new OperationsUser().SetLastMessageIdUser(user, Id);

            await new OperationsUser().SetPositionDialogUser(user, -200);
        }
        #endregion

        #region Акция
        private async Task SendGift(Message message, Domain.Models.User user)
        {
            Domain.Models.Rate rate = (await new OperationsRate().GetAllAsync()).Find(p => p.Demo);

            var inlineKeyboard = new InlineKeyboardMarkup(OperationsCreateMessagesBot.GetGiftButtons(rate.Name));

            if (message.From.Id == Bot.BotId)
            {
                await Bot.EditMessageTextAsync(
                    chatId: message.Chat.Id,
                    messageId: message.MessageId,
                    text: OperationsCreateMessagesBot.GetGiftText(rate),
                    replyMarkup: inlineKeyboard,
                    parseMode: ParseMode.Html
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetGiftText(rate),
                    replyMarkup: inlineKeyboard,
                    parseMode: ParseMode.Html
                    )).MessageId;

                if (user.LastMessageId != 0 && Id != user.LastMessageId)
                {
                    await DeleteMessage(message.Chat.Id, user.LastMessageId);
                }

                await new OperationsUser().SetLastMessageIdUser(user, Id);
            }

            await new OperationsUser().SetPositionDialogUser(user, -100);
        }
        #endregion

        #region Главное меню
        private async Task SendUsage(Message message, Domain.Models.User user)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(OperationsCreateMessagesBot.GetUsageButtons());

            if (message.From.Id == Bot.BotId)
            {
                await Bot.EditMessageTextAsync(
                    chatId: message.Chat.Id,
                    messageId: message.MessageId,
                    text: OperationsCreateMessagesBot.GetUsageText(),
                    replyMarkup: inlineKeyboard,
                    parseMode: ParseMode.Html
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetUsageText(),
                    replyMarkup: inlineKeyboard,
                    parseMode: ParseMode.Html
                    )).MessageId;

                if (user.LastMessageId != 0 && Id != user.LastMessageId)
                {
                    await DeleteMessage(message.Chat.Id, user.LastMessageId);
                }

                await new OperationsUser().SetLastMessageIdUser(user, Id);
            }

            await new OperationsUser().SetPositionDialogUser(user, 0);
        }
        #endregion

        #region Фильтры

        #region Определение действия
        private async Task SelectingSubActionFilter(CallbackQuery callbackQuery, Domain.Models.User user)
        {
            int PositionDialog = user.PositionDialog - 100;

            switch (PositionDialog / 10)
            {
                case 1:
                    if (PositionDialog == 10)
                    {
                        switch (callbackQuery.Data)
                        {
                            case "Name":
                                await SendName(callbackQuery.Message, user);
                                break;
                            case "Mark":
                                await new OperationsUser().SetPositionViewUser(user, 0);
                                await SendMarks(callbackQuery.Message, user);
                                break;
                            case "Model":
                                if (user.Filters.Last().MarkId != 0)
                                {
                                    await new OperationsUser().SetPositionViewUser(user, 0);
                                    await SendModels(callbackQuery.Message, user);
                                }
                                else
                                {
                                    await SendPopUp(callbackQuery.Id, "Необходимо выбрать марку, перед выбором модели");
                                }
                                break;
                            case "Year":
                                await SendYears(callbackQuery.Message, user);
                                break;
                            case "Price":
                                await SendPrices(callbackQuery.Message, user);
                                break;
                            case "Region":
                                await SendRegions(callbackQuery.Message, user);
                                break;
                            case "City":
                                if (user.Filters.Last().RegionId != 0)
                                {
                                    await new OperationsUser().SetPositionViewUser(user, 0);
                                    await SendCities(callbackQuery.Message, user);
                                }
                                else
                                {
                                    await SendPopUp(callbackQuery.Id, "Необходимо выбрать регион, перед выбором города");
                                }
                                break;
                            case "Optionally":
                                await SendCreateOptionallyFitler(callbackQuery.Message, user);
                                break;
                            case "Back":
                                await new OperationsFilter().DeleteFilter(user.Filters.Last());
                                await SendFilters(callbackQuery.Message, user);
                                break;
                            case "Next":
                                if (user.Filters.Last().Name != null)
                                {
                                    await SendLoadFilter(callbackQuery.Message, user);

                                    if (await new OperationsAd().GetCountAds(user.Filters.Last()) != 0)
                                    {
                                        await SendSave(callbackQuery.Message, user);
                                    }
                                    else
                                    {
                                        await SendPopUp(callbackQuery.Id, "По данным параметрам не найдено автомобилей");
                                        await SendCreateFitler(callbackQuery.Message, user);
                                    }
                                }
                                else
                                {
                                    await SendPopUp(callbackQuery.Id, "Необходимо задать название фильтра перед продолжением");
                                }
                                break;
                        }
                    }
                    else
                    {
                        await SelectingSubActionMainFilter(callbackQuery, user);
                    }
                    break;
                case 2:
                    if (PositionDialog == 20)
                    {
                        switch (callbackQuery.Data)
                        {
                            case "CustomsСleared":
                                await SendCustomsСleared(callbackQuery.Message, user);
                                break;
                            case "Transmission":
                                await SendTransmission(callbackQuery.Message, user);
                                break;
                            case "Actuator":
                                await SendActuator(callbackQuery.Message, user);
                                break;
                            case "EngineCapacity":
                                await SendEngineCapacity(callbackQuery.Message, user);
                                break;
                            case "Mileage":
                                await SendMileage(callbackQuery.Message, user);
                                break;
                            case "Back":
                                await SendCreateFitler(callbackQuery.Message, user);
                                break;
                            case "Next":
                                if (user.Filters.Last().Name != null)
                                {
                                    await SendLoadFilter(callbackQuery.Message, user);

                                    if (await new OperationsAd().GetCountAds(user.Filters.Last()) != 0)
                                    {
                                        await SendSave(callbackQuery.Message, user);
                                    }
                                    else
                                    {
                                        await SendPopUp(callbackQuery.Id, "По данным параметрам не найдено автомобилей");
                                        await SendCreateOptionallyFitler(callbackQuery.Message, user);
                                    }
                                }
                                else
                                {
                                    await SendPopUp(callbackQuery.Id, "Необходимо задать название фильтра перед продолжением");
                                }
                                break;
                        }
                    }
                    else
                    {
                        await SelectingSubActionOptionallyFilter(callbackQuery, user);
                    }
                    break;
                case 3:
                    switch (callbackQuery.Data)
                    {
                        case "Back":
                            await SendCreateFitler(callbackQuery.Message, user);
                            break;
                        case "Main":
                            await SendUsage(callbackQuery.Message, user);
                            break;
                        case "Save":
                            await new OperationsFilter().SaveWorkFilter(user);
                            await SendWork(callbackQuery.Message, user);
                            break;
                    }
                    break;
                case 4:
                    if (PositionDialog == 40)
                    {
                        switch (callbackQuery.Data)
                        {
                            case "Back":
                                await SendFilters(callbackQuery.Message, user);
                                break;
                            default:
                                await SendSelectedFilter(callbackQuery.Message, user, callbackQuery.Data);
                                break;
                        }
                    }
                    else
                    {
                        await SelectingSubActionSelected(callbackQuery, user);
                    }
                    break;
            }
        }
        private async Task SelectingSubActionMainFilter(CallbackQuery callbackQuery, Domain.Models.User user)
        {
            int PositionDialog = user.PositionDialog - 110;

            switch (PositionDialog)
            {
                case 1:
                    switch (callbackQuery.Data)
                    {
                        case "Clear":
                            await new OperationsFilter().SaveNameFilter(null, user);
                            await SendCreateFitler(callbackQuery.Message, user);
                            break;
                        case "Back":
                            await SendCreateFitler(callbackQuery.Message, user);
                            break;
                    }
                    break;
                case 2:
                    switch (callbackQuery.Data)
                    {
                        case "Clear":
                            await new OperationsFilter().SaveModelIdFilter("0", user);
                            await new OperationsFilter().SaveMarkIdFilter("0", user);
                            await SendCreateFitler(callbackQuery.Message, user);
                            break;
                        case "Back":
                            if (user.PositionView > 0)
                            {
                                await new OperationsUser().SetPositionViewUser(user, --user.PositionView);
                                await SendMarks(callbackQuery.Message, user);
                            }
                            else
                            {
                                await SendCreateFitler(callbackQuery.Message, user);
                            }
                            break;
                        case "Next":
                            await new OperationsUser().SetPositionViewUser(user, ++user.PositionView);
                            await SendMarks(callbackQuery.Message, user);
                            break;
                        default:
                            await new OperationsFilter().SaveMarkIdFilter(callbackQuery.Data, user);
                            await SendCreateFitler(callbackQuery.Message, user);
                            break;
                    }
                    break;
                case 3:
                    switch (callbackQuery.Data)
                    {
                        case "Clear":
                            await new OperationsFilter().SaveModelIdFilter("0", user);
                            await SendCreateFitler(callbackQuery.Message, user);
                            break;
                        case "Back":
                            if (user.PositionView > 0)
                            {
                                await new OperationsUser().SetPositionViewUser(user, --user.PositionView);
                                await SendModels(callbackQuery.Message, user);
                            }
                            else
                            {
                                await SendCreateFitler(callbackQuery.Message, user);
                            }
                            break;
                        case "Next":
                            await new OperationsUser().SetPositionViewUser(user, ++user.PositionView);
                            await SendModels(callbackQuery.Message, user);
                            break;
                        default:
                            await new OperationsFilter().SaveModelIdFilter(callbackQuery.Data, user);
                            await SendCreateFitler(callbackQuery.Message, user);
                            break;
                    }
                    break;
                case 4:
                    switch (callbackQuery.Data)
                    {
                        case "Clear":
                            await new OperationsFilter().SaveYearsFilter("0-0", user);
                            await SendCreateFitler(callbackQuery.Message, user);
                            break;
                        case "Back":
                            await SendCreateFitler(callbackQuery.Message, user);
                            break;
                        case "OK":
                            await SendYears(callbackQuery.Message, user);
                            break;
                    }
                    break;
                case 5:
                    switch (callbackQuery.Data)
                    {
                        case "Clear":
                            await new OperationsFilter().SavePricesFilter("0-0", user);
                            await SendCreateFitler(callbackQuery.Message, user);
                            break;
                        case "Back":
                            await SendCreateFitler(callbackQuery.Message, user);
                            break;
                        case "OK":
                            await SendPrices(callbackQuery.Message, user);
                            break;
                    }
                    break;
                case 6:
                    switch (callbackQuery.Data)
                    {
                        case "Clear":
                            await new OperationsFilter().SaveCityIdFilter("0", user);
                            await new OperationsFilter().SaveRegionIdFilter("0", user);
                            await SendCreateFitler(callbackQuery.Message, user);
                            break;
                        case "Back":
                            await SendCreateFitler(callbackQuery.Message, user);
                            break;
                        default:
                            await new OperationsFilter().SaveRegionIdFilter(callbackQuery.Data, user);
                            await SendCreateFitler(callbackQuery.Message, user);
                            break;
                    }
                    break;
                case 7:
                    switch (callbackQuery.Data)
                    {
                        case "Clear":
                            await new OperationsFilter().SaveCityIdFilter("0", user);
                            await SendCreateFitler(callbackQuery.Message, user);
                            break;
                        case "Back":
                            if (user.PositionView > 0)
                            {
                                await new OperationsUser().SetPositionViewUser(user, --user.PositionView);
                                await SendCities(callbackQuery.Message, user);
                            }
                            else
                            {
                                await SendCreateFitler(callbackQuery.Message, user);
                            }
                            break;
                        case "Next":
                            await new OperationsUser().SetPositionViewUser(user, ++user.PositionView);
                            await SendCities(callbackQuery.Message, user);
                            break;
                        default:
                            await new OperationsFilter().SaveCityIdFilter(callbackQuery.Data, user);
                            await SendCreateFitler(callbackQuery.Message, user);
                            break;
                    }
                    break;
            }
        }
        private async Task SelectingSubActionOptionallyFilter(CallbackQuery callbackQuery, Domain.Models.User user)
        {
            int PositionDialog = user.PositionDialog - 120;

            switch (PositionDialog)
            {
                case 1:
                    switch (callbackQuery.Data)
                    {
                        case "Clear":
                            await new OperationsFilter().SaveCustomsСlearedFilter(callbackQuery.Data, user);
                            await SendCreateOptionallyFitler(callbackQuery.Message, user);
                            break;
                        case "Back":
                            await SendCreateOptionallyFitler(callbackQuery.Message, user);
                            break;
                        default:
                            await new OperationsFilter().SaveCustomsСlearedFilter(callbackQuery.Data, user);
                            await SendCreateOptionallyFitler(callbackQuery.Message, user);
                            break;
                    }
                    break;
                case 2:
                    switch (callbackQuery.Data)
                    {
                        case "Clear":
                            await new OperationsFilter().SaveTransmissionFilter(callbackQuery.Data, user);
                            await SendCreateOptionallyFitler(callbackQuery.Message, user);
                            break;
                        case "Back":
                            await SendCreateOptionallyFitler(callbackQuery.Message, user);
                            break;
                        default:
                            await new OperationsFilter().SaveTransmissionFilter(callbackQuery.Data, user);
                            await SendCreateOptionallyFitler(callbackQuery.Message, user);
                            break;
                    }
                    break;
                case 3:
                    switch (callbackQuery.Data)
                    {
                        case "Clear":
                            await new OperationsFilter().SaveActuatorFilter(callbackQuery.Data, user);
                            await SendCreateOptionallyFitler(callbackQuery.Message, user);
                            break;
                        case "Back":
                            await SendCreateOptionallyFitler(callbackQuery.Message, user);
                            break;
                        default:
                            await new OperationsFilter().SaveActuatorFilter(callbackQuery.Data, user);
                            await SendCreateOptionallyFitler(callbackQuery.Message, user);
                            break;
                    }
                    break;
                case 4:
                    switch (callbackQuery.Data)
                    {
                        case "Clear":
                            await new OperationsFilter().SaveEngineCapacityFilter("0-0", user);
                            await SendCreateOptionallyFitler(callbackQuery.Message, user);
                            break;
                        case "Back":
                            await SendCreateOptionallyFitler(callbackQuery.Message, user);
                            break;
                        case "OK":
                            await SendEngineCapacity(callbackQuery.Message, user);
                            break;
                    }
                    break;
                case 5:
                    switch (callbackQuery.Data)
                    {
                        case "Clear":
                            await new OperationsFilter().SaveMileageFilter("0", user);
                            await SendCreateOptionallyFitler(callbackQuery.Message, user);
                            break;
                        case "Back":
                            await SendCreateOptionallyFitler(callbackQuery.Message, user);
                            break;
                        case "OK":
                            await SendMileage(callbackQuery.Message, user);
                            break;
                    }
                    break;
            }
        }
        private async Task SelectingSubActionSelected(CallbackQuery callbackQuery, Domain.Models.User user)
        {
            int PositionDialog = user.PositionDialog - 140;

            switch (PositionDialog)
            {
                case 1:
                    switch (callbackQuery.Data)
                    {
                        case "Back":
                            await SendMyFilters(callbackQuery.Message, user);
                            break;
                        default:
                            await new OperationsFilter().DeleteFilter(user.Filters.FirstOrDefault(p => p.Id == Convert.ToInt64(callbackQuery.Data)));
                            await SendMyFilters(callbackQuery.Message, user);
                            break;
                    }
                    break;
            }
        }
        #endregion

        #region Основное
        private async Task SendFilters(Message message, Domain.Models.User user)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(OperationsCreateMessagesBot.GetFiltersButtons());

            if (message.From.Id == Bot.BotId)
            {
                await Bot.EditMessageTextAsync(
                        chatId: message.Chat.Id,
                        messageId: message.MessageId,
                        text: OperationsCreateMessagesBot.GetFiltersText(),
                        replyMarkup: inlineKeyboard,
                        parseMode: ParseMode.Html
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetFiltersText(),
                    replyMarkup: inlineKeyboard,
                    parseMode: ParseMode.Html
                    )).MessageId;

                if (user.LastMessageId != 0 && Id != user.LastMessageId)
                {
                    await DeleteMessage(message.Chat.Id, user.LastMessageId);
                }

                await new OperationsUser().SetLastMessageIdUser(user, Id);
            }

            await new OperationsUser().SetPositionDialogUser(user, 100);
        }
        #endregion

        #region Создание фильтра
        private async Task SendCreateFitler(Message message, Domain.Models.User user)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(OperationsCreateMessagesBot.GetCreateFilterButtons());

            if (message.From.Id == Bot.BotId)
            {
                await Bot.EditMessageTextAsync(
                        chatId: message.Chat.Id,
                        messageId: message.MessageId,
                        text: await OperationsCreateMessagesBot.GetSaveText(user.Filters.Last()) + "\n\r" + OperationsCreateMessagesBot.GetCreateFilterText(),
                        replyMarkup: inlineKeyboard,
                        parseMode: ParseMode.Html
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: await OperationsCreateMessagesBot.GetSaveText(user.Filters.Last()) + "\n\r" + OperationsCreateMessagesBot.GetCreateFilterText(),
                    replyMarkup: inlineKeyboard,
                    parseMode: ParseMode.Html
                    )).MessageId;

                if (user.LastMessageId != 0 && Id != user.LastMessageId)
                {
                    await DeleteMessage(message.Chat.Id, user.LastMessageId);
                }

                await new OperationsUser().SetLastMessageIdUser(user, Id);
            }
            await new OperationsUser().SetPositionDialogUser(user, 110);
        }
        private async Task SendCreateOptionallyFitler(Message message, Domain.Models.User user)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(OperationsCreateMessagesBot.GetCreateFilterOptionallyButtons());

            if (message.From.Id == Bot.BotId)
            {
                await Bot.EditMessageTextAsync(
                        chatId: message.Chat.Id,
                        messageId: message.MessageId,
                        text: await OperationsCreateMessagesBot.GetSaveText(user.Filters.Last()) + "\n\r" + OperationsCreateMessagesBot.GetCreateFilterOptionallyText(),
                        replyMarkup: inlineKeyboard,
                        parseMode: ParseMode.Html
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: await OperationsCreateMessagesBot.GetSaveText(user.Filters.Last()) + "\n\r" + OperationsCreateMessagesBot.GetCreateFilterOptionallyText(),
                    replyMarkup: inlineKeyboard,
                    parseMode: ParseMode.Html
                    )).MessageId;

                if (user.LastMessageId != 0 && Id != user.LastMessageId)
                {
                    await DeleteMessage(message.Chat.Id, user.LastMessageId);
                }

                await new OperationsUser().SetLastMessageIdUser(user, Id);
            }

            await new OperationsUser().SetPositionDialogUser(user, 120);
        }

        #region Настройки фильтра
        private async Task SendName(Message message, Domain.Models.User user)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(OperationsCreateMessagesBot.GetNameButtons());

            if (message.From.Id == Bot.BotId)
            {
                await Bot.EditMessageTextAsync(
                        chatId: message.Chat.Id,
                        messageId: message.MessageId,
                        text: OperationsCreateMessagesBot.GetNameText(),
                        replyMarkup: inlineKeyboard,
                        parseMode: ParseMode.Html
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetNameText(),
                    replyMarkup: inlineKeyboard,
                    parseMode: ParseMode.Html
                    )).MessageId;

                if (user.LastMessageId != 0 && Id != user.LastMessageId)
                {
                    await DeleteMessage(message.Chat.Id, user.LastMessageId);
                }

                await new OperationsUser().SetLastMessageIdUser(user, Id);
            }

            await new OperationsUser().SetPositionDialogUser(user, 111);
        }
        private async Task SendMarks(Message message, Domain.Models.User user)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(await OperationsCreateMessagesBot.GetMarksButtons(user));

            if (message.From.Id == Bot.BotId)
            {
                await Bot.EditMessageTextAsync(
                        chatId: message.Chat.Id,
                        messageId: message.MessageId,
                        text: OperationsCreateMessagesBot.GetMarksText(),
                        replyMarkup: inlineKeyboard,
                        parseMode: ParseMode.Html
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetMarksText(),
                    replyMarkup: inlineKeyboard,
                    parseMode: ParseMode.Html
                    )).MessageId;

                if (user.LastMessageId != 0 && Id != user.LastMessageId)
                {
                    await DeleteMessage(message.Chat.Id, user.LastMessageId);
                }

                await new OperationsUser().SetLastMessageIdUser(user, Id);
            }

            await new OperationsUser().SetPositionDialogUser(user, 112);
        }
        private async Task SendModels(Message message, Domain.Models.User user)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(await OperationsCreateMessagesBot.GetModelsButtons(user));

            if (message.From.Id == Bot.BotId)
            {
                await Bot.EditMessageTextAsync(
                        chatId: message.Chat.Id,
                        messageId: message.MessageId,
                        text: OperationsCreateMessagesBot.GetModelsText(),
                        replyMarkup: inlineKeyboard,
                        parseMode: ParseMode.Html
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetModelsText(),
                    replyMarkup: inlineKeyboard,
                    parseMode: ParseMode.Html
                    )).MessageId;

                if (user.LastMessageId != 0 && Id != user.LastMessageId)
                {
                    await DeleteMessage(message.Chat.Id, user.LastMessageId);
                }

                await new OperationsUser().SetLastMessageIdUser(user, Id);
            }

            await new OperationsUser().SetPositionDialogUser(user, 113);
        }
        private async Task SendYears(Message message, Domain.Models.User user)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(OperationsCreateMessagesBot.GetYearsButtons());

            if (message.From.Id == Bot.BotId)
            {
                await Bot.EditMessageTextAsync(
                        chatId: message.Chat.Id,
                        messageId: message.MessageId,
                        text: OperationsCreateMessagesBot.GetYearsText(),
                        replyMarkup: inlineKeyboard,
                        parseMode: ParseMode.Html
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetYearsText(),
                    replyMarkup: inlineKeyboard,
                    parseMode: ParseMode.Html
                    )).MessageId;

                if (user.LastMessageId != 0 && Id != user.LastMessageId)
                {
                    await DeleteMessage(message.Chat.Id, user.LastMessageId);
                }

                await new OperationsUser().SetLastMessageIdUser(user, Id);
            }

            await new OperationsUser().SetPositionDialogUser(user, 114);
        }
        private async Task SendPrices(Message message, Domain.Models.User user)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(OperationsCreateMessagesBot.GetPricesButtons());

            if (message.From.Id == Bot.BotId)
            {
                await Bot.EditMessageTextAsync(
                        chatId: message.Chat.Id,
                        messageId: message.MessageId,
                        text: OperationsCreateMessagesBot.GetPricesText(),
                        replyMarkup: inlineKeyboard,
                        parseMode: ParseMode.Html
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetPricesText(),
                    replyMarkup: inlineKeyboard,
                    parseMode: ParseMode.Html
                    )).MessageId;

                if (user.LastMessageId != 0 && Id != user.LastMessageId)
                {
                    await DeleteMessage(message.Chat.Id, user.LastMessageId);
                }

                await new OperationsUser().SetLastMessageIdUser(user, Id);
            }

            await new OperationsUser().SetPositionDialogUser(user, 115);
        }
        private async Task SendRegions(Message message, Domain.Models.User user)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(await OperationsCreateMessagesBot.GetRegionsButtons());

            if (message.From.Id == Bot.BotId)
            {
                await Bot.EditMessageTextAsync(
                        chatId: message.Chat.Id,
                        messageId: message.MessageId,
                        text: OperationsCreateMessagesBot.GetRegionsText(),
                        replyMarkup: inlineKeyboard,
                        parseMode: ParseMode.Html
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetRegionsText(),
                    replyMarkup: inlineKeyboard,
                    parseMode: ParseMode.Html
                    )).MessageId;

                if (user.LastMessageId != 0 && Id != user.LastMessageId)
                {
                    await DeleteMessage(message.Chat.Id, user.LastMessageId);
                }

                await new OperationsUser().SetLastMessageIdUser(user, Id);
            }

            await new OperationsUser().SetPositionDialogUser(user, 116);
        }
        private async Task SendCities(Message message, Domain.Models.User user)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(await OperationsCreateMessagesBot.GetCitiesButtons(user));

            if (message.From.Id == Bot.BotId)
            {
                await Bot.EditMessageTextAsync(
                        chatId: message.Chat.Id,
                        messageId: message.MessageId,
                        text: OperationsCreateMessagesBot.GetCitiesText(),
                        replyMarkup: inlineKeyboard,
                        parseMode: ParseMode.Html
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetCitiesText(),
                    replyMarkup: inlineKeyboard,
                    parseMode: ParseMode.Html
                    )).MessageId;

                if (user.LastMessageId != 0 && Id != user.LastMessageId)
                {
                    await DeleteMessage(message.Chat.Id, user.LastMessageId);
                }

                await new OperationsUser().SetLastMessageIdUser(user, Id);
            }

            await new OperationsUser().SetPositionDialogUser(user, 117);
        }
        private async Task SendCustomsСleared(Message message, Domain.Models.User user)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(OperationsCreateMessagesBot.GetCustomsСlearedButtons());

            if (message.From.Id == Bot.BotId)
            {
                await Bot.EditMessageTextAsync(
                        chatId: message.Chat.Id,
                        messageId: message.MessageId,
                        text: OperationsCreateMessagesBot.GetCustomsСlearedText(),
                        replyMarkup: inlineKeyboard,
                        parseMode: ParseMode.Html
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetCustomsСlearedText(),
                    replyMarkup: inlineKeyboard,
                    parseMode: ParseMode.Html
                    )).MessageId;

                if (user.LastMessageId != 0 && Id != user.LastMessageId)
                {
                    await DeleteMessage(message.Chat.Id, user.LastMessageId);
                }

                await new OperationsUser().SetLastMessageIdUser(user, Id);
            }

            await new OperationsUser().SetPositionDialogUser(user, 121);
        }
        private async Task SendTransmission(Message message, Domain.Models.User user)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(OperationsCreateMessagesBot.GetTransmissionButtons());

            if (message.From.Id == Bot.BotId)
            {
                await Bot.EditMessageTextAsync(
                        chatId: message.Chat.Id,
                        messageId: message.MessageId,
                        text: OperationsCreateMessagesBot.GetTransmissionText(),
                        replyMarkup: inlineKeyboard,
                        parseMode: ParseMode.Html
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetTransmissionText(),
                    replyMarkup: inlineKeyboard,
                    parseMode: ParseMode.Html
                    )).MessageId;

                if (user.LastMessageId != 0 && Id != user.LastMessageId)
                {
                    await DeleteMessage(message.Chat.Id, user.LastMessageId);
                }

                await new OperationsUser().SetLastMessageIdUser(user, Id);
            }

            await new OperationsUser().SetPositionDialogUser(user, 122);
        }
        private async Task SendActuator(Message message, Domain.Models.User user)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(OperationsCreateMessagesBot.GetActuatorButtons());

            if (message.From.Id == Bot.BotId)
            {
                await Bot.EditMessageTextAsync(
                        chatId: message.Chat.Id,
                        messageId: message.MessageId,
                        text: OperationsCreateMessagesBot.GetActuatorText(),
                        replyMarkup: inlineKeyboard,
                        parseMode: ParseMode.Html
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetActuatorText(),
                    replyMarkup: inlineKeyboard,
                    parseMode: ParseMode.Html
                    )).MessageId;

                if (user.LastMessageId != 0 && Id != user.LastMessageId)
                {
                    await DeleteMessage(message.Chat.Id, user.LastMessageId);
                }

                await new OperationsUser().SetLastMessageIdUser(user, Id);
            }

            await new OperationsUser().SetPositionDialogUser(user, 123);
        }
        private async Task SendEngineCapacity(Message message, Domain.Models.User user)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(OperationsCreateMessagesBot.GetEngineCapacityButtons());

            if (message.From.Id == Bot.BotId)
            {
                await Bot.EditMessageTextAsync(
                        chatId: message.Chat.Id,
                        messageId: message.MessageId,
                        text: OperationsCreateMessagesBot.GetEngineCapacityText(),
                        replyMarkup: inlineKeyboard,
                        parseMode: ParseMode.Html
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetEngineCapacityText(),
                    replyMarkup: inlineKeyboard,
                    parseMode: ParseMode.Html
                    )).MessageId;

                if (user.LastMessageId != 0 && Id != user.LastMessageId)
                {
                    await DeleteMessage(message.Chat.Id, user.LastMessageId);
                }

                await new OperationsUser().SetLastMessageIdUser(user, Id);
            }

            await new OperationsUser().SetPositionDialogUser(user, 124);
        }
        private async Task SendMileage(Message message, Domain.Models.User user)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(OperationsCreateMessagesBot.GetMileageButtons());

            if (message.From.Id == Bot.BotId)
            {
                await Bot.EditMessageTextAsync(
                        chatId: message.Chat.Id,
                        messageId: message.MessageId,
                        text: OperationsCreateMessagesBot.GetMileageText(),
                        replyMarkup: inlineKeyboard,
                        parseMode: ParseMode.Html
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetMileageText(),
                    replyMarkup: inlineKeyboard,
                    parseMode: ParseMode.Html
                    )).MessageId;

                if (user.LastMessageId != 0 && Id != user.LastMessageId)
                {
                    await DeleteMessage(message.Chat.Id, user.LastMessageId);
                }

                await new OperationsUser().SetLastMessageIdUser(user, Id);
            }

            await new OperationsUser().SetPositionDialogUser(user, 125);
        }
        private async Task SendLoadFilter(Message message, Domain.Models.User user)
        {
            if (message.From.Id == Bot.BotId)
            {
                await Bot.EditMessageTextAsync(
                        chatId: message.Chat.Id,
                        messageId: message.MessageId,
                        text: OperationsCreateMessagesBot.GetLoadFilterText(),
                        parseMode: ParseMode.Html
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetLoadFilterText(),
                    parseMode: ParseMode.Html
                    )).MessageId;

                if (user.LastMessageId != 0 && Id != user.LastMessageId)
                {
                    await DeleteMessage(message.Chat.Id, user.LastMessageId);
                }

                await new OperationsUser().SetLastMessageIdUser(user, Id);
            }

            await new OperationsUser().SetPositionDialogUser(user, 131);
        }
        private async Task SendSave(Message message, Domain.Models.User user)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(OperationsCreateMessagesBot.GetSaveButtons());

            if (message.From.Id == Bot.BotId)
            {
                await Bot.EditMessageTextAsync(
                        chatId: message.Chat.Id,
                        messageId: message.MessageId,
                        text: "<b>Проверьте перед сохранением</b>\n\r" + await OperationsCreateMessagesBot.GetSaveText(user.Filters.Last()),
                        replyMarkup: inlineKeyboard,
                        parseMode: ParseMode.Html
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "<b>Проверьте перед сохранением</b>\n\r" + await OperationsCreateMessagesBot.GetSaveText(user.Filters.Last()),
                    replyMarkup: inlineKeyboard,
                    parseMode: ParseMode.Html
                    )).MessageId;

                if (user.LastMessageId != 0 && Id != user.LastMessageId)
                {
                    await DeleteMessage(message.Chat.Id, user.LastMessageId);
                }

                await new OperationsUser().SetLastMessageIdUser(user, Id);
            }

            await new OperationsUser().SetPositionDialogUser(user, 132);
        }
        private async Task SendWork(Message message, Domain.Models.User user)
        {
            InlineKeyboardButton[][] inlineKeyboards = new InlineKeyboardButton[2][];

            inlineKeyboards[0] = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithUrl("Посмотреть объявления", $@"https://t.me/{SenderName}")
            };
            inlineKeyboards[1] = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("Главное меню", "Main")
            };

            var inlineKeyboard = new InlineKeyboardMarkup(inlineKeyboards);

            if (message.From.Id == Bot.BotId)
            {
                await Bot.EditMessageTextAsync(
                        chatId: message.Chat.Id,
                        messageId: message.MessageId,
                        text: "Фильтр успешно сохранен!",
                        replyMarkup: inlineKeyboard,
                        parseMode: ParseMode.Html
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Фильтр успешно сохранен!",
                    replyMarkup: inlineKeyboard,
                    parseMode: ParseMode.Html
                    )).MessageId;

                if (user.LastMessageId != 0 && Id != user.LastMessageId)
                {
                    await DeleteMessage(message.Chat.Id, user.LastMessageId);
                }

                await new OperationsUser().SetLastMessageIdUser(user, Id);
            }
        }
        #endregion

        #endregion

        #region Мои фильтры
        private async Task SendMyFilters(Message message, Domain.Models.User user)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(OperationsCreateMessagesBot.GetMyFiltersButtons(user));

            if (message.From.Id == Bot.BotId)
            {
                await Bot.EditMessageTextAsync(
                        chatId: message.Chat.Id,
                        messageId: message.MessageId,
                        text: OperationsCreateMessagesBot.GetMyFiltersText(),
                        replyMarkup: inlineKeyboard,
                        parseMode: ParseMode.Html
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetMyFiltersText(),
                    replyMarkup: inlineKeyboard,
                    parseMode: ParseMode.Html
                    )).MessageId;

                if (user.LastMessageId != 0 && Id != user.LastMessageId)
                {
                    await DeleteMessage(message.Chat.Id, user.LastMessageId);
                }

                await new OperationsUser().SetLastMessageIdUser(user, Id);
            }

            await new OperationsUser().SetPositionDialogUser(user, 140);
        }
        private async Task SendSelectedFilter(Message message, Domain.Models.User user, string Id)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(OperationsCreateMessagesBot.GetSelectedFilterButtons(Id));

            if (message.From.Id == Bot.BotId)
            {
                await Bot.EditMessageTextAsync(
                        chatId: message.Chat.Id,
                        messageId: message.MessageId,
                        text: await OperationsCreateMessagesBot.GetSaveText(user.Filters.First(p => p.Id == Convert.ToInt64(Id))),
                        replyMarkup: inlineKeyboard,
                        parseMode: ParseMode.Html
                    );
            }
            else
            {
                int MessageId = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: await OperationsCreateMessagesBot.GetSaveText(user.Filters.First(p => p.Id == Convert.ToInt64(Id))),
                    replyMarkup: inlineKeyboard,
                    parseMode: ParseMode.Html
                    )).MessageId;

                if (user.LastMessageId != 0 && MessageId != user.LastMessageId)
                {
                    await DeleteMessage(message.Chat.Id, user.LastMessageId);
                }

                await new OperationsUser().SetLastMessageIdUser(user, MessageId);
            }

            await new OperationsUser().SetPositionDialogUser(user, 141);
        }
        #endregion

        #endregion

        #region Тарифы

        #region Определение действия
        private async Task SelectingSubActionRates(CallbackQuery callbackQuery, Domain.Models.User user)
        {
            int PositionDialog = user.PositionDialog - 200;

            switch (PositionDialog / 10)
            {
                case 1:
                    if (PositionDialog == 10)
                    {
                        switch (callbackQuery.Data)
                        {
                            case "Next":
                                await new OperationsUser().SetPositionViewUser(user, ++user.PositionView);
                                await SendAllRates(callbackQuery.Message, user);
                                break;
                            case "Back":
                                if (user.PositionView > 0)
                                {
                                    await new OperationsUser().SetPositionViewUser(user, --user.PositionView);
                                    await SendAllRates(callbackQuery.Message, user);
                                }
                                else
                                {
                                    await SendRates(callbackQuery.Message, user);
                                }
                                break;
                            default:
                                await SendRate(callbackQuery.Message, user, callbackQuery.Data);
                                break;
                        }
                    }
                    else
                    {
                        await SelectingSubActionChangeRateRates(callbackQuery, user);
                    }
                    break;
                case 2:
                    switch (callbackQuery.Data)
                    {
                        case "Back":
                            await SendRates(callbackQuery.Message, user);
                            break;
                    }
                    break;
            }
        }
        private async Task SelectingSubActionChangeRateRates(CallbackQuery callbackQuery, Domain.Models.User user)
        {
            int PositionDialog = user.PositionDialog - 210;

            switch (PositionDialog)
            {
                case 1:
                    switch (callbackQuery.Data)
                    {
                        case "Back":
                            await SendAllRates(callbackQuery.Message, user);
                            break;
                    }
                    break;
            }
        }
        #endregion

        #region Основное
        private async Task SendRates(Message message, Domain.Models.User user)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(OperationsCreateMessagesBot.GetRatesButtons(user));

            if (message.From.Id == Bot.BotId)
            {
                await Bot.EditMessageTextAsync(
                        chatId: message.Chat.Id,
                        messageId: message.MessageId,
                        text: OperationsCreateMessagesBot.GetRatesText(user),
                        replyMarkup: inlineKeyboard,
                        parseMode: ParseMode.Html
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetRatesText(user),
                    replyMarkup: inlineKeyboard,
                    parseMode: ParseMode.Html
                    )).MessageId;

                if (user.LastMessageId != 0 && Id != user.LastMessageId)
                {
                    await DeleteMessage(message.Chat.Id, user.LastMessageId);
                }

                await new OperationsUser().SetLastMessageIdUser(user, Id);
            }

            await new OperationsUser().SetPositionDialogUser(user, 200);
        }
        #endregion

        #region Изменение тарифа
        private async Task SendAllRates(Message message, Domain.Models.User user)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(OperationsCreateMessagesBot.GetAllRatesButtons(user, await new OperationsRate().GetAllAsync()));

            if (message.From.Id == Bot.BotId)
            {
                await Bot.EditMessageTextAsync(
                        chatId: message.Chat.Id,
                        messageId: message.MessageId,
                        text: OperationsCreateMessagesBot.GetAllRatesText(),
                        replyMarkup: inlineKeyboard,
                        parseMode: ParseMode.Html
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetAllRatesText(),
                    replyMarkup: inlineKeyboard,
                    parseMode: ParseMode.Html
                    )).MessageId;

                if (user.LastMessageId != 0 && Id != user.LastMessageId)
                {
                    await DeleteMessage(message.Chat.Id, user.LastMessageId);
                }

                await new OperationsUser().SetLastMessageIdUser(user, Id);
            }

            await new OperationsUser().SetPositionDialogUser(user, 210);
        }
        private async Task SendRate(Message message, Domain.Models.User user, string RateId)
        {
            Domain.Models.Payment payment = user.Payments.Where(p => p.Action == "rateSet" && p.ActionId == Convert.ToInt64(RateId) && p.Status != 1).FirstOrDefault();

            if (payment == null)
            {
                payment = await new OperationsPayment().Create(await new OperationsUnitpay().GetPayment("rateSet", Convert.ToInt64(RateId), user));
            }

            var inlineKeyboard = new InlineKeyboardMarkup(OperationsCreateMessagesBot.GetRateButtons(payment.PaymentURL));

            if (message.From.Id == Bot.BotId)
            {
                await Bot.EditMessageTextAsync(
                        chatId: message.Chat.Id,
                        messageId: message.MessageId,
                        text: OperationsCreateMessagesBot.GetRateText(await new OperationsRate().Get(RateId)),
                        replyMarkup: inlineKeyboard,
                        parseMode: ParseMode.Html
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetRateText(await new OperationsRate().Get(RateId)),
                    replyMarkup: inlineKeyboard,
                    parseMode: ParseMode.Html
                    )).MessageId;

                if (user.LastMessageId != 0 && Id != user.LastMessageId)
                {
                    await DeleteMessage(message.Chat.Id, user.LastMessageId);
                }

                await new OperationsUser().SetLastMessageIdUser(user, Id);
            }

            await new OperationsUser().SetPositionDialogUser(user, 211);
        }
        #endregion

        #region Продление тарифа
        private async Task SendExtendRate(Message message, Domain.Models.User user)
        {
            Domain.Models.Payment payment = user.Payments.Where(p => p.Action == "rateExtend" && p.ActionId == user.Rate.Id && p.Status != 1).FirstOrDefault();

            if (payment == null)
            {
                payment = await new OperationsPayment().Create(await new OperationsUnitpay().GetPayment("rateExtend", user.Rate.Id, user));
            }

            var inlineKeyboard = new InlineKeyboardMarkup(OperationsCreateMessagesBot.GetRateButtons(payment.PaymentURL));

            if (message.From.Id == Bot.BotId)
            {
                await Bot.EditMessageTextAsync(
                        chatId: message.Chat.Id,
                        messageId: message.MessageId,
                        text: OperationsCreateMessagesBot.GetExtendRateText(await new OperationsRate().Get(user.Rate.Id.ToString())),
                        replyMarkup: inlineKeyboard,
                        parseMode: ParseMode.Html
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetExtendRateText(await new OperationsRate().Get(user.Rate.Id.ToString())),
                    replyMarkup: inlineKeyboard,
                    parseMode: ParseMode.Html
                    )).MessageId;

                if (user.LastMessageId != 0 && Id != user.LastMessageId)
                {
                    await DeleteMessage(message.Chat.Id, user.LastMessageId);
                }

                await new OperationsUser().SetLastMessageIdUser(user, Id);
            }

            await new OperationsUser().SetPositionDialogUser(user, 220);
        }
        #endregion

        #endregion

        #region Проверка авто

        #region Определение действия
        private async Task SelectingSubActionChekingCar(CallbackQuery callbackQuery, Domain.Models.User user)
        {
            int PositionDialog = user.PositionDialog - 300;

            switch (PositionDialog / 10)
            {
                case 1:
                    if (PositionDialog == 10)
                    {
                        switch (callbackQuery.Data)
                        {
                            case "Back":
                                await SendCheckingCar(callbackQuery.Message, user);
                                break;
                            case "OK":
                                await SendCheckCar(callbackQuery.Message, user);
                                break;
                        }
                    }
                    else
                    {
                        await SelectingSubActionCheckCarCheckingCar(callbackQuery, user);
                    }
                    break;
                case 2:
                    if (PositionDialog == 20)
                    {
                        switch (callbackQuery.Data.Split('_')[0])
                        {
                            case "Pay":
                                if (user.CountReports <= 0)
                                {
                                    await SendMyReportPay(callbackQuery.Message, user, callbackQuery.Data.Split('_')[1]);
                                }
                                else
                                {
                                    await SendMyReportPayInclude(callbackQuery.Message, user, callbackQuery.Data.Split('_')[1]);
                                }
                                break;
                            case "Show":
                                await SendMyReport(callbackQuery.Message, user, callbackQuery.Data.Split('_')[1]);
                                break;
                            case "Next":
                                await new OperationsUser().SetPositionViewUser(user, ++user.PositionView);
                                await SendMyReports(callbackQuery.Message, user);
                                break;
                            case "Main":
                                await SendUsage(callbackQuery.Message, user);
                                break;
                            case "Back":
                                if (user.PositionView > 0)
                                {
                                    await new OperationsUser().SetPositionViewUser(user, --user.PositionView);
                                    await SendMyReports(callbackQuery.Message, user);
                                }
                                else
                                {
                                    await SendCheckingCar(callbackQuery.Message, user);
                                }
                                break;
                        }
                    }
                    else
                    {
                        await SelectingSubActionMyReportsCheckingCar(callbackQuery, user);
                    }
                    break;
            }
        }
        private async Task SelectingSubActionCheckCarCheckingCar(CallbackQuery callbackQuery, Domain.Models.User user)
        {
            int PositionDialog = user.PositionDialog - 310;

            switch (PositionDialog)
            {
                case 1:
                    switch (callbackQuery.Data)
                    {
                        case "OK":
                            await SendLoadReport(callbackQuery.Message, user);
                            break;
                    }
                    break;
                case 2:
                    switch (callbackQuery.Data.Split('_')[0])
                    {
                        case "Pay":
                            if (user.CountReports <= 0)
                            {
                                await SendReportPay(callbackQuery.Message, user, callbackQuery.Data.Split('_')[1]);
                            }
                            else
                            {
                                await SendReportPayInclude(callbackQuery.Message, user, callbackQuery.Data.Split('_')[1]);
                            }
                            break;
                        case "Back":
                            await SendCheckCar(callbackQuery.Message, user);
                            break;
                        case "OK":
                            await SendCheckCar(callbackQuery.Message, user);
                            break;
                    }
                    break;
                case 3:
                    switch (callbackQuery.Data.Split('_')[0])
                    {
                        case "Get":
                            await SendFullReport(callbackQuery.Message, user, callbackQuery.Data.Split('_')[1]);
                            break;
                        case "Back":
                            await SendCheckCar(callbackQuery.Message, user);
                            break;
                        case "OK":
                            await SendCheckCar(callbackQuery.Message, user);
                            break;
                    }
                    break;
            }
        }
        private async Task SelectingSubActionMyReportsCheckingCar(CallbackQuery callbackQuery, Domain.Models.User user)
        {
            int PositionDialog = user.PositionDialog - 320;

            switch (PositionDialog)
            {
                case 1:
                    switch (callbackQuery.Data)
                    {
                        case "Get":
                            await SendMyReportInclude(callbackQuery.Message, user, callbackQuery.Data.Split('_')[1]);
                            break;
                        case "Back":
                            await SendMyReports(callbackQuery.Message, user);
                            break;
                        case "OK":
                            await SendMyReports(callbackQuery.Message, user);
                            break;
                    }
                    break;
                case 2:
                    switch (callbackQuery.Data)
                    {
                        case "OK":
                            await SendLoadReport(callbackQuery.Message, user);
                            break;
                    }
                    break;
                case 3:
                    switch (callbackQuery.Data)
                    {
                        case "Back":
                            await SendMyReports(callbackQuery.Message, user);
                            break;
                        case "OK":
                            await SendMyReports(callbackQuery.Message, user);
                            break;
                    }
                    break;
            }
        }
        #endregion

        #region Основное
        private async Task SendCheckingCar(Message message, Domain.Models.User user)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(OperationsCreateMessagesBot.GetCheckingCarButtons());

            if (message.From.Id == Bot.BotId)
            {
                await Bot.EditMessageTextAsync(
                        chatId: message.Chat.Id,
                        messageId: message.MessageId,
                        text: OperationsCreateMessagesBot.GetCheckingCarText(),
                        replyMarkup: inlineKeyboard,
                        parseMode: ParseMode.Html
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetCheckingCarText(),
                    replyMarkup: inlineKeyboard,
                    parseMode: ParseMode.Html
                    )).MessageId;

                if (user.LastMessageId != 0 && Id != user.LastMessageId)
                {
                    await DeleteMessage(message.Chat.Id, user.LastMessageId);
                }

                await new OperationsUser().SetLastMessageIdUser(user, Id);
            }

            await new OperationsUser().SetPositionDialogUser(user, 300);
        }
        #endregion

        #region Получение отчета
        private async Task SendCheckCar(Message message, Domain.Models.User user)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(OperationsCreateMessagesBot.GetCheckCarButtons());

            if (message.From.Id == Bot.BotId)
            {
                await Bot.EditMessageTextAsync(
                        chatId: message.Chat.Id,
                        messageId: message.MessageId,
                        text: OperationsCreateMessagesBot.GetCheckCarText(),
                        replyMarkup: inlineKeyboard,
                        parseMode: ParseMode.Html
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetCheckCarText(),
                    replyMarkup: inlineKeyboard,
                    parseMode: ParseMode.Html
                    )).MessageId;

                if (user.LastMessageId != 0 && Id != user.LastMessageId)
                {
                    await DeleteMessage(message.Chat.Id, user.LastMessageId);
                }

                await new OperationsUser().SetLastMessageIdUser(user, Id);
            }

            await new OperationsUser().SetPositionDialogUser(user, 310);
        }
        private async Task SendFreeReport(Message message, Domain.Models.User user)
        {
            try
            {
                Domain.Models.Report temp = user.Reports.FirstOrDefault(p => p.Name == message.Text && !p.Pay);

                if (temp == null)
                {
                    string Link = OperationsCheckCar.GetLinkReport(message.Text);
                    temp = await new OperationsReport().Create(user.ChatId, message.Text, Link);
                }

                string Report = OperationsCheckCar.GetFreeReport(temp.Link);

                var inlineKeyboard = new InlineKeyboardMarkup(OperationsCreateMessagesBot.GetFreeReportButtons(temp.Id, user));

                if (message.From.Id == Bot.BotId)
                {
                    await Bot.EditMessageTextAsync(
                            chatId: message.Chat.Id,
                            messageId: message.MessageId,
                            text: Report,
                            replyMarkup: inlineKeyboard,
                            parseMode: ParseMode.Html
                        );
                }
                else
                {
                    int Id = (await Bot.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: Report,
                        replyMarkup: inlineKeyboard,
                        parseMode: ParseMode.Html
                        )).MessageId;

                    if (user.LastMessageId != 0 && Id != user.LastMessageId)
                    {
                        await DeleteMessage(message.Chat.Id, user.LastMessageId);
                    }

                    await new OperationsUser().SetLastMessageIdUser(user, Id);
                }

                await new OperationsUser().SetPositionDialogUser(user, 312);
            }
            catch (Exception Ex)
            {
                logger.Error(Ex.Message);
                await new OperationsUser().SetPositionDialogUser(user, 312);
                await SendError(message, Ex.Message, user);
            }
        }
        private async Task SendReportPay(Message message, Domain.Models.User user, string Data)
        {
            Domain.Models.Report report = await new OperationsReport().Get(Convert.ToInt64(Data));

            Domain.Models.Payment payment = user.Payments.Where(p => p.Action == "report" && p.ActionId == report.Id && p.Status != 1).FirstOrDefault();

            if (payment == null)
            {
                payment = await new OperationsPayment().Create(await new OperationsUnitpay().GetPayment("report", report.Id, user));
            }

            var inlineKeyboard = new InlineKeyboardMarkup(OperationsCreateMessagesBot.GetPayReportButtons(payment.PaymentURL));

            if (message.From.Id == Bot.BotId)
            {
                await Bot.EditMessageTextAsync(
                        chatId: message.Chat.Id,
                        messageId: message.MessageId,
                        text: OperationsCreateMessagesBot.GetPayReportText(report),
                        replyMarkup: inlineKeyboard,
                        parseMode: ParseMode.Html
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetPayReportText(report),
                    replyMarkup: inlineKeyboard,
                    parseMode: ParseMode.Html
                    )).MessageId;

                if (user.LastMessageId != 0 && Id != user.LastMessageId)
                {
                    await DeleteMessage(message.Chat.Id, user.LastMessageId);
                }

                await new OperationsUser().SetLastMessageIdUser(user, Id);
            }

            await new OperationsUser().SetPositionDialogUser(user, 313);
        }
        private async Task SendReportPayInclude(Message message, Domain.Models.User user, string Data)
        {
            Domain.Models.Report report = await new OperationsReport().Get(Convert.ToInt64(Data));

            var inlineKeyboard = new InlineKeyboardMarkup(OperationsCreateMessagesBot.GetPayIncludeReportButtons(report.Id.ToString()));

            if (message.From.Id == Bot.BotId)
            {
                await Bot.EditMessageTextAsync(
                        chatId: message.Chat.Id,
                        messageId: message.MessageId,
                        text: OperationsCreateMessagesBot.GetPayIncludeReportText(report),
                        replyMarkup: inlineKeyboard,
                        parseMode: ParseMode.Html
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetPayIncludeReportText(report),
                    replyMarkup: inlineKeyboard,
                    parseMode: ParseMode.Html
                    )).MessageId;

                if (user.LastMessageId != 0 && Id != user.LastMessageId)
                {
                    await DeleteMessage(message.Chat.Id, user.LastMessageId);
                }

                await new OperationsUser().SetLastMessageIdUser(user, Id);
            }

            await new OperationsUser().SetPositionDialogUser(user, 313);
        }
        private async Task SendFullReport(Message message, Domain.Models.User user, string ReportId)
        {
            try
            {
                await SendLoadReport(message, user);

                Domain.Models.Report temp = user.Reports.FirstOrDefault(p => p.Id == Convert.ToInt64(ReportId));

                if (!new OperationsCheckCar().PayReport(new OperationsCheckCar().Auth(temp.Link), Constants.CheckCarUserName, temp.Link))
                {
                    throw new Exception("Не удалось оплатить отчет!");
                }

                await new OperationsUser().SetCountReportsUser(user.CountReports - 1, user);
                await new OperationsReport().SetReportPay(true, temp);

                string Report = OperationsCheckCar.GetPayReport(temp.Link);

                var inlineKeyboard = new InlineKeyboardMarkup(OperationsCreateMessagesBot.GetFullReportButtons(temp.Link));

                if (message.From.Id == Bot.BotId)
                {
                    await Bot.EditMessageTextAsync(
                            chatId: message.Chat.Id,
                            messageId: message.MessageId,
                            text: Report,
                            replyMarkup: inlineKeyboard,
                            parseMode: ParseMode.Html
                        );
                }
                else
                {
                    int Id = (await Bot.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: Report,
                        replyMarkup: inlineKeyboard,
                        parseMode: ParseMode.Html
                        )).MessageId;

                    if (user.LastMessageId != 0 && Id != user.LastMessageId)
                    {
                        await DeleteMessage(message.Chat.Id, user.LastMessageId);
                    }

                    await new OperationsUser().SetLastMessageIdUser(user, Id);
                }

                await new OperationsUser().SetPositionDialogUser(user, 313);
            }
            catch (Exception Ex)
            {
                logger.Error(Ex.Message);
                await new OperationsUser().SetPositionDialogUser(user, 313);
                await SendError(message, "При получении отчета произошла ошибка, баланс отчетов не уменьшен", user);
            }
        }
        private async Task SendLoadReport(Message message, Domain.Models.User user)
        {
            if (message.From.Id == Bot.BotId)
            {
                await Bot.EditMessageTextAsync(
                        chatId: message.Chat.Id,
                        messageId: message.MessageId,
                        text: OperationsCreateMessagesBot.GetLoadReportText(),
                        parseMode: ParseMode.Html
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetLoadReportText(),
                    parseMode: ParseMode.Html
                    )).MessageId;

                if (user.LastMessageId != 0 && Id != user.LastMessageId)
                {
                    await DeleteMessage(message.Chat.Id, user.LastMessageId);
                }

                await new OperationsUser().SetLastMessageIdUser(user, Id);
            }

            await new OperationsUser().SetPositionDialogUser(user, 311);
        }
        #endregion

        #region Просмотр отчетов
        private async Task SendMyReports(Message message, Domain.Models.User user)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(OperationsCreateMessagesBot.GetMyReportsButtons(user));

            if (message.From.Id == Bot.BotId)
            {
                await Bot.EditMessageTextAsync(
                        chatId: message.Chat.Id,
                        messageId: message.MessageId,
                        text: OperationsCreateMessagesBot.GetMyReportsText(user),
                        replyMarkup: inlineKeyboard,
                        parseMode: ParseMode.Html
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetMyReportsText(user),
                    replyMarkup: inlineKeyboard,
                    parseMode: ParseMode.Html
                    )).MessageId;

                if (user.LastMessageId != 0 && Id != user.LastMessageId)
                {
                    await DeleteMessage(message.Chat.Id, user.LastMessageId);
                }

                await new OperationsUser().SetLastMessageIdUser(user, Id);
            }

            await new OperationsUser().SetPositionDialogUser(user, 320);
        }
        private async Task SendMyReportPay(Message message, Domain.Models.User user, string Data)
        {
            Domain.Models.Report report = await new OperationsReport().Get(Convert.ToInt64(Data));

            Domain.Models.Payment payment = user.Payments.Where(p => p.Action == "report" && p.ActionId == report.Id && p.Status != 1).FirstOrDefault();

            if (payment == null)
            {
                payment = await new OperationsPayment().Create(await new OperationsUnitpay().GetPayment("report", report.Id, user));
            }

            var inlineKeyboard = new InlineKeyboardMarkup(OperationsCreateMessagesBot.GetPayReportButtons(payment.PaymentURL));

            if (message.From.Id == Bot.BotId)
            {
                await Bot.EditMessageTextAsync(
                        chatId: message.Chat.Id,
                        messageId: message.MessageId,
                        text: OperationsCreateMessagesBot.GetPayReportText(report),
                        replyMarkup: inlineKeyboard,
                        parseMode: ParseMode.Html
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetPayReportText(report),
                    replyMarkup: inlineKeyboard,
                    parseMode: ParseMode.Html
                    )).MessageId;

                if (user.LastMessageId != 0 && Id != user.LastMessageId)
                {
                    await DeleteMessage(message.Chat.Id, user.LastMessageId);
                }

                await new OperationsUser().SetLastMessageIdUser(user, Id);
            }

            await new OperationsUser().SetPositionDialogUser(user, 321);
        }
        private async Task SendMyReportPayInclude(Message message, Domain.Models.User user, string Data)
        {
            Domain.Models.Report report = await new OperationsReport().Get(Convert.ToInt64(Data));

            var inlineKeyboard = new InlineKeyboardMarkup(OperationsCreateMessagesBot.GetPayIncludeReportButtons(report.Id.ToString()));

            if (message.From.Id == Bot.BotId)
            {
                await Bot.EditMessageTextAsync(
                        chatId: message.Chat.Id,
                        messageId: message.MessageId,
                        text: OperationsCreateMessagesBot.GetPayIncludeReportText(report),
                        replyMarkup: inlineKeyboard,
                        parseMode: ParseMode.Html
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetPayIncludeReportText(report),
                    replyMarkup: inlineKeyboard,
                    parseMode: ParseMode.Html
                    )).MessageId;

                if (user.LastMessageId != 0 && Id != user.LastMessageId)
                {
                    await DeleteMessage(message.Chat.Id, user.LastMessageId);
                }

                await new OperationsUser().SetLastMessageIdUser(user, Id);
            }

            await new OperationsUser().SetPositionDialogUser(user, 321);
        }
        private async Task SendMyReport(Message message, Domain.Models.User user, string ReportId)
        {
            try
            {
                await SendLoadMyReport(message, user);

                Domain.Models.Report temp = user.Reports.FirstOrDefault(p => p.Id == Convert.ToInt64(ReportId));

                string Report = OperationsCheckCar.GetPayReport(temp.Link);

                var inlineKeyboard = new InlineKeyboardMarkup(OperationsCreateMessagesBot.GetMyReportButtons(temp.Link));

                if (message.From.Id == Bot.BotId)
                {
                    await Bot.EditMessageTextAsync(
                            chatId: message.Chat.Id,
                            messageId: message.MessageId,
                            text: Report,
                            replyMarkup: inlineKeyboard,
                            parseMode: ParseMode.Html
                        );
                }
                else
                {
                    int Id = (await Bot.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: Report,
                        replyMarkup: inlineKeyboard,
                        parseMode: ParseMode.Html
                        )).MessageId;

                    if (user.LastMessageId != 0 && Id != user.LastMessageId)
                    {
                        await DeleteMessage(message.Chat.Id, user.LastMessageId);
                    }

                    await new OperationsUser().SetLastMessageIdUser(user, Id);
                }

                await new OperationsUser().SetPositionDialogUser(user, 323);
            }
            catch (Exception Ex)
            {
                logger.Error(Ex.Message);
                await new OperationsUser().SetPositionDialogUser(user, 323);
                await SendError(message, "При получении отчета произошла ошибка", user);
            }
        }
        private async Task SendMyReportInclude(Message message, Domain.Models.User user, string ReportId)
        {
            try
            {
                await SendLoadMyReport(message, user);

                Domain.Models.Report temp = user.Reports.FirstOrDefault(p => p.Id == Convert.ToInt64(ReportId));

                if (!new OperationsCheckCar().PayReport(new OperationsCheckCar().Auth(temp.Link), Constants.CheckCarUserName, temp.Link))
                {
                    throw new Exception("Не удалось оплатить отчет!");
                }

                await new OperationsUser().SetCountReportsUser(user.CountReports - 1, user);
                await new OperationsReport().SetReportPay(true, temp);

                string Report = OperationsCheckCar.GetPayReport(temp.Link);

                var inlineKeyboard = new InlineKeyboardMarkup(OperationsCreateMessagesBot.GetMyReportButtons(temp.Link));

                if (message.From.Id == Bot.BotId)
                {
                    await Bot.EditMessageTextAsync(
                            chatId: message.Chat.Id,
                            messageId: message.MessageId,
                            text: Report,
                            replyMarkup: inlineKeyboard,
                            parseMode: ParseMode.Html
                        );
                }
                else
                {
                    int Id = (await Bot.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: Report,
                        replyMarkup: inlineKeyboard,
                        parseMode: ParseMode.Html
                        )).MessageId;

                    if (user.LastMessageId != 0 && Id != user.LastMessageId)
                    {
                        await DeleteMessage(message.Chat.Id, user.LastMessageId);
                    }

                    await new OperationsUser().SetLastMessageIdUser(user, Id);
                }

                await new OperationsUser().SetPositionDialogUser(user, 323);
            }
            catch (Exception Ex)
            {
                logger.Error(Ex.Message);
                await new OperationsUser().SetPositionDialogUser(user, 323);
                await SendError(message, "При получении отчета произошла ошибка, баланс отчетов не уменьшен", user);
            }
        }
        private async Task SendLoadMyReport(Message message, Domain.Models.User user)
        {
            if (message.From.Id == Bot.BotId)
            {
                await Bot.EditMessageTextAsync(
                        chatId: message.Chat.Id,
                        messageId: message.MessageId,
                        text: OperationsCreateMessagesBot.GetLoadReportText(),
                        parseMode: ParseMode.Html
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetLoadReportText(),
                    parseMode: ParseMode.Html
                    )).MessageId;

                if (user.LastMessageId != 0 && Id != user.LastMessageId)
                {
                    await DeleteMessage(message.Chat.Id, user.LastMessageId);
                }

                await new OperationsUser().SetLastMessageIdUser(user, Id);
            }

            await new OperationsUser().SetPositionDialogUser(user, 322);
        }
        #endregion

        #endregion

        #region Помощь

        #region Определение действия
        private async Task SelectingSubActionHelp(CallbackQuery callbackQuery, Domain.Models.User user)
        {
            int PositionDialog = user.PositionDialog - 400;

            switch (PositionDialog / 10)
            {
                case 2:
                    switch (callbackQuery.Data)
                    {
                        case "Back":
                            await SendHelp(callbackQuery.Message, user);
                            break;
                    }
                    break;
            }
        }
        #endregion

        #region Основное
        private async Task SendHelp(Message message, Domain.Models.User user)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(OperationsCreateMessagesBot.GetHelpButtons());

            if (message.From.Id == Bot.BotId)
            {
                await Bot.EditMessageTextAsync(
                        chatId: message.Chat.Id,
                        messageId: message.MessageId,
                        text: OperationsCreateMessagesBot.GetHelpText(user),
                        replyMarkup: inlineKeyboard,
                        parseMode: ParseMode.Html
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetHelpText(user),
                    replyMarkup: inlineKeyboard,
                    parseMode: ParseMode.Html
                    )).MessageId;

                if (user.LastMessageId != 0 && Id != user.LastMessageId)
                {
                    await DeleteMessage(message.Chat.Id, user.LastMessageId);
                }

                await new OperationsUser().SetLastMessageIdUser(user, Id);
            }

            await new OperationsUser().SetPositionDialogUser(user, 400);
        }
        #endregion

        #region Актуальные вопросы
        private async Task SendTopQuestions(Message message, Domain.Models.User user)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(OperationsCreateMessagesBot.GetTopQuestionsButtons());

            if (message.From.Id == Bot.BotId)
            {
                await Bot.EditMessageTextAsync(
                        chatId: message.Chat.Id,
                        messageId: message.MessageId,
                        text: OperationsCreateMessagesBot.GetTopQuestionsText(SenderName),
                        replyMarkup: inlineKeyboard,
                        parseMode: ParseMode.Html
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetTopQuestionsText(SenderName),
                    replyMarkup: inlineKeyboard,
                    parseMode: ParseMode.Html
                    )).MessageId;

                if (user.LastMessageId != 0 && Id != user.LastMessageId)
                {
                    await DeleteMessage(message.Chat.Id, user.LastMessageId);
                }

                await new OperationsUser().SetLastMessageIdUser(user, Id);
            }

            await new OperationsUser().SetPositionDialogUser(user, 420);
        }
        #endregion

        #endregion

        #region Общие сообщения
        private async Task SendPopUp(string Id, string Message)
        {
            await Bot.AnswerCallbackQueryAsync(Id, Message, true);
        }
        private async Task SendError(Message message, string Text, Domain.Models.User user)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(OperationsCreateMessagesBot.GetErrorButtons());

            int Id = (await Bot.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: Text,
                replyMarkup: inlineKeyboard,
                parseMode: ParseMode.Html
                )).MessageId;

            if (user.LastMessageId != 0 && Id != user.LastMessageId)
            {
                await DeleteMessage(message.Chat.Id, user.LastMessageId);
            }

            await new OperationsUser().SetLastMessageIdUser(user, Id);

            logger.Error("Пользователь:" + message.Chat.Id + " Ошибка:" + Text);
        }
        private async Task DeleteMessage(ChatId chatId, int messageId)
        {
            try
            {
                await Bot.DeleteMessageAsync(
                    chatId: chatId,
                    messageId: messageId
                );
            }
            catch (Exception Ex)
            {
                logger.Error(Ex.ToString());
            }
        }
        #endregion
        #endregion

        #region Методы
        public async void CheckPay(Domain.Models.User user)
        {
            //if (user.BillId != null)
            //{
            //    bool Check = true;

            //    while (Check)
            //    {
            //        try
            //        {
            //            switch (await OperationsQIWI.CheckBill(user))
            //            {
            //                case 0:
            //                    Thread.Sleep(1000);
            //                    continue;
            //                case 1:
            //                    UpdateInfo?.Invoke();
            //                    Check = false;
            //                    break;
            //                default:
            //                    Check = false;
            //                    break;
            //            }
            //        }
            //        catch
            //        {
            //            Thread.Sleep(1000);
            //        }
            //    }
            //}
        }
        private bool CheckCountSearch(Domain.Models.User user)
        {
            //if (OperationsKolesa.GetCountSearch(user.Filters.Last()) > 0)
            //{
            //    return true;
            //}
            //else
            //{
            //    return false;
            //}

            return true;
        }
        #endregion

        #region Настройки
        public void Load()
        {
            try
            {
                XDocument doc = XDocument.Load(Constants.PathBots);

                Constants.BotMain.Token = doc.Element("Bots").Element("TokenMain").Value;
                Constants.BotSender.Token = doc.Element("Bots").Element("TokenSender").Value;
            }
            catch (Exception Ex)
            {
                throw new Exception(Ex.Message, Ex);
            }
        }
        #endregion
    }
}
