using NLog;
using porulyu.BotMain.Common;
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

namespace porulyu.BotMain.Services
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
                                    }
                                    break;
                                case 3:
                                    PositionDialog = user.PositionDialog - 300;

                                    switch (PositionDialog)
                                    {
                                        case 10:
                                            await SendFreeReport(message, user);
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
                    case -1:
                        switch (callbackQuery.Data)
                        {
                            case "Accept":
                                await new OperationsUser().SetUserAgreementUser(user);
                                await SendFirstUsage(callbackQuery.Message, user);
                                break;
                            case "OK":
                                await SendUserAgreement(callbackQuery.Message, user);
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
                                    await new OperationsFilter().CreateFilter(user);
                                    await SendCreateFitler(callbackQuery.Message, user);
                                    break;
                                case "MyFilters":
                                    await SendMyFilters(callbackQuery.Message, user);
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
                                case "ChangeRate":
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

            await new OperationsUser().SetPositionDialogUser(user, -100);
        }
        #endregion

        #region Главное меню
        private async Task SendFirstUsage(Message message, Domain.Models.User user)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(OperationsCreateMessagesBot.GetUsageButtons());

            if (message.From.Id == Bot.BotId)
            {
                await Bot.EditMessageTextAsync(
                    chatId: message.Chat.Id,
                    messageId: message.MessageId,
                    text: OperationsCreateMessagesBot.GetFirstUsageText(),
                    replyMarkup: inlineKeyboard
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetFirstUsageText(),
                    replyMarkup: inlineKeyboard
                    )).MessageId;

                if (user.LastMessageId != 0 && Id != user.LastMessageId)
                {
                    await DeleteMessage(message.Chat.Id, user.LastMessageId);
                }

                await new OperationsUser().SetLastMessageIdUser(user, Id);
            }

            await new OperationsUser().SetPositionDialogUser(user, 0);
        }
        private async Task SendUsage(Message message, Domain.Models.User user)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(OperationsCreateMessagesBot.GetUsageButtons());

            if (message.From.Id == Bot.BotId)
            {
                await Bot.EditMessageTextAsync(
                    chatId: message.Chat.Id,
                    messageId: message.MessageId,
                    text: OperationsCreateMessagesBot.GetUsageText(),
                    replyMarkup: inlineKeyboard
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetUsageText(),
                    replyMarkup: inlineKeyboard
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
                                if (user.Filters.Last().MarkAlias != null)
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
                                if (user.Filters.Last().RegionAlias != null)
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
                                    await SendSave(callbackQuery.Message, user);
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
                                    await SendSave(callbackQuery.Message, user);
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
                            await new OperationsFilter().SaveModelAliasFilter(null, user);
                            await new OperationsFilter().SaveMarkAliasFilter(null, user);
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
                            await new OperationsFilter().SaveMarkAliasFilter(callbackQuery.Data, user);
                            await SendCreateFitler(callbackQuery.Message, user);
                            break;
                    }
                    break;
                case 3:
                    switch (callbackQuery.Data)
                    {
                        case "Clear":
                            await new OperationsFilter().SaveModelAliasFilter(null, user);
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
                            await new OperationsFilter().SaveModelAliasFilter(callbackQuery.Data, user);
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
                            await new OperationsFilter().SaveCityAliasFilter(null, user);
                            await new OperationsFilter().SaveRegionAliasFilter(null, user);
                            await SendCreateFitler(callbackQuery.Message, user);
                            break;
                        case "Back":
                            await SendCreateFitler(callbackQuery.Message, user);
                            break;
                        default:
                            await new OperationsFilter().SaveRegionAliasFilter(callbackQuery.Data, user);
                            await SendCreateFitler(callbackQuery.Message, user);
                            break;
                    }
                    break;
                case 7:
                    switch (callbackQuery.Data)
                    {
                        case "Clear":
                            await new OperationsFilter().SaveCityAliasFilter(null, user);
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
                            await new OperationsFilter().SaveCityAliasFilter(callbackQuery.Data, user);
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
                            await new OperationsFilter().SaveCustomsСlearedFilter(null, user);
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
                            await new OperationsFilter().SaveTransmissionFilter(null, user);
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
                            await new OperationsFilter().SaveActuatorFilter(null, user);
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
                            await new OperationsFilter().SaveMileageFilter("0-0", user);
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
                        replyMarkup: inlineKeyboard
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetFiltersText(),
                    replyMarkup: inlineKeyboard
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
                        text: OperationsCreateMessagesBot.GetSaveText(user.Filters.Last()) + "\n\r" + OperationsCreateMessagesBot.GetCreateFilterText(),
                        replyMarkup: inlineKeyboard,
                        parseMode: ParseMode.Html
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetSaveText(user.Filters.Last()) + "\n\r" + OperationsCreateMessagesBot.GetCreateFilterText(),
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
                        text: OperationsCreateMessagesBot.GetSaveText(user.Filters.Last()) + "\n\r" + OperationsCreateMessagesBot.GetCreateFilterOptionallyText(),
                        replyMarkup: inlineKeyboard,
                        parseMode: ParseMode.Html
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetSaveText(user.Filters.Last()) + "\n\r" + OperationsCreateMessagesBot.GetCreateFilterOptionallyText(),
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
                        replyMarkup: inlineKeyboard
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetNameText(),
                    replyMarkup: inlineKeyboard
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
            var inlineKeyboard = new InlineKeyboardMarkup(OperationsCreateMessagesBot.GetMarksButtons(user));

            if (message.From.Id == Bot.BotId)
            {
                await Bot.EditMessageTextAsync(
                        chatId: message.Chat.Id,
                        messageId: message.MessageId,
                        text: OperationsCreateMessagesBot.GetMarksText(),
                        replyMarkup: inlineKeyboard
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetMarksText(),
                    replyMarkup: inlineKeyboard
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
            var inlineKeyboard = new InlineKeyboardMarkup(OperationsCreateMessagesBot.GetModelsButtons(user));

            if (message.From.Id == Bot.BotId)
            {
                await Bot.EditMessageTextAsync(
                        chatId: message.Chat.Id,
                        messageId: message.MessageId,
                        text: OperationsCreateMessagesBot.GetModelsText(),
                        replyMarkup: inlineKeyboard
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetModelsText(),
                    replyMarkup: inlineKeyboard
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
                        replyMarkup: inlineKeyboard
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetYearsText(),
                    replyMarkup: inlineKeyboard
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
                        replyMarkup: inlineKeyboard
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetPricesText(),
                    replyMarkup: inlineKeyboard
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
            var inlineKeyboard = new InlineKeyboardMarkup(OperationsCreateMessagesBot.GetRegionsButtons());

            if (message.From.Id == Bot.BotId)
            {
                await Bot.EditMessageTextAsync(
                        chatId: message.Chat.Id,
                        messageId: message.MessageId,
                        text: OperationsCreateMessagesBot.GetRegionsText(),
                        replyMarkup: inlineKeyboard
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetRegionsText(),
                    replyMarkup: inlineKeyboard
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
            var inlineKeyboard = new InlineKeyboardMarkup(OperationsCreateMessagesBot.GetCitiesButtons(user));

            if (message.From.Id == Bot.BotId)
            {
                await Bot.EditMessageTextAsync(
                        chatId: message.Chat.Id,
                        messageId: message.MessageId,
                        text: OperationsCreateMessagesBot.GetCitiesText(),
                        replyMarkup: inlineKeyboard
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetCitiesText(),
                    replyMarkup: inlineKeyboard
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
                        replyMarkup: inlineKeyboard
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetCustomsСlearedText(),
                    replyMarkup: inlineKeyboard
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
                        replyMarkup: inlineKeyboard
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetTransmissionText(),
                    replyMarkup: inlineKeyboard
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
                        replyMarkup: inlineKeyboard
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetActuatorText(),
                    replyMarkup: inlineKeyboard
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
                        replyMarkup: inlineKeyboard
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetEngineCapacityText(),
                    replyMarkup: inlineKeyboard
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
                        replyMarkup: inlineKeyboard
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetMileageText(),
                    replyMarkup: inlineKeyboard
                    )).MessageId;

                if (user.LastMessageId != 0 && Id != user.LastMessageId)
                {
                    await DeleteMessage(message.Chat.Id, user.LastMessageId);
                }

                await new OperationsUser().SetLastMessageIdUser(user, Id);
            }

            await new OperationsUser().SetPositionDialogUser(user, 125);
        }
        private async Task SendSave(Message message, Domain.Models.User user)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(OperationsCreateMessagesBot.GetSaveButtons());

            if (message.From.Id == Bot.BotId)
            {
                await Bot.EditMessageTextAsync(
                        chatId: message.Chat.Id,
                        messageId: message.MessageId,
                        text: "<b>Проверьте перед сохранением</b>\n\r" + OperationsCreateMessagesBot.GetSaveText(user.Filters.Last()),
                        replyMarkup: inlineKeyboard,
                        parseMode: ParseMode.Html
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "<b>Проверьте перед сохранением</b>\n\r" + OperationsCreateMessagesBot.GetSaveText(user.Filters.Last()),
                    replyMarkup: inlineKeyboard,
                    parseMode: ParseMode.Html
                    )).MessageId;

                if (user.LastMessageId != 0 && Id != user.LastMessageId)
                {
                    await DeleteMessage(message.Chat.Id, user.LastMessageId);
                }

                await new OperationsUser().SetLastMessageIdUser(user, Id);
            }

            await new OperationsUser().SetPositionDialogUser(user, 130);
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
                        replyMarkup: inlineKeyboard
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Фильтр успешно сохранен!",
                    replyMarkup: inlineKeyboard
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
                        replyMarkup: inlineKeyboard
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetMyFiltersText(),
                    replyMarkup: inlineKeyboard
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
                        text: OperationsCreateMessagesBot.GetSaveText(user.Filters.First(p => p.Id == Convert.ToInt64(Id))),
                        replyMarkup: inlineKeyboard,
                        parseMode: ParseMode.Html
                    );
            }
            else
            {
                int MessageId = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetSaveText(user.Filters.First(p => p.Id == Convert.ToInt64(Id))),
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
                            case "Back":
                                await SendRates(callbackQuery.Message, user);
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
            var inlineKeyboard = new InlineKeyboardMarkup(OperationsCreateMessagesBot.GetRatesButtons());

            if (message.From.Id == Bot.BotId)
            {
                await Bot.EditMessageTextAsync(
                        chatId: message.Chat.Id,
                        messageId: message.MessageId,
                        text: OperationsCreateMessagesBot.GetRatesText(user),
                        replyMarkup: inlineKeyboard
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetRatesText(user),
                    replyMarkup: inlineKeyboard
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
                        replyMarkup: inlineKeyboard
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetAllRatesText(),
                    replyMarkup: inlineKeyboard
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
            var inlineKeyboard = new InlineKeyboardMarkup(OperationsCreateMessagesBot.GetRateButtons());

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
                        switch (callbackQuery.Data)
                        {
                            case "Back":
                                await SendCheckingCar(callbackQuery.Message, user);
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
                        case "Pay":
                            await SendReportPay();
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
                        replyMarkup: inlineKeyboard
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetCheckingCarText(),
                    replyMarkup: inlineKeyboard
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
                        replyMarkup: inlineKeyboard
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetCheckCarText(),
                    replyMarkup: inlineKeyboard
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
            var inlineKeyboard = new InlineKeyboardMarkup(OperationsCreateMessagesBot.GetFreeReportButtons());

            string Link = OperationsCheckCar.GetLinkReport(message.Text);
            string Report = OperationsCheckCar.GetFreeReport(Link);

            await new OperationsReport().Create(user.ChatId, message.Text, Link);

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

            await new OperationsUser().SetPositionDialogUser(user, 311);
        }
        private async Task SendReportPay(Message message, Domain.Models.User user)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(OperationsCreateMessagesBot.GetFreeReportButtons());

            string Link = OperationsCheckCar.GetLinkReport(message.Text);
            string Report = OperationsCheckCar.GetFreeReport(Link);

            await new OperationsReport().Create(user.ChatId, message.Text, Link);

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

            await new OperationsUser().SetPositionDialogUser(user, 311);
        }
        #endregion

        #region Просмотр отчетов

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
                        replyMarkup: inlineKeyboard
                    );
            }
            else
            {
                int Id = (await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: OperationsCreateMessagesBot.GetHelpText(user),
                    replyMarkup: inlineKeyboard
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
                replyMarkup: inlineKeyboard
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
