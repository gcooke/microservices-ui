using System;
using System.Data;
using System.Data.SqlClient;
using Bagl.Cib.MIT.IoC;
using Gateway.Web.Controllers;
using Gateway.Web.Models.Home;

namespace Gateway.Web.Services
{
    public class DatabaseStateProvider : IDatabaseStateProvider
    {
        private readonly ISystemInformation _systemInformation;

        public DatabaseStateProvider(ISystemInformation systemInformation)
        {
            _systemInformation = systemInformation;
        }

        public DatabaseState GetDatabaseState(string DatabaseConfigId)
        {
            var database = _systemInformation.GetSetting(DatabaseConfigId);
            var server = _systemInformation.GetSetting("DatabaseServer");

            var database_size = string.Empty;
            var unallocatedspace = string.Empty;
            var state = StateItemState.Okay;
            var message = string.Empty;

            try
            {
                using (SqlConnection dbConnection = new SqlConnection($@"Data Source={server};Database={database};Integrated Security=true;"))
                using (SqlCommand dbCommand = new SqlCommand("sp_spaceused", dbConnection))
                {
                    dbCommand.CommandType = CommandType.Text;
                    dbConnection.Open();

                    SqlDataReader reader = dbCommand.ExecuteReader();

                    while (reader.Read())
                    {
                        database_size = (string) reader["database_size"];
                        unallocatedspace = (string) reader["unallocated space"];
                    }

                    dbConnection.Close();
                }
            }
            catch (Exception ex)
            {
                state = StateItemState.Error;
                message = "Connection";
            }


            try
            {
                var databaseSize = decimal.Parse(database_size.Split(' ')[0]);
                var unallocated = decimal.Parse(unallocatedspace.Split(' ')[0]);

                var percentageused = (databaseSize / (databaseSize + unallocated))*100;
                message = $"{percentageused.ToString("0.##")} %";

                if (percentageused > 70.0M)
                {
                    state = StateItemState.Warn;
                }

            }
            catch (Exception ex)
            {
                state = StateItemState.Warn;
                message = "No Db Size";
            }

            return new DatabaseState(database, DateTime.Now, state, message);
        }
    }
}