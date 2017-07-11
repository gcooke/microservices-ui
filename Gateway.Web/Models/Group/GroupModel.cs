﻿using System.Xml.Serialization;

namespace Gateway.Web.Models.Group
{
    [XmlType("Group")]
    public class GroupModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}