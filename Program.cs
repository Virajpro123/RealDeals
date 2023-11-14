using Hangfire;
using Hangfire.SQLite;
using Microsoft.EntityFrameworkCore;
using RealDealsAPI.Comparers;
using RealDealsAPI.Data;
using RealDealsAPI.Entities;
using RealDealsAPI.Helpers;
using RealDealsAPI.Middleware;
using RealDealsAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddAutoMapper(typeof(MappingProfiles).Assembly);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<MovieContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));

});
builder.Services.AddTransient<IMovieDataAccessService, MovieDataAccessService>();
builder.Services.AddTransient<MovieDTOComparer>();

//Hangfire Config
var sqliteOptions = new SQLiteStorageOptions();
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSQLiteStorage("Filename=store.db;", sqliteOptions)
);
builder.Services.AddHangfireServer();

builder.Services.AddCors();
builder.Services.AddHttpClient();

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build();
builder.Configuration.AddConfiguration(config);
var settings = builder.Configuration.Get<Settings>();
builder.Services.AddSingleton(settings);

var app = builder.Build();

//Exception handler on top
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(opt =>
{
    opt.AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithOrigins("http://localhost:3000");
});

app.UseAuthorization();

app.UseHangfireDashboard();

//Save Movies to DB Scheduled Task
RecurringJob.AddOrUpdate<MovieDataAccessService>("update movies database scheduled task", task => task.SaveMoviesToDBTask(), Cron.Hourly);

app.MapControllers();

var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<MovieContext>();
var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

try
{
    await context.Database.MigrateAsync();
}
catch (Exception ex)
{
    logger.LogError(ex, "A problem occurred during migration");
}

app.Run();
