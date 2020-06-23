using DevCommander.Library;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;

namespace DevCommander
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            this.KeyDown += MainWindow_KeyDown;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Init();
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                Init();
            }
            else if (e.Key == Key.OemPlus || e.Key == Key.Add)
            {
                string seletedOrg = Orgs.SelectedItem as string;
                string selectedRepoName = RepoNames.SelectedItem as string;
                Repo selectedRepo = Repos.SelectedItem as Repo;
                var cloneRepoWindow = new CloneRepoWindow(seletedOrg, selectedRepoName, selectedRepo, Settings);
                cloneRepoWindow.ShowDialog();
            }
            else if (e.Key == Key.Delete && Repos.SelectedItem != null)
            {
                var selectedRepo = Repos.SelectedItem as Repo;

                bool useRecycleBin = true;
                string warningMessage = null;
                if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                {
                    useRecycleBin = false;
                    warningMessage = "CAUTION -- permanently deleting this repo!!!";
                }
                else
                {
                    warningMessage = "CAUTION -- moving this local repo to the recylce bin";
                }

                if (selectedRepo != null && RepoNames.SelectedItems.Count > 0)
                {
                    var selectedRepoName = RepoNames.SelectedItems[0] as string;
                    var result = MessageBox.Show(warningMessage, "CAREFUL - DELETING LOCAL REPO", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.OK)
                    {
                        try
                        {
                            FileSystem.DeleteDirectory(
                                selectedRepo.RootPath.FullName,
                                UIOption.OnlyErrorDialogs,
                                useRecycleBin ? RecycleOption.SendToRecycleBin : RecycleOption.DeletePermanently);

                            repoNames[selectedRepoName].Remove(selectedRepo);
                        }
                        catch (Exception)
                        { }
                    }
                }
            }
        }

        public void Init()
        {
            LoadOrCreateSettings();

            tools = new ObservableCollection<Tool>();
            orgUrls = new ObservableCollection<string>();
            localRepos = new Dictionary<string, ObservableCollection<Repo>>();

            foreach (var repoRoot in Settings.RepoRoots)
            {
                try
                {
                    Repo.AddRepos(orgUrls, localRepos, repoRoot);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }

            Tools.DataContext = tools;
            Orgs.DataContext = orgUrls;
            RepoNames.ItemsSource = null;
            Repos.ItemsSource = null;

            //Visual Studio 2019
            VSTool.AddTools(tools, Settings.VSRoot);

            //VS Code
            VSCodeTool.AddTools(tools, Settings.VSCodeExe);

            //dotnet sdk
            DotNetSDKTool.AddTools(tools, Settings.DotnetSDKRoot, "SDK");

            //Windows Terminal
            NuGetFreeTool.AddTools(tools, Settings.WindowsTerminalExe, argument: "-p PowerShell");

            //Windows Explorer
            NuGetFreeTool.AddTools(tools, Settings.ExplorerExe);

            foreach (var toolExe in Settings.ToolExes)
            {
                NuGetFreeTool.AddTools(tools, Environment.ExpandEnvironmentVariables(toolExe));
            }

            foreach (var webUrl in Settings.WebUrls)
            {
                Uri uri = new Uri(webUrl);
                WebTool.AddTools(tools, webUrl, uri.Host, Settings.BrowserExe);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var VSInstall = ((FrameworkElement)sender).DataContext as VSTool;
            VSInstall.DownGrade();
        }

        private void LaunchRepoInTool(object sender, MouseButtonEventArgs e)
        {
            var tools = Tools.SelectedItems;
            var repos = new List<Repo>();

            foreach (var selectedRepo in Repos.SelectedItems)
            {
                repos.Add(selectedRepo as Repo);
            }

            foreach (var toolObj in tools)
            {
                var tool = toolObj as Tool;
                if (tool.HasCustomExec)
                {
                    tool.Exec(repos);
                }
                else
                {
                    foreach (var repoObj in repos)
                    {
                        var repo = repoObj as Repo;
                        if (tool != null && repo != null)
                        {
                            var psi = new ProcessStartInfo()
                            {
                                FileName = tool.ToolPath.FullName,
                                Arguments = (tool.Arguments == null ? "" : tool.Arguments + " ") + repo.RootPath.FullName,
                                WorkingDirectory = repo.RootPath.FullName,
                                WindowStyle = ProcessWindowStyle.Maximized
                            };
                            var p = Process.Start(psi);
                            var processId = p.Id;
                            console.Text += (p.Id + " \"" + psi.FileName + "\" " + psi.Arguments + "\n");
                        }
                    }
                }
            }
        }

        // TODO: consider moving this back to app.xaml.cs -- was chasing a jsonserializer problem that was hard to figure out...so moved it here.
        public void LoadOrCreateSettings()
        {
            bool settingsDirty = false;
            var settingsFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "rrelyea\\devcommander");
            SettingsPath = Path.Combine(settingsFolderPath, "settings.json");
            if (File.Exists(SettingsPath))
            {
                var jsonString = File.ReadAllText(SettingsPath);
                Settings = JsonSerializer.Deserialize<DevCommanderSettings>(jsonString);
            }
            else
            {
                Settings = new DevCommanderSettings();
                settingsDirty = true;
            }

            if (settingsDirty)
            {
                Directory.CreateDirectory(settingsFolderPath);
                var options = new JsonSerializerOptions() { WriteIndented = true };
                var jsonString = JsonSerializer.Serialize(Settings);
                File.WriteAllText(SettingsPath, jsonString);
            }

            if (fileWatcher == null)
            {
                fileWatcher = new FileSystemWatcher(settingsFolderPath);
                fileWatcher.EnableRaisingEvents = true;
                fileWatcher.Changed += FileWatcher_Changed;
            }
        }

        private void FileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.FullPath == SettingsPath)
            {
                Dispatcher.Invoke(() => Init());
            }
        }

        private void settings_Click(object sender, RoutedEventArgs e)
        {
            var psi = new ProcessStartInfo() { FileName = Environment.ExpandEnvironmentVariables("%windir%\\explorer.exe"), Arguments = SettingsPath, Verb = "edit" };
            Process.Start(psi);
        }

        private void Orgs_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var org = e.AddedItems[0] as string;
                repoNames = new SortedDictionary<string, List<Repo>>();
                foreach (var repo in localRepos[org])
                {
                    var repoName = repo.RepoName.ToLowerInvariant();
                    if (!repoNames.ContainsKey(repoName))
                    {
                        repoNames.Add(repoName, new List<Repo>());
                    }
                    repoNames[repoName].Add(repo);
                }

                RepoNames.ItemsSource = repoNames.Keys;
                if (RepoNames.SelectedItems.Count > 0)
                {
                    var selectedRepoName = RepoNames.SelectedItems[0] as string;
                    Repos.ItemsSource = repoNames[selectedRepoName];
                }
                else
                {
                    Repos.ItemsSource = null;
                }
            }
        }

        private void RepoNames_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (RepoNames.SelectedItems.Count > 0)
            {
                var selectedRepoName = RepoNames.SelectedItems[0] as string;
                var repos = repoNames[selectedRepoName];

                Repos.ItemsSource = repos;
                if (repos.Count == 1)
                {
                    // select the only item in the repo list, if only one.
                    Dispatcher.BeginInvoke(() => { Repos.SelectedItem = repos[0]; }, DispatcherPriority.Background);
                }
            }
        }

        public ObservableCollection<string> orgUrls;
        SortedDictionary<string, List<Repo>> repoNames = null;
        public Dictionary<string, ObservableCollection<Repo>> localRepos;
        public ObservableCollection<Tool> tools;
        public string SettingsPath { get; set; }
        public DevCommanderSettings Settings { get; set; }
        FileSystemWatcher fileWatcher;

        private void Window_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            var hyperlink = e.OriginalSource as Hyperlink;
            string navigateUri = hyperlink.NavigateUri.ToString();
            string browserPath = GetBrowserPath();
            if (string.IsNullOrEmpty(browserPath))
            {
                browserPath = "iexplore";
            }
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo(browserPath);
            process.StartInfo.Arguments = "\"" + navigateUri + "\"";
            process.Start();
        }

        public string GetBrowserPath()
        {
            const string userChoice = @"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\http\UserChoice";
            using (RegistryKey userChoiceKey = Registry.CurrentUser.OpenSubKey(userChoice))
            {
                if (userChoiceKey == null)
                {
                    return null;
                }
                string progId = userChoiceKey.GetValue("Progid") as string;
                if (progId == null)
                {
                    return null;
                }

                const string exeSuffix = ".exe";
                string path = progId + @"\shell\open\command";
                FileInfo browserPath;
                using (RegistryKey pathKey = Registry.ClassesRoot.OpenSubKey(path))
                {
                    if (pathKey == null)
                    {
                        return null;
                    }

                    // Trim parameters.
                    try
                    {
                        path = pathKey.GetValue(null).ToString().ToLower().Replace("\"", "");
                        if (!path.EndsWith(exeSuffix))
                        {
                            path = path.Substring(0, path.LastIndexOf(exeSuffix, StringComparison.Ordinal) + exeSuffix.Length);
                            browserPath = new FileInfo(path);
                            return browserPath.FullName;
                        }
                    }
                    catch
                    {
                        // Assume the registry value is set incorrectly, or some funky browser is used which currently is unknown.
                    }
                }
            }

            return null;
        }
    }
}
