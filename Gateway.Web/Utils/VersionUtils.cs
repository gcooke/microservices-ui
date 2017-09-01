using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Gateway.Web.Utils
{
    public static class VersionUtils
    {
        private static string _version;

        public static string CurrentVersion
        {
            get
            {
                if (string.IsNullOrEmpty(_version))
                {
                    try
                    {
                        var assembly = Assembly.GetAssembly(typeof(VersionUtils));
                        var assemblyDirectory = assembly.CodeBase;
                        assemblyDirectory = assemblyDirectory.Replace(@"file:///", "");
                        var path = assemblyDirectory;
                        var info = FileVersionInfo.GetVersionInfo(path);
                        _version = info.ProductVersion;
                    }
                    catch (Exception ex)
                    {
                        _version = ex.Message;
                    }
                }
                return _version;
            }
        }
    }
}