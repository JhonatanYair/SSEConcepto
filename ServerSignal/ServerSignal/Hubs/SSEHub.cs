using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Queues.AbstracionLayer;
using Queues.AbstracionLayer.Enums;
using System.Linq;


namespace ServerSignal.Hubs;

public class SSEHub : Hub
{
    private readonly QueueServiceBase queueService;

    private Dictionary<string, Action<object, string>> messageGropusReceivedHandlers = new Dictionary<string, Action<object, string>>();
    private List<ExchangeEventSSE> ExchangesPrivates = new List<ExchangeEventSSE>();
    private List<ExchangeEventSSE> Exchanges = new List<ExchangeEventSSE>();

    public SSEHub(QueueServiceBase _queueService)
    {
        queueService= _queueService;
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        Console.WriteLine();
        Console.WriteLine(Context.ConnectionId);
        Console.WriteLine();
        await base.OnDisconnectedAsync(exception);
    }


    public async Task UnsubscribeUser(string userId)
    {

    }

    public async Task SendMessage(string mensaje)
    {
        Console.WriteLine(mensaje);
        await Clients.All.SendAsync("ReceivedMessage", mensaje);
    }

    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task JoinUser(string UserId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, UserId);
    }

    public async Task JoinUserExchange(string groupName,string UserId)
    {
        string nameGroup = $"{groupName}_{UserId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, nameGroup);
    }

    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task SendMessageToGroup(string groupName, string message)
    {
        await Clients.Group(groupName).SendAsync("ReceiveMessageGroup", message);
    }

    public async Task SendPrivateMessageGroup(string userId, string message)
    {
        await Clients.Group(userId).SendAsync("ReceiveMessagePrivate", message);
    }

    public async Task SendPrivateMessage(string userId, string message)
    {
        await Clients.Group(userId).SendAsync("ReceiveMessagePrivate", message);
    }

    public async Task SendPrivateExchangeMessage(string groupName,string userId, string message)
    {
        string nameGroup = $"{groupName}_{userId}";
        await Clients.Group(nameGroup).SendAsync("ReceiveMessagePrivExchange", message);
    }

    public ExchangeEventSSE ExchangeExist(string ConnectionId, string groupName)
    {
        var ExchangeIndex = Exchanges.FirstOrDefault(Exchanges => Exchanges.ConnetionID == ConnectionId && Exchanges.GroupNames.Contains(groupName));
        if (ExchangeIndex == null)
        {
            Exchanges.Add(
                new ExchangeEventSSE
                {
                    ConnetionID = Context.ConnectionId,
                    GroupNames = new List<string> { groupName },
                }
            );
            ExchangeIndex = Exchanges[Exchanges.Count - 1];
        }
        else
        {
            ExchangeIndex.GroupNames.Add(groupName);
        }
        return ExchangeIndex;
    }

    public bool ExchangeExistUser(string userId)
    {
        bool boolExist = ExchangesPrivates.Exists(Exchanges => Exchanges.UserID == userId);
        if (boolExist == false)
        {
            ExchangesPrivates.Add(
                new ExchangeEventSSE
                {
                    ConnetionID = Context.ConnectionId,
                    UserID = userId,
                    GroupNames = new List<string> { },
                    ExchangeEvents = new List<Action<object, string>> { },
                }
            );
        }
        return boolExist;
    }

    public async Task SubsPrivateMessageExchange(string groupName, string userId)
    {
        bool existExchangeUser = ExchangeExistUser(userId);
        ExchangeEventSSE ExchangeIndex = ExchangesPrivates.FirstOrDefault(Exchanges => Exchanges.UserID == userId);
        bool existExchangeUserGroup = ExchangesPrivates.Exists(Exchanges => Exchanges.UserID == userId && Exchanges.GroupNames.Contains(groupName));
        NotificationSignal.NotificationSignal notificationSignal = new NotificationSignal.NotificationSignal();

        if (existExchangeUserGroup == false)
        {
            Action<object, string> messageReceivedHandlerPrivate = async (sender, message) =>
            {
                Console.WriteLine($"{message} {userId} SSE private");
                Payload messageObject = JsonConvert.DeserializeObject<Payload>(message);

                if (userId == messageObject.To && messageObject.Exchange == groupName)
                {
                    notificationSignal.NotifyASignalRPrivate(groupName,userId, message);
                }
            };

            queueService.SubscribeToMessageReceived(messageReceivedHandlerPrivate);
            ExchangeIndex.GroupNames.Add(groupName);
            ExchangeIndex.ExchangeEvents.Add(messageReceivedHandlerPrivate);

            if (Enum.TryParse(groupName, out ExchangeTypes valueEnum))
            {
                Console.WriteLine($"El valor enum es: {valueEnum}");
                var message = queueService.ConsumeMessage(valueEnum);
            }
            else
            {
                Console.WriteLine("El string no coincide con ningún valor enum.");
            }
        }
    }

    public async Task SubsMessageToGroupExchange(string groupName)
    {
        var ExchangeIndex = ExchangeExist(Context.ConnectionId, groupName);
        NotificationSignal.NotificationSignal notificationSignal = new NotificationSignal.NotificationSignal();

        if (!messageGropusReceivedHandlers.ContainsKey(groupName))
        {
            Action<object, string> messageReceivedHandler = async (sender, message) =>
            {
                Payload messageObject = JsonConvert.DeserializeObject<Payload>(message);
                if (messageObject.Exchange == groupName)
                {
                    Console.WriteLine($"{message} {groupName} SSE grupo");
                    notificationSignal.NotifyASignalRGroup(groupName, message);
                }
            };

            queueService.SubscribeToMessageReceived(messageReceivedHandler);
            messageGropusReceivedHandlers[groupName] = messageReceivedHandler;


            if (Enum.TryParse(groupName, out ExchangeTypes valueEnum))
            {
                Console.WriteLine($"El valor enum es: {valueEnum}");
                var message = queueService.ConsumeMessage(valueEnum);
            }
            else
            {
                Console.WriteLine("El string no coincide con ningún valor enum.");
            }
        }

    }

}
