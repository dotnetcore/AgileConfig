﻿using System;
using System.Text;
using AgileConfig.Server.Apisite.UIExtension;
using AgileConfig.Server.Apisite.Websocket;
using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Freesql;
using AgileConfig.Server.IService;
using AgileConfig.Server.Service;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0).AddRazorRuntimeCompilation();
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
            app.UseMiddleware<ReactUIMiddleware>();

            app.UseCors(op=> {
                op.AllowAnyOrigin();
                op.AllowAnyMethod();
                op.AllowAnyHeader();
            });
            app.UseWebSockets(new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(60),
                ReceiveBufferSize = 2 * 1024
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
    }
}
