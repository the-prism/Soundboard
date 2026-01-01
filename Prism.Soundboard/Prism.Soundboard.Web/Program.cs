using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prism.Soundboard.Web.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Prism.Soundboard.Web
{
    public class Program
    {
        private static AudioService audioService;

        public static void Main(string[] args)
        {
            audioService = new AudioService();
            CreateHostBuilder(args).Build().Run();
        }

        public static AudioService AudioService { get { return audioService; } }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
