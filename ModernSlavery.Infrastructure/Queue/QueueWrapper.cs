using Newtonsoft.Json;

namespace ModernSlavery.Infrastructure.Queue
{
    public class QueueWrapper
    {

        public QueueWrapper(object message)
        {
            Message = JsonConvert.SerializeObject(message);
            Type = message.GetType().ToString();
        }

        public string Type { get; set; }

        public string Message { get; set; }

    }
}
