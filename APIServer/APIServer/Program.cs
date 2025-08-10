using APIServer.Config;
using APIServer.Repository;
using APIServer.Repository.Implements;
using APIServer.Service;
using APIServer.Service.Implements;

var builder = WebApplication.CreateBuilder(args);

// Configuration
IConfiguration configuration = builder.Configuration;
builder.Services.Configure<DbConfig>(configuration.GetSection(nameof(DbConfig)));

// Register services
builder.Services.AddScoped<ITestService, TestService>();

// Register Repositories
builder.Services.AddScoped<IAccountDb, AccountDb>();
builder.Services.AddScoped<IGameDb, GameDb>();
builder.Services.AddSingleton<IMasterDb, MasterDb>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Set Dapper
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

// Load Master Data
var masterDb = app.Services.GetRequiredService<IMasterDb>();
if (await masterDb.Load() == false)
{   
    // TODO: 로그 남기고 프로그램 종료
    return;
}

app.Run();
