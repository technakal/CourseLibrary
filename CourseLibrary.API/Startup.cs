using AutoMapper;
using CourseLibrary.API.DbContexts;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CourseLibrary.API
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
      services.AddControllers(setupAction => setupAction.ReturnHttpNotAcceptable = true)
        // adds Newtonsoft polyfill to allow patch documents
        .AddNewtonsoftJson(setupAction =>
        {
          setupAction.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        })
        // adds serializers for xml input and output
        .AddXmlDataContractSerializerFormatters()
        // applies custom error responses based on validation and input errors
        .ConfigureApiBehaviorOptions(setupAction =>
        {
          setupAction.InvalidModelStateResponseFactory = context =>
          {
            var problemDetailsFactory = context.HttpContext.RequestServices
              .GetRequiredService<ProblemDetailsFactory>();
            var problemDetails = problemDetailsFactory.CreateValidationProblemDetails(
                context.HttpContext,
                context.ModelState
              );
            problemDetails.Detail = "See the errors field for details.";
            problemDetails.Instance = context.HttpContext.Request.Path;

            var actionExecutingContext = context as ActionExecutingContext;

            if ((context.ModelState.ErrorCount > 0) &&
              (actionExecutingContext?.ActionArguments.Count == context.ActionDescriptor.Parameters.Count))
            {
              problemDetails.Type = "localhost:51044/swagger";
              problemDetails.Status = StatusCodes.Status422UnprocessableEntity;
              problemDetails.Title = "One or more validation errors occurred. You suck.";

              return new UnprocessableEntityObjectResult(problemDetails)
              {
                ContentTypes = { "application/problem+json" }
              };
            }

            problemDetails.Status = StatusCodes.Status400BadRequest;
            problemDetails.Title = "One or more errors on input occurred.";
            return new BadRequestObjectResult(problemDetails)
            {
              ContentTypes = { "application/problem+json" }
            };
          };
        });

      services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

      services.AddSwaggerGen(c =>
      {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
          Title = "CourseLibraryAPI",
          Version = "0.1.0",
          Description = "API for managing a course library.",
          Contact = new OpenApiContact
          {
            Name = "Noel Keener",
            Email = "noel@nakeener.com",
            Url = new Uri("https://github.com/technakal")
          }
        });

        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        c.IncludeXmlComments(xmlPath);
        c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
      });

      services.AddScoped<ICourseLibraryRepository, CourseLibraryRepository>();

      services.AddDbContext<CourseLibraryContext>(options =>
      {
        options.UseSqlServer(
          @"Server=(localdb)\mssqllocaldb;Database=CourseLibraryDB;Trusted_Connection=True;");
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
        app.UseExceptionHandler(appBuilder =>
        {
          appBuilder.Run(async context =>
          {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("An unexpected fault happened. Try again later.");
          });
        });
      }

      app.UseSwagger();
      app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CourseLibrary v1"));
      app.UseRouting();

      app.UseAuthorization();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers();
      });
    }
  }
}
