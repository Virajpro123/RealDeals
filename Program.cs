using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using RealDealsAPI;
using RealDealsAPI.Data;
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

string connString;
if (builder.Environment.IsDevelopment())
    connString = builder.Configuration.GetConnectionString("DefaultConnection");
else
{
    // Use connection string provided at runtime by FlyIO.
    var connUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

    // Parse connection URL to connection string for Npgsql
    connUrl = connUrl.Replace("postgres://", string.Empty);
    var pgUserPass = connUrl.Split("@")[0];
    var pgHostPortDb = connUrl.Split("@")[1];
    var pgHostPort = pgHostPortDb.Split("/")[0];
    var pgDb = pgHostPortDb.Split("/")[1];
    var pgUser = pgUserPass.Split(":")[0];
    var pgPass = pgUserPass.Split(":")[1];
    var pgHost = pgHostPort.Split(":")[0];
    var pgPort = pgHostPort.Split(":")[1];
    var updatedHost = pgHost.Replace("flycast", "internal");

connString = $"Server={updatedHost};Port={pgPort};User Id={pgUser};Password={pgPass};Database={pgDb};";
}
builder.Services.AddDbContext<MovieContext>(opt =>
{
    opt.UseNpgsql(connString);
});

builder.Services.AddServices();
builder.Services.AddSettings(builder);
builder.Services.AddRateLimitting();

//Hangfire Config
builder.Services.AddHangfire(x =>
    x.UsePostgreSqlStorage(connString));

builder.Services.AddHangfireServer();

builder.Services.AddCors();
builder.Services.AddHttpClient();

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
app.UseStaticFiles();

app.UseDefaultFiles();

app.UseCors(opt =>
{
    opt.AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithOrigins("http://localhost:3000");
});

app.UseAuthorization();

app.UseHangfireDashboard();

//Save Movies to DB Scheduled Task
RecurringJob.AddOrUpdate<MovieDataAccessService>("update movies database scheduled task", task => task.SaveMoviesToDBTask(), Cron.Hourly);

app.MapControllers();
app.MapFallbackToController("Index","Fallback");

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

app.UseRateLimiter();

app.Run();
