using System.Linq;
using System.Web.Mvc;
using Gateway.Web.Models.Schedule.Input;
using Gateway.Web.Services.Batches.Interfaces;
using Gateway.Web.Utils;

namespace Gateway.Web.Services.Schedule.Utils
{
    public static class BaseScheduleModelEx
    {
        public static void SetGroups(this BaseScheduleModel model, IBatchConfigDataService service, string[] selectedValues)
        {
            model.Groups = service
                .GetGroups()
                .Select(x => new SelectListItem
                {
                    Value = x.GroupId.ToString(),
                    Text = CronExpressionDescriptor.ExpressionDescriptor.GetDescription(CronTabExpression.Parse(x.Schedule).FromUtcCronExpression())
                })
                .ToList();

            model.Groups.Insert(0, new SelectListItem { Text = null, Value = null });
            var selectListItems = model.Groups.Where(x => selectedValues.Contains(x.Value)).ToList();
            selectListItems.ForEach(x => x.Selected = true);
        }

        public static void SetParents(this BaseScheduleModel model, IBatchConfigDataService service, string[] selectedValues)
        {
            model.Parents = service
                .GetSchedules()
                .Select(x => new SelectListItem { Value = x.ScheduleId.ToString(), Text = $"{x.Name}" })
                .ToList();

            model.Parents.Insert(0, new SelectListItem { Text = null, Value = null });
            var selectListItems = model.Parents.Where(x => selectedValues.Contains(x.Value)).ToList();
            selectListItems.ForEach(x => x.Selected = true);
        }

        public static void SetChildren(this BaseScheduleModel model, IBatchConfigDataService service, string[] selectedValues)
        {
            model.ChildSchedules = service
                .GetSchedules()
                .Select(x => new SelectListItem { Value = x.ScheduleId.ToString(), Text = $"{x.Name}" })
                .ToList();

            var selectListItems = model.ChildSchedules.Where(x => selectedValues.Contains(x.Value)).ToList();
            selectListItems.ForEach(x => x.Selected = true);
        }
    }
}