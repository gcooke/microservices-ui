using Gateway.Web.Models;

namespace Gateway.Web.Database
{
    public interface IGatewayDataService
    {
        Catalogue GetCatalogue();
        ControllerDetailModel GetControllerInfo(string name);
    }
}