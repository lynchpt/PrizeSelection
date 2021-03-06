﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.PlatformAbstractions;
using PrizeSelection.Logic;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using Swashbuckle.AspNetCore.Swagger;

namespace PrizeSelection.Api
{
    public class Startup
    {
        #region Constants

        private const string LoggingOptionsAppComponentNameKey = "AppComponent";
        #endregion

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
                             {
                                 options.AddPolicy("AllowAll",
                                     builder =>
                                     {
                                         builder
                                             .AllowAnyOrigin()
                                             .AllowAnyMethod()
                                             .AllowAnyHeader()
                                             .AllowCredentials();
                                     });
                             });

            services.AddMvc();

            services.AddSwaggerGen(c =>
                                   {
                                       c.SwaggerDoc("v1", new Info { Title = "Prize Selection Api", Version = "v1" });

                                       var filePath = Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "PrizeSelection.Api.xml");
                                       c.IncludeXmlComments(filePath);

                                   });

            ConfigureLogger(services);
        }

        /// <summary>
        /// This method now gets called automatically during the startup process in ASP.NET 2.0
        /// Documentation has not yet been authored by Microsoft
        /// See Issue #3659 here: https://github.com/aspnet/Docs/issues/3659
        /// </summary>
        /// <param name="services"></param>
        public virtual void ConfigureContainer(IServiceCollection services)
        {
            services.AddOptions();
            ConfigureOptions(services);
            ConfigureDependencyInjection(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("AllowAll");

            app.UseMvc();

            app.UseSwagger();

            app.UseSwaggerUI(c =>
                             {
                                 c.RoutePrefix = "swagger/ui";
                                 c.SwaggerEndpoint("/swagger/v1/swagger.json", "Prize Selection Api v1.0");
                             });
        }

        #region Private Configuration Methods

        protected virtual void ConfigureDependencyInjection(IServiceCollection services)
        {
            services.AddScoped<IResultsFormatter, ResultsFormatter>();
            services.AddScoped<IPrizeSelectionTableHelper, PrizeSelectionTableHelper>();
            services.AddScoped<IPrizeResultsTableHelper, PrizeResultsTableHelper>();
            services.AddScoped<ISelectionEngine, SelectionEngine>();
            services.AddScoped<ISelectionSuccessCalculator, SelectionSuccessCalculator>();

            services.AddSingleton<IMapper>(ConfigureMappings);
        }

        private void ConfigureOptions(IServiceCollection services)
        {

        }
        #endregion

        #region Private Methods

        private void ConfigureLogger(IServiceCollection services)
        {
            string appInsightsKey = Configuration["LoggingOptions:ApplicationInsightsKey"];
            string appComponentName = Configuration["LoggingOptions:AppComponentName"];
            string logFilePath = Configuration["LoggingOptions:LogFilePath"];

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .Enrich.WithProperty(LoggingOptionsAppComponentNameKey, appComponentName)
                //.WriteTo.RollingFile(logFilePath).MinimumLevel.Information()
                .WriteTo.ApplicationInsightsEvents(appInsightsKey).MinimumLevel.Information()
                .WriteTo.Console(theme: SystemConsoleTheme.Literate).MinimumLevel.Information()
                .CreateLogger();

            services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog());
        }

        private IMapper ConfigureMappings(IServiceProvider provider)
        {
            MapperConfiguration mapperConfiguration =
                new MapperConfiguration(
                    mce =>
                    {
                        mce.AddProfile<PrizeSelectionModelMappingProfile>();
                        mce.ConstructServicesUsing(t => ActivatorUtilities.CreateInstance(provider, t));
                    });

            mapperConfiguration.AssertConfigurationIsValid();

            IMapper mapper = mapperConfiguration.CreateMapper();

            return mapper;
        }
        #endregion
    }
}
