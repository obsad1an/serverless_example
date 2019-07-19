using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BroadcasterService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddApiVersioning(o =>
                {
                    o.ReportApiVersions = true;
                    o.AssumeDefaultVersionWhenUnspecified = true;
                    o.DefaultApiVersion = new ApiVersion(1, 0);
                });

            var connectionString = Configuration["SignalR.ConnectionString"];
            services.AddSignalR().AddAzureSignalR(connectionString);
            services.AddHealthChecks();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseStaticFiles();
            app.UseAuthentication();
            
            app.UseAzureSignalR(routes =>
            {
                routes.MapHub<Hubs.ChatHub>("/chat");
            });

            app.UseMvc();

            Task.Run(() => { DoRunner(); });
        }        

        public static async Task DoRunner()
        {
            var broadcast = new Hubs.ChatHub();

            Thread.Sleep(10000);

            while (true)
            {
                var count = 1;
                await broadcast.BroadcastMessage("User 1", count.ToString());
                count++;

                Thread.Sleep(2000);

                await broadcast.BroadcastMessage("User 2", count.ToString());                
            }
        }
    }
}