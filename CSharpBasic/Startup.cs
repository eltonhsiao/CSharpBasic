using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mnemosyne.Logging.Interfaces;

namespace CSharpBasic
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
            RegisterLogger(services);
            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private static void RegisterLogger(IServiceCollection services)
        {
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<ITraceIdProvider, TraceIdProvider>();
            services.AddSingleton<ILogNameProvider, LogNameProvider>();
            services.AddSingleton<ILoggerFactory, LoggerFactory>();
        }
    }

    public class LogNameProvider : ILogNameProvider
    {
        private readonly IConfiguration _config;

        public LogNameProvider(IConfiguration config)
        {
            _config = config;
        }

        public string GetLogName(string categoryName = "")
        {
            return "Elton";
        }
    }

    public class TraceIdProvider : ITraceIdProvider
    {
        private readonly IHttpContextAccessor _httpCtxAccessor;

        public TraceIdProvider(IHttpContextAccessor httpCtxAccessor)
        {
            _httpCtxAccessor = httpCtxAccessor;
        }

        public string TraceId =>
            _httpCtxAccessor.HttpContext?.TraceIdentifier
            ?? $"P{Process.GetCurrentProcess().Id:X8}T{Thread.CurrentThread.ManagedThreadId:X8}";
    }
}
