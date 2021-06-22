using Microsoft.EntityFrameworkCore;
using porulyu.Domain.Models;
using porulyu.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace porulyu.Infrastructure.Services
{
    public class OperationsFilter
    {
        public async Task CreateFilter(Domain.Models.User user)
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                Filter filter = new Filter { DateCreate = DateTime.Now, UserId = user.Id };

                context.Add(filter);
                user.Filters.Add(filter);
                context.Update(user);

                await context.SaveChangesAsync();
            }
        }
        public async Task SaveRegionAliasFilter(string Data, Domain.Models.User user)
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                user.Filters.Last().RegionAlias = Data;

                context.Update(user.Filters.Last());
                await context.SaveChangesAsync();
            }
        }
        public async Task SaveCityAliasFilter(string Data, Domain.Models.User user)
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                user.Filters.Last().CityAlias = Data;

                context.Update(user.Filters.Last());
                await context.SaveChangesAsync();
            }
        }
        public async Task SaveMarkAliasFilter(string Data, Domain.Models.User user)
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                user.Filters.Last().MarkAlias = Data;

                context.Update(user.Filters.Last());
                await context.SaveChangesAsync();
            }
        }
        public async Task SaveModelAliasFilter(string Data, Domain.Models.User user)
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                user.Filters.Last().ModelAlias = Data;

                context.Update(user.Filters.Last());
                await context.SaveChangesAsync();
            }
        }
        public async Task SaveYearsFilter(string Text, Domain.Models.User user)
        {
            try
            {
                using (ApplicationContext context = new ApplicationContext())
                {
                    string[] years = Text.Replace(" ", "").Split('-');

                    if (String.IsNullOrEmpty(years[0]) && !String.IsNullOrEmpty(years[1]))
                    {
                        user.Filters.Last().SecondYear = Convert.ToInt32(years[1]);
                    }
                    else if (!String.IsNullOrEmpty(years[0]) && String.IsNullOrEmpty(years[1]))
                    {
                        user.Filters.Last().FirstYear = Convert.ToInt32(years[0]);
                    }
                    else
                    {
                        user.Filters.Last().FirstYear = Convert.ToInt32(years[0]);
                        user.Filters.Last().SecondYear = Convert.ToInt32(years[1]);
                    }

                    context.Update(user.Filters.Last());
                    await context.SaveChangesAsync();
                }
            }
            catch
            {
                throw new Exception($"Неверная запись периода выпуска автомобиля\n\r");
            }
        }
        public async Task SavePricesFilter(string Text, Domain.Models.User user)
        {
            try
            {
                using (ApplicationContext context = new ApplicationContext())
                {
                    string[] prices = Text.Replace(" ", "").Split('-');

                    if (String.IsNullOrEmpty(prices[0]) && !String.IsNullOrEmpty(prices[1]))
                    {
                        user.Filters.Last().SecondPrice = Convert.ToInt32(prices[1]);
                    }
                    else if (!String.IsNullOrEmpty(prices[0]) && String.IsNullOrEmpty(prices[1]))
                    {
                        user.Filters.Last().FirstPrice = Convert.ToInt32(prices[0]);
                    }
                    else
                    {
                        user.Filters.Last().FirstPrice = Convert.ToInt32(prices[0]);
                        user.Filters.Last().SecondPrice = Convert.ToInt32(prices[1]);
                    }

                    context.Update(user.Filters.Last());
                    await context.SaveChangesAsync();
                }
            }
            catch
            {
                throw new Exception($"Неверная запись цен автомобиля\n\r");
            }
        }
        public async Task SaveCustomsСlearedFilter(string Data, Domain.Models.User user)
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                switch (Data)
                {
                    case "No":
                        user.Filters.Last().CustomsСleared = -1;
                        break;
                    case "Clear":
                        user.Filters.Last().CustomsСleared = 0;
                        break;
                    case "Yes":
                        user.Filters.Last().CustomsСleared = 1;
                        break;
                }

                context.Update(user.Filters.Last());
                await context.SaveChangesAsync();
            }
        }
        public async Task SaveTransmissionFilter(string Data, Domain.Models.User user)
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                switch (Data)
                {
                    case "Clear":
                        user.Filters.Last().Transmission = 0;
                        break;
                    case "Automate":
                        user.Filters.Last().Transmission = 1;
                        break;
                    case "Mechanics":
                        user.Filters.Last().Transmission = 2;
                        break;
                }

                context.Update(user.Filters.Last());
                await context.SaveChangesAsync();
            }
        }
        public async Task SaveActuatorFilter(string Data, Domain.Models.User user)
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                switch (Data)
                {
                    case "Clear":
                        user.Filters.Last().Actuator = 0;
                        break;
                    case "Front":
                        user.Filters.Last().Actuator = 1;
                        break;
                    case "Full":
                        user.Filters.Last().Actuator = 2;
                        break;
                    case "Rear":
                        user.Filters.Last().Actuator = 3;
                        break;
                }

                context.Update(user.Filters.Last());
                await context.SaveChangesAsync();
            }
        }
        public async Task SaveEngineCapacityFilter(string Text, Domain.Models.User user)
        {
            try
            {
                using (ApplicationContext context = new ApplicationContext())
                {
                    string[] capacities = Text.Replace(" ", "").Replace(".", ",").Split('-');

                    if (String.IsNullOrEmpty(capacities[0]) && !String.IsNullOrEmpty(capacities[1]))
                    {
                        user.Filters.Last().SecondEngineCapacity = Convert.ToDouble(capacities[1]);
                    }
                    else if (!String.IsNullOrEmpty(capacities[0]) && String.IsNullOrEmpty(capacities[1]))
                    {
                        user.Filters.Last().FirstEngineCapacity = Convert.ToDouble(capacities[0]);
                    }
                    else
                    {
                        user.Filters.Last().FirstEngineCapacity = Convert.ToDouble(capacities[0]);
                        user.Filters.Last().SecondEngineCapacity = Convert.ToDouble(capacities[1]);
                    }

                    context.Update(user.Filters.Last());
                    await context.SaveChangesAsync();
                }
            }
            catch
            {
                throw new Exception($"Неверная запись объема двигателя автомобиля\n\r");
            }
        }
        public async Task SaveMileageFilter(string Text, Domain.Models.User user)
        {

            try
            {
                using (ApplicationContext context = new ApplicationContext())
                {
                    string[] ranges = Text.Replace(" ", "").Split('-');

                    if (String.IsNullOrEmpty(ranges[0]) && !String.IsNullOrEmpty(ranges[1]))
                    {
                        user.Filters.Last().SecondMileage = Convert.ToInt32(ranges[1]);
                    }
                    else if (!String.IsNullOrEmpty(ranges[0]) && String.IsNullOrEmpty(ranges[1]))
                    {
                        user.Filters.Last().FirstMileage = Convert.ToInt32(ranges[0]);
                    }
                    else
                    {
                        user.Filters.Last().FirstMileage = Convert.ToInt32(ranges[0]);
                        user.Filters.Last().SecondMileage = Convert.ToInt32(ranges[1]);
                    }

                    context.Update(user.Filters.Last());
                    await context.SaveChangesAsync();
                }
            }
            catch
            {
                throw new Exception($"Неверная запись пробега автомобиля\n\r");
            }
        }
        public async Task SaveNameFilter(string Text, Domain.Models.User user)
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                user.Filters.Last().Name = Text;

                context.Update(user.Filters.Last());
                await context.SaveChangesAsync();
            }
        }
        public async Task SaveWorkFilter(Domain.Models.User user)
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                user.Filters.Last().Work = true;

                context.Update(user.Filters.Last());
                await context.SaveChangesAsync();
            }
        }
        public async Task SaveLastIdAdFilter(Filter filter, string Id)
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                context.Update(filter);
                await context.SaveChangesAsync();
            }
        }
        public async Task<bool> CheckFilter(Filter filter)
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                if (await context.Filters.FirstOrDefaultAsync(p => p.Id == filter.Id) != null)
                {
                    return true;
                }

                return false;
            }
        }
        public async Task DeleteFilter(Domain.Models.Filter filter)
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                context.Filters.Remove(filter);
                await context.SaveChangesAsync();
            }
        }
    }
}
