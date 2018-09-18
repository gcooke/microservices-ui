using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Gateway.Web.Database;
using Gateway.Web.Models.Schedule.Input;

namespace Gateway.Web.Services.Schedule.Models
{
    public abstract class BaseScheduleParameters
    {
        public IList<Database.Schedule> Children { get; set; }

        public ScheduleGroup Group { get; set; }

        public Database.Schedule Parent { get; set; }

        public bool ModifyChildren { get; set; }

        public bool ModifyParent { get; set; }

        public bool IsAsync { get; set; }

        public long ScheduleId { get; set; }

        public virtual void Populate(GatewayEntities db, BaseScheduleModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.Group))
            {
                var groupId = long.Parse(model.Group);
                Group = db.ScheduleGroups
                    .SingleOrDefault(x => x.GroupId == groupId);

                if (Group == null)
                    throw new Exception($"Unable to find group with ID {model.Group}");
            }

            if (!string.IsNullOrWhiteSpace(model.Parent))
            {
                var parentId = long.Parse(model.Parent);
                Parent = db.Schedules
                    .Include("RiskBatchConfiguration")
                    .Include("RequestConfiguration")
                    .Include("ParentSchedule")
                    .Include("Children")
                    .SingleOrDefault(x => x.ScheduleId == parentId);

                if (Parent == null)
                    throw new Exception($"Unable to find parent with ID {model.Parent}");
            }

            if (model.Children != null)
            {
                Children = db.Schedules
                    .Include("RiskBatchConfiguration")
                    .Include("RequestConfiguration")
                    .Include("ParentSchedule")
                    .Include("Children")
                    .Where(x => model.Children.Contains(x.ScheduleId.ToString()))
                    .ToList();
            }

            ModifyChildren = !model.BulkUpdate;
            ModifyParent = !model.BulkUpdate;
            ScheduleId = model.ScheduleId;
            IsAsync = true;
        }

        public virtual ModelErrorCollection Validate(Database.Schedule model)
        {
            var errors = new ModelErrorCollection();

            if (Parent != null && Parent.ScheduleId == model.ScheduleId)
            {
                errors.Add("Parent cannot be the same as current batch schedule");
            }

            if (Children != null && Children.Any(x => x.ScheduleId == Parent?.ScheduleId))
            {
                errors.Add("Parent cannot also be a child batch schedule");
            }

            if (Children != null && Children.Any(x => x.ScheduleId == model.ScheduleId))
            {
                errors.Add("Children cannot contain current batch schedule");
            }

            return errors;
        }

    }
}