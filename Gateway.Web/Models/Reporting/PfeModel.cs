using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gateway.Web.Models.Reporting
{
    public class PfeModel
    {
        public PfeModel()
        {
            Items = new List<XvaResult>();
        }

        public List<XvaResult> Items { get; set; }
    }

    public class XvaResult
    {
        public XvaResult()
        {
            Point_0D = new PfePoint("0D");
            Point_3D = new PfePoint("3D");
            Point_1W = new PfePoint("1W");
            Point_2W = new PfePoint("2W");
            Point_3W = new PfePoint("3W");
            Point_1M = new PfePoint("1M");
            Point_2M = new PfePoint("2M");
            Point_1Y = new PfePoint("1Y");
            Point_2Y = new PfePoint("2Y");
            Point_3Y = new PfePoint("3Y");
            Point_4Y = new PfePoint("4Y");
            Point_Other = new PfePoint("etc");
        }
        public string Site { get; set; }
        public string Counterparty { get; set; }
        public PfePoint Point_0D { get; set; }
        public PfePoint Point_3D { get; set; }
        public PfePoint Point_1W { get; set; }
        public PfePoint Point_2W { get; set; }
        public PfePoint Point_3W { get; set; }
        public PfePoint Point_1M { get; set; }
        public PfePoint Point_2M { get; set; }
        public PfePoint Point_1Y { get; set; }
        public PfePoint Point_2Y { get; set; }
        public PfePoint Point_3Y { get; set; }
        public PfePoint Point_4Y { get; set; }
        public PfePoint Point_Other { get; set; }

        public PfePoint Get(DateTime date)
        {
            if (date < DateTime.Today.AddDays(3))
                return Point_0D;
            if (date < DateTime.Today.AddDays(7))
                return Point_3D;
            if (date < DateTime.Today.AddDays(14))
                return Point_1W;
            if (date < DateTime.Today.AddDays(21))
                return Point_2W;
            if (date < DateTime.Today.AddMonths(1))
                return Point_3W;
            if (date < DateTime.Today.AddMonths(2))
                return Point_1M;
            if (date < DateTime.Today.AddMonths(3))
                return Point_2M;
            if (date < DateTime.Today.AddYears(1))
                return Point_1Y;
            if (date < DateTime.Today.AddYears(2))
                return Point_2Y;
            if (date < DateTime.Today.AddYears(3))
                return Point_3Y;
            if (date < DateTime.Today.AddYears(4))
                return Point_4Y;
            return Point_Other;
        }
    }

    public class PfePoint
    {
        public PfePoint(string date)
        {
            Date = date;
        }
        public string Date { get; set; }
        public double Value { get; set; }

        public string FormattedValue
        {
            get { return Value.ToString("N0"); }
        }
    }
}