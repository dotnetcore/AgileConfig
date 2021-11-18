using System;
using System.IO;
using System.Text;
using AgileConfig.Server.Apisite.UIExtension;
using AgileConfig.Server.Apisite.Websocket;
using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Freesql;
using AgileConfig.Server.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace AgileConfig.Server.Apisite
{
    public class Startup
    {
        public Startup(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            Configuration = configuration;
            Global.LoggerFactory = loggerFactory;
        }

        public IConfiguration Configuration
        {
            get;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                      .AddJwtBearer(options =>
                      {
                          options.TokenValidationParameters = new TokenValidationParameters
                          {
                              ValidIssuer = JwtSetting.Instance.Issuer,
                              ValidAudience = JwtSetting.Instance.Audience,
                              IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSetting.Instance.SecurityKey)),
                          };
                      });
            services.AddCors();
            services.AddMvc().AddRazorRuntimeCompilation();

            if (Appsettings.IsPreviewMode)
            {
                AddSwaggerService(services);
            }

            services.AddFreeSqlDbContext();
            services.AddBusinessServices();
            services.AddHostedService<InitService>();
            services.AddAntiforgery(o => o.SuppressXFrameOptionsHeader = true);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
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

            app.UseCors(op=> {
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
