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
                if (RiskBatchConfiguration != null)
                {
                    return $"{RiskBatchConfiguration.Type} - {TradeSource}";
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
                if (RiskBatchConfiguration != null)
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