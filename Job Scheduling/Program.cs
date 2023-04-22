using Job_Scheduling.Database;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using Hangfire;
using Hangfire.SqlServer;
using Job_Scheduling.Model;
using Job_Scheduling.Controllers;
using System.Net.Http;
using System.Security.Policy;
using Newtonsoft.Json.Linq;
using System.IO;
using System;
using RestSharp;
using System.Net.Http.Headers;

var config = new ConfigurationBuilder()
		.SetBasePath(Directory.GetCurrentDirectory())
		.AddJsonFile("appsettings.json", optional: true)
		.AddCommandLine(args)
		.Build();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var MyAllowOrigins = "_myAllowOrigins";
builder.Services.AddDbContext<Entity_Conf_Context>(options => options.UseSqlServer(config.GetConnectionString("DefaultConnection")));
builder.Services.AddDbContext<Job_Context>(options => options.UseSqlServer(config.GetConnectionString("DefaultConnection")));
builder.Services.AddDbContext<User_Context>(options => options.UseSqlServer(config.GetConnectionString("DefaultConnection")));
builder.Services.AddDbContext<Vehicle_Context>(options => options.UseSqlServer(config.GetConnectionString("DefaultConnection")));
builder.Services.AddDbContext<Schedule_Context>(options => options.UseSqlServer(config.GetConnectionString("DefaultConnection")));
builder.Services.AddDbContext<Material_Context>(options => options.UseSqlServer(config.GetConnectionString("DefaultConnection")));
builder.Services.AddDbContext<Tool_Context>(options => options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSession(options => {
	options.IdleTimeout = TimeSpan.FromHours(1);
	options.Cookie.HttpOnly = true;
	options.Cookie.IsEssential = true;
});
builder.Services.AddCors(options => {
	options.AddPolicy(name: MyAllowOrigins, builder =>
	{
		builder.WithOrigins("http://localhost:8080", "https://localhost:8080", "https://cyc-ui.dev-code.tk","http://192.168.1.16", "https://192.168.1.16").AllowAnyHeader().AllowAnyMethod();
	});
});
builder.Services.AddDistributedMemoryCache();
builder.Services.AddHangfire(configuration => configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(config.GetConnectionString("HangfireConnection"), new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.Zero,
            UseRecommendedIsolationLevel = true,
            DisableGlobalLocks = true
        }));
builder.Services.AddHangfireServer();
var app = builder.Build();
app.UseSession();
// Configure the HTTP request pipeline.
/*if (app.Environment.IsDevelopment())
{*/
    app.UseSwagger();
    app.UseSwaggerUI();
//}
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.UseCors(MyAllowOrigins);
app.UseRouting();
 
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHangfireDashboard();
});
var manager = new RecurringJobManager(); 
//manager.AddOrUpdate("some-id", "", Cron.Yearly());
 RecurringJob.AddOrUpdate(
    "myrecurringjob",  () => new HttpClient().GetAsync(config.GetValue<string>("HostnameStrings") + "/cronschedule/?schedule_date=" + DateTime.Now.ToString("dd/MM/yyyy")), Cron.Daily(18,0));
 
app.Run(); 
