using System.Threading.Tasks;

namespace Function.Services
{
    public interface IEmailService
    {
        public Task<bool> SendMailAsync(string from, string to, string subject, string body, string sendGridKey);
        public Task<bool> SendMailsAsync(string from, string[] to, string subject, string body, string sendGridKey);
    }
}