using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Queues.AbstracionLayer;
using Queues.AbstracionLayer.Enums;
using System.Linq;

namespace ServerSignal.Hubs;

public class SSEHub : Hub
{
    private readonly QueueServiceBase queueService;
    private readonly NotificationSignal.NotificationSignal notificationSignal;
    private Dictionary<string, Action<object, string>> messageGropusReceivedHandlers = new Dictionary<string, Action<object, string>>();
    private List<ConsumeEventSSE> exchangesPrivate = new List<ConsumeEventSSE>();
    private List<ConsumeEventSSE> exchanges = new List<ConsumeEventSSE>();

    public SSEHub(QueueServiceBase _queueService)
    {
        queueService= _queueService;
        notificationSignal = new NotificationSignal.NotificationSignal();
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        await UnsubscribeClient(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task UnsubscribeClient(string connectionID)
    {
        var indexExcPriva = exchangesPrivate.FirstOrDefault(Exchange => Exchange.ConnetionID == connectionID);
        if (indexExcPriva != null)
        {
            foreach (var eventS in indexExcPriva.ExchangeEvents)
            {
                queueService.UnsubscribeFromMessage(eventS.EventMessage,eventS.QueueName);
            }
            exchangesPrivate.Remove(indexExcPriva);
        }

        var indexExchanges = exchanges.FirstOrDefault(Exchange => Exchange.ConnetionID == connectionID);
        if (indexExchanges != null)
        {
            var listGroups = indexExchanges.GroupNames;
            exchanges.Remove(indexExchanges);
            await UnsubscribeGroup(listGroups);
        }

    }

    public async Task UnsubscribeGroup(List<string> listGroupNames)
    {
        foreach (var groupName in listGroupNames)
        {
            bool existEvent = exchanges.Any(Exchange => Exchange.GroupNames.Contains(groupName));
            if (existEvent == false)
            {
                var eventGroup = messageGropusReceivedHandlers[groupName];
                queueService.UnsubscribeFromMessage(eventGroup, groupName);
                messageGropusReceivedHandlers.Remove(groupName);
            }
        }
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

    public ConsumeEventSSE ExchangeExist(string ConnectionId, string groupName)
    {
        var exchangeIndex = exchanges.FirstOrDefault(Exchanges => Exchanges.ConnetionID == ConnectionId);
        if (exchangeIndex == null)
        {
             exchanges.Add(
                new ConsumeEventSSE
                {
                    ConnetionID = Context.ConnectionId,
                    GroupNames = new List<string> { groupName },
                }
            );
            exchangeIndex = exchanges[exchanges.Count - 1];
        }
        else if(!exchangeIndex.GroupNames.Contains(groupName))
        {
            exchangeIndex.GroupNames.Add(groupName);
        }
        return exchangeIndex;
    }

    public bool ExchangeExistPrivate(string userId)
    {
        bool boolExist = exchangesPrivate.Exists(Exchanges => Exchanges.UserID == userId);
        if (boolExist == false)
        {
            exchangesPrivate.Add(
                new ConsumeEventSSE
                {
                    ConnetionID = Context.ConnectionId,
                    UserID = userId,
                    GroupNames = new List<string> { },
                    ExchangeEvents = new List<ConsumeEvent> { },
                }
            );
        }
        return boolExist;
    }

    public async Task SubsPrivateMessageExchange(string groupName, string userId)
    {
        bool existExchangeUser = ExchangeExistPrivate(userId);
        ConsumeEventSSE exchangeIndex = exchangesPrivate.FirstOrDefault(Exchanges => Exchanges.UserID == userId);
        bool existExchangeUserGroup = exchangesPrivate.Exists(Exchanges => Exchanges.UserID == userId && Exchanges.GroupNames.Contains(groupName));

        if (existExchangeUserGroup == false)
        {
            Action<object, string> messageReceivedHandlerPrivate = async (sender, message) =>
            {
                Payload messageObject = JsonConvert.DeserializeObject<Payload>(message);
                if (userId == messageObject.to && messageObject.exchange == groupName)
                {
                    notificationSignal.NotifyASignalRPrivate(groupName,userId, message);
                }
            };

            queueService.SubscribeToMessage(messageReceivedHandlerPrivate);
            exchangeIndex.ExchangeEvents.Add(
                new ConsumeEvent {
                    EventMessage = messageReceivedHandlerPrivate, 
                    QueueName = $"{groupName}{userId}"
                    }
                );
            exchangeIndex.GroupNames.Add(groupName);

            if (Enum.TryParse(groupName, out ExchangeTypes valueEnum))
            {
                queueService.ConsumeMessage(valueEnum, userId);
            }
            else
            {
                Console.WriteLine("El string no coincide con ningún valor enum.");
            }
        }
    }

    public async Task SubsMessageToGroupExchange(string groupName)
    {
        ExchangeExist(Context.ConnectionId, groupName);
        if (!messageGropusReceivedHandlers.ContainsKey(groupName))
        {
            Action<object, string> messageReceivedHandler = async (sender, message) =>
            {
                Payload messageObject = JsonConvert.DeserializeObject<Payload>(message);
                if (messageObject.exchange == groupName && string.IsNullOrEmpty(messageObject.to))
                {
                    Console.WriteLine($"{message} {groupName} SSE grupo");
                    notificationSignal.NotifyASignalRGroup(groupName, message);
                }
            };

            queueService.SubscribeToMessage(messageReceivedHandler);
            messageGropusReceivedHandlers[groupName] = messageReceivedHandler;

            if (Enum.TryParse(groupName, out ExchangeTypes valueEnum))
            {
                queueService.ConsumeMessage(valueEnum,"");
            }
            else
            {
                Console.WriteLine("El string no coincide con ningún valor enum.");
            }
        }
    }

}
