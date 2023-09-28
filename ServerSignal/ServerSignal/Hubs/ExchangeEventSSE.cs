namespace ServerSignal.Hubs
{
    public class ExchangeEventSSE
    {

        public string ConnetionID { get; set; }
        public string UserID { get; set; }
        public List<string> GroupNames { get; set; }
        public List<Action<object, string>> ExchangeEvents { get; set; }

    }
 
}
