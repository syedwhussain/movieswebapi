using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Movies.Api.Mapping;
using Movies.Application;

namespace Movies.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
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

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();



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

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseMiddleware<ValidationMappingMiddleware>();
        
        app.MapControllers();

        //add service dbinistialiser
        var dbInitializer = app.Services.GetRequiredService<DbInitializer>();
        await dbInitializer.InitializeAsync();

        
        app.Run();
    }
}