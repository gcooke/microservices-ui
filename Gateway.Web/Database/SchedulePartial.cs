namespace Gateway.Web.Database
{
    public partial class Schedule
    {
        public string Name
        {
            get
            {
                if (RiskBatchSchedule != null)
                {
                    return $"{RiskBatchSchedule.RiskBatchConfiguration.Type} - {RiskBatchSchedule.TradeSource} ({RiskBatchSchedule.TradeSourceType})";
                }

                if (RequestConfiguration != null)
                {
                    return $"{RequestConfiguration.Name}";
                }

                if (ExecutableConfiguration != null)
                {
                    return $"{ExecutableConfiguration.Name}";
                }

                return string.Empty;
            }
        }

        public string Key
        {
            get
            {
                if (RiskBatchSchedule != null)
                {
                    return $"{RiskBatchSchedule.RiskBatchConfiguration.Type} - {RiskBatchSchedule.TradeSource} ({RiskBatchSchedule.TradeSourceType})";
                }

                if (RequestConfiguration != null)
                {
                    return $"{RequestConfiguration.Name}";
                }

                if (ExecutableConfiguration != null)
                {
                    return $"{ExecutableConfiguration.Name}";
                }

                return string.Empty;
            }
        }

        public string Type
        {
            get
            {
                if (RiskBatchSchedule != null)
                {
                    return "BATCH";
                }

                if (RequestConfiguration != null)
                {
                    return "REQUEST";
                }

                if (ExecutableConfiguration != null)
                {
                    return "EXECUTABLE";
                }

                return string.Empty;
            }
        }
    }
}