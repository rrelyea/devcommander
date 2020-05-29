using System;
using System.Collections.Generic;

namespace DevCommander
{
    public class DevCommanderSettings
    {
        public List<string> RepoRoots
        {
            get
            {
                if (_RepoRoots == null)
                {
                    _RepoRoots = new List<string>();
                }
                return _RepoRoots;
            }
            // (temporary?) workaround for: https://github.com/dotnet/runtime/issues/30258 (discussion here https://twitter.com/rrelyea/status/1266132779764379654?s=20)
            set
            {
                _RepoRoots = value;
            }
        }
        private List<string> _RepoRoots;

        public List<string> ToolExes
        {
            get
            {
                if (_ToolExes == null)
                {
                    _ToolExes = new List<string>();
                }
                return _ToolExes;
            }
            // (temporary?) workaround for: https://github.com/dotnet/runtime/issues/30258 (discussion here https://twitter.com/rrelyea/status/1266132779764379654?s=20)
            set
            {
                _ToolExes = value;
            }
        }
        private List<string> _ToolExes;


        public List<string> WebUrls
        {
            get
            {
                if (_WebUrls == null)
                {
                    _WebUrls = new List<string>();
                }
                return _WebUrls;
            }
            // (temporary?) workaround for: https://github.com/dotnet/runtime/issues/30258 (discussion here https://twitter.com/rrelyea/status/1266132779764379654?s=20)
            set
            {
                _WebUrls = value;
            }
        }
        private List<string> _WebUrls;


        public string VSRoot { get; set; } = Environment.ExpandEnvironmentVariables(@"%ProgramFiles(x86)%\Microsoft Visual Studio\2019");
        public string VSCodeExe { get; set; } = Environment.ExpandEnvironmentVariables(@"%userprofile%\AppData\Local\Programs\Microsoft VS Code\Code.exe");
        public string WindowsTerminalExe { get; set; } = Environment.ExpandEnvironmentVariables(@"%userprofile%\AppData\Local\Microsoft\WindowsApps\wt.exe");
        public string ExplorerExe { get; set; } = Environment.ExpandEnvironmentVariables(@"%windir%\explorer.exe");
        public string DotnetSDKRoot { get; set; } = Environment.ExpandEnvironmentVariables(@"%programfiles%\dotnet\sdk");
        public string BrowserExe { get; set; } = Environment.ExpandEnvironmentVariables(@"%ProgramFiles(x86)%\Microsoft\Edge Dev\Application\msedge.exe");
    }
}