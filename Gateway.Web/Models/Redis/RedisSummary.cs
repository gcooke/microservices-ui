namespace Gateway.Web.Models.Redis
{
    public class RedisSummary
    {
        public int Workers { get; set; }
        public int WorkersIdle { get; set; }
        public int WorkersBusy { get { return Workers - WorkersIdle; } }
        public string ControllerName { get; set; }
        public string ControllerVersion { get; set; }
        public long QueueLength { get; set; }
        public RedisHealth RedisHealth { get; set; }
        public string Message { get; set; }
    }
}