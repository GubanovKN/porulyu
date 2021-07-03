using porulyu.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace porulyu.BotSender.Services.Main
{
    public class OperationsCreateMessagesBot
    {
        #region Основное
        public InlineKeyboardButton[] GetUsageButtons(string UserName)
        {
            InlineKeyboardButton[] inlineKeyboards = new InlineKeyboardButton[1];

            inlineKeyboards = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithUrl("Перейти в основного бота", $@"https://t.me/{UserName}")
            };

            return inlineKeyboards;
        }
        public string GetUsageText()
        {
            string Text = "Привет!\n\r" +
                   "Этот бот рассылки быстрого поиска автомобилей\n\r" +
                   "\n\r" +
                   "Если вы уже настроили фильтры, пожалуйста подождите, скоро вы получите новые объявления.\n\r" +
                   "Иначе перейдите в основного бота нашего сервиса";

            return Text;
        }
        #endregion

        #region Объявление
        public string GetAdText(Ad ad)
        {
            string text = "";

            if (!String.IsNullOrEmpty(ad.Title))
            {
                text += $"{ad.Title}\n";
            }
            if (!String.IsNullOrEmpty(ad.City))
            {
                text += $"├Город - {ad.City}\n";
            }
            if (!String.IsNullOrEmpty(ad.Body))
            {
                text += $"├Кузов - {ad.Body}\n";
            }
            if (!String.IsNullOrEmpty(ad.EngineCapacity))
            {
                text += $"├Объем двиг., л - {ad.EngineCapacity}\n";
            }
            if (!String.IsNullOrEmpty(ad.Mileage))
            {
                text += $"├Пробег - {ad.Mileage}\n";
            }
            if (!String.IsNullOrEmpty(ad.Transmission))
            {
                text += $"├Коробка передач - {ad.Transmission}\n";
            }
            if (!String.IsNullOrEmpty(ad.Wheel))
            {
                text += $"├Руль - {ad.Wheel}\n";
            }
            if (!String.IsNullOrEmpty(ad.Color))
            {
                text += $"├Цвет - {ad.Color}\n";
            }
            if (!String.IsNullOrEmpty(ad.Actuator))
            {
                text += $"├Привод - {ad.Actuator}\n";
            }
            if (!String.IsNullOrEmpty(ad.CustomsClearedKZ))
            {
                text += $"├Растаможен в РК - {ad.CustomsClearedKZ}\n";
            }
            if (!String.IsNullOrEmpty(ad.State))
            {
                text += $"├Состояние - {ad.State}\n";
            }

            if (!String.IsNullOrEmpty(ad.Discription))
            {
                string check = $"{text}\n\nОписание владельца\n{ad.Discription}\n\nЦена - {ad.Price}\n\nИсточник - {ad.URL}";

                if ((check.Length >= 1024))
                {
                    text += $"\nОписание владельца\n{ad.Discription.Remove(ad.Discription.Length - (check.Length - 1023))}\n\n";
                }
                else
                {
                    text += $"\nОписание владельца\n{ad.Discription}\n\n";
                }
            }

            if (!String.IsNullOrEmpty(ad.Price))
            {
                text += $"Цена - {ad.Price}\n\n";
            }

            if (!String.IsNullOrEmpty(ad.URL))
            {
                text += $"Источник - {ad.URL}";
            }

            return text;
        }
        public InlineKeyboardButton[][] GetAdButtons(string Link)
        {
            InlineKeyboardButton[][] inlineKeyboards = new InlineKeyboardButton[2][];

            inlineKeyboards[0] = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithUrl("К объявлению", Link),
                InlineKeyboardButton.WithCallbackData("Посмотреть отчет", "Report")
            };
            inlineKeyboards[1] = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("Пожаловаться", $"Complains'{Link}")
            };

            return inlineKeyboards;
        }
        #endregion

        #region Отзывы
        public InlineKeyboardButton[][] GetComplainsButtons(string Link)
        {
            InlineKeyboardButton[][] inlineKeyboards = new InlineKeyboardButton[4][];

            inlineKeyboards[0] = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("Снято с публикации", $"ComplainExpired'{Link}")
            };
            inlineKeyboards[1] = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("Ошибки в объявлении", $"ComplainNotValid'{Link}")
            };
            inlineKeyboards[2] = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("Другое", $"ComplainOther'{Link}")
            };
            inlineKeyboards[3] = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("Назад", $"Back'{Link}")
            };

            return inlineKeyboards;
        }
        public string GetReportText()
        {
            string Text = "Для получения отчета по автомобилю:\n\r" +
                          "\n\r" +
                          "1)Перейдите в основного бота\n\r" +
                          "2)Вернитесь в главное меню, если уже не находитесь в нём\n\r" +
                          "3)Нажмите на кнопку \"Проверка авто\"\n\r" +
                          "4)Нажмите на кнопку \"Проверить авто\"\n\r" +
                          "5)Введите Гос. Номер или VIN код автомобиля\n\r";

            return Text;
        }
        public InlineKeyboardButton[] GetReportButtons(string UserName)
        {
            InlineKeyboardButton[] inlineKeyboards = new InlineKeyboardButton[1];

            inlineKeyboards = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithUrl("Перейти в основного бота", $@"https://t.me/{UserName}")
            };

            return inlineKeyboards;
        }
        #endregion
    }
}
