using Newtonsoft.Json;
using PhoneApp.Domain.Attributes;
using PhoneApp.Domain.DTO;
using PhoneApp.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace EmployeesDownloaderPlugin
{
    [Author(Name = "Ilia Antuhov")]
    public class Plugin : IPluggable
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public IEnumerable<DataTransferObject> Run(IEnumerable<DataTransferObject> args)
        {
            logger.Info("Loading employees");

            var newEmployees = LoadEmployeesFromUrl("https://dummyjson.com/users");
            var result = new List<DataTransferObject>(args);
            result.AddRange(newEmployees);
            
            logger.Info($"Loading employees count : {result.Count()}");
            return result;
        }

        private IEnumerable<DataTransferObject> LoadEmployeesFromUrl(string url)
        {
            #region Затычка для SSL
            //что бы ошибка во время работы плагина не вылетала, как вариант можно сделать самоподписный SSL сертификат
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
            {
                logger.Error($"SSL Policy Errors: {sslPolicyErrors}");
                return true;
            };
            #endregion

            using (var client = new HttpClient())
            {
                var response = client.GetStringAsync(url).Result;
                var users = JsonConvert.DeserializeObject<UserResponse>(response);
                var employees = new List<EmployeesDTO>();

                foreach (var user in users.Users)
                {
                    var employee = new EmployeesDTO
                    {
                        Name = $"{user.FirstName} {user.LastName}",
                        // Phone = user.Phone, поле только для чтения в модели
                    };
                    employees.Add(employee);
                }

                return employees.Cast<DataTransferObject>();
            }
        }
    }
}
