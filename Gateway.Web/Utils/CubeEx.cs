using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Bagl.Cib.MIT.Cube;

namespace Gateway.Web.Utils
{
    public static class CubeEx
    {
        public static ICube CloneStructure(this ICube cube)
        {
            var builder = new CubeBuilder();
            foreach (var definition in cube.ColumnDefinitions)
                builder.AddColumn(definition.Name, definition.ValueType);
            return builder.Build();
        }
    }
}