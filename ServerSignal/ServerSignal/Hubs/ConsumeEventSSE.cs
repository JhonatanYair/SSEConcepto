namespace ServerSignal.Hubs
{
    public class ConsumeEventSSE
    {

        public string ConnetionID { get; set; }
        public string UserID { get; set; }
        public List<string> GroupNames { get; set; }
        public List<ConsumeEvent> ExchangeEvents { get; set; }

    }

    public class ConsumeEvent
    {

        public string QueueName { get; set; }
        public Action<object, string> EventMessage { get; set; }

    }
 
}
