using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EventBuilder.NuGet;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.PackageManagement;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Resolver;
using NuGet.Versioning;
using Polly;
using Serilog;

namespace EventBuilder.NuGet
{
    /// <summary>
    /// A helper class for handling NuGet packages.
    /// </summary>
    public static class NuGetPackageHelper
    {
        /// <summary>
        /// Installs a nuget package into the specified directory.
        /// </summary>
        /// <param name="packageName">The name of the package to find.</param>
        /// <param name="packageRoot">The directory where to root folder will be.</param>
        /// <param name="versionId">The version of the package to find.</param>
        /// <returns>A task to monitor the progress.</returns>
        public static async Task InstallPackage(string packageName, string packageRoot, string versionId)
        {
            var packagesPath = Path.Combine(packageRoot, "packages");
            var settings = Settings.LoadDefaultSettings(packageRoot, null, new NuGetMachineWideSettings());
            var sourceRepositoryProvider = new SourceRepositoryProvider(settings);
            var folder = new FolderNuGetProject(packageRoot);
            var packageManager = new NuGetPackageManager(sourceRepositoryProvider, settings, packagesPath)
            {
                PackagesFolderNuGetProject = folder
            };

            var resolutionContext = new ResolutionContext(
                DependencyBehavior.Lowest,
                includePrelease: false,
                includeUnlisted: false,
                VersionConstraints.None);
            var projectContext = new NuGetProjectContext(settings);

            var packageIdentity = new PackageIdentity(packageName, new NuGetVersion(versionId));

            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    5,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, _, __) =>
                    {
                        Log.Warning(
                            "An exception was thrown whilst retrieving or installing {0}: {1}",
                            packageName,
                            exception);
                    });

            await retryPolicy.ExecuteAsync(async () =>
            {
                await packageManager.InstallPackageAsync(
                    packageManager.PackagesFolderNuGetProject,
                    packageIdentity,
                    resolutionContext,
                    projectContext,
                    sourceRepositoryProvider.GetDefaultRepositories(),
                    Array.Empty<SourceRepository>(),
                    CancellationToken.None).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }
    }
}
