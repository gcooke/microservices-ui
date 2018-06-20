using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.HtmlControls;
using Bagl.Cib.MIT.Cube;
using Gateway.Web.Database;

namespace Gateway.Web.Models.Request
{
    public class CubeModel
    {
        private readonly ICube _cube;

        public CubeModel(PayloadData data)
        {
            _cube = CubeBuilder.FromBytes(data.GetCubeBytes());
            RenderAttributes();
            RenderRows();
        }

        public bool HasAttributes
        {
            get { return !string.IsNullOrEmpty(Attributes); }
        }
        public string Attributes { get; private set; }
        public string Rows { get; private set; }

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
            var builder = new StringBuilder();
            builder.AppendLine("<table class=\"datatable\">");
            builder.AppendLine("<thead>");
            builder.AppendLine("<tr>");
            foreach (var column in _cube.ColumnDefinitions)
            {
                builder.Append($"<td>{column.Name}</td>");
            }
            builder.AppendLine("</tr>");
            builder.AppendLine("</thead>");

            foreach (var row in _cube.GetRows())
            {
                builder.AppendLine("<tr>");
                for (int index = 0; index < _cube.Columns; index++)
                {
                    builder.Append($"<td>{row[index]}</td>");
                }
                builder.AppendLine("</tr>");
            }

            builder.AppendLine("</table>");
            Rows = builder.ToString();
        }

    }
}