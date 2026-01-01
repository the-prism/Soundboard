using Microsoft.AspNetCore.Hosting;
using Prism.Soundboard.Web.Components;

namespace Prism.Soundboard.Web
{
    public class Program
    {
        private static IHost _host;

        public static void Main(string[] args)
        {
            _host = CreateHostBuilder(args);

            _host.Run();
        }

        public static IHost CreateHostBuilder(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
            app.UseHttpsRedirection();

            app.UseAntiforgery();

            app.MapStaticAssets("Prism.Soundboard.Web.staticwebassets.endpoints.json");
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            return app;
        }

        //StartAsync is called by the WPF app
        public static Task StartAsync(CancellationToken token)
        {
            _host = CreateHostBuilder(Array.Empty<string>());
            return _host.StartAsync(token);
        }

        public static async Task StopAsync(CancellationToken token)
        {
            using (_host)
            {
                await _host.StopAsync(token);
            }
        }
    }
}
