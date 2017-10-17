﻿using System.Collections.Generic;
using System.Xml.Serialization;
using Gateway.Web.Models.User;

namespace Gateway.Web.Models.Group
{
    [XmlType("Group")]
    public class GroupModel : IGroupModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}