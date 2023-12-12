using Movies.Api.Mapping;
using Movies.Application;

namespace Movies.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        /*
        
        instead of adding it like this..
        
        builder.Services.AddSingleton<IMovieRespository, MovieRepository>();
        
        we want to move these dependencey delcarations to the business layer / or application layer liek this:
        
        builder.Services.AddApplication();
        
        */

        var config = builder.Configuration;

        builder.Services.AddApplication();//this is our custom extension that has all the props`
        builder.Services.AddDatabase(config["Database:ConnectionString"]!);//this is our custom extension that has all the props`
        
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.UseMiddleware<ValidationMappingMiddleware>();
        
        app.MapControllers();

        //add service dbinistialiser
        var dbInitializer = app.Services.GetRequiredService<DbInitializer>();
        await dbInitializer.InitializeAsync();

        
        app.Run();
    }
}