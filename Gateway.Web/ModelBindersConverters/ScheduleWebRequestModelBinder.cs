using System.Linq;
using System.Web.Mvc;
using Gateway.Web.Models.Schedule.Input;

namespace Gateway.Web.ModelBindersConverters
{
    public class ScheduleWebRequestModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var performValidation = controllerContext.Controller.ValidateRequest &&
                                    bindingContext.ModelMetadata.RequestValidationEnabled;
            var payloadValue = GetValueFromValueProvider(bindingContext, "Payload", performValidation);
            var request = controllerContext.HttpContext.Request;
            var requestConfigurationId = request.Form["RequestConfigurationId"];

            var model = new ScheduleWebRequestModel
            {
                Name = request.Form["Name"],
                Url = request.Form["Url"],
                Verb = request.Form["Verb"],
                Payload = payloadValue?.AttemptedValue,
                Group = request.Form["Group"],
                Parent = request.Form["Parent"],
                GroupName = request.Form["GroupName"],
                RequestConfigurationId = string.IsNullOrWhiteSpace(requestConfigurationId)
                    ? (long?)null
                    : long.Parse(request.Form["RequestConfigurationId"]),
            };


            foreach (var argumentIndex in Enumerable.Range(0, model.Arguments.Count))
            {
                model.Arguments[argumentIndex].Type = request.Form[$"Arguments[{argumentIndex}].Type"];
                model.Arguments[argumentIndex].FormatValue = request.Form[$"Arguments[{argumentIndex}].FormatValue"];
                model.Arguments[argumentIndex].Key = request.Form[$"Arguments[{argumentIndex}].Key"];
            }

            foreach (var headerIndex in Enumerable.Range(0, model.Headers.Count))
            {
                model.Headers[headerIndex].Key = request.Form[$"Headers[{headerIndex}].Key"];
                model.Headers[headerIndex].Value = request.Form[$"Headers[{headerIndex}].Value"];
            }

            model.Arguments = model.Arguments.Where(x => !string.IsNullOrWhiteSpace(x.Key)).ToList();
            model.Headers = model.Headers.Where(x => !string.IsNullOrWhiteSpace(x.Key)).ToList();

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