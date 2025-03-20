using Labb3CVApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics;

namespace Labb3CVApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

         
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

           
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowLocalhostAndFrontend",
                    policy =>
                    {
                        var frontendUrl = builder.Configuration["FrontendUrl"] ?? "https://labb3cvfrontend.azurewebsites.net";

                        policy.WithOrigins("http://localhost:5000", frontendUrl)
                              .AllowAnyMethod()
                              .AllowAnyHeader()
                              .AllowCredentials();
                    });
            });

          
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContext<CvDbContext>(options =>
                options.UseSqlServer(connectionString));

            var app = builder.Build();

            app.UseCors("AllowLocalhostAndFrontend");

            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                    var logger = app.Services.GetRequiredService<ILogger<Program>>();

                    if (exceptionHandlerPathFeature?.Error is not null)
                    {
                        logger.LogError(exceptionHandlerPathFeature.Error, "🔥 Ett fel uppstod!");

                        context.Response.StatusCode = 500;
                        context.Response.ContentType = "application/json";

                        var errorResponse = new
                        {
                            Message = "Ett oväntat fel inträffade.",
                            Details = exceptionHandlerPathFeature.Error.Message
                        };

                        await context.Response.WriteAsJsonAsync(errorResponse);
                    }
                });
            });

            if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
            app.MapGet("/health", () => Results.Ok("API is running")).WithTags("Health Check");

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
