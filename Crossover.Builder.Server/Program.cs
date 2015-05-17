using System;
using System.Configuration;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.Text;
using System.Threading.Tasks;
using Crossover.Builder.Common.Interfaces;
using Crossover.Builder.Service;
using Newtonsoft.Json;

namespace Crossover.Builder.Server
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            int port;
            if (!int.TryParse(ConfigurationManager.AppSettings["port"], out port)) port = 9000;
            var cert = ConfigurationManager.AppSettings["cert"] ?? "cert.pfx";

            Run(cert, port);

            Console.WriteLine("Press <Enter> to close application.");
            Console.ReadLine();
        }

        public static void Run(string cert, int port)
        {
            var baseAddress = new UriBuilder {Port = port}.Uri;

            Console.WriteLine("Starting host at {0}", baseAddress);

            var host = new ServiceHost(typeof (CommandService), baseAddress);
            host.Credentials.ServiceCertificate.Certificate = new X509Certificate2(cert, string.Empty);
            host.Credentials.UserNameAuthentication.UserNamePasswordValidationMode =
                UserNamePasswordValidationMode.Custom;
            host.Credentials.UserNameAuthentication.CustomUserNamePasswordValidator = new UserNamePassValidator();

            AddBehaviors(host);
            AddEndpoints(host);

            try
            {
                host.Open();
                Console.WriteLine("Host has been started successfully at {0}.", baseAddress);
                Console.WriteLine("Press <Enter> to stop host...");
                Console.ReadLine();
                host.Close();
                Console.WriteLine("Host has been stopped successfully.");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                host.Abort();
                Console.WriteLine("Host has been aborted because of exception.");
            }
        }

        private static void AddBehaviors(ServiceHost host)
        {
            host.Description.Behaviors.Add(new ServiceMetadataBehavior {HttpGetEnabled = true});
        }

        private static void AddEndpoints(ServiceHost host)
        {
            host.AddServiceEndpoint(typeof (ICommandService),
                new WSHttpBinding(SecurityMode.Message)
                {
                    Security =
                    {
                        Message =
                        {
                            ClientCredentialType = MessageCredentialType.UserName
                        }
                    }
                },
                string.Empty);
            host.AddServiceEndpoint(typeof (IMetadataExchange), MetadataExchangeBindings.CreateMexHttpBinding(), "mex");
        }
    }

    internal class UserNamePassValidator: UserNamePasswordValidator
    {
        public override void Validate(string userName, string password)
        {
            if (userName == null || password == null)
            {
                throw new ArgumentNullException();
            }

            var client = new HttpClient { BaseAddress = new Uri("http://localhost:2038") };
            var response = client.PostAsync("token",
                new StringContent(
                    string.Format("grant_type=password&username={0}&password={1}", userName, password),
                    Encoding.UTF8,
                    "application/x-www-form-urlencoded")).Result;

            if (response.IsSuccessStatusCode)
            {
                var message = response.Content.ReadAsStringAsync().Result;
                var tokenInfo = JsonConvert.DeserializeObject<TokenInfo>(message);
                Console.WriteLine("Username: {0}, Token: {1}", tokenInfo.Username, tokenInfo.AccessToken);
                return;
            }

            throw new SecurityTokenException("Incorrect Username or Password");
        }
    }
    class TokenInfo
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("userName")]
        public string Username { get; set; }

        [JsonProperty(".issued")]
        public string IssuedAt { get; set; }

        [JsonProperty(".expires")]
        public string ExpiresAt { get; set; }
    }
}