﻿using System;
using System.Diagnostics;
using Addon.Core.Helpers;
using Addon.Core.Models;
using Addon.Helpers;
using Addon.Views;

namespace Addon.ViewModels
{
    public class MainViewModel : Observable
    {
        public bool NULLVALUE { get => false; }
        public Game Game { get; set; }

        public MainViewModel()
        {
            var temp = Singleton<Session>.Instance.SelectedGame;
            Game = temp ?? new Game("") { IsLoading = false };
        }

    }
}
