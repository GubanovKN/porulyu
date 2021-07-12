using porulyu.BotMain.Common;
using porulyu.Domain.Models;
using porulyu.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace porulyu.BotMain.Services.Main
{
    public class OperationsCreateMessagesBot
    {
        #region Пользовательское соглашение
        public InlineKeyboardButton[] GetUserAgreementButtons()
        {
            InlineKeyboardButton[] inlineKeyboards = new InlineKeyboardButton[1];

            inlineKeyboards = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("Принимаю", "Accept")
            };

            return inlineKeyboards;
        }
        public string GetUserAgreementText()
        {
            string Text = "Привет, это бот поРулю – помощник в поиске авто\n\r" +
                          "Для продолжения работы необходимо ознакомиться и принять\n\r" +
                          "<a href=\"https://test.com\">\"Пользовательское соглашение\"</a>.";

            return Text;
        }
        #endregion

        #region Акция
        public InlineKeyboardButton[] GetGiftButtons(string Name)
        {
            InlineKeyboardButton[] inlineKeyboards = new InlineKeyboardButton[1];

            inlineKeyboards = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData($"Активировать {Name} тариф", "Activate")
            };

            return inlineKeyboards;
        }
        public string GetGiftText(Rate rate)
        {
            string Text = $"Новым пользователям мы дарим одну неделю {rate.Name} пакета бесплатно.\n\r" +
                          $"В {rate.Name} входит:\n\r" +
                          $"├Бесплатное создание до {rate.CountFilters} пользовательских фильтров поиска авто\n\r" +
                          "├Возможность получения отчёта по авто\n\r" +
                          "└Самые свежие и актуальные объявления со всех площадок по продаже авто\n\r";

            return Text;
        }
        #endregion

        #region Главное меню
        public InlineKeyboardButton[][] GetUsageButtons()
        {
            InlineKeyboardButton[][] inlineKeyboards = new InlineKeyboardButton[2][];

            inlineKeyboards[0] = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("Фильтры", "Filters"),
                InlineKeyboardButton.WithCallbackData("Тарифы", "Rates")
            };
            inlineKeyboards[1] = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("Проверка авто", "CheckingCar"),
                InlineKeyboardButton.WithCallbackData("Помощь", "Help")
            };

            return inlineKeyboards;
        }
        public string GetUsageText()
        {
            string Text = $"<i>Начать получать объявления одним из первых очень просто</i>\n\r" +
                          $"\n\r" +
                          $"<b>Для начала создай собственный фильтр для поиска авто</b>\n\r" +
                          $"После чего бот поРулю займётся поиском авто по вашим критериям на всех популярных площадках по продаже авто";

            return Text;
        }
        #endregion

        #region Фильтры
        public string GetFiltersText()
        {
            string Text = $"Здесь ты можешь <b>создавать</b> и <b>удалять</b> свои фильтры поиска";

            return Text;
        }
        public InlineKeyboardButton[][] GetFiltersButtons()
        {
            InlineKeyboardButton[][] inlineKeyboards = new InlineKeyboardButton[2][];

            inlineKeyboards[0] = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("Создать фильтр", "Create"),
                InlineKeyboardButton.WithCallbackData("Мои фильтры", "MyFilters")
            };
            inlineKeyboards[1] = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("Главное меню", "Main")
            };

            return inlineKeyboards;
        }
        public string GetCreateFilterText()
        {
            string Text = $"<b>Выбери необходимые данные в фильтре</b>\n\r" +
                           "<b>* - поля обязательные для заполнения</b>\n\r";

            return Text;
        }
        public InlineKeyboardButton[][] GetCreateFilterButtons()
        {
            InlineKeyboardButton[][] inlineKeyboards = new InlineKeyboardButton[5][];

            inlineKeyboards[0] = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("Имя*", "Name"),
                InlineKeyboardButton.WithCallbackData("Марка", "Mark")
            };
            inlineKeyboards[1] = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("Модель", "Model"),
                InlineKeyboardButton.WithCallbackData("Год", "Year")
            };
            inlineKeyboards[2] = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("Цена", "Price"),
                InlineKeyboardButton.WithCallbackData("Регион", "Region")
            };
            inlineKeyboards[3] = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("Город", "City"),
                InlineKeyboardButton.WithCallbackData("Дополнительно", "Optionally")
            };
            inlineKeyboards[4] = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("Назад", "Back"),
                InlineKeyboardButton.WithCallbackData("Далее", "Next")
            };

            return inlineKeyboards;
        }
        public string GetCreateFilterOptionallyText()
        {
            string Text = $"<b>Дополнительные параметры создания</b>\n\r" +
                           "<b>Выбери необходимые данные в фильтре</b>\n\r";

            return Text;
        }
        public InlineKeyboardButton[][] GetCreateFilterOptionallyButtons()
        {
            InlineKeyboardButton[][] inlineKeyboards = new InlineKeyboardButton[4][];

            inlineKeyboards[0] = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("Растаможен", "CustomsСleared"),
                InlineKeyboardButton.WithCallbackData("Тип КПП", "Transmission")
            };
            inlineKeyboards[1] = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("Привод", "Actuator"),
                InlineKeyboardButton.WithCallbackData("Объем двигателя", "EngineCapacity")
            };
            inlineKeyboards[2] = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("Пробег", "Mileage")
            };
            inlineKeyboards[3] = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("Назад", "Back"),
                InlineKeyboardButton.WithCallbackData("Далее", "Next")
            };

            return inlineKeyboards;
        }
        public string GetMyFiltersText()
        {
            string Text = $"После выбора фильтра, его можно удалить";

            return Text;
        }
        public InlineKeyboardButton[][] GetMyFiltersButtons(User user)
        {
            double Pages = user.Filters.Count / 12.0;

            int countKeyBoard = (user.Filters.Count - user.PositionView * 12) / 2;

            InlineKeyboardButton[][] inlineKeyboards;

            int row = 0;

            inlineKeyboards = new InlineKeyboardButton[countKeyBoard > 6 ? 7 : (user.Filters.Count - user.PositionView * 12) % 2 == 0 ? countKeyBoard + 1 : countKeyBoard + 2][];

            for (int i = 0; user.PositionView * 12 + i < user.Filters.Count && i < 12; i = i + 2)
            {
                if (user.PositionView * 12 + i == user.Filters.Count - 1)
                {
                    inlineKeyboards[row] = new InlineKeyboardButton[]
                    {
                       InlineKeyboardButton.WithCallbackData(user.Filters.ToList()[user.PositionView * 12 + i].Name, user.Filters.ToList()[user.PositionView * 12 + i].Id.ToString())
                    };
                }
                else
                {
                    inlineKeyboards[row] = new InlineKeyboardButton[]
                    {
                       InlineKeyboardButton.WithCallbackData(user.Filters.ToList()[user.PositionView * 12 + i].Name, user.Filters.ToList()[user.PositionView * 12 + i].Id.ToString()),
                       InlineKeyboardButton.WithCallbackData(user.Filters.ToList()[user.PositionView * 12 + i + 1].Name, user.Filters.ToList()[user.PositionView * 12 + i + 1].Id.ToString()),
                    };
                }

                row++;
            }

            if (Pages > user.PositionView + 1)
            {
                inlineKeyboards[row] = new InlineKeyboardButton[]
                {
                       InlineKeyboardButton.WithCallbackData("Назад", "Back"),
                       InlineKeyboardButton.WithCallbackData("Вперед", "Next")
                };
            }
            else
            {
                inlineKeyboards[row] = new InlineKeyboardButton[]
                {
                       InlineKeyboardButton.WithCallbackData("Назад", "Back")
                };
            }

            return inlineKeyboards;
        }
        public InlineKeyboardButton[][] GetSelectedFilterButtons(string Id)
        {
            InlineKeyboardButton[][] inlineKeyboards = new InlineKeyboardButton[2][];

            inlineKeyboards[0] = new InlineKeyboardButton[]
            {
                       InlineKeyboardButton.WithCallbackData("Удалить", Id)
            };
            inlineKeyboards[1] = new InlineKeyboardButton[]
            {
                       InlineKeyboardButton.WithCallbackData("Назад", "Back")
            };

            return inlineKeyboards;
        }

        #region Настройки фильтра
        public string GetNameText()
        {
            string Text = $"В данном пункте необходимо задать название фильтра\n\r";

            return Text;
        }
        public InlineKeyboardButton[][] GetNameButtons()
        {
            InlineKeyboardButton[][] inlineKeyboards = new InlineKeyboardButton[1][];

            inlineKeyboards[0] = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("Назад", "Back"),
                InlineKeyboardButton.WithCallbackData("Очистить", "Clear")
            };

            return inlineKeyboards;
        }
        public string GetMarksText()
        {
            string Text = $"В данном пункте необходимо выбрать марку автомобиля\n\r";

            return Text;
        }
        public async Task<InlineKeyboardButton[][]> GetMarksButtons(User user)
        {
            List<Mark> Marks = await new OperationsMark().GetAll();

            double Pages = Marks.Count / 12.0;

            int countKeyBoard = (Marks.Count - user.PositionView * 12) / 2;

            InlineKeyboardButton[][] inlineKeyboards;

            int row = 0;

            inlineKeyboards = new InlineKeyboardButton[countKeyBoard > 6 ? 7 : (Marks.Count - user.PositionView * 12) % 2 == 0 ? countKeyBoard + 1 : countKeyBoard + 2][];

            for (int i = 0; user.PositionView * 12 + i < Marks.Count && i < 12; i = i + 2)
            {
                if (user.PositionView * 12 + i == Marks.Count - 1)
                {
                    inlineKeyboards[row] = new InlineKeyboardButton[]
                    {
                       InlineKeyboardButton.WithCallbackData(Marks[user.PositionView * 12 + i].Name, Marks[user.PositionView * 12 + i].Id.ToString())
                    };
                }
                else
                {
                    inlineKeyboards[row] = new InlineKeyboardButton[]
                    {
                       InlineKeyboardButton.WithCallbackData(Marks[user.PositionView * 12 + i].Name, Marks[user.PositionView * 12 + i].Id.ToString()),
                       InlineKeyboardButton.WithCallbackData(Marks[user.PositionView * 12 + i + 1].Name, Marks[user.PositionView * 12 + i + 1].Id.ToString()),
                    };
                }

                row++;
            }

            if (Pages > user.PositionView + 1)
            {
                inlineKeyboards[row] = new InlineKeyboardButton[]
                {
                       InlineKeyboardButton.WithCallbackData("Назад", "Back"),
                       InlineKeyboardButton.WithCallbackData("Очистить", "Clear"),
                       InlineKeyboardButton.WithCallbackData("Вперед", "Next")
                };
            }
            else
            {
                inlineKeyboards[row] = new InlineKeyboardButton[]
                {
                       InlineKeyboardButton.WithCallbackData("Назад", "Back"),
                       InlineKeyboardButton.WithCallbackData("Очистить", "Clear")
                };
            }

            return inlineKeyboards;
        }
        public string GetModelsText()
        {
            string Text = $"В данном пункте необходимо выбрать модель автомобиля\n\r";

            return Text;
        }
        public async Task<InlineKeyboardButton[][]> GetModelsButtons(User user)
        {
            Mark Mark = await new OperationsMark().Get(user.Filters.Last().MarkId);
            List<Model> Models = Mark.Models.ToList();

            double Pages = Models.Count / 12.0;

            int countKeyBoard = (Models.Count - user.PositionView * 12) / 2;

            InlineKeyboardButton[][] inlineKeyboards;

            int row = 0;

            inlineKeyboards = new InlineKeyboardButton[countKeyBoard > 6 ? 7 : (Models.Count - user.PositionView * 12) % 2 == 0 ? countKeyBoard + 1 : countKeyBoard + 2][];

            for (int i = 0; user.PositionView * 12 + i < Models.Count && i < 12; i = i + 2)
            {
                if (user.PositionView * 12 + i == Models.Count - 1)
                {
                    inlineKeyboards[row] = new InlineKeyboardButton[]
                    {
                       InlineKeyboardButton.WithCallbackData(Models[user.PositionView * 12 + i].Name, Models[user.PositionView * 12 + i].Id.ToString())
                    };
                }
                else
                {
                    inlineKeyboards[row] = new InlineKeyboardButton[]
                    {
                       InlineKeyboardButton.WithCallbackData(Models[user.PositionView * 12 + i].Name, Models[user.PositionView * 12 + i].Id.ToString()),
                       InlineKeyboardButton.WithCallbackData(Models[user.PositionView * 12 + i + 1].Name, Models[user.PositionView * 12 + i + 1].Id.ToString()),
                    };
                }

                row++;
            }

            if (Pages > user.PositionView + 1)
            {
                inlineKeyboards[row] = new InlineKeyboardButton[]
                {
                       InlineKeyboardButton.WithCallbackData("Назад", "Back"),
                       InlineKeyboardButton.WithCallbackData("Очистить", "Clear"),
                       InlineKeyboardButton.WithCallbackData("Вперед", "Next")
                };
            }
            else
            {
                inlineKeyboards[row] = new InlineKeyboardButton[]
                {
                       InlineKeyboardButton.WithCallbackData("Назад", "Back"),
                       InlineKeyboardButton.WithCallbackData("Очистить", "Clear")
                };
            }

            return inlineKeyboards;
        }
        public string GetYearsText()
        {
            string Text = $"Введите года выпуска в формате (1960-{DateTime.Now.Year}).\n\r" +
                          $"Если не хотите указывать начальный или конечный год, просто оставтье его место пустым.";

            return Text;
        }
        public InlineKeyboardButton[][] GetYearsButtons()
        {
            InlineKeyboardButton[][] inlineKeyboards = new InlineKeyboardButton[1][];

            inlineKeyboards[0] = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("Назад", "Back"),
                InlineKeyboardButton.WithCallbackData("Очистить", "Clear")
            };

            return inlineKeyboards;
        }
        public string GetPricesText()
        {
            string Text = $"Введите цены в формате (1000-1000000).\n\r" +
                          $"Если не хотите указывать начальную или конечную цену, просто оставтье её место пустым.";

            return Text;
        }
        public InlineKeyboardButton[][] GetPricesButtons()
        {
            InlineKeyboardButton[][] inlineKeyboards = new InlineKeyboardButton[1][];

            inlineKeyboards[0] = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("Назад", "Back"),
                InlineKeyboardButton.WithCallbackData("Очистить", "Clear")
            };

            return inlineKeyboards;
        }
        public string GetRegionsText()
        {
            string Text = $"В данном пункте необходимо выбрать регион поиска\n\r";

            return Text;
        }
        public async Task<InlineKeyboardButton[][]> GetRegionsButtons()
        {
            List<Region> Regions = await new OperationsRegion().GetAll(); 

            InlineKeyboardButton[][] inlineKeyboards = new InlineKeyboardButton[8][];

            int row = 0;

            for (int i = 0; i < Regions.Count; i = i + 2)
            {
                inlineKeyboards[row] = new InlineKeyboardButton[]
                {
                       InlineKeyboardButton.WithCallbackData(Regions[i].Name, Regions[i].Id.ToString()),
                       InlineKeyboardButton.WithCallbackData(Regions[i+1].Name, Regions[i+1].Id.ToString()),
                };

                row++;
            }
            inlineKeyboards[7] = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("Назад", "Back"),
                InlineKeyboardButton.WithCallbackData("Очистить", "Clear")
            };

            return inlineKeyboards;
        }
        public string GetCitiesText()
        {
            string Text = $"В данном пункте необходимо выбрать город поиска\n\r";

            return Text;
        }
        public async Task<InlineKeyboardButton[][]> GetCitiesButtons(User user)
        {
            Region Region = await new OperationsRegion().Get(user.Filters.Last().RegionId);
            List<City> Cities = Region.Cities.ToList();

            double Pages = Cities.Count / 12.0;

            int countKeyBoard = (Cities.Count - user.PositionView * 12) / 2;

            InlineKeyboardButton[][] inlineKeyboards;

            int row = 0;

            inlineKeyboards = new InlineKeyboardButton[countKeyBoard > 6 ? 7 : (Cities.Count - user.PositionView * 12) % 2 == 0 ? countKeyBoard + 1 : countKeyBoard + 2][];

            for (int i = 0; user.PositionView * 12 + i < Cities.Count && i < 12; i = i + 2)
            {
                if (user.PositionView * 12 + i == Cities.Count - 1)
                {
                    inlineKeyboards[row] = new InlineKeyboardButton[]
                    {
                       InlineKeyboardButton.WithCallbackData(Cities[user.PositionView * 12 + i].Name, Cities[user.PositionView * 12 + i].Id.ToString())
                    };
                }
                else
                {
                    inlineKeyboards[row] = new InlineKeyboardButton[]
                    {
                       InlineKeyboardButton.WithCallbackData(Cities[user.PositionView * 12 + i].Name, Cities[user.PositionView * 12 + i].Id.ToString()),
                       InlineKeyboardButton.WithCallbackData(Cities[user.PositionView * 12 + i + 1].Name, Cities[user.PositionView * 12 + i + 1].Id.ToString()),
                    };
                }

                row++;
            }

            if (Pages > user.PositionView + 1)
            {
                inlineKeyboards[row] = new InlineKeyboardButton[]
                {
                       InlineKeyboardButton.WithCallbackData("Назад", "Back"),
                       InlineKeyboardButton.WithCallbackData("Очистить", "Clear"),
                       InlineKeyboardButton.WithCallbackData("Вперед", "Next")
                };
            }
            else
            {
                inlineKeyboards[row] = new InlineKeyboardButton[]
                {
                       InlineKeyboardButton.WithCallbackData("Назад", "Back"),
                       InlineKeyboardButton.WithCallbackData("Очистить", "Clear")
                };
            }

            return inlineKeyboards;
        }
        public string GetCustomsСlearedText()
        {
            string Text = $"В данном пункте необходимо выбрать растаможен ли автомобиль\n\r";

            return Text;
        }
        public InlineKeyboardButton[][] GetCustomsСlearedButtons()
        {
            InlineKeyboardButton[][] inlineKeyboards = new InlineKeyboardButton[2][];

            inlineKeyboards[0] = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("Да", "Yes")
            };
            inlineKeyboards[1] = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("Назад", "Back"),
                InlineKeyboardButton.WithCallbackData("Очистить", "Clear")
            };

            return inlineKeyboards;
        }
        public string GetTransmissionText()
        {
            string Text = $"В данном пункте необходимо выбрать тип КПП\n\r";

            return Text;
        }
        public InlineKeyboardButton[][] GetTransmissionButtons()
        {
            InlineKeyboardButton[][] inlineKeyboards = new InlineKeyboardButton[2][];

            inlineKeyboards[0] = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("АКПП", "Automate"),
                InlineKeyboardButton.WithCallbackData("Механика", "Mechanics")
            };
            inlineKeyboards[1] = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("Назад", "Back"),
                InlineKeyboardButton.WithCallbackData("Очистить", "Clear")
            };

            return inlineKeyboards;
        }
        public string GetActuatorText()
        {
            string Text = $"В данном пункте необходимо выбрать привод автомобиля\n\r";

            return Text;
        }
        public InlineKeyboardButton[][] GetActuatorButtons()
        {
            InlineKeyboardButton[][] inlineKeyboards = new InlineKeyboardButton[2][];

            inlineKeyboards[0] = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("Передний", "Front"),
                InlineKeyboardButton.WithCallbackData("Полный", "Full"),
                InlineKeyboardButton.WithCallbackData("Задний", "Rear")
            };
            inlineKeyboards[1] = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("Назад", "Back"),
                InlineKeyboardButton.WithCallbackData("Очистить", "Clear")
            };

            return inlineKeyboards;
        }
        public string GetEngineCapacityText()
        {
            string Text = $"Введите объем двигателя в формате (1.0-6.0).\n\r" +
                          $"Если не хотите указывать начальный или конечный объем, просто оставтье его место пустым.";

            return Text;
        }
        public InlineKeyboardButton[][] GetEngineCapacityButtons()
        {
            InlineKeyboardButton[][] inlineKeyboards = new InlineKeyboardButton[1][];

            inlineKeyboards[0] = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("Назад", "Back"),
                InlineKeyboardButton.WithCallbackData("Очистить", "Clear")
            };

            return inlineKeyboards;
        }
        public string GetMileageText()
        {
            string Text = $"Введите максимальный пробег автомобиля в км.\n\r";

            return Text;
        }
        public InlineKeyboardButton[][] GetMileageButtons()
        {
            InlineKeyboardButton[][] inlineKeyboards = new InlineKeyboardButton[1][];

            inlineKeyboards[0] = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("Назад", "Back"),
                InlineKeyboardButton.WithCallbackData("Очистить", "Clear")
            };

            return inlineKeyboards;
        }
        public async Task<string> GetSaveText(Filter filter)
        {
            string Params = $"<b>Параметры фильтра</b>\n\r";

            Params += $"Имя фильтра: {filter.Name}\n\r";

            if (filter.MarkId != 0)
            {
                Mark mark = await new OperationsMark().Get(filter.MarkId);

                Params += $"Марка: {mark.Name}\n\r";

                if (filter.ModelId != 0)
                {
                    Model model = mark.Models.FirstOrDefault(p => p.Id == filter.ModelId);

                    Params += $"Модель: {model.Name}\n\r";
                }
                else
                {
                    Params += $"Модель: Все модели\n\r";
                }
            }
            else
            {
                Params += $"Марка: Все марки\n\r";
            }

            if (filter.FirstYear != 0 && filter.SecondYear != 0)
            {
                Params += $"Год: от {filter.FirstYear} до {filter.SecondYear}\n\r";
            }
            else if (filter.FirstYear != 0)
            {
                Params += $"Год: от {filter.FirstYear}\n\r";
            }
            else if (filter.SecondYear != 0)
            {
                Params += $"Год: до {filter.SecondYear}\n\r";
            }
            else
            {
                Params += $"Год: не указан\n\r";
            }

            if (filter.FirstPrice != 0 && filter.SecondPrice != 0)
            {
                Params += $"Цена: от {filter.FirstPrice} до {filter.SecondPrice}\n\r";
            }
            else if (filter.FirstPrice != 0)
            {
                Params += $"Цена: от {filter.FirstPrice}\n\r";
            }
            else if (filter.SecondPrice != 0)
            {
                Params += $"Цена: до {filter.SecondPrice}\n\r";
            }
            else
            {
                Params += $"Цена: не указана\n\r";
            }

            if (filter.RegionId != 0)
            {
                Region region = await new OperationsRegion().Get(filter.RegionId);

                Params += $"Регион: {region.Name}\n\r";

                if (filter.CityId != 0)
                {
                    City city = region.Cities.FirstOrDefault(p => p.Id == filter.CityId);

                    Params += $"Город: {city.Name}\n\r";
                }
                else
                {
                    Params += $"Город: Все города\n\r";
                }
            }
            else
            {
                Params += $"Регион: Все регионы\n\r";
            }

            switch (filter.CustomsСleared)
            {
                case 0:
                    Params += $"Растаможен: Не задано\n\r";
                    break;
                case 1:
                    Params += $"Растаможен: Да\n\r";
                    break;
            }

            switch (filter.Transmission)
            {
                case 0:
                    Params += $"Тип КПП: Не задано\n\r";
                    break;
                case 1:
                    Params += $"Тип КПП: Автомат\n\r";
                    break;
                case 2:
                    Params += $"Тип КПП: Механика\n\r";
                    break;
            }

            switch (filter.Actuator)
            {
                case 0:
                    Params += $"Привод: Не задано\n\r";
                    break;
                case 1:
                    Params += $"Привод: Передний\n\r";
                    break;
                case 2:
                    Params += $"Привод: Полный\n\r";
                    break;
                case 3:
                    Params += $"Привод: Задний\n\r";
                    break;
            }

            if (filter.FirstEngineCapacity != 0 && filter.SecondEngineCapacity != 0)
            {
                Params += $"Объем двигателя: от {filter.FirstEngineCapacity} до {filter.SecondEngineCapacity}\n\r";
            }
            else if (filter.FirstEngineCapacity != 0)
            {
                Params += $"Объем двигателя: от {filter.FirstEngineCapacity}\n\r";
            }
            else if (filter.SecondEngineCapacity != 0)
            {
                Params += $"Объем двигателя: до {filter.SecondEngineCapacity}\n\r";
            }
            else
            {
                Params += $"Объем двигателя: не указан\n\r";
            }

            if (filter.Mileage != 0)
            {
                Params += $"Пробег: до {filter.Mileage}\n\r";
            }
            else
            {
                Params += $"Пробег: не указан\n\r";
            }

            return Params;
        }
        public InlineKeyboardButton[] GetSaveButtons()
        {
            InlineKeyboardButton[] inlineKeyboards = new InlineKeyboardButton[2];

            inlineKeyboards[0] = InlineKeyboardButton.WithCallbackData("Назад", "Back");
            inlineKeyboards[1] = InlineKeyboardButton.WithCallbackData("Сохранить", "Save");

            return inlineKeyboards;
        }
        #endregion

        #endregion

        #region Тарифы
        public string GetRatesText(User user)
        {
            string Params = $"Здесь ты можешь увидеть <b>текущий тариф</b>. Если тебе нужно больше функций, то тариф можно поменять, нажав на кнопку <b>\"Изменить тариф\"</b>.\n\r\n\r" +
                            $"Ваш тариф: <b>{user.Rate.Name}</b>\n\r" +
                            $"Количество фильтров: <b>{user.Rate.CountFilters}</b>\n\r" +
                            $"Баланс отчетов: <b>{user.CountReports}</b>\n\r";

            if (user.Rate.Id != 1)
            {
                Params += $"Период до: <b>{user.DateExpired}</b>\n\r";
            }

            return Params;
        }
        public InlineKeyboardButton[][] GetRatesButtons(User user)
        {
            InlineKeyboardButton[][] inlineKeyboards = new InlineKeyboardButton[2][];

            if (user.Rate.Id != 1 && !user.Rate.Demo && user.Rate.CanBuy && user.DateExpired < DateTime.Now.AddDays(7))
            {
                inlineKeyboards[0] = new InlineKeyboardButton[]
                {
                       InlineKeyboardButton.WithCallbackData("Изменить тариф", "ChangeRate"),
                       InlineKeyboardButton.WithCallbackData("Продлить тариф", "ExtendRate")
                };
                inlineKeyboards[1] = new InlineKeyboardButton[]
                {
                       InlineKeyboardButton.WithCallbackData("Главное меню", "Main")
                };
            }
            else
            {
                inlineKeyboards[0] = new InlineKeyboardButton[]
                {
                       InlineKeyboardButton.WithCallbackData("Изменить тариф", "ChangeRate")
                };
                inlineKeyboards[1] = new InlineKeyboardButton[]
                {
                       InlineKeyboardButton.WithCallbackData("Главное меню", "Main")
                };
            }

            return inlineKeyboards;
        }
        public string GetAllRatesText()
        {
            string Params = $"Выбери тариф на который хочешь перейти\n\r";

            return Params;
        }
        public InlineKeyboardButton[][] GetAllRatesButtons(User user, List<Rate> rates)
        {
            rates.RemoveAll(p => p.Id == user.Rate.Id || !p.CanBuy || p.Demo);

            double Pages = rates.Count / 12.0;

            int countKeyBoard = (rates.Count - user.PositionView * 12) / 2;

            InlineKeyboardButton[][] inlineKeyboards;

            int row = 0;

            inlineKeyboards = new InlineKeyboardButton[countKeyBoard > 6 ? 7 : (rates.Count - user.PositionView * 12) % 2 == 0 ? countKeyBoard + 1 : countKeyBoard + 2][];

            for (int i = 0; user.PositionView * 12 + i < rates.Count && i < 12; i = i + 2)
            {
                if (user.PositionView * 12 + i == rates.Count - 1)
                {
                    inlineKeyboards[row] = new InlineKeyboardButton[]
                    {
                       InlineKeyboardButton.WithCallbackData(rates[user.PositionView * 12 + i].Name, rates[user.PositionView * 12 + i].Id.ToString())
                    };
                }
                else
                {
                    inlineKeyboards[row] = new InlineKeyboardButton[]
                    {
                       InlineKeyboardButton.WithCallbackData(rates[user.PositionView * 12 + i].Name, rates[user.PositionView * 12 + i].Id.ToString()),
                       InlineKeyboardButton.WithCallbackData(rates[user.PositionView * 12 + i + 1].Name, rates[user.PositionView * 12 + i + 1].Id.ToString()),
                    };
                }

                row++;
            }

            if (Pages > user.PositionView + 1)
            {
                inlineKeyboards[row] = new InlineKeyboardButton[]
                {
                       InlineKeyboardButton.WithCallbackData("Назад", "Back"),
                       InlineKeyboardButton.WithCallbackData("Вперед", "Next")
                };
            }
            else
            {
                inlineKeyboards[row] = new InlineKeyboardButton[]
                {
                       InlineKeyboardButton.WithCallbackData("Назад", "Back")
                };
            }

            return inlineKeyboards;
        }
        public string GetRateText(Rate rate)
        {
            string Params = "<b>Информация о тарифе</b>\n\r\n\r" +
                            $"Имя тарифа: <b>{rate.Name}</b>\n\r" +
                            $"Цена: <b>{rate.Price}{Constants.Currency}</b>\n\r" +
                            $"Количество фильтров: <b>{rate.CountFilters}</b>\n\r" +
                            $"Баланс отчетов: <b>{rate.CountReports}</b>\n\r\n\r" +
                            "<b>После оплаты, в течении нескольких минут тариф будет присвоен автоматически</b>";

            return Params;
        }
        public string GetExtendRateText(Rate rate)
        {
            string Params = "<b>Информация о тарифе</b>\n\r\n\r" +
                            $"Имя тарифа: <b>{rate.Name}</b>\n\r" +
                            $"Цена: <b>{rate.Price}{Constants.Currency}</b>\n\r" +
                            $"Количество фильтров: <b>{rate.CountFilters}</b>\n\r" +
                            $"Баланс отчетов: <b>{rate.CountReports}</b>\n\r\n\r" +
                            "<b>После оплаты, в течении нескольких минут тариф будет продлен автоматически</b>";

            return Params;
        }
        public InlineKeyboardButton[][] GetRateButtons(string Link)
        {
            InlineKeyboardButton[][] inlineKeyboards = new InlineKeyboardButton[2][];

            inlineKeyboards[0] = new InlineKeyboardButton[]
            {
                       InlineKeyboardButton.WithUrl("Оплатить", Link)
            };
            inlineKeyboards[1] = new InlineKeyboardButton[]
            {
                       InlineKeyboardButton.WithCallbackData("Назад", "Back")
            };

            return inlineKeyboards;
        }
        #endregion

        #region Проверка авто
        public InlineKeyboardButton[][] GetCheckingCarButtons()
        {
            InlineKeyboardButton[][] inlineKeyboards = new InlineKeyboardButton[2][];

            inlineKeyboards[0] = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("Проверить авто", "CheckCar"),
                InlineKeyboardButton.WithCallbackData("Мои отчеты", "MyReports")
            };
            inlineKeyboards[1] = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("Главное меню", "Main")
            };

            return inlineKeyboards;
        }
        public string GetCheckingCarText()
        {
            string Text = $"Здесь ты можешь <b>проверить авто</b> или просмотреть ранее полученные <b>отчёты</b>.\n\r";

            return Text;
        }

        #region Получение отчета
        public InlineKeyboardButton[][] GetCheckCarButtons()
        {
            InlineKeyboardButton[][] inlineKeyboards = new InlineKeyboardButton[1][];

            inlineKeyboards[0] = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("Назад", "Back")
            };

            return inlineKeyboards;
        }
        public string GetCheckCarText()
        {
            string Text = $"Напишите VIN или Гос. номер автомобиля для получения отчета\n\r";

            return Text;
        }
        public string GetLoadReportText()
        {
            string Text = $"Собираю информацию, ожидайте\n\r";

            return Text;
        }
        public InlineKeyboardButton[][] GetFreeReportButtons(long Id, User user)
        {
            InlineKeyboardButton[][] inlineKeyboards = new InlineKeyboardButton[2][];

            if (user.CountReports <= 0)
            {
                inlineKeyboards[0] = new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithCallbackData($"Купить полный отчет({Constants.CheckCarPrice}{Constants.Currency})", $"Pay_{Id}")
                };
            }
            else
            {
                inlineKeyboards[0] = new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithCallbackData($"Получить полный отчет", $"Pay_{Id}")
                };
            }
            inlineKeyboards[1] = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("Назад", "Back")
            };

            return inlineKeyboards;
        }
        #endregion

        #region Покупка отчета
        public string GetPayReportText(Report report)
        {
            string Text = $"<b>Для оплаты нажмите кнопку \"Оплатить\"</b>\n\r" +
                          $"\n\r" +
                          $"Отчет № <b>{report.Id}</b>\n\r" +
                          $"VIN или Госномер - <b>{report.Name}</b>\n\r" +
                          $"Сумма - <b>{Constants.CheckCarPrice}{Constants.Currency}</b>\n\r" +
                          $"\n\r" +
                          $"<b>Платеж будет проведен в течении нескольких минут, полный отчет можно будет посмотреть в \"Мои отчеты\"</b>";

            return Text;
        }
        public InlineKeyboardButton[][] GetPayReportButtons(string Link)
        {
            InlineKeyboardButton[][] inlineKeyboards = new InlineKeyboardButton[2][];

            inlineKeyboards[0] = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithUrl("Оплатить", Link)
            };
            inlineKeyboards[1] = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("Назад", "Back")
            };

            return inlineKeyboards;
        }
        public string GetPayIncludeReportText(Report report)
        {
            string Text = $"<b>Для получения нажмите кнопку \"Получить\"</b>\n\r" +
                          $"\n\r" +
                          $"Отчет № <b>{report.Id}</b>\n\r" +
                          $"VIN или Госномер - <b>{report.Name}</b>\n\r" +
                          $"Сумма - <b>{Constants.CheckCarPrice}{Constants.Currency}</b>\n\r" +
                          $"\n\r" +
                          $"<b>После нажатия вы будете автоматически перемещены назад, для просмотра полного отчета</b>";

            return Text;
        }
        public InlineKeyboardButton[][] GetPayIncludeReportButtons(string Id)
        {
            InlineKeyboardButton[][] inlineKeyboards = new InlineKeyboardButton[2][];

            inlineKeyboards[0] = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("Получить", $"Get_{Id}")
            };
            inlineKeyboards[1] = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("Назад", "Back")
            };

            return inlineKeyboards;
        }
        public InlineKeyboardButton[][] GetFullReportButtons(string Link)
        {
            InlineKeyboardButton[][] inlineKeyboards = new InlineKeyboardButton[2][];

            inlineKeyboards[0] = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithUrl("Посмотреть подробнее", Link)
            };
            inlineKeyboards[1] = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("Назад", "Back")
            };

            return inlineKeyboards;
        }
        #endregion

        #region Просмотр отчетов
        public InlineKeyboardButton[][] GetMyReportsButtons(User user)
        {
            List<Report> reports = user.Reports.ToList();

            InlineKeyboardButton[][] inlineKeyboards = new InlineKeyboardButton[2][];

            int row = 0;

            if (reports[user.PositionView].Pay)
            {
                inlineKeyboards[row] = new InlineKeyboardButton[]
                {
                       InlineKeyboardButton.WithCallbackData("Показать отчет", $"Show_{reports[user.PositionView].Id}")
                };
            }
            else
            {
                if (user.CountReports <= 0)
                {
                    inlineKeyboards[row] = new InlineKeyboardButton[]
                    {
                       InlineKeyboardButton.WithCallbackData($"Купить отчет({Constants.CheckCarPrice}{Constants.Currency})", $"Pay_{reports[user.PositionView].Id}")
                    };
                }
                else
                {
                    inlineKeyboards[row] = new InlineKeyboardButton[]
                    {
                       InlineKeyboardButton.WithCallbackData($"Получить отчет", $"Pay_{reports[user.PositionView].Id}")
                    };
                }
            }

            row++;

            if (reports.Count > user.PositionView + 1)
            {
                inlineKeyboards[row] = new InlineKeyboardButton[]
                {
                       InlineKeyboardButton.WithCallbackData("Назад", "Back"),
                       InlineKeyboardButton.WithCallbackData("Главное меню", "Main"),
                       InlineKeyboardButton.WithCallbackData("Вперед", "Next")
                };
            }
            else
            {
                inlineKeyboards[row] = new InlineKeyboardButton[]
                {
                       InlineKeyboardButton.WithCallbackData("Назад", "Back"),
                       InlineKeyboardButton.WithCallbackData("Главное меню", "Main")
                };
            }

            return inlineKeyboards;
        }
        public string GetMyReportsText(User user)
        {
            Report report = user.Reports.ToList()[user.PositionView];

            string Text = $"Отчет № <b>{report.Id}</b>\n\r" +
                          $"VIN или Госномер - <b>{report.Name}</b>\n\r";

            if (report.Pay)
            {
                Text += "Статус: <b>Оплачен</b>\n\r" +
                        "\n\r" +
                        "<b>Если хотите просмотреть отчет, нажмите на кнопку \"Показать отчет\"</b>";
            }
            else
            {
                if (user.CountReports <= 0)
                {
                    Text += "Статус: <b>Ожидается оплата</b>\n\r" +
                            "\n\r" +
                            "<b>Для оплаты отчета, нажмите на кнопку \"Купить отчет\"</b>\n\r";
                }
                else
                {
                    Text += "Статус: <b>Ожидается оплата</b>\n\r" +
                            "\n\r" +
                            "<b>Для оплаты отчета, нажмите на кнопку \"Получить отчет\"</b>\n\r";
                }
            }

            return Text;
        }
        public InlineKeyboardButton[][] GetMyReportButtons(string Link)
        {
            InlineKeyboardButton[][] inlineKeyboards = new InlineKeyboardButton[2][];

            inlineKeyboards[0] = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithUrl("Посмотреть подробнее", Link)
            };
            inlineKeyboards[1] = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("Назад", "Back")
            };

            return inlineKeyboards;
        }
        #endregion
        #endregion

        #region Помощь
        public InlineKeyboardButton[][] GetHelpButtons()
        {
            InlineKeyboardButton[][] inlineKeyboards = new InlineKeyboardButton[2][];

            inlineKeyboards[0] = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithUrl("Написать нам", $@"https://t.me/porulyu_podderzhka"),
                InlineKeyboardButton.WithCallbackData("Актуальные вопросы", "TopQuestions")
            };
            inlineKeyboards[1] = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("Главное меню", "Main")
            };

            return inlineKeyboards;
        }
        public string GetHelpText(User user)
        {
            string Text = $"Здесь ты можешь посмотреть <b>часто задаваемые вопросы</b> и ответы на них.\n\r" +
                           "\n\r" +
                           "Если у тебя остались вопросы или <b>твоя проблема требует индивидуального подхода</b>, ты можешь написать в чат поддержки.\n\r" +
                           "\n\r" +
                           $"Твой ID: {user.ChatId}\n\r";

            return Text;
        }
        public InlineKeyboardButton[][] GetTopQuestionsButtons()
        {
            InlineKeyboardButton[][] inlineKeyboards = new InlineKeyboardButton[1][];

            inlineKeyboards[0] = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("Назад", "Back")
            };

            return inlineKeyboards;
        }
        public string GetTopQuestionsText(string UserName)
        {
            string Text = $"<b>Как мы работаем?</b>\n\r" +
                           $"Каждую минуту наш бот просматривает все сайты с объявлениями о продаже авто и отправляет самые свежие автомобили, если они подходят под ваш фильтр.\n\r" +
                           "\n\r" +
                           $"<b>Как настроить фильтр?</b>\n\r" +
                           $"Для настройки фильтра перейдите в раздел \"Фильтры\" и нажмите на кнопку \"Создать фильтр\".\n\r" +
                           "\n\r" +
                           $"<b>Могу ли я добавить несколько фильтров?</b>\n\r" +
                           $"Да. В зависимости от вашего тарифа вы можете добавлять несколько фильтров.\n\r" +
                           "\n\r" +
                           $"<b>Где я могу посмотреть объявления по моему фильтру?</b>\n\r" +
                           $"Для этого перейдите во второго бота -> <a href=\"https://t.me/{UserName}\">поРулю Рассылка ✉️</a>\n\r";

            return Text;
        }
        #endregion

        #region Общие сообщения
        public InlineKeyboardButton[][] GetErrorButtons()
        {
            InlineKeyboardButton[][] inlineKeyboards = new InlineKeyboardButton[1][];

            inlineKeyboards[0] = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("OK", "OK")
            };

            return inlineKeyboards;
        }
        #endregion
    }
}
