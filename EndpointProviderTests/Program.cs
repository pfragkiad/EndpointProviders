
using EndpointProviders;

namespace EndpointProviderTests;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddAuthorization();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        //-------------------
        
        //add repository
        builder.Services.AddScoped<WeatherForecastRepository>();
        //add endpoint provider factory
        builder.Services.AddEndpointProviderFactory();
        
        //-------------------

        var app = builder.Build();
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        app.UseHttpsRedirection();
        app.UseAuthorization();

        //----------------
        
        //add the endpoints here
        app.AddEndpointsFromEndpointProviders(typeof(Program));
        
        //----------------

        app.Run();
    }
}