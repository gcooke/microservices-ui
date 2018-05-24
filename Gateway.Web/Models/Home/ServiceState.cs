using System;

namespace Gateway.Web.Models.Home
{
    public class ServiceState : StateItem
    {
        public ServiceState(string name, DateTime last, StateItemState state, string text)
        {
            Name = name;
            Time = ""; //last.ToString("dd MMM HH:mm");
            State = state;
            Text = text;
            Link = "Controllers/Servers";
        }
    }
}