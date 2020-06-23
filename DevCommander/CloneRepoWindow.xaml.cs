using DevCommander.Library;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DevCommander
{
    /// <summary>
    /// Interaction logic for CloneRepoWindow.xaml
    /// </summary>
    public partial class CloneRepoWindow : Window
    {
        private CloneRepoWindow()
        {
            InitializeComponent();
        }

        private string SelectedOrg;
        private string SelectedRepoName;
        private Repo SelectedRepo;

        public CloneRepoWindow(string selectedOrg, string selectedRepoName, Repo selectedRepo, DevCommanderSettings settings)
            : this()
        {
            SelectedOrg = selectedOrg;
            SelectedRepoName = selectedRepoName;
            SelectedRepo = selectedRepo;

            RepoUrl.Text = SelectedOrg + SelectedRepoName;
            RepoRoots.ItemsSource = settings.RepoRoots;
            RepoRoots.SelectedIndex = 0;
            localName.Text = "\\" + SelectedRepoName;
        }

        
        private async void Add_Click(object sender, RoutedEventArgs e)
        {
            // execute in this directory: RepoRoots.SelectedItem 
            // git clone {RepoUrl.Text} {localName.Text.Substring(1)}

            var psi = new ProcessStartInfo();
            psi.WorkingDirectory = RepoRoots.SelectedItem as string;
            psi.FileName = Path.Combine(Environment.GetEnvironmentVariable("programfiles"), "Git\\cmd\\git.exe");
            psi.Arguments = "clone " + "https://" + RepoUrl.Text + " " + localName.Text.Substring(1);

            if (GetValidPath() == null)
            {
                // repo already exists - add PR and Issue info to directory, if appropriate
                UpdateIssueAndPRInfo(psi.WorkingDirectory, 0);
            }
            else
            {
                int cloneResult = await RunProcessAsync(psi);
                UpdateIssueAndPRInfo(psi.WorkingDirectory, cloneResult);
            }
        }

        private void UpdateIssueAndPRInfo(string repoParentDir, int cloneResult)
        {
            if (cloneResult == 0)
            {
                var gitDir = Path.Combine(repoParentDir, localName.Text.Substring(1), ".git\\");
                if (!string.IsNullOrEmpty(IssueUrl.Text))
                {
                    var issueFile = Path.Combine(gitDir, "LinkToIssue");
                    File.WriteAllText(issueFile, IssueUrl.Text);
                }

                if (!string.IsNullOrEmpty(PRUrl.Text))
                {
                    var prFile = Path.Combine(gitDir, "LinkToPR");
                    File.WriteAllText(prFile, PRUrl.Text);
                }
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // TODO: consider improvement as mentioned here: https://stackoverflow.com/questions/10788982/is-there-any-async-equivalent-of-process-start#comment18776088_10789196
        public static Task<int> RunProcessAsync(ProcessStartInfo psi)
        {
            var tcs = new TaskCompletionSource<int>();

            var process = new Process()
            {
                StartInfo = psi,
                EnableRaisingEvents = true
            };

            process.Exited += (sender, args) =>
            {
                tcs.SetResult(process.ExitCode);
                process.Dispose();
            };

            process.Start();

            return tcs.Task;
        }

        private void RepoRoots_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetValidPath();
        }

        private void localName_TextChanged(object sender, TextChangedEventArgs e)
        {
            GetValidPath();
        }

        private string GetValidPath()
        {
            string fullPath = RepoRoots.SelectedItem + localName.Text;
            if (Directory.Exists(fullPath))
            {
                localName.Foreground = Brushes.Red;
                return null;
            }
            else
            {
                localName.Foreground = Brushes.Black;
                return fullPath;
            }
        }
    }
}
