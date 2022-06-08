var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var MyAllowOrigins = "_myAllowOrigins";
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
		builder.WithOrigins("http://localhost:8080", "https://localhost:8080").AllowAnyHeader().AllowAnyMethod();
	});
});
builder.Services.AddDistributedMemoryCache();
var app = builder.Build();
app.UseSession();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.UseCors(MyAllowOrigins);
app.UseRouting();




app.Run();
