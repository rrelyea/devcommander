using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;

namespace DevCommander.Library
{
    public class VSTool : Tool
    {
        public static void AddTools(ObservableCollection<Tool> tools, string basePath)
        {
            basePath = Environment.ExpandEnvironmentVariables(basePath);
            foreach (var dir in Directory.EnumerateDirectories(basePath))
            {
                var vsInstall = new VSTool() { RootPath = new DirectoryInfo(dir) };
                
                var devEnvPath = Path.Combine(vsInstall.RootPath.FullName, @"common7\ide\devenv.exe");
                if (File.Exists(devEnvPath))
                {
                    vsInstall.ToolPath = new FileInfo(devEnvPath);
                }

                var packagingDllPath = Path.Combine(vsInstall.GetNuGetPath(), "NuGet.Packaging.dll");
                if (File.Exists(packagingDllPath))
                {
                    vsInstall.NuGetFileVersion = FileVersionInfo.GetVersionInfo(packagingDllPath);
                }

                tools.Add(vsInstall);
            }
        }

        public override string GetNuGetPath()
        {
            return Path.Combine(RootPath.FullName, @"Common7\IDE\CommonExtensions\Microsoft\NuGet");
        }

        public string VSIXInstallerPath { get { return Path.Combine(RootPath.FullName, @"common7\ide\vsixinstaller.exe"); } }

        public void DownGrade()
        {
            ProcessStartInfo psi = new ProcessStartInfo()
            {
                FileName = VSIXInstallerPath,
                Arguments = "/downgrade:NuGet.72c5d240-f742-48d4-a0f1-7016671e405b"
                //TODO: can direct per sku?? ----   + " /appIdInstallPath:\""+BasePath.FullName+"\" /appName:VS /skuName:Enterprise /skuVersion:16.5.0"
            };
            Process.Start(psi);
        }

        public override bool HasCustomExec
        {
            get
            {
                return false;
            }
        }

        public override string Exec(IEnumerable<Repo> repos)
        {
            throw new NotImplementedException();
        }
    }
}
