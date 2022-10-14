using Job_Scheduling.Database;
using Microsoft.EntityFrameworkCore;
using System.Configuration;

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
		builder.WithOrigins("http://localhost:8080", "https://localhost:8080", "https://cyc-ui.dev-code.tk").AllowAnyHeader().AllowAnyMethod();
	});
});
builder.Services.AddDistributedMemoryCache();
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




app.Run();
