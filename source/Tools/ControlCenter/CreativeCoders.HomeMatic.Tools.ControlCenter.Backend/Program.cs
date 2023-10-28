using CreativeCoders.Core.IO;
using CreativeCoders.HomeMatic.Tools.ControlCenter.Backend.Repositories.Ccus;
using CreativeCoders.HomeMatic.Tools.ControlCenter.Backend.Repositories.LiteDbRepository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//builder.Services.AddSingleton<ICcuRepository, InMemoryCcuRepository>();
builder.Services.AddScoped<ICcuRepository, ObjectCcuRepository>();
builder.Services.AddCors(x => x.AddDefaultPolicy(x => x.AllowAnyOrigin()));

builder.Services.AddLiteDbObjectRepository(FileSys.Path.Combine(FileSys.Path.GetTempPath(), "hmcc-backend.db"))
    .AddCollection<CcuModel, string>();

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

app.UseAuthorization();

app.MapControllers();

app.Run();
