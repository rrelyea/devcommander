using DevCommander.Library;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;

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
        }

        public void Init()
        {
            LoadOrCreateSettings();

            tools = new ObservableCollection<Tool>();
            repos = new ObservableCollection<Repo>();

            foreach (var repoRoot in Settings.RepoRoots)
            {
                try
                {
                    Repo.AddRepos(repos, repoRoot);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }
            
            Tools.DataContext = tools;
            Repos.DataContext = repos;

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

        public ObservableCollection<Repo> repos;
        public ObservableCollection<Tool> tools;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var VSInstall = ((FrameworkElement)sender).DataContext as VSTool;
            VSInstall.DownGrade();
        }

        private void LaunchRepoInTool(object sender, MouseButtonEventArgs e)
        {
            var tools = Tools.SelectedItems;
            var repos = new List<Repo>();
            foreach (var repo in Repos.SelectedItems)
            {
                repos.Add(repo as Repo);
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
        public string SettingsPath { get; set; }
        public DevCommanderSettings Settings { get; set; }

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

        FileSystemWatcher fileWatcher;
        private void settings_Click(object sender, RoutedEventArgs e)
        {
            var psi = new ProcessStartInfo() { FileName = Environment.ExpandEnvironmentVariables("%windir%\\explorer.exe"), Arguments = SettingsPath ,Verb = "edit" };
            Process.Start(psi);
        }
    }
}
