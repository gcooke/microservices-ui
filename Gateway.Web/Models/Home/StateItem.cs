namespace Gateway.Web.Models.Home
{
    public abstract class StateItem
    {
        public string Name { get; set; }
        public string Time { get; set; }
        public StateItemState State { get; set; }
        public string Text { get; set; }
        public string Link { get; set; }
    }

    public enum StateItemState
    {
        Unknown,
        Okay,
        Warn,
        Error
    }
}