using System.IO;
using System;
using System.Threading.Tasks;
using Function.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Function.Utils;

namespace Function
{
    public static class NotifySale
    {
        /// <summary>
        /// Runs 7:00 pm UTC everyday i.e, 12:30 am IST
        /// </summary>
        [FunctionName("NotifySale")]
        public static async Task Run([TimerTrigger("0 0 19 * * *")] TimerInfo myTimer, ILogger log, ExecutionContext context)
        {
            log.LogInformation(context.FunctionAppDirectory);
            log.LogInformation($"C# Timer trigger function executed at: {Util.GetCurrentIST()}");

            var SENDGRID_APIKEY = Environment.GetEnvironmentVariable("SENDGRID_APIKEY");
            var RETRY_ATTEMPTS = int.Parse(Environment.GetEnvironmentVariable("RETRY_ATTEMPTS"));
            var FROM = Environment.GetEnvironmentVariable("EMAIL_SENDER");
            var TO = Environment.GetEnvironmentVariable("EMAIL_RECEIVERS"); // Can be many
            var PRICE_THRESHOLD = Environment.GetEnvironmentVariable("PRICE_THRESHOLD");
            var GOS_URL = Environment.GetEnvironmentVariable("GOS_PS5");
            var SM_URL = Environment.GetEnvironmentVariable("SM_PS5");
            var templateFilePath = Path.Combine(context.FunctionAppDirectory, "EmailTemplate.cshtml");

            log.LogInformation($"SendGridKey:{SENDGRID_APIKEY}\nFrom:{FROM}\nTo:{TO}\nPriceThreshold:{PRICE_THRESHOLD}\nTemplatePath:{templateFilePath}");


            var emailService = new EmailService(log);
            var scraper = new ScrapeWeb(log);
            

            try
            {
                var emailBody = await Util.GetEmailBodyAsync (
                    templateFilePath, 
                    new {CheckedDateTime = Util.GetCurrentIST(), SM_PS5 = SM_URL, GOS_PS5 = GOS_URL}
                );
                
                if (await scraper.HasPriceDroppedAsync())
                {
                    log.LogInformation("Hurray, there's a price drop");

                    var success = await emailService.SendMailsAsync(
                        from: FROM,
                        tos: TO.Split("<>"),
                        subject: "Game Price Frop 😎",
                        body: emailBody,
                        sendGridKey: SENDGRID_APIKEY
                    );

                    if (success)
                        log.LogInformation("Email successfully sent to all recipients.");
                }
                else
                {
                    log.LogInformation("There isn't any price drop. Will check again tomorrow..");
                }
                
            }
            catch(Exception e)
            {
                log.LogError("Something went wrong 😢");
                log.LogError(e.Source + " Exception");
                log.LogError("Message: " + e.Message);
                log.LogError(e.StackTrace);
            }
            
        }

        
    }
}
