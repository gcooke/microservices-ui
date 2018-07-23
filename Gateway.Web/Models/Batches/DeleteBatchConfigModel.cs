using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace Gateway.Web.Models.Batches
{
    public class DeleteBatchConfigModel
    {
        public long ConfigurationId { get; set; }
        public string Type { get; set; }
    }
}