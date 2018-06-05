using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Mail;
using ApiServer.Model;

namespace ApiServer.Core.Email
{
    public class EmailHelper : IEmailHelper
    {
        private readonly VdsContext context;
        private readonly SmtpClient smtpServer;
        private readonly string source;
        public EmailHelper(VdsContext vdsContext)
        {
            context = vdsContext;
            source = "veda.futurisx@gmail.com";
            smtpServer = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new System.Net.NetworkCredential("veda.futurisx", "veda@FUTURISX#2017"),
                EnableSsl = true
            };
        }
        public async Task Send(string destination, string subject, string body)
        {
            var mail = new MailMessage
            {
                From = new MailAddress(source)
            };
            mail.To.Clear();
            mail.To.Add(destination);
            mail.Subject = subject;
            mail.IsBodyHtml = true;
            mail.Body = body;
            try
            {
                var userState = "ok";
                smtpServer.SendAsync(mail, userState);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task Broadcast(string subject, string body)
        {
            var mail = new MailMessage
            {
                From = new MailAddress(source)
            };
            mail.To.Clear();
            mail.Subject = subject;
            mail.IsBodyHtml = true;
            mail.Body = body;

            var users = context.Users;
            foreach(var user in users)
            {
                mail.To.Add(user.Email);
            }

            try
            {
                var userState = "ok";
                smtpServer.SendAsync(mail, userState);
            }
            catch(Exception ex)
            {
                throw ex;
            }
            
        }
    }
}
