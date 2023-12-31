using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Movies.Api.Auth;
using Movies.Api.Controllers;
using Movies.Api.Mapping;
using Movies.Application;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace Movies.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Debug)
            .Enrich.FromLogContext()
            .WriteTo.Console() // Output to console
            .WriteTo.File(new CompactJsonFormatter()
                , "webapi-log2.json"
                , rollingInterval: RollingInterval.Day) // Output to a log file
            .CreateLogger();
        
        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(Log.Logger);
        
        var config = builder.Configuration;
        
        //add the jwtbearerdefault authenucation
        builder.Services.AddAuthentication(a =>
        {
            a.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            a.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            a.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

        })
        .AddJwtBearer(
            jwt =>
            {
                jwt.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
                    ValidateIssuerSigningKey =true,
                    ValidateLifetime = true, 
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = config["Jwt:Issuer"],
                    ValidAudience = config["Jwt:Audience"],
                };

            }


        );

        Log.Logger.Information(">> Configured JWT");
        
        //add authorization
        builder.Services.AddAuthorization(
            x =>
            {
                x.AddPolicy("admin", p =>
                {
                    p.RequireClaim("admin", "true");
                });
                
                x.AddPolicy("trusted_member", p =>
                {
                    p.RequireAssertion(c =>
                    {
                        return c.User.HasClaim(m => m is {Type: "admin", Value:"true"}) ||
                               c.User.HasClaim(m => m is {Type: "trusted_member", Value:"true"});
                    });
                });
            });

        // Add services to the container.

        Log.Logger.Information(">> Configured Authorization");
        
        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();


        /*** Add All Dependencies ***/
        builder.Services.AddApplication();//this is our custom extension that has all the props`
        builder.Services.AddSingleton<IUserIdentityProvider, UserIdentityProvider>();
        /*** End All Dependencies ***/


        builder.Services.AddDatabase(config["Database:ConnectionString"]!);//this is our custom extension that has all the props`
        
        Log.Logger.Information(">> Configured DB string :"+config["Database:ConnectionString"]);

        //provide generic location from where the user identity will come from
       
        
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseMiddleware<ValidationMappingMiddleware>();
        
        app.MapControllers();

        //add service dbinistialiser
        var dbInitializer = app.Services.GetRequiredService<DbInitializer>();
        await dbInitializer.InitializeAsync();

        Log.Logger.Information(">> Configuration completed");

        app.Run();
    }
}