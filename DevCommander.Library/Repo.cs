using System;
using System.Collections.Generic;
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

        public static void AddRepos(ObservableCollection<string> orgUrls, Dictionary<string, ObservableCollection<Repo>> localRepoLookup, string basePath)
        {
            if (Directory.Exists(basePath))
            {
                foreach (var dir in Directory.EnumerateDirectories(basePath))
                {
                    if (Directory.Exists(Path.Combine(dir, ".GIT")))
                    {
                        var repo = new Repo() { RootPath = new DirectoryInfo(dir) };
                        repo.ProcessConfig();
                        string repoUrl = repo.RemoteUrl != null ? repo.RemoteUrl : repo.RootPath.Name;
                        string orgUri = null;
                        if (repo.RemoteUrl != null)
                        {
                            Uri remoteUri = new Uri(repo.RemoteUrl);
                            var segments = remoteUri.Segments;
                            orgUri = (remoteUri.Host + segments[0] + segments[1]).ToLowerInvariant();
                            repo.RepoName = segments[segments.Length - 1];

                            //remove any ".git" at end of
                            if (repo.RepoName.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
                            {
                                repo.RepoName = repo.RepoName.Substring(0, repo.RepoName.Length - 4);
                            }
                        }
                        else
                        {
                            orgUri = repoUrl.ToLowerInvariant();
                            repo.RepoName = orgUri;
                        }

                        repo.OrgUrl = orgUri;

                        if (!localRepoLookup.ContainsKey(repo.OrgUrl))
                        {
                            orgUrls.Add(repo.OrgUrl);
                            localRepoLookup.Add(repo.OrgUrl, new ObservableCollection<Repo>());
                        }

                        localRepoLookup[repo.OrgUrl].Add(repo);
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
            var gitConfigPath = GetGitFilePath("config");
            if (gitConfigPath != null)
            {
                var allLines = File.ReadAllLines(gitConfigPath);
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

            var gitIssueUrl = GetGitFilePath("LinkToIssue");
            if (gitIssueUrl != null)
            {
                IssueUrl = File.ReadAllText(gitIssueUrl).Trim();
            }

            var gitPRUrl = GetGitFilePath("LinkToPR");
            if (gitPRUrl != null)
            {
                PRUrl = File.ReadAllText(gitPRUrl).Trim();
            }
        }

        public string GetGitFilePath(string fileName)
        {
            var gitFilePath = Path.Combine(GetGitDirectoryPath(), fileName);
            if (!File.Exists(gitFilePath))
            {
                gitFilePath = null;
            }

            return gitFilePath;
        }

        public string GetGitDirectoryPath()
        {
            var gitConfigDirectoryPath = Path.Combine(RootPath.FullName, @".git\");
            if (!Directory.Exists(gitConfigDirectoryPath))
            {
                gitConfigDirectoryPath = null;
            }

            return gitConfigDirectoryPath;
        }

        public string OrgUrl { get; set; }
        public string RemoteUrl { get; set; }

        public string RepoName { get; set; }
        public string Branches { get; set; }
        public string PRUrl { get; set; }
        public string IssueUrl { get; set; }
    }
}
