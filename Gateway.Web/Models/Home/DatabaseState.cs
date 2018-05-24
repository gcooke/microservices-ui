using System;

namespace Gateway.Web.Models.Home
{
    public class DatabaseState : StateItem
    {
        public DatabaseState(string name, DateTime last, StateItemState state, string text)
        {
            Name = name;            
            Time = state == StateItemState.Okay ? "Okay" : "?% Full";
            State = state;
            Text = text;
        }
    }
}