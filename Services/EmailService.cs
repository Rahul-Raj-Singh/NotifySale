using System;
using System.Threading.Tasks;
using FluentEmail.Core;
using FluentEmail.Core.Models;
using FluentEmail.SendGrid;
using Microsoft.Extensions.Logging;


namespace Function.Services
{
    public class EmailService : IEmailService
    {
        private ILogger _log;
        public EmailService(ILogger log) => _log = log;
        
        public async Task<bool> SendMailAsync(string from, string to, string subject, string body, string sendGridKey)
        {
            try
            {
                
                var email = 
                    new Email()
                    {Sender = new SendGridSender(apiKey: sendGridKey)}
                    .SetFrom(from)
                    .To(to)
                    .Subject(subject)
                    .Body(body, isHtml: true);
 
                var res = await email.SendAsync();

                if(res.Successful) return true;

                LogErrors(res, null); return false;
                
            }
            catch(Exception e)
            {
                LogErrors(null, e);
                return false;
            }

        }

        public async Task<bool> SendMailsAsync(string from, string[] tos, string subject, string body, string sendGridKey)
        {
            int sentTo = 0;
            foreach(var to in tos)
            {
                bool sent = await SendMailAsync(from, to, subject, body, sendGridKey);
                if (sent)
                {
                    _log.LogInformation($"Email sent to {to}");
                    sentTo ++;
                }
                    
            }
            // Return true only when mail is sent to every recipient
            return sentTo == tos.Length;
           
        }

        private void LogErrors(SendResponse res = null, Exception e = null)
        {
            _log.LogError("Error Occured Sending Mail");

            if(e != null)
            {
                _log.LogError(e.Source + " Exception");
                _log.LogError("Message: " + e.Message);
                _log.LogError(e.StackTrace);
            }

            if(res != null)
            {
                _log.LogError("Message Id:" + res.MessageId);
                _log.LogError(string.Join("\n", res.ErrorMessages));
            }
        }
    }
}