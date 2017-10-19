using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Bagl.Cib.MIT.IO;
using Bagl.Cib.MIT.Logging;

namespace Gateway.Web.Services
{
    public class DifferentialDownloadService : IDifferentialDownloadService
    {
        private const string DifferentialDirectory = @"D:\Services\Redstone\Differentials";
        private const string RemoteAppsDirectory = @"\\Intranet.barcapint.com\dfs-emea\Group\Jhb\IT_Pricing_Risk\Builds\Redstone\Apps";

        private readonly IFileService _fileService;
        private readonly IDifferentialArchiveService _archiveService;
        private readonly ILogger _logger;
        private readonly object _synchLock = new object();

        public DifferentialDownloadService(ILoggingService loggingService,
                                           IFileService fileService,
                                           IDifferentialArchiveService archiveService)
        {
            _fileService = fileService;
            _archiveService = archiveService;
            _logger = loggingService.GetLogger(this);
        }

        public Differential Get(string app, string @from, string to)
        {
            lock (_synchLock)
            {
                var path = GetDifferentialPath(app, from, to);
                var result = new Differential(app, from, to, path);

                // If differential already exists then return.
                if (File.Exists(path))
                {
                    result.IsValid = true;
                    return result;
                }

                // Create differential and return result
                try
                {
                    var stopwatch = Stopwatch.StartNew();
                    CreateDifferential(result);
                    _logger.InfoFormat("Created differential in {3}ms ({0} from {1} to {2})", result.App, result.From, result.To, stopwatch.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    _logger.Error("Failed to create differential: " + ex.Message);
                }
                return result;
            }
        }

        private void CreateDifferential(Differential result)
        {
            if(!DifferentialDirectoryExists())
                throw new InvalidOperationException("Differential directory does not exist: " + DifferentialDirectory);

            _logger.InfoFormat("Creating differential of {0} from {1} to {2}", result.App, result.From, result.To);

            var fromZip = GetRemoteAppVersionPath(result.App, result.From);
            var toZip = GetRemoteAppVersionPath(result.App, result.To);

            var tempPath = Path.GetTempPath();
            var targetBaselinePath = Path.Combine(tempPath, "Baseline");
            var targetSourcePath = Path.Combine(tempPath, "Source");
            var tempFile = Path.Combine(tempPath, result.Name);
            DeleteDirectory(targetBaselinePath);
            DeleteDirectory(targetSourcePath);

            try
            {
                // Unpack files
                _fileService.ExtractToDirectory(fromZip, targetBaselinePath);
                _fileService.ExtractToDirectory(toZip, targetSourcePath);

                // If differential already exists then return before creating archive.
                if (File.Exists(result.FullName))
                {
                    result.IsValid = true;
                    return;
                }

                // Create differential locally
                _archiveService.CreateArchive(targetBaselinePath, targetSourcePath, tempFile);

                // Copy differential to final location
                if (!File.Exists(result.FullName))
                {
                    _fileService.EnsureDirectoryExists(Path.GetDirectoryName(result.FullName));
                    _fileService.CopyFile(tempFile, result.FullName);
                }

                Thread.Sleep(1000);
                result.IsValid = true;
            }
            finally
            {
                DeleteDirectory(targetBaselinePath);
                DeleteDirectory(targetSourcePath);
            }
        }

        private void DeleteDirectory(string path)
        {
            if (_fileService.DirectoryExists(path))
                _fileService.DeleteDirectoryAndContents(path);
        }

        private void DeleteFile(string path)
        {
            if (_fileService.FileExists(path))
                _fileService.DeleteFile(path);
        }

        private bool DifferentialDirectoryExists()
        {
            return Directory.Exists(DifferentialDirectory);
        }

        private string GetDifferentialPath(string app, string @from, string to)
        {
            var path = string.Format("{0}\\{1}\\{2}\\", DifferentialDirectory, app, to);
            return Path.Combine(path, string.Format("Differential_{0}_to_{1}.zip", from, to));
        }

        private string GetRemoteAppVersionPath(string app, string version)
        {
            // Determine if path is valid.
            var path = string.Format("{0}\\{1}\\{2}\\", RemoteAppsDirectory, app, version);

            // Find file to download.
            var files = Directory.GetFiles(path);

            // If there is more than one file in the target folder then take the largest?
            if (files.Length > 1)
            {
                path = files.Select(f => new FileInfo(f))
                    .OrderBy(f => f.Length)
                    .Select(f => f.FullName)
                    .LastOrDefault();
            }
            else
            {
                path = files[0];
            }

            return path;
        }
    }
}