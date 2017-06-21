using System;
using System.Collections.Generic;
using System.Linq;

namespace Gateway.Web.Models.Controllers
{
    public class AliasesModel
    {
        private readonly Dictionary<string, Alias> _items;

        public AliasesModel()
        {
            _items = new Dictionary<string, Alias>(StringComparer.OrdinalIgnoreCase);
        }

        public IEnumerable<Alias> Items { get { return _items.Values.OrderBy(i => i.Name); } }

        public Alias GetOrAdd(string alias)
        {
            Alias result;
            if (!_items.TryGetValue(alias, out result))
            {
                result = new Alias(alias);
                _items.Add(alias, result);
            }
            return result;
        }
    }

    public class Alias
    {
        public Alias(string name)
        {
            Name = name;
            Controllers = new List<ControllerVersion>();
        }

        public string Name { get; set; }
        public List<ControllerVersion> Controllers { get; private set; }
    }

    public class ControllerVersion
    {
        public ControllerVersion(string name, string version, string status, bool isActive)
        {
            Name = name;
            Version = version;
            Status = status;
            IsActive = isActive;
        }

        public string Name { get; set; }
        public string Version { get; set; }
        public string Status { get; set; }
        public bool IsActive { get; set; }
    }
}