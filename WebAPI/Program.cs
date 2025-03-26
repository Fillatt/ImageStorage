using DataBaseService;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

builder.Services.AddControllers();
builder.Services.AddDbContext<FilesContext>
    (options => options.UseNpgsql(configuration.GetConnectionString("DataBaseConnectionString")));

var app = builder.Build();

app.MapControllers();

app.Run();
