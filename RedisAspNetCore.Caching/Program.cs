using Newtonsoft.Json;
using RedisAspNetCore.Caching;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddNewtonsoftJson(x => x.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<SalesContext>();
builder.Services.AddHostedService<InitService>();
builder.Services.AddStackExchangeRedisCache(x => x.ConfigurationOptions = new ConfigurationOptions()
{
    EndPoints = { "localhost:32768" },
    User = "default",
    Password = "redispw",
});

// TODO Section 3.2 Step 1
// call AddStackExchangeRedisCache here.

// End Section 3.2 Step 1
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

app.Run();
