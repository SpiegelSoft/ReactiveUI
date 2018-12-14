// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EventBuilder.NuGet;
using Polly;
using Serilog;

namespace EventBuilder.Platforms
{
    /// <summary>
    /// Tizen platform assemblies and events.
    /// </summary>
    /// <seealso cref="EventBuilder.Platforms.BasePlatform" />
    public class Tizen : BasePlatform
    {
        private const string _packageName = "Tizen.NET";
        private const string _packageVersion = "5.0.0.14562";

        /// <inheritdoc />
        public override AutoPlatform Platform => AutoPlatform.Tizen;

        /// <inheritdoc />
        public async override Task Extract()
        {
            var packageUnzipPath = Environment.CurrentDirectory;

            await NuGetPackageHelper.InstallPackage(_packageName, packageUnzipPath, _packageVersion).ConfigureAwait(false);

            Log.Debug($"Package unzip path is {packageUnzipPath}");

            var elmSharp = Directory.GetFiles(packageUnzipPath, "ElmSharp*.dll", SearchOption.AllDirectories);
            Assemblies.AddRange(elmSharp);

            var tizenNet = Directory.GetFiles(packageUnzipPath, "Tizen*.dll", SearchOption.AllDirectories);
            Assemblies.AddRange(tizenNet);

            CecilSearchDirectories.Add($"{packageUnzipPath}\\Tizen.NET.{_packageVersion}\\build\\tizen40\\ref");
            CecilSearchDirectories.Add($"{packageUnzipPath}\\Tizen.NET.{_packageVersion}\\lib\\netstandard2.0");
        }
    }
}
