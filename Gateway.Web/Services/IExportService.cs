using Gateway.Web.Models.Export;
using System;
using System.Collections.Generic;

namespace Gateway.Web.Services
{
    public interface IExportService
    {
        IList<ExportCRONGroup> FetchExports(DateTime date);

        ExportUpdate CreateExport(ExportUpdate insert);

        void UpdateExport(ExportUpdate update);
    }
}