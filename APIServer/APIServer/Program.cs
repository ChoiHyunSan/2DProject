using APIServer;
using APIServer.Config;
using APIServer.Middleware;
using APIServer.Repository;
using APIServer.Repository.Implements;
using APIServer.Repository.Implements.Memory;
using APIServer.Service;
using APIServer.Service.Implements;
using Prometheus;
using Prometheus.DotNetRuntime;
using ZLogger;
using ZLogger.Providers;
using ZLogger.Formatters;

var builder = WebApplication.CreateBuilder(args);

// Configuration
var configuration = builder.Configuration;
builder.Services.Configure<DbConfig>(configuration.GetSection(nameof(DbConfig)));

// Register services
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IShopService, ShopService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IStageService, StageService>();
builder.Services.AddScoped<IDataLoadService, DataLoadService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IMailService, MailService>();
builder.Services.AddScoped<IQuestService, QuestService>();

// Register Repositories
builder.Services.AddScoped<IAccountDb, AccountDb>();
builder.Services.AddScoped<IGameDb, GameDb>();
builder.Services.AddSingleton<IMasterDb, MasterDb>();
builder.Services.AddSingleton<IMemoryDb, MemoryDb>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Set Logger
SettingLogger();

// DotNetRuntime 계측 활성화 (GC/JIT/스레드풀/예외 등)
var runtimeCollector = DotNetRuntimeStatsBuilder
    .Default()
    .StartCollecting();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.UseHttpMetrics(options =>
{
    options.ReduceStatusCodeCardinality();
    options.RequestDuration.Enabled = true; 
});

app.UseMiddleware<ResponseStatusCodeMiddleware>();
app.UseMiddleware<SessionCheckMiddleware>();
app.UseMiddleware<RequestLockMiddleware>();

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

// Prometheus 스크랩 엔드포인트 (/metrics)
app.MapMetrics(); 

// Set Dapper
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

// Load Master Data
var masterDb = app.Services.GetRequiredService<IMasterDb>();
if (await masterDb.Load() != ErrorCode.None)
{
    return;
}

app.MapGet("/", () => "OK");

app.Lifetime.ApplicationStopping.Register(() => runtimeCollector.Dispose());
app.Run();

void SettingLogger()
{
    var logging = builder.Logging;
    logging.ClearProviders();
    logging.SetMinimumLevel(LogLevel.Information);

    var fileDir = configuration["LogDir"] ?? Path.Combine(AppContext.BaseDirectory, "Logs");
    Directory.CreateDirectory(fileDir);

    // 공용 JSON 설정 (TimeStamp & KeyValues)
    void ConfigureJson(SystemTextJsonZLoggerFormatter f)
    {
        f.IncludeProperties = IncludeProperties.Timestamp | IncludeProperties.ParameterKeyValues;
    }

    // 콘솔: JSON
    logging.AddZLoggerConsole(options =>
    {
        options.UseJsonFormatter(ConfigureJson);
    });

    // 롤링 파일: 일 단위 + 1MB, 콘솔과 동일 JSON
    logging.AddZLoggerRollingFile(options =>
    {
        options.FilePathSelector = (ts, seq) =>
            Path.Combine(fileDir, $"{ts.ToLocalTime():yyyy-MM-dd}_{seq:000}.log");
        options.RollingInterval = RollingInterval.Day;
        options.RollingSizeKB   = 1024;

        options.UseJsonFormatter(ConfigureJson);
    });
}
