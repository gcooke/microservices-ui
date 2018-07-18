using System;

namespace Gateway.Web.Models.Home
{
    public class ControllerState : StateItem
    {
        public int ProcessingRequestCount { get; set; }
        public int TotalRequestCount { get; set; }
        public int TotalWorkerCount { get; set; }
        public int BusyWorkerCount { get; set; }

        public ControllerState()
        {
            ProcessingRequestCount = 0;
            TotalWorkerCount = 0;
            BusyWorkerCount = 0;
            TotalRequestCount = 0;
        }
    }
}