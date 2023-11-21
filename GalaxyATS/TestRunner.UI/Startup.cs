using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TestRunner.UI.Services;
using TestRunner.UI.Services.Interfaces;
using TestRunner.Utilities;
//using ScenarioBuilder;

namespace TestRunner.UI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options => options.EnableEndpointRouting = false);
            services.AddHostedService<QueuedHostedService>();
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            services.AddSingleton<IAnalyzeTests, TestAnalyzerService>();
            services.AddTransient<IManageUserSettings, UserSettingManager>();
            services.AddTransient<IAZClient, AZRestClient>();

            switch (RunnerConfiguration.StorageType)
            {
                case "file":
                    services.AddScoped<IReadResults, FileSystemResultsReader>();
                    break;
                case "azure":
                    services.AddScoped<IReadResults, AZStorageRESTResultsReader>();
                    services.AddScoped<ILogResults, AZStorageRESTResultsLogger>();
                    break;
            }

            //services.AddScoped<IConsumerBuilderService, ConsumerBuilderService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseMiddleware<HostFilteringMiddleware>();

            app.UseStaticFiles();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            if (RunnerConfiguration.ApplicationType == "local")
            {
                OpenUrl($"http://localhost:{RunnerConfiguration.DashboardPort}/runner");
            }
        }

        public void OpenUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
