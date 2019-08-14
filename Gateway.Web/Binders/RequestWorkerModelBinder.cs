using Gateway.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

namespace Gateway.Web.Binders
{

    public class RequestWorkerModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            HttpRequestBase request = controllerContext.HttpContext.Request;

            List<string> nameValues = new List<string>();

            int instances = Convert.ToInt32(request.Form.Get("_instances"));

            return new RequestedWorkers()
            {
                ControllerName = request.Form.Get("_controllerName"),
                Instances = instances,
                Version =  request.Form.Get("_version"),
                Priority =  request.Form.Get("_priority"),
            };
        }
    }
}