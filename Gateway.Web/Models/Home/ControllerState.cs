using System;

namespace Gateway.Web.Models.Home
{
    public class ControllerState : StateItem
    {
        public int IncompleteRequestCount { get; set; }

        public ControllerState()
        {
            IncompleteRequestCount = 0;
        }
    }
}