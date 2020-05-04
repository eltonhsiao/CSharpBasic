using CSharpBasic.Attributes;
using CSharpBasic.Models;
using CSharpBasic.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;
using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using CSharpBasic.Controllers;
using CSharpBasic.Extensions;
using CSharpBasic.Middlewares;
using Microsoft.AspNetCore.Http;

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
            services.AddControllersWithViews()
                .AddApplicationPart(typeof(WhereAmIController).GetTypeInfo().Assembly);

            // Register IHttpClientFactory
            services.AddHttpClient<IGeoIpService, GeoIpService>()
                .AddTransientHttpErrorPolicy(GetRetryPolicy);
            services.AddTransient<IGeoIpService, GeoIpService>();
            services.AddScoped<ApiTrackingAttribute>();

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
            });
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
            app.UseAccessRestrict();
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

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });
        }

        private IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(PolicyBuilder<HttpResponseMessage> arg)
        {
            return HttpPolicyExtensions.HandleTransientHttpError()
                .OrResult(msg =>
                    msg.StatusCode == HttpStatusCode.RequestTimeout
                    || msg.StatusCode == HttpStatusCode.GatewayTimeout)
                .WaitAndRetryAsync(3, retryCount => TimeSpan.FromMilliseconds(retryCount * 100));
        }
    }
}