using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DevCommander.Library
{
    public class WebTool : Tool
    {
        public static void AddTools(ObservableCollection<Tool> tools, string toolUriString, string toolName, string browserExe, string argument = null)
        {
            if (toolUriString.StartsWith("http"))
            {
                Uri toolUri;
                var ok = Uri.TryCreate(toolUriString, UriKind.Absolute, out toolUri);
                if (ok)
                {
                    var tool = new WebTool() { ToolPage = toolUri, Name = toolName, BrowserExe = browserExe };
                    tools.Add(tool);
                }
            }
        }

        public string BrowserExe { get; set; }
        public Uri ToolPage { get; set; }

        public override string GetNuGetPath()
        {
            return null;
        }

        public override bool HasCustomExec
        {
            get
            {
                return true;
            }
        }

        public override string Exec(IEnumerable<Repo> repos)
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
                FileName = Environment.ExpandEnvironmentVariables(BrowserExe),
                Arguments = tool.ToolPage.AbsoluteUri,
                WindowStyle = ProcessWindowStyle.Maximized
            };
            var p = Process.Start(psi);
            var consoleText = (p.Id + " \"" + psi.FileName + "\" " + psi.Arguments);
            return consoleText;
        }
    }
}