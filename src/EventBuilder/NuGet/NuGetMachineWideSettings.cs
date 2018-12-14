using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Configuration;

namespace EventBuilder.NuGet
{
    internal class NuGetMachineWideSettings : IMachineWideSettings
    {
        private readonly Lazy<ISettings> _settings;

        public NuGetMachineWideSettings()
        {
            string baseDirectory = NuGetEnvironment.GetFolderPath(NuGetFolderPath.MachineWideConfigDirectory);
            _settings = new Lazy<ISettings>(
                () => global::NuGet.Configuration.Settings.LoadMachineWideSettings(baseDirectory));
        }

        public ISettings Settings => _settings.Value;
    }
}
