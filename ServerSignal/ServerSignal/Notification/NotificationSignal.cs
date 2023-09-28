using Microsoft.AspNetCore.SignalR.Client;

namespace ServerSignal.NotificationSignal
{
    public class NotificationSignal
    {
        private readonly string signalUrl;
        public NotificationSignal() 
        {
            signalUrl = Environment.GetEnvironmentVariable("URL_SIGNAL_HUB") ?? "http://localhost:5140/SSEHub";
        }

        public async Task NotifyASignalRGroup(string groupName, string mensaje)
        {

            var hubConnection = new HubConnectionBuilder()
            .WithUrl(signalUrl)
            .Build();

            try
            {
                await hubConnection.StartAsync();
                await hubConnection.InvokeAsync("SendMessageToGroup", groupName, mensaje);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al conectar: {ex.Message}");
                Console.WriteLine();
                Console.WriteLine(ex);
            }
        }

        public async Task NotifyASignalRPrivate(string groupName,string userId, string mensaje)
        {
            var hubConnection = new HubConnectionBuilder()
            .WithUrl(signalUrl)
            .Build();

            try
            {
                await hubConnection.StartAsync();
                await hubConnection.InvokeAsync("SendPrivateExchangeMessage", groupName, userId, mensaje);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al conectar: {ex.Message}");
            }
        }

    }

}
