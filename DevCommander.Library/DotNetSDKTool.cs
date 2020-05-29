using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace DevCommander.Library
{
    public class DotNetSDKTool : Tool
    {
        public static void AddTools(ObservableCollection<Tool> tools, string basePath, string name)
        {
            basePath = Environment.ExpandEnvironmentVariables(basePath);
            foreach (var dir in Directory.EnumerateDirectories(basePath))
            {
                var dirInfo = new DirectoryInfo(dir);
                if (File.Exists(Path.Combine(dir, "dotnet.dll")))
                {
                    var dotnetsdk = new DotNetSDKTool()
                    {
                        RootPath = new DirectoryInfo(dir),
                        ToolPath = new FileInfo(dirInfo.Parent.FullName)  // "sdk" -- so each sdk is labeled.
                    };

                    var packagingDllPath = Path.Combine(dotnetsdk.GetNuGetPath(), "NuGet.Packaging.dll");
                    if (File.Exists(packagingDllPath))
                    {
                        dotnetsdk.NuGetFileVersion = FileVersionInfo.GetVersionInfo(packagingDllPath);
                    }

                    tools.Add(dotnetsdk);
                }
            }
        }

        public override string GetNuGetPath()
        {
            return RootPath.FullName;
        }

        public string VSIXInstallerPath { get { return Path.Combine(RootPath.FullName, @"common7\ide\vsixinstaller.exe"); } }

        public override bool HasCustomExec
        {
            get
            {
                return true;
            }
        }

        public override string Exec(IEnumerable<Repo> repos)
        {
            if (repos.Count() == 1)
            {
                var repo = repos.First();
                var binSourceDir = Path.Combine(repo.RootPath.FullName, @"artifacts\NuGet.CommandLine.XPlat\16.0\bin\Debug\netcoreapp2.1");
                var binSourceDirInfo = new DirectoryInfo(binSourceDir);
                
                var packDllDir = Path.Combine(repo.RootPath.FullName, @"artifacts\NuGet.Build.Tasks.Pack\16.0\bin\Debug\netstandard2.0\ilmerge\");
                var packSdkDestDir = Path.Combine(RootPath.FullName, @"Sdks\NuGet.Build.Tasks.Pack\CoreCLR\");

                try
                {
                    CopyFiles("*.dll", binSourceDir, RootPath.FullName);
                    CopyFiles("NuGet.Build.Tasks.Pack.dll", packDllDir, packSdkDestDir);
                }
                catch (Exception)
                {
                    //TODO: needs elevation
                    MessageBox.Show("Command needs admin rights");
                }
            }
            else
            {
                throw new NotImplementedException("not expecting more than 1 repo");
            }

            //TODO: improve message
            return "patched sdk";
        }

        private void CopyFiles(string filePattern, string sourceDir, string destinationDirectory)
        {
            var sourceDirInfo = new DirectoryInfo(sourceDir);
            foreach (var file in sourceDirInfo.GetFiles(filePattern))
            {
                File.Copy(Path.Combine(sourceDir, file.Name), Path.Combine(destinationDirectory, file.Name), true);
            }
        }

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
    }
}
