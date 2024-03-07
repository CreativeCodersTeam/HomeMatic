using CreativeCoders.AspNetCore.Jwt;
using CreativeCoders.AspNetCore.TokenAuth;
using CreativeCoders.Core.Text;
using CreativeCoders.HomeMatic.Tools.ControlCenter.Backend.Repositories;

namespace CreativeCoders.HomeMatic.Tools.ControlCenter.Backend;

public class Startup(IConfiguration configuration)
{
    public IConfiguration Configuration { get; } = configuration;

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers()
            .AddApplicationPart(typeof(TokenAuthController).Assembly);

        services.AddJwtSupport<HomeMaticTokenAuthHandler>(RandomString.Create());

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddCors(x => x.AddDefaultPolicy(x => x.AllowAnyOrigin()));

        services.AddHmccRepositories();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.Use(async (context, next) =>
        {
            context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
            context.Response.Headers.Append("Access-Control-Allow-Methods", "*");
            context.Response.Headers.Append("Access-Control-Allow-Headers",
                "Origin, X-Requested-With, Content-Type, Accept");
            await next();
        });


        app.UseCors();

// Configure the HTTP request pipeline.
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();


        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
}
