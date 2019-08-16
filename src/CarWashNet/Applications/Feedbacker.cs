using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Net;
using System.Net.Mail;
using System.Reactive;
using System.Threading.Tasks;

namespace CarWashNet.Applications
{
    public class Feedbacker : ReactiveObject
    {
        [Reactive] public int Rating { get; set; } = 5;
        [Reactive] public string From { get; set; }
        [Reactive] public string Comment { get; set; }

        private string publicKey = "b3c2b8d22ec2ea2c076833d56f013d41";
        private string secretKey = "091ae1cd9d9a70fc90010c0df01b2c5d";
        private string from { get; set; } = "carwashnet@yandex.ru";
        private string to = "carwashnet@yandex.ru";
        private string clientID;

        public ReactiveCommand<Unit, Unit> SendFeedback { get; set; }

        public Feedbacker(string client)
        {
            clientID = client;
            SendFeedback = ReactiveCommand.CreateFromTask(async () =>
            {
                await sendMailAsync(from);
            });
            SendFeedback.ThrownExceptions.Subscribe(ex => {
                var x = ex;
            });
        }



        private async Task sendMailAsync(string from)
        {
            MailMessage msg = new MailMessage();
            msg.From = new MailAddress(from);
            msg.To.Add(new MailAddress(to));

            msg.Subject = $"[{clientID}] Обратная связь";
            msg.Body =
                $"Дата: {DateTime.Now}" + Environment.NewLine +
                $"Код клиента: {clientID}" + Environment.NewLine +
                $"От: {From}" + Environment.NewLine +
                $"Оценка: {Rating}" + Environment.NewLine + Environment.NewLine + 
                Comment;

            SmtpClient client = new SmtpClient("in.mailjet.com", 25);
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            //client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(publicKey, secretKey);

            await client.SendMailAsync(msg);
        }
    }
}
