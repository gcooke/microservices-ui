using System;
using System.Collections.Generic;

namespace Gateway.Web.Utils
{
    public static class ExceptionEx
    {
        public static IEnumerable<Exception> EnumerateExceptions(this Exception ex)
        {
            var item = ex;
            while (item != null)
            {
                yield return item;
                item = item.InnerException;
            }
        }
    }
}