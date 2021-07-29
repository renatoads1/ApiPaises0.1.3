// Creation Date: 01/06/2021
// Author: Mariana Silva
// Author: Renato Lacerda
// Developed by: VMI Security System
// Copyright © 2021 VMI Security System
// All rights are reserved. Reproduction in whole or part is prohibited without the written consent of the copyright owner.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using EventBus.Extensions;
using EventBus.Mqtt;
using Extensions.Swagger;
using Extensions.Logger;
using Extensions.SystemConfiguration;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Swashbuckle.AspNetCore.SwaggerUI;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using Calibration.Api.Models;
using Calibration.Api.Services;
using Calibration.Api.EventProcessors;
using Spectrum.DataAccess.Extensions;

namespace Calibration.Api
{
	public class Startup
	{
		private IWebHostEnvironment env;
		public Startup(IConfiguration configuration, IWebHostEnvironment env)
		{
			Configuration = configuration;
			this.env = env;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddHttpClient();
			services.AddControllers();
			var systemSettings = services.GetConfiguration(env, Configuration);
			services.AddMongo(systemSettings.mongoConfiguration);
			services.RegisterDaosServices();
			var loggerFactory = services.AddLoggingCapabilities(systemSettings.logging);
			var logger = loggerFactory.CreateLogger<Startup>();
			logger.LogInformation("Iniciando Calibration.API");

			services.ConfigureMqttClient(systemSettings, logger, typeof(Startup).Namespace);
			//builder dependency injection declaration for XRayEventProcessor
			services.AddSingleton<XRayEventProcessor>();
			services.AddSingleton<DetectorEventProcessor>();
			services.AddSingleton<ReloadSettingsEventProcessor>();

			services.AddSingleton<EventPublisher>();
			services.AddSingleton<DetectionOrchestrator>();
			services.AddSingleton<XRayOrchestrator>();
			services.AddSingleton<ICalibrationOrchestrator, CalibrationOrchestrator>();

			services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
			services.AddApiVersioning(options =>
			{
				options.DefaultApiVersion = new ApiVersion(1, 0);
				options.AssumeDefaultVersionWhenUnspecified = true;
				options.ReportApiVersions = true;
			});

			services.AddVersionedApiExplorer(options =>
			{
				// add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
				// note: the specified format code will format the version as "'v'major[.minor][-status]"
				options.GroupNameFormat = "'v'VVV";

				// note: this option is only necessary when versioning by url segment. the SubstitutionFormat
				// can also be used to control the format of the API version in route templates
				options.SubstituteApiVersionInUrl = true;
			});

			services.AddSwaggerGen(options =>
			{
				options.OperationFilter<SwaggerDefaultValues>();
			});

		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
		{
			this.env = env;
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseSwagger();

			// Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
			// specifying the Swagger JSON endpoint.
			app.UseSwaggerUI(c =>
			{
				// build a swagger endpoint for each discovered API version
				foreach (var description in provider.ApiVersionDescriptions)
				{
					c.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
				}

				c.DocExpansion(DocExpansion.List);
				c.RoutePrefix = string.Empty;
			});

			InitializeEventProcessors(app);

			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}

		/// <summary>
		/// Intializes the event processors.
		/// <param name="app">the application builder</param>
		/// <summary>
		private void InitializeEventProcessors(IApplicationBuilder app)
		{
			app.ApplicationServices.GetService<ReloadSettingsEventProcessor>();
			app.ApplicationServices.GetService<DetectorEventProcessor>();
			app.ApplicationServices.GetService<XRayEventProcessor>();
		}
	}
}
