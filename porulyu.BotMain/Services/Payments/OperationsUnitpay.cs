using Newtonsoft.Json;
using porulyu.BotMain.Common;
using porulyu.Domain.Models;
using porulyu.Infrastructure.Cryptography;
using porulyu.Infrastructure.Services;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace porulyu.BotMain.Services.Payments
{
    class OperationsUnitpay
    {
        public async Task<Payment> GetPayment(string Action, long ActionId, User user)
        {
            try
            {
                double Price = 0;
                string Desc = "";

                switch (Action)
                {
                    case "report":
                        Price = Constants.CheckCarPrice;
                        Desc = $"Покупка отчета № {ActionId}";
                        break;
                    case "rateSet":
                        Rate rateSet = await new OperationsRate().Get(ActionId.ToString());
                        Price = rateSet.Price;
                        Desc = $"Изменение тарифа {rateSet.Name}";
                        break;
                    case "rateExtend":
                        Rate rateExtend = await new OperationsRate().Get(ActionId.ToString());
                        Price = rateExtend.Price;
                        Desc = $"Продление тарифа {rateExtend.Name}";
                        break;
                }

                var client = new RestClient($"https://unitpay.money/api?method=initPayment&params[paymentType]=card&params[account]={user.Id}_{Action}_{ActionId}&params[sum]={Price}&params[projectId]={Constants.UnitpayProjectId}&params[secretKey]={new Encryption().DecryptRSA(Constants.UnitpaySecretKey)}&params[desc]={Desc}&params[currency]={Constants.UnitpayCurrency}");
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                IRestResponse response = client.Execute(request);

                dynamic content = JsonConvert.DeserializeObject(response.Content);

                return new Payment
                {
                    DateCreate = DateTime.Now,
                    PaymentId = content.result.paymentId,
                    PaymentURL = content.result.redirectUrl,
                    ReceiptURL = content.result.receiptUrl,
                    Action = Action,
                    ActionId = ActionId,
                    UserId = user.Id
                };
            }
            catch
            {
                throw new Exception("Ошибка создания счета для оплаты");
            }
        }
        public int CheckPay(Payment payment)
        {
            var client = new RestClient($"https://unitpay.money/api?method=getPayment&params[paymentId]={payment.PaymentId}&params[secretKey]={new Encryption().DecryptRSA(Constants.UnitpaySecretKey)}");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);

            dynamic content = JsonConvert.DeserializeObject(response.Content);

            switch (content.result.status.ToString())
            {
                case "success":
                    return 1;
                case "wait":
                    return 0;
                case "secure":
                    return -1;
                default:
                    return -2;
            }
        }
        public void Load()
        {
            try
            {
                XDocument doc = XDocument.Load(Constants.PathUnitpay);

                Constants.UnitpayProjectId = doc.Element("Settings").Element("ProjectId").Value;
                Constants.UnitpaySecretKey = doc.Element("Settings").Element("SecretKey").Value;
                Constants.UnitpayCurrency = doc.Element("Settings").Element("Currency").Element("Value").Value;
                Constants.Currency = doc.Element("Settings").Element("Currency").Element("Text").Value;
            }
            catch (Exception Ex)
            {
                throw new Exception(Ex.Message, Ex);
            }
        }
    }
}
