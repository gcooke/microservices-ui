using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Bagl.Cib.MIT.Cube;
using Gateway.Web.Utils;

namespace Gateway.Web.Database
{
    public class RiskBatchModel
    {
        public RiskBatchModel()
        {
            ControllerName = "Batch";
            Items = new List<RiskBatchGroup>();
            AvailableSites = new List<string>();
        }

        public string ControllerName { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime ReportDate { get; set; }

        public string ReportDateUrl
        {
            get { return ReportDate.ToString(SiteConstants.DateParamFormat); }
        }

        public string ReportDateFormatted
        {
            get { return ReportDate.ToString(SiteConstants.DateFormat); }
        }

        public List<string> AvailableSites { get; private set; }

        public string Site { get; set; }

        public List<RiskBatchGroup> Items { get; private set; }

        public bool ShowOverwrittenResults { get; set; }

        public string Summary
        {
            get
            {
                var success = 0;
                var failure = 0;

                foreach (var item in Items)
                {
                    var previousWasFailure = false;
                    for (int index = 0; index < item.Items.Count; index++)
                    {
                        if (previousWasFailure && item.Items[index].IsRerun.Value)
                            failure--;
                        var state = item.Items[index].State;
                        if (state == "Complete" || state == "Okay")
                        {
                            previousWasFailure = false;
                            success++;
                        }
                        else
                        {
                            previousWasFailure = true;
                            failure++;
                        }
                    }
                }

                return string.Format("{0}/{1} successfully completed batches", success, success + failure);
            }
        }
    }    
}