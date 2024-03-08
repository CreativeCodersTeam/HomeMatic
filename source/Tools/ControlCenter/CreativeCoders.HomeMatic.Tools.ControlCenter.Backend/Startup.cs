using System.Text;
using CreativeCoders.AspNetCore.TokenAuth.Jwt;
using CreativeCoders.AspNetCore.TokenAuthApi;
using CreativeCoders.AspNetCore.TokenAuthApi.Abstractions;
using CreativeCoders.AspNetCore.TokenAuthApi.Jwt;
using CreativeCoders.Core.Text;
using CreativeCoders.HomeMatic.Tools.ControlCenter.Backend.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace CreativeCoders.HomeMatic.Tools.ControlCenter.Backend;

public class Startup(IConfiguration configuration)
{
    public IConfiguration Configuration { get; } = configuration;

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers()
            .AddTokenAuthApiController();

        var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(RandomString.Create()));

        services.AddScoped<IUserProvider, HomeMaticApiUserProvider>();

        services.AddJwtTokenAuthApi(
            x => { x.SecurityKey = securityKey; },
            x => { x.UseRefreshTokens = true; });

        services.AddJwtTokenAuthentication(x => { x.SecurityKey = securityKey; });

        services.AddAuthorizationBuilder()
            .AddPolicy("RestrictedApi", policy => policy.RequireAuthenticatedUser()
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme));

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
