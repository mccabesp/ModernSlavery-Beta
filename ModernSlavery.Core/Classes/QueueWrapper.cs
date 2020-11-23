using ModernSlavery.Core.Extensions;
using Newtonsoft.Json;

namespace ModernSlavery.Core.Classes
{
    public class QueueWrapper
    {
        public QueueWrapper(object message)
        {
            Message = Json.SerializeObject(message);
            Type = message.GetType().ToString();
        }

        public string Type { get; set; }

        public string Message { get; set; }
    }
}