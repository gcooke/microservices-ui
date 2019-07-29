using Gateway.Web.Models.Export;
using System;
using System.Collections.Generic;

namespace Gateway.Web.Services
{
    public interface IExportService
    {
        IList<ExportCRONGroup> FetchExports(DateTime date);

        FileExport FetchExport(long id);

        ExportSchedule CreateExport(ExportSchedule insert);

        void UpdateExport(ExportSchedule update);

        ExportResponse RunExport(long id, DateTime time, bool force);

        ExportResponse RunScheduleExport(DateTime time);
    }
}