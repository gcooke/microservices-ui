using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gateway.Web.Database
{
    public partial class Schedule
    {
        public string Name
        {
            get
            {
                if (RiskBatchConfiguration != null)
                {
                    return $"{RiskBatchConfiguration.Type} - {TradeSource}";
                }

                if (RequestConfiguration != null)
                {
                    return $"{RequestConfiguration.Name}";
                }

                return string.Empty;
            }
        }

        public string Key
        {
            get
            {
                if (RiskBatchConfiguration != null)
                {
                    return $"{RiskBatchConfiguration.Type} - {TradeSource}";
                }

                if (RequestConfiguration != null)
                {
                    return $"{RequestConfiguration.Name}";
                }

                return string.Empty;
            }
        }

        public string Type
        {
            get
            {
                if (RiskBatchConfiguration != null)
                {
                    return "BATCH";
                }

                if (RequestConfiguration != null)
                {
                    return "REQUEST";
                }

                return string.Empty;
            }
        }

        public bool IsUpdating => ScheduleId != 0;
    }
}