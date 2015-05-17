using System;
using System.Configuration;
using System.IdentityModel.Selectors;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using Crossover.Builder.Common.Interfaces;
using Crossover.Builder.Service;

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

    internal class UserNamePassValidator :
        System.IdentityModel.Selectors.UserNamePasswordValidator
    {
        public override void Validate(string userName, string password)
        {
            return;
            if (userName == null || password == null)
            {
                throw new ArgumentNullException();
            }

            if (!(userName == "fayaz" && password == "soomro"))
            {
                throw new FaultException("Incorrect Username or Password");
            }
        }
    }
}