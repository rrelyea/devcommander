using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace DevCommander.Library
{
    public abstract class Tool : INotifyPropertyChanged
    {
        public string Name { get; set;  }

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

        private FileInfo toolPath;
        public FileInfo ToolPath {
            get
            {
                return toolPath;
            }
            set
            {
                toolPath = value;
                Name = toolPath.Name;
            }
        }

        public abstract string GetNuGetPath();
        
        private FileVersionInfo nugetFileVersion;
        public FileVersionInfo NuGetFileVersion
        {
            get
            {
                return nugetFileVersion;
            }
            set
            {
                if (value != nugetFileVersion)
                {
                    nugetFileVersion = value;
                    NuGetVersion = nugetFileVersion.FileVersion;
                    NotifyPropertyChanged();
                }
            }
        }
        private string nugetVersion;
        public string NuGetVersion
        {
            get
            {
                return nugetVersion;
            }
            set
            {
                if (value != nugetVersion)
                {
                    nugetVersion = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public abstract bool HasCustomExec { get; }
        public abstract string Exec(IEnumerable<Repo> repos);

        public string Arguments { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
