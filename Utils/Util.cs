using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Function.Services;
using RazorLight;

namespace Function.Utils
{
    public static class Util
    {
        public static DateTime GetCurrentIST() => DateTime.UtcNow.AddMinutes(330); // UTC +5:30

        public async static Task<string> GetEmailBodyAsync(string templateFilePath, dynamic model)
        {
            // Setup RazorLight Engine
            var engine = new RazorLightEngineBuilder()
                        .SetOperatingAssembly(Assembly.GetExecutingAssembly())
                        .UseEmbeddedResourcesProject(typeof(EmailService))
                        .UseMemoryCachingProvider()
                        .Build();
            
            using var templateStream = new StreamReader(templateFilePath);
            var templateContent = await templateStream.ReadToEndAsync();
            var result = await engine.CompileRenderStringAsync("my_template", templateContent, model);

            return result;
        }
    }
}