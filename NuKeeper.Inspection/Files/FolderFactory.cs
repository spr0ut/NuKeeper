using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using NuKeeper.Abstractions.Inspections.Files;
using NuKeeper.Abstractions.Logging;

namespace NuKeeper.Inspection.Files
{
    public class FolderFactory : IFolderFactory
    {
        private readonly INuKeeperLogger _logger;
        private const string FolderPrefix = "repo-";

        public FolderFactory(INuKeeperLogger logger)
        {
            _logger = logger;
        }

        public IFolder UniqueTemporaryFolder()
        {
            var tempDir = new DirectoryInfo(GetUniqueTemporaryPath());
            tempDir.Create();
            return new Folder(_logger, tempDir);
        }

        public static string NuKeeperTempFilesPath()
        {
            return Path.Combine(Path.GetTempPath(), "NuKeeper");
        }

        private static string GetUniqueTemporaryPath()
        {
            var uniqueName = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
            return Path.Combine(NuKeeperTempFilesPath(), $"{FolderPrefix}{uniqueName}");
        }

        /// <summary>
        /// Select folders to cleanup at startup
        /// </summary>
        /// <param name="nukeeperTemp">NuKeepers temp folder</param>
        /// <returns></returns>
        public static IEnumerable<DirectoryInfo> GetTempDirsToCleanup(DirectoryInfo nukeeperTemp)
        {
            var dirs = nukeeperTemp.Exists ? nukeeperTemp.EnumerateDirectories() : Enumerable.Empty<DirectoryInfo>();
            var filterDatetime = DateTime.Now.AddHours(-1);
            return dirs.Where(d =>
                d.Name.StartsWith(FolderPrefix, StringComparison.InvariantCultureIgnoreCase) &&
                d.LastWriteTime < filterDatetime);
        }

        /// <summary>
        /// Cleanup folders that are not automatically have been cleaned.
        /// </summary>
        public void DeleteExistingTempDirs()
        {
            var dirInfo = new DirectoryInfo(NuKeeperTempFilesPath());
            foreach (var dir in GetTempDirsToCleanup(dirInfo))
            {
                var folder = new Folder(_logger, dir);
                folder.TryDelete();
            }
        }
    }
}
