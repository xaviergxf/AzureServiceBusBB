using Azure.Identity;
using Microsoft.Extensions.Azure;
using WarehouseAPI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var conf = builder.Configuration;
builder.Services.AddAzureClients(builder =>
{
    builder.AddServiceBusClient(conf.GetConnectionString("ServiceBus"));
});
builder.Services.AddHostedService<OrderPaidHostedService>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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