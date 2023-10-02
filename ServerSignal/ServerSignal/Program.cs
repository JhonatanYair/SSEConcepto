using ServerSignal.Config;
using ServerSignal.Hubs;
using Queues.RabbitMQ;
using log4net.Config;
using log4net;
using System.Reflection;

//[assembly: XmlConfigurator(ConfigFile = "log4net.config", Watch = true)]

var log4NetRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
log4net.Config.XmlConfigurator.Configure(log4NetRepository, new FileInfo("log4net.config"));

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddSingleton<SSEHub>();
builder.Services.AddQueuesStreaming();

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

var app = builder.Build();

// Configure el pipeline de solicitud HTTP.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<SSEHub>("/SSEHub");
    endpoints.MapControllers();
});

app.Run();
