using System.ComponentModel.DataAnnotations;
using API;
using API.Nswag;
using Infa;
using LinqToDB;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

var options = new DataOptions().UseSQLite(builder.Configuration["DB"] ?? "Data Source=dev.db");
builder.Services.AddSingleton(new DataOptions<LibraryDatabase>(options));
builder.Services.AddScoped<LibraryDatabase>();
builder.Services.AddControllers();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<MyCustomExceptionHandler>();

builder.Services.AddOpenApiDocument(settings =>
    settings.SchemaSettings.SchemaProcessors.Add(new RequireNotNullableSchemaProcessor()));


builder.Services.AddCors();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var libraryDb = scope.ServiceProvider.GetRequiredService<LibraryDatabase>();
    LibrarySeed.EnsureSeeded(libraryDb);
}

app.UseExceptionHandler();
app.UseCors(_ => _.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().SetIsOriginAllowed(_ => true));
app.UseOpenApi();
app.UseSwaggerUi();
app.MapControllers();
app.Run();

namespace API
{
    public class MyCustomExceptionHandler : IExceptionHandler
    {
        public ValueTask<bool> TryHandleAsync(HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            httpContext.Response.StatusCode = exception switch
            {
                ValidationException => StatusCodes.Status400BadRequest,
                KeyNotFoundException => StatusCodes.Status404NotFound,
                InvalidOperationException => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
            };
            httpContext.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Title = exception.Message
            });

            return default;
        }
    }
}