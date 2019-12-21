using System;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace MonitoringProject
{
    public class EmailService
    {
        public async Task Send(object email)
        {
            using (var client = new SmtpClient())
            {
                client.Connect("smtp.gmail.com", 587);

                // use the OAuth2.0 access token obtained above
                //var oauth2 = new SaslMechanismOAuth2("mymail@gmail.com", credential.Token.AccessToken);
                //client.Authenticate(oauth2);

                //await client.SendAsync("Your site is not available!");
                client.Disconnect(true);
                Console.WriteLine("EMAIL. You site is not available.");
            }
        }
    }
}
