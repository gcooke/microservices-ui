using System.Text;
using Bagl.Cib.MIT.Cube;
using Gateway.Web.Database;

using System.Text;

using System.Collections.Generic;
using System.Linq;

using System.Text;

namespace Gateway.Web.Models.Request
{
    public class CubeModel
    {
        public string Header { get; }
        private readonly ICube _cube;

        public CubeModel(PayloadData data)
        {
            _cube = CubeBuilder.FromBytes(data.GetCubeBytes());
            Errors = new Dictionary<string, string>();
            RenderAttributes();
            RenderRows();
        }

        public CubeModel(ICube cube, string header = "Data")
        {
            Header = header;
            _cube = cube;
            Errors = new Dictionary<string, string>();
            RenderAttributes();
            RenderRows();
        }

        public bool HasAttributes
        {
            get { return !string.IsNullOrEmpty(Attributes); }
        }

        public ICube Cube => _cube;
        public string Attributes { get; private set; }
        public string Rows { get; private set; }
        public int RowCount { get; private set; }

        public Dictionary<string, string> Errors { get; private set; }

        private void RenderAttributes()
        {
            var attributes = _cube.GetAttributes().ToArray();
            if (attributes.Length == 0) return;

            // Render attributes
            var builder = new StringBuilder();
            builder.AppendLine("<table class=\"datatable\" style='width:auto'>");
            builder.AppendLine("<thead>");
            builder.AppendLine("<tr>");
            builder.Append("<td>Attribute</td>");
            builder.Append("<td>Value</td>");
            builder.AppendLine("</tr>");
            builder.AppendLine("</thead>");

            foreach (var attribute in attributes)
            {
                builder.AppendLine("<tr>");
                builder.Append($"<td style='background: #B7F6BD;'>{attribute.Key}</td>");
                builder.Append($"<td>{attribute.Value}</td>");
                builder.AppendLine("</tr>");
            }
            builder.AppendLine("</table>");
            Attributes = builder.ToString();
        }

        private void RenderRows()
        {
            // Render rows
            int? errorColoumnIndex = null;
            var builder = new StringBuilder();
            int colIndex = 0;
            RowCount = _cube.Rows;
            builder.AppendLine("<table class=\"datatable\">");
            builder.AppendLine("<thead>");
            builder.AppendLine("<tr>");
            foreach (var column in _cube.ColumnDefinitions)
            {
                if (column.Name == "ErrorDescription")
                    errorColoumnIndex = colIndex;
                builder.Append($"<td>{column.Name}</td>");
                colIndex++;
            }
            builder.AppendLine("</tr>");
            builder.AppendLine("</thead>");

            foreach (var row in _cube.GetRows())
            {
                builder.AppendLine("<tr>");
                for (int index = 0; index < _cube.Columns; index++)
                {
                    builder.Append($"<td>{row[index]}</td>");
                    if (errorColoumnIndex.HasValue && errorColoumnIndex == index && !string.IsNullOrEmpty(row[index]?.ToString()) && !Errors.ContainsKey(row[index].ToString()))
                        Errors.Add(row[index].ToString(), row[index].ToString());
                }
                builder.AppendLine("</tr>");
            }

            builder.AppendLine("</table>");
            Rows = builder.ToString();
        }
    }
}