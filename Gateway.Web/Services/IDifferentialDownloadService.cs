namespace Gateway.Web.Services
{
    public interface IDifferentialDownloadService
    {
        Differential Get(string app, string from, string to);
    }

    public class Differential
    {
        public Differential(string app, string from, string to, string fullPath)
        {
            App = app;
            From = from;
            To = to;
            FullName = fullPath;
            Name = System.IO.Path.GetFileName(fullPath);
        }

        public string App { get; private set; }
        public string From { get; private set; }
        public string To { get; private set; }
        public string FullName { get; private set; }
        public string Name { get; private set; }

        public bool IsValid { get; set; }
    }
}