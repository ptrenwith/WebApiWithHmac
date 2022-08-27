using Company.Product.AuthenticationService;
using Company.Product.Models.Responses;
using Company.Product.Services.Interfaces;
using Company.Product.Services.Services;
using Company.Product.WebApi.Attributes;
using Company.Product.WebApi.Authentication;
using Company.Product.WebApi.SwaggerConfig;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Company.Product.WebApi
{
    public class Startup
    {
        private readonly AppSettings _appSettings;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            var _appsettingsConfigurationSection = Configuration.GetSection(nameof(AppSettings));
            _appSettings = _appsettingsConfigurationSection.Get<AppSettings>();
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            if (services != null)
            {
                services.AddHealthChecks();
                services.AddAuthentication(HMACAuthentication.AuthenticationScheme)
                    .AddScheme<HmacAuthenticationOptions, HmacAuthenticationHandler>(HMACAuthentication.AuthenticationScheme, options => { });
                services
                    .AddControllers(options =>
                    {
                        options.Filters.Add(typeof(ApiControllerExceptionFilterAttribute));
                        options.Filters.Add(new ProducesAttribute("application/json"));
                    })
                    .AddJsonOptions(options =>
                    {
                        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    })
                    .SetCompatibilityVersion(CompatibilityVersion.Latest)
                    .ConfigureApiBehaviorOptions(options =>
                    {
                        options.InvalidModelStateResponseFactory = context =>
                        {
                            var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
                            var logger = loggerFactory.CreateLogger(context.ActionDescriptor.DisplayName);
                            var errors = string.Join(", ", context.ModelState.Values.SelectMany(x => x.Errors).Select(x => $"[{ x.ErrorMessage }]"));
                            logger.LogDebug($"One or more validation errors occurred: {errors}");

                            var result = new HttpErrorResponse
                            {
                                Status = (int)HttpStatusCode.BadRequest,
                                Message = errors,
                                TraceId = context.HttpContext.TraceIdentifier
                            };
                            return new BadRequestObjectResult(result);
                        };
                    });

                services.AddApiVersioning(config =>
                {
                    config.DefaultApiVersion = new ApiVersion(1, 0);
                    config.AssumeDefaultVersionWhenUnspecified = true;
                    config.ReportApiVersions = true;
                });

                services.AddVersionedApiExplorer(options =>
                {
                    options.GroupNameFormat = "'v'VVV";
                    options.SubstituteApiVersionInUrl = true;
                });

                if (_appSettings.Api.Swagger.Enabled)
                {
                    services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
                    services.AddSwaggerGen(options =>
                    {
                        options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
                        options.OperationFilter<SwaggerDefaultValues>();

                        Assembly currentAssembly = Assembly.GetExecutingAssembly();
                        AssemblyName[] referencedAssemblies = currentAssembly.GetReferencedAssemblies();
                        IEnumerable<AssemblyName> allAssemblies = null;

                        if (referencedAssemblies != null && referencedAssemblies.Any())
                        {
                            allAssemblies = referencedAssemblies.Union(new AssemblyName[] { currentAssembly.GetName() });
                        }
                        else
                        {
                            allAssemblies = new AssemblyName[] { currentAssembly.GetName() };
                        }
                        IEnumerable<string> xmlDocs = allAssemblies
                        .Select(a => Path.Combine(Path.GetDirectoryName(currentAssembly.Location), $"{a.Name}.xml"))
                        .Where(f => File.Exists(f));

                        foreach (var item in xmlDocs)
                        {
                            options.IncludeXmlComments(item);
                        }
                    });
                }

                services.AddSingleton<AppSettings>(_appSettings);

                services.AddMemoryCache();
                services.AddScoped<Microsoft.Extensions.Internal.ISystemClock, Microsoft.Extensions.Internal.SystemClock>();

                services.AddHttpClient();

                services.AddScoped<ICapacityService, CapacityService>();
            }
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
        {
            if (app != null)
            {
                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }

                app.UseHttpsRedirection();

                app.UseRouting();
                app.Use(async (context, next) =>
                {
                    context.Request.EnableBuffering();
                    await next();
                });
                app.UseAuthentication();
                app.UseAuthorization();

                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                    endpoints.MapSwagger();
                    endpoints.MapHealthChecks("/health");
                });

                if (_appSettings.Api.Swagger.Enabled)
                {
                    app.UseSwagger(options =>
                    {
                        options.RouteTemplate = "/swagger/{documentName}/swagger.json";
                    });
                    app.UseSwaggerUI(options =>
                    {
                        foreach (var description in provider.ApiVersionDescriptions)
                        {
                            options.SwaggerEndpoint($"./swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                            options.RoutePrefix = string.Empty;
                        }
                    });
                }
            }
        }
    }
}
