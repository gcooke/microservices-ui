using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Gateway.Web.Models.Home;
using Gateway.Web.Models.Monitoring;

namespace Gateway.Web.Utils
{
    public static class DashboardEx
    {
        public static StateItemState GetCpuState(this ServerDiagnostics frameworkStateItem)
        {
            if (!frameworkStateItem.Available)
                return StateItemState.Error;

            if (frameworkStateItem.CpuUsage >= 90)
                return StateItemState.Error;

            if (frameworkStateItem.CpuUsage >= 80)
                return StateItemState.Warn;

            return StateItemState.Okay;
        }

        public static StateItemState GetMemoryState(this ServerDiagnostics frameworkStateItem)
        {
            if (!frameworkStateItem.Available)
                return StateItemState.Error;

            if (frameworkStateItem.MemoryAvailableGigabytes <= 2)
                return StateItemState.Error;

            if (frameworkStateItem.MemoryAvailableGigabytes <= 5)
                return StateItemState.Warn;

            return StateItemState.Okay;
        }

        public static StateItemState GetWorkerState(this ServerDiagnostics frameworkStateItem)
        {
            if (!frameworkStateItem.Available)
                return StateItemState.Error;

            if (frameworkStateItem.BusyWorkerCount >= 50)
                return StateItemState.Warn;

            return StateItemState.Okay;
        }
    }
}