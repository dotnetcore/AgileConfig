using System;
using System.IO;
using System.Net;
using AgileConfig.Server.Apisite.UIExtension;
using AgileConfig.Server.Apisite.Websocket;
using AgileConfig.Server.Common;
using AgileConfig.Server.Common.RestClient;
using AgileConfig.Server.Data.Freesql;
using AgileConfig.Server.OIDC;
using AgileConfig.Server.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using AgileConfig.Server.Data.Repository.Freesql;
using AgileConfig.Server.Data.Repository.Mongodb;
using AgileConfig.Server.Data.Repository.Selector;

namespace AgileConfig.Server.Apisite
{
    public class Startup
    {
        public Startup(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            Configuration = configuration;
            Global.LoggerFactory = loggerFactory;

            SetTrustSSL(configuration);
        }

        private static bool IsTrustSSL(IConfiguration configuration)
        {
            var alwaysTrustSsl = configuration["alwaysTrustSsl"];
            if (!string.IsNullOrEmpty(alwaysTrustSsl) && alwaysTrustSsl.ToLower() == "true")
            {
                return true;
            }

            return false;
        }

        private static void SetTrustSSL(IConfiguration configuration)
        {
            if (IsTrustSSL(configuration))
            {
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            }
        }

        public IConfiguration Configuration
        {
            get;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDefaultHttpClient(IsTrustSSL(Configuration));
            services.AddRestClient();

            services.AddMemoryCache();

            services.AddCors();
            services.AddMvc().AddRazorRuntimeCompilation();

            if (Appsettings.IsPreviewMode)
            {
                AddSwaggerService(services);
            }

            services.AddEnvAccessor();
            services.AddFreeSqlFactory();
            // Add freesqlRepositories or other repositories
            services.AddRepositories();

            services.AddBusinessServices();

            services.ConfigureOptions<ConfigureJwtBearerOptions>();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer();

            services.AddHostedService<InitService>();
            services.AddAntiforgery(o => o.SuppressXFrameOptionsHeader = true);

            services.AddOIDC();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            var basePath = Configuration["pathBase"];
            if (!string.IsNullOrWhiteSpace(basePath))
            {
                app.UsePathBase(basePath);
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseMiddleware<ExceptionHandlerMiddleware>();
            }
            if (Appsettings.IsPreviewMode)
            {
                AddSwaggerMiddleWare(app);
            }

            app.UseMiddleware<ReactUIMiddleware>();

            app.UseCors(op =>
            {
                op.AllowAnyOrigin();
                op.AllowAnyMethod();
                op.AllowAnyHeader();
            });
            app.UseWebSockets(new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(60),
            });
            app.UseMiddleware<WebsocketHandlerMiddleware>();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }

        private void AddSwaggerService(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
                var basePath = Path.GetDirectoryName(typeof(Program).Assembly.Location);
                var xmlPath = Path.Combine(basePath, "AgileConfig.Server.Apisite.xml");
                c.IncludeXmlComments(xmlPath);
            });
        }

        private void AddSwaggerMiddleWare(IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("v1/swagger.json", "My API V1");
            });
        }

    }
}
