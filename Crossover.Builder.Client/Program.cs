using System;
using System.Configuration;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using Crossover.Builder.Common.Interfaces;

namespace Crossover.Builder.Client
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            int port;
            if (!int.TryParse(ConfigurationManager.AppSettings["port"], out port)) port = 9000;
            var host = ConfigurationManager.AppSettings["host"] ?? "localhost";

            Run(host, port);

            Console.WriteLine("Press <Enter> to close application.");
            Console.ReadLine();
        }

        public static void Run(string host, int port)
        {
            var baseAddress = new UriBuilder {Host = host, Port = port}.Uri;
            var endPointAddress = new EndpointAddress(baseAddress);
            var channelFactory = new ChannelFactory<ICommandService>(
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
                endPointAddress);
            if (channelFactory.Credentials != null)
            {
                channelFactory.Credentials.ServiceCertificate.Authentication.CertificateValidationMode =
                    X509CertificateValidationMode.None;
                channelFactory.Credentials.UserName.UserName = "Ivan";
                channelFactory.Credentials.UserName.Password = "password";
            }

            ICommandService channel = null;

            Console.WriteLine("Connecting to {0}", baseAddress);
            try
            {
                channel = channelFactory.CreateChannel();
                var instruction = channel.GetInstruction();
                channel.SetResult(instruction, "OK");
                Console.WriteLine("Received instruction {0}.", instruction);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                Console.WriteLine("Host has been aborted because of exception.");
            }
            finally
            {
                // ReSharper disable once SuspiciousTypeConversion.Global
                var clientChannel = channel as IClientChannel;
                if (clientChannel != null && clientChannel.State != CommunicationState.Faulted)
                    clientChannel.Close();
            }
        }
    }
}