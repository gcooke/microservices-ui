using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gateway.Web.Models.Security
{
    public class BusinessFunction
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int GroupTypeId { get; set; }
        public string GroupType { get; set; }
    }
}