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
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var config = new ConfigurationBuilder()
		.SetBasePath(Directory.GetCurrentDirectory())
		.AddJsonFile("appsettings.json", optional: false, true)
        .AddJsonFile("appsettings.Development.json", optional: true, true)
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
builder.Services.AddSwaggerGen(setup => {
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        BearerFormat = "JWT",
        Name = "JWT Authentication",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        Description = "Put **_ONLY_** your JWT Bearer token on textbox below!",

        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };
    setup.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
    setup.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });

});
builder.Services.AddSession(options => {
	options.IdleTimeout = TimeSpan.FromHours(1);
	options.Cookie.HttpOnly = true;
	options.Cookie.IsEssential = true;
});
builder.Services.AddCors(options => {
	options.AddPolicy(name: MyAllowOrigins, builder =>
	{
		builder.WithOrigins("http://localhost:8080", "https://localhost:8080", "https://jobsys.uzycode.com", "https://jobsys-ws.uzycode.com", "http://192.168.1.16", "https://192.168.1.16").AllowAnyHeader().AllowAnyMethod();
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
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey
        (Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = false,
        ValidateIssuerSigningKey = true
    };
});
builder.Services.AddAuthorization(); 
builder.Services.AddHangfireServer();
var app = builder.Build();
app.UseSession();

// enable swagger ui for development mode
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();

app.MapControllers();
app.UseCors(MyAllowOrigins);
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers().RequireAuthorization();
    endpoints.MapHangfireDashboard();
});
var manager = new RecurringJobManager(); 
//manager.AddOrUpdate("some-id", "", Cron.Yearly());
RecurringJob.AddOrUpdate("schedulerRoute",  () => new HttpClient().GetAsync(config.GetValue<string>("HostnameStrings") + "/cronschedule/?accessCode=4gTq7qh6U53GrBPUmbRPgrXgZ"), Cron.Daily(18,0), TimeZoneInfo.Local);
RecurringJob.AddOrUpdate("jobChecker", () => new HttpClient().GetAsync(config.GetValue<string>("HostnameStrings") + "/cronJob/?accessCode=4gTq7qh6U53GrBPUmbRPgrXgZ"), Cron.Daily(1, 0), TimeZoneInfo.Local);
app.Run(); 
