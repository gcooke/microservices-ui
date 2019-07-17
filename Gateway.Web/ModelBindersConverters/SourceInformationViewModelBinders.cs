using Gateway.Web.Models.Export;
using System.Linq;
using System.Web.Mvc;

namespace Gateway.Web.ModelBindersConverters
{
    public class SourceInformationViewModelBinders : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var request = controllerContext.HttpContext.Request;

            var model = new SourceInformationViewModel()
            {
                Controller = request.Form["SourceInformation.Controller"],
                ExpectedResponseType = request.Form["SourceInformation.ExpectedResponseType"],
                Verb = request.Form["SourceInformation.Verb"],
                Query = request.Form["SourceInformation.Query"],
            };

            foreach (var argumentIndex in Enumerable.Range(0, model.Arguments.Count))
            {
                model.Arguments[argumentIndex].Type = request.Form[$"SourceInformation.Arguments[{argumentIndex}].Type"];
                model.Arguments[argumentIndex].FormatValue = request.Form[$"SourceInformation.Arguments[{argumentIndex}].FormatValue"];
                model.Arguments[argumentIndex].Key = request.Form[$"SourceInformation.Arguments[{argumentIndex}].Key"];
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