using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;

namespace DevCommander.Library
{
    public class VSCodeTool : Tool
    {

        public static void AddTools(ObservableCollection<Tool> tools, string toolExe)
        {
            toolExe = Environment.ExpandEnvironmentVariables(toolExe);
            if (File.Exists(toolExe))
            {
                VSCodeTool vsCodeTool = new VSCodeTool() { ToolPath = new FileInfo(toolExe) };
                var packagingDllPath = Path.Combine(vsCodeTool.GetNuGetPath(), "NuGet.Packaging.dll");
                if (File.Exists(packagingDllPath))
                {
                    vsCodeTool.NuGetFileVersion = FileVersionInfo.GetVersionInfo(packagingDllPath);
                }

                tools.Add(vsCodeTool);
            }
        }

        public override string GetNuGetPath()
        {
            var vsCodeExtensionsDir = Environment.ExpandEnvironmentVariables(@"%userprofile%\.vscode\extensions");
            foreach (var dir in Directory.EnumerateDirectories(vsCodeExtensionsDir, "ms-dotnettools.csharp-*"))
            {
                DirectoryInfo omnisharpDir = new DirectoryInfo(Path.Combine(dir, ".omnisharp"));
                foreach (var omnisharpVersion in omnisharpDir.EnumerateDirectories())
                {
                    // return first found
                    return omnisharpVersion.FullName;
                }
            }

            return null;
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
