using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace DevCommander.Library
{
    public class NuGetFreeTool : Tool
    {
        public static void AddTools(ObservableCollection<Tool> tools, string toolExeOrUri, string argument = null)
        {
            toolExeOrUri = Environment.ExpandEnvironmentVariables(toolExeOrUri);
            if (File.Exists(toolExeOrUri))
            {
                var tool = new NuGetFreeTool() { ToolPath = new FileInfo(toolExeOrUri), Arguments = argument };
                tools.Add(tool);
            }
        }

        public override string GetNuGetPath()
        {
            return null;
        }

        public override bool HasCustomExec
        {
            get
            {
                if (this.ToolPath != null && this.ToolPath.Name.EndsWith("bcompare.exe", StringComparison.OrdinalIgnoreCase)
                    || this.ToolPath.Name.EndsWith("wt.exe", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public override string Exec(IEnumerable<Repo> repos)
        {
            if (this.ToolPath.Name.EndsWith("BCompare.exe", StringComparison.OrdinalIgnoreCase))
            {
                var tool = this;

                string args = null;
                foreach (var repoObj in repos)
                {
                    var repo = repoObj as Repo;
                    if (tool != null && repo != null)
                    {
                        args += args == null ? "\"" + repo.RootPath.FullName + "\"" : " \"" + repo.RootPath.FullName + "\"";
                    }
                }

                var psi = new ProcessStartInfo()
                {
                    FileName = tool.ToolPath.FullName,
                    Arguments = (tool.Arguments == null ? "" : tool.Arguments + " ") + args,
                    WorkingDirectory = repos.Last<Repo>().RootPath.FullName,
                    WindowStyle = ProcessWindowStyle.Maximized
                };
                var p = Process.Start(psi);
                var processId = p.Id;
                var consoleText = (p.Id + " \"" + psi.FileName + "\" " + psi.Arguments);
                return consoleText;
            }
            else if (this.ToolPath.Name.EndsWith("wt.exe", StringComparison.OrdinalIgnoreCase))
            {
                var tool = this;

                string args = null;
                foreach (var repoObj in repos)
                {
                    var repo = repoObj as Repo;
                    if (tool != null && repo != null)
                    {
                        args += args == null ? "-d \"" + repo.RootPath.FullName + "\"" : " -d \"" + repo.RootPath.FullName + "\"";
                    }
                }

                var psi = new ProcessStartInfo()
                {
                    FileName = tool.ToolPath.FullName,
                    Arguments = (tool.Arguments == null ? "" : tool.Arguments + " ") + args,
                    WorkingDirectory = repos.Last<Repo>().RootPath.FullName,
                    WindowStyle = ProcessWindowStyle.Maximized
                };
                var p = Process.Start(psi);
                var processId = p.Id;
                var consoleText = (p.Id + " \"" + psi.FileName + "\" " + psi.Arguments);
                return consoleText;
            }
            return null;
        }
    }
}
