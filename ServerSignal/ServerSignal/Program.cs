using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Queues.AbstracionLayer;
using Queues.RabbitMQ;
using ServerSignal.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Agregar servicios al contenedor.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddSingleton<QueueServiceBase>();
builder.Services.AddSingleton<SSEHub>();


builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        builder =>
        {
            builder.WithOrigins("http://127.0.0.1:64332", "http://localhost:8082")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

string hostIpAddress = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?.ToString();
Console.WriteLine(hostIpAddress);

var app = builder.Build();

// Configure el pipeline de solicitud HTTP.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();

app.UseRouting(); // Agregar UseRouting antes de UseEndpoints

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<SSEHub>("/SSEHub");
    endpoints.MapControllers();
});

app.Run();
