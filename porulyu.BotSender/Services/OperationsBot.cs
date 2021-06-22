﻿using NLog;
using porulyu.BotSender.Common;
using porulyu.Domain.Models;
using porulyu.Infrastructure.Cryptography;
using porulyu.Infrastructure.Data;
using porulyu.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace porulyu.BotSender.Services
{
    public class OperationsBot
    {
        #region Поля
        private readonly Logger logger;

        private TelegramBotClient Bot;

        private OperationsUser OperationsUser;
        private OperationsCreateMessagesBot OperationsCreateMessagesBot;

        private string MainName;
        #endregion

        #region Инициализация
        public OperationsBot()
        {
            logger = LogManager.GetCurrentClassLogger();

            OperationsCreateMessagesBot = new OperationsCreateMessagesBot();
        }

        public async Task Start()
        {
            Encryption encryption = new Encryption();
            Bot = new TelegramBotClient(encryption.DecryptRSA(Constants.BotSender.Token));

            TelegramBotClient mainUserName = new TelegramBotClient(encryption.DecryptRSA(Constants.BotMain.Token));
            var me = await mainUserName.GetMeAsync();
            MainName = me.Username;

            OperationsUser = new OperationsUser();

            Bot.OnMessage += BotOnMessageReceived;
            Bot.OnMessageEdited += BotOnMessageReceived;
            Bot.OnCallbackQuery += BotOnCallbackQueryReceived;

            Bot.StartReceiving(Array.Empty<UpdateType>());
        }
        #endregion

        #region События бота
        private async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            try
            {
                var message = messageEventArgs.Message;
                if (message == null || message.Type != MessageType.Text)
                    return;

                Domain.Models.User user = await OperationsUser.GetUser(message.Chat.Id);

                if (user != null)
                {
                    await OperationsUser.Activate(user.ChatId, true);
                }

                await SendUsage(message);
            }
            catch (Exception Ex)
            {
                logger.Error(Ex.Message);
            }
        }
        private async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            try
            {
                var callbackQuery = callbackQueryEventArgs.CallbackQuery;

                Domain.Models.User user = await new OperationsUser().GetUser(callbackQuery.Message.Chat.Id);

                string[] Data = callbackQuery.Data.Split('\'');

                switch (Data[0])
                {
                    case "Report":
                        await SendReport(callbackQuery.Message);
                        break;
                    case "Complains":
                        await SendComplains(callbackQuery.Message, Data[1]);
                        break;
                    case "ComplainExpired":
                        await new OperationsComplain().Create(callbackQuery.Message.Chat.Id, Data[0], Data[1]);
                        await SendPopUp(callbackQuery.Id, "Выша жалоба принята!");
                        await SendAd(callbackQuery.Message, Data[1]);
                        break;
                    case "ComplainNotValid":
                        await new OperationsComplain().Create(callbackQuery.Message.Chat.Id, Data[0], Data[1]);
                        await SendPopUp(callbackQuery.Id, "Выша жалоба принята!");
                        await SendAd(callbackQuery.Message, Data[1]);
                        break;
                    case "ComplainOther":
                        await new OperationsComplain().Create(callbackQuery.Message.Chat.Id, Data[0], Data[1]);
                        await SendPopUp(callbackQuery.Id, "Выша жалоба принята!");
                        await SendAd(callbackQuery.Message, Data[1]);
                        break;
                    case "Back":
                        await SendAd(callbackQuery.Message, Data[1]);
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

        #region Сообщения бота
        private async Task SendUsage(Message message)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(OperationsCreateMessagesBot.GetUsageButtons(MainName));

            await Bot.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: OperationsCreateMessagesBot.GetUsageText(),
                replyMarkup: inlineKeyboard
                );
        }
        private async Task SendReport(Message message)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(OperationsCreateMessagesBot.GetReportButtons(MainName));

            await Bot.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: OperationsCreateMessagesBot.GetReportText(),
                replyMarkup: inlineKeyboard
                );
        }
        private async Task SendComplains(Message message, string Link)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(OperationsCreateMessagesBot.GetComplainsButtons(Link));

            if (message.Text == null)
            {
                await Bot.EditMessageCaptionAsync(
                    messageId: message.MessageId,
                    chatId: message.Chat.Id,
                    caption: message.Caption,
                    replyMarkup: inlineKeyboard
                    );
            }
            else
            {
                await Bot.EditMessageTextAsync(
                    messageId: message.MessageId,
                    chatId: message.Chat.Id,
                    text: message.Text,
                    replyMarkup: inlineKeyboard
                    );
            }
        }
        private async Task SendAd(Message message, string Link)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(OperationsCreateMessagesBot.GetAdButtons(Link));

            if (message.Text == null)
            {
                await Bot.EditMessageCaptionAsync(
                    messageId: message.MessageId,
                    chatId: message.Chat.Id,
                    caption: message.Caption,
                    replyMarkup: inlineKeyboard
                    );
            }
            else
            {
                await Bot.EditMessageTextAsync(
                    messageId: message.MessageId,
                    chatId: message.Chat.Id,
                    text: message.Text,
                    replyMarkup: inlineKeyboard
                    );
            }
        }
        private async Task SendPopUp(string Id, string Message)
        {
            await Bot.AnswerCallbackQueryAsync(Id, Message);
        }
        public async Task SendNewAd(Ad ad, long ChatId)
        {
            try
            {
                Encryption encryption = new Encryption();
                Bot = new TelegramBotClient(encryption.DecryptRSA(Constants.BotSender.Token));

                Thread.Sleep(Constants.TimeoutBeforeSend);

                bool Send = false;

                var inlineKeyboard = new InlineKeyboardMarkup(OperationsCreateMessagesBot.GetAdButtons(ad.URL));

                while (!Send)
                {
                    try
                    {
                        if (ad.PhotosFileName != null)
                        {
                            var fileStream = new FileStream(ad.PhotosFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                            var fileName = ad.PhotosFileName.Split(Path.DirectorySeparatorChar).Last();
                            await Bot.SendPhotoAsync(
                                chatId: ChatId,
                                photo: new InputOnlineFile(fileStream, fileName),
                                caption: OperationsCreateMessagesBot.GetAdText(ad),
                                replyMarkup: inlineKeyboard,
                                parseMode: ParseMode.Html
                            );
                        }
                        else
                        {
                            await Bot.SendTextMessageAsync(
                                chatId: ChatId,
                                text: OperationsCreateMessagesBot.GetAdText(ad),
                                replyMarkup: inlineKeyboard,
                                parseMode: ParseMode.Html
                                );
                        }

                        Send = true;
                    }
                    catch (Exception Ex)
                    {
                        if (Ex.Message == "Forbidden: bot was blocked by the user")
                        {
                            throw new Exception(Ex.Message, Ex);
                        }
                        else
                        {
                            Thread.Sleep(1000);
                        }
                    }
                }

                DeleteTemp(ad.Id, ChatId.ToString());
            }
            catch (Exception Ex)
            {
                throw new Exception(Ex.Message, Ex);
            }
        }
        private void DeleteTemp(string Id, string ChatId)
        {
            try
            {
                Directory.Delete(Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName) + $@"\Temp\{ChatId}\{Id}", true);
            }
            catch (Exception Ex)
            {
                throw new Exception(Ex.Message, Ex);
            }
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