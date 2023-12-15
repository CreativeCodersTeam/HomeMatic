using CreativeCoders.AspNetCore.Jwt;
using CreativeCoders.AspNetCore.TokenAuth;
using CreativeCoders.Core;
using CreativeCoders.Core.Text;
using CreativeCoders.HomeMatic.Tools.ControlCenter.Backend;
using CreativeCoders.HomeMatic.Tools.ControlCenter.Backend.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddApplicationPart(typeof(TokenAuthController).Assembly);

builder.Services.AddJwtSupport<HomeMaticTokenAuthHandler>(RandomString.Create());
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(x => x.AddDefaultPolicy(x => x.AllowAnyOrigin()));

builder.Services.AddHmccRepositories();

var app = builder.Build();

app.Use(async (context, next) =>
{
    context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
    context.Response.Headers.Add("Access-Control-Allow-Methods", "*");
    context.Response.Headers.Add("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept");
    await next();
});

app.UseCors();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();

app.Run();