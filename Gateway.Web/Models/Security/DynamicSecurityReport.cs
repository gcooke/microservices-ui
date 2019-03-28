using System;

namespace Gateway.Web.Models.Security
{
    public class DynamicSecurityReport
    {
        public DynamicSecurityReport(string parameter)
        {
            try
            {
                var args = parameter.Split('|');
                GroupName = args[0].ToUpper() + " REPORTS";
                Report = args[1];
                Name = args[2];
                
                if (args.Length > 3)
                    ParameterName = args[3];
            }
            catch (Exception ex)
            {
                Name = "Failed to load report";
                ParameterName = ex.Message;
            }
        }

        public string Report { get; }
        public string Name { get; }
        public string ParameterName { get; }
        public bool HasParameters { get { return !string.IsNullOrEmpty(ParameterName); } }

        public bool IsSystemReport { get { return GroupName == "SYSTEM REPORTS"; } }
        public string GroupName { get; }
    }
}