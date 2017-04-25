namespace Gateway.Web.Models.Controller
{
    public abstract class BaseControllerModel
    {
        protected BaseControllerModel(string name)
        {
            Name = name;
        }
        public string Name { get; set; }
    }
}