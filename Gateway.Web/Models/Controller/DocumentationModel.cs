using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gateway.Web.Models.Controller
{
    public class DocumentationModel : BaseControllerModel
    {
        public string Controller { get; set; }

        public IList<VersionDocumentationModel> VersionDocumentationModels { get; set; }

        public DocumentationModel(string name) : base(name)
        {
            VersionDocumentationModels = new List<VersionDocumentationModel>();
        }
    }

    public class VersionDocumentationModel
    {
        public string VersionName { get; set; }
        public bool HasDocumentation { get; set; }
        public string Status { get; set; }
    }
}