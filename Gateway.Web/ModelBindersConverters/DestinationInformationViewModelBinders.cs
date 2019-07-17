using Gateway.Web.Models.Export;
using System.Linq;
using System.Web.Mvc;

namespace Gateway.Web.ModelBindersConverters
{
    public class DestinationInformationViewModelBinders : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var request = controllerContext.HttpContext.Request;

            var model = new DestinationInfoViewModel()
            {
                DestinationUrl = request.Form["DestinationInformation.DestinationUrl"],
                FileName = request.Form["DestinationInformation.FileName"],
            };

            foreach (var argumentIndex in Enumerable.Range(0, model.Arguments.Count))
            {
                model.Arguments[argumentIndex].Type = request.Form[$"DestinationInformation.Arguments[{argumentIndex}].Type"];
                model.Arguments[argumentIndex].FormatValue = request.Form[$"DestinationInformation.Arguments[{argumentIndex}].FormatValue"];
                model.Arguments[argumentIndex].Key = request.Form[$"DestinationInformation.Arguments[{argumentIndex}].Key"];
            }

            model.Arguments = model.Arguments.Where(x => !string.IsNullOrWhiteSpace(x.Key)).ToList();

            return model;
        }

        private ValueProviderResult GetValueFromValueProvider(ModelBindingContext bindingContext, string name,
            bool performRequestValidation)
        {
            var unvalidatedValueProvider = bindingContext.ValueProvider as IUnvalidatedValueProvider;
            return (unvalidatedValueProvider != null)
                ? unvalidatedValueProvider.GetValue(name, !performRequestValidation)
                : bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        }
    }
}