namespace Gateway.Web.Models.Redis
{
    public class RedisStats
    {
        public int Workers { get; set; }
        public int WorkersIdle { get; set; }
        public int WorkersBusy { get { return Workers - WorkersIdle; } }
        public long QueueLength { get; set; }
        public RedisHealth RedisHealth { get; set; }
        public int Priority { get; set; }
        public string Message { get; set; }

        public string RedisHealthDisplay { get { return RedisHealth.ToString(); } }
    }
}