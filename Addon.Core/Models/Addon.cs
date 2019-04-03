﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using Addon.Core.Helpers;
using static System.String;


namespace Addon.Core.Models
{
    public class Addon : INotifyPropertyChanged

    {
        public Game Game { get; }
        public string FolderName { get; }
        public string AbsolutePath { get; }

        public Addon(Game game, string folderName, string absolutePath)
        {
            Game = game ?? throw new NullReferenceException(); ;
            FolderName = folderName ?? throw new NullReferenceException(); ;
            AbsolutePath = absolutePath ?? throw new NullReferenceException(); ;
            SetIgnored = new RelayCommand(() => IsIgnored = !IsIgnored);
            SetAlpha = new RelayCommand(() => PreferredReleaseType = "Alpha");
            SetBeta = new RelayCommand(() => PreferredReleaseType = "Beta");
            SetRelease = new RelayCommand(() => PreferredReleaseType = "Release");
        }

        //private string _title;

        //public string Title
        //{
        //    get => _title;
        //    set
        //    {
        //        if (value == _title)
        //            return;
        //        _title = value;
        //        NotifyPropertyChanged();
        //    }
        //}

        private string preferredReleaseType = "Release";

        public string PreferredReleaseType
        {
            get => preferredReleaseType;
            set
            {
                if (value.Equals(preferredReleaseType))
                    return;
                preferredReleaseType = value;
                NotifyPropertyChanged("IsAlpha");
                NotifyPropertyChanged("IsBeta");
                NotifyPropertyChanged("IsRelease");
                NotifyPropertyChanged();
            }
        }

        public string Version { get; set; } = Empty;

        public string CurrentReleaseTypeAndVersion => (CurrentDownload != null) ? CurrentDownload.ReleaseType + " " + CurrentDownload.Version : $"{Version}";

        //public string ReleaseType_Version => $"{ReleaseType} {Version}";
        private Download currentDownload;
        public Download CurrentDownload
        {
            get => currentDownload;
            set
            {
                if (currentDownload != null && value == currentDownload)
                    return;
                currentDownload = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("CurrentReleaseTypeAndVersion");
            }
        }


        public ICommand SetIgnored { get; set; }
        public ICommand SetAlpha { get; set; }
        public ICommand SetBeta { get; set; }
        public ICommand SetRelease { get; set; }

        private bool isIgnored;
        public bool IsIgnored
        {
            get => isIgnored;
            set
            {
                if (value == isIgnored)
                    return;
                isIgnored = value;
                NotifyPropertyChanged();

            }
        }

        public bool IsAlpha => PreferredReleaseType.ToLower().Equals("alpha");
        public bool IsBeta => PreferredReleaseType.ToLower().Equals("beta");
        public bool IsRelease => PreferredReleaseType.ToLower().Equals("release");








        public string GameVersion { get; set; } = Empty;

        public bool IsUpdateable => status.Equals("Updateable");
        public bool IsNotUpdateable => !status.Equals("Updateable");

        private string status = "Initialized";
        public string Status
        {
            get => status;
            set
            {
                if (value.Equals(status))
                    return;
                status = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("IsUpdateable");
                NotifyPropertyChanged("IsNotUpdateable");
            }
        }









        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public override string ToString()
        {
            return $"{nameof(FolderName)}: {FolderName}, {nameof(AbsolutePath)}: {AbsolutePath}, {nameof(PreferredReleaseType)}: {PreferredReleaseType}, {nameof(CurrentReleaseTypeAndVersion)}: {CurrentReleaseTypeAndVersion}, {nameof(IsIgnored)}: {IsIgnored}, {nameof(Status)}: {Status}, {nameof(GameVersion)}: {GameVersion}";
        }


    }
}
