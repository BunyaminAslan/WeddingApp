using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Formatting.Json;
using Wedding.Repository;
using Wedding.Repository.Interfaces;
using Wedding.Repository.Services;
using WeddingApp.UI.Cache;
using WeddingApp.UI.ImageUpload;
using WeddingApp.UI.Jop;
using WeddingApp.UI.Logging;
using StackExchange.Redis;
using WeddingApp.UI.Redis;



var builder = WebApplication.CreateBuilder(args);

DotNetEnv.Env.Load();

builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings"));

//builder.Services.Configure<CloudinarySettings>(options =>
//{
//    options.CloudName = Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME");
//    options.ApiKey = Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY");
//    options.ApiSecret = Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET");
//});


Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName()
    .WriteTo.Console()
    //.WriteTo.File("../RsCBA/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

// Varsayýlan logger'ý Serilog ile deðiþtir
builder.Host.UseSerilog();


builder.Configuration["ConnectionStrings:DefaultConnection"] = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 600_000_000; // 100MB
});


builder.Services.AddDbContext<Supabase_WeddingDbContext>(options =>
    options.UseNpgsql(builder.Configuration["ConnectionStrings:DefaultConnection"]));

//builder.Services.AddDbContext<Supabase_WeddingDbContext>(options =>
//    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHostedService<UploadJob>();


// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IPhotoRepository, PhotoRepository>();
builder.Services.AddSingleton<IUploadQueue, UploadQueue>();
builder.Services.AddSingleton<CloudinaryService>();

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var connString = builder.Configuration.GetConnectionString("Redis") ?? "redis:6379";
    try
    {
        return ConnectionMultiplexer.Connect(connString);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Redis connection failed: {ex.Message}");
        return null;
    }
});

builder.Services.AddSingleton<IUploadQueue>(sp => {

    var redisConn = sp.GetService<IConnectionMultiplexer>();

    var memoryQueue = new UploadQueue();

    if(redisConn != null && redisConn.IsConnected)
    {
        return new FallbackQueueService(new RedisQueueService(redisConn),memoryQueue);
    }

    Console.WriteLine("Redis not available, using MemoryQueue");


    return memoryQueue;
});

//builder.Services.AddSingleton<IRedisQueueService, RedisQueueService>();


var app = builder.Build();

//app.UseSerilogRequestLogging();

app.UseMiddleware<RequestTracingMiddleware>();

app.UseDeveloperExceptionPage();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseStatusCodePages();
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
