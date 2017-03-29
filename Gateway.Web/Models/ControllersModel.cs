using System.Collections.Generic;
using Gateway.Web.Database;

namespace Gateway.Web.Models
{
    public class ControllersModel
    {
        private readonly IGatewayDataService _dataService;

        public ControllersModel(IGatewayDataService dataService)
        {
            _dataService = dataService;

            Controllers = new List<ControllerModel>();

            var catalogue = _dataService.GetCatalogue();
            foreach (var controller in catalogue.Controllers)
                Controllers.Add(controller);
        }

        public List<ControllerModel> Controllers { get; private set; }
    }
}