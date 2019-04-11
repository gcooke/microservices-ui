using System.Collections.Generic;

namespace Gateway.Web.Database
{
    public interface IPnRFoDatabaseService
    {
        List<string> GetControllerNames();        
    }
}