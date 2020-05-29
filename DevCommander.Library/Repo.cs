using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace DevCommander.Library
{
    public class Repo : INotifyPropertyChanged
    {
        private DirectoryInfo rootPath;
        public DirectoryInfo RootPath
        {
            get
            {
                return rootPath;
            }
            set
            {
                if (value != rootPath)
                {
                    rootPath = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;


        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public static void AddRepos(ObservableCollection<Repo> repos, string basePath)
        {
            if (Directory.Exists(basePath))
            {
                foreach (var dir in Directory.EnumerateDirectories(basePath))
                {
                    if (Directory.Exists(Path.Combine(dir, ".GIT")))
                    {
                        var repo = new Repo() { RootPath = new DirectoryInfo(dir) };
                        repos.Add(repo);
                        repo.ProcessConfig();
                    }
                }
            }
            else
            {
                throw new Exception("settings.json file configured a repoRoot that doesn't exist: " + basePath);
            }
        }

        public void ProcessConfig()
        {
            var allLines = File.ReadAllLines(GetGitConfig());
            bool inRemoteOriginSection = false;
            string branchName = null;
            foreach (var line in allLines)
            {
                if (line.StartsWith("["))
                {
                    inRemoteOriginSection = false;
                    if (line == "[remote \"origin\"]")
                    {
                        inRemoteOriginSection = true;
                    }
                    
                    var prefix = "[branch \"";
                    if (line.StartsWith(prefix))
                    {
                        var closingQuote = line.IndexOf('"', prefix.Length, line.Length - prefix.Length);
                        branchName = line.Substring(prefix.Length, closingQuote - prefix.Length);
                        Branches = Branches == null ? branchName : branchName + "; " + Branches;
                    }
                }
                else
                {
                    var tLine = line.Trim();
                    var chunks = tLine.Split('=');
                    if (inRemoteOriginSection && chunks[0] == "url ")
                    {
                        this.RemoteUrl = chunks[1].Trim();
                    }
                }
            }
        }

        public string GetGitConfig()
        {
            var gitConfigPath = Path.Combine(RootPath.FullName, @".git\config");
            return gitConfigPath;
        }

        public string RemoteUrl { get; set; }
        public string Branches { get; set; }
    }
}
