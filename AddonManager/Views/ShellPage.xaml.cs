﻿using AddonManager.Core.Models;
using AddonManager.Logic;
using AddonManager.ViewModels;
using System;
using System.Diagnostics;
using System.Linq;
using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AddonManager.Views
{

    public sealed partial class ShellPage : Page
    {
        public ShellViewModel ViewModel { get; } = new ShellViewModel();

        public ShellPage()
        {
            Debug.WriteLine("shellpage constructor start");
            InitializeComponent();
            DataContext = ViewModel;

            var hoverbgColor = ((Windows.UI.Xaml.Media.SolidColorBrush)Application.Current.Resources["SystemControlBackgroundListLowBrush"]).Color;
            var hoverfgColor = ((Windows.UI.Xaml.Media.SolidColorBrush)Application.Current.Resources["SystemControlForegroundBaseHighBrush"]).Color;
            var pressedBgColor = ((Windows.UI.Xaml.Media.SolidColorBrush)Application.Current.Resources["SystemControlBackgroundListMediumBrush"]).Color;
            var pressedFgColor = ((Windows.UI.Xaml.Media.SolidColorBrush)Application.Current.Resources["SystemControlForegroundBaseHighBrush"]).Color;
            var inactivefgColor = ((Windows.UI.Xaml.Media.SolidColorBrush)Application.Current.Resources["SystemControlForegroundChromeDisabledLowBrush"]).Color;

            /*
             * From Microsoft UWP app
            Color fgColor = safe_cast<SolidColorBrush^>(Application::Current->Resources->Lookup("SystemControlPageTextBaseHighBrush"))->Color;
            applicationTitleBar->ButtonBackgroundColor = bgColor;
            applicationTitleBar->ButtonForegroundColor = fgColor;
            applicationTitleBar->ButtonInactiveBackgroundColor = bgColor;
            */



            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            var half = Windows.UI.Colors.White;
            half.A = 90;
            // Set active window colors
            titleBar.ButtonForegroundColor = Windows.UI.Colors.Black;
            titleBar.ButtonBackgroundColor = Windows.UI.Colors.Transparent;
            titleBar.ButtonHoverForegroundColor = hoverfgColor;
            titleBar.ButtonHoverBackgroundColor = hoverbgColor;
            titleBar.ButtonPressedForegroundColor = pressedFgColor;
            titleBar.ButtonPressedBackgroundColor = pressedBgColor;

            // Set inactive window colors
            titleBar.ButtonInactiveForegroundColor = inactivefgColor;
            titleBar.ButtonInactiveBackgroundColor = Windows.UI.Colors.Transparent;




            // Hide default title bar.
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;

            coreTitleBar.ExtendViewIntoTitleBar = true;
            UpdateTitleBarLayout(coreTitleBar);

            // Set XAML element as a draggable region.
            Window.Current.SetTitleBar(AppTitleBar);

            // Register a handler for when the size of the overlaid caption control changes.
            // For example, when the app moves to a screen with a different DPI.
            coreTitleBar.LayoutMetricsChanged += CoreTitleBar_LayoutMetricsChanged;

            // Register a handler for when the title bar visibility changes.
            // For example, when the title bar is invoked in full screen mode.
            coreTitleBar.IsVisibleChanged += CoreTitleBar_IsVisibleChanged;


            ViewModel.Initialize(shellFrame, navigationView, KeyboardAccelerators);

            //ApplicationView.GetForCurrentView().Title = Singleton<Session>.Instance.SelectedGame.AbsolutePath;
            // Singleton<Session>.Instance.PropertyChanged += Session_PropertyChanged;

            Debug.WriteLine("shellpage constructor end");
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as ListBox;
            var clickedGame = listBox.SelectedValue as Game;
            if (clickedGame == ViewModel.Session.SelectedGame)
                return;

            ViewModel.Session.SelectedGame = listBox.SelectedValue as Game;
            // NavigationService.ForceNavigateMainPage();

            //GameSelectorFlyout.Hide();
        }

        //private void ListBox_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        //{
        //    GameSelectorFlyout.Hide();
        //}


        private async void OpenFolder_OnClick(object sender, RoutedEventArgs e)
        {

            var folderPicker = new Windows.Storage.Pickers.FolderPicker
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop
            };
            folderPicker.FileTypeFilter.Add("*");

            Windows.Storage.StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                if (ViewModel.Session.Games.Any(g => g.AbsolutePath.Equals(folder.Path))) return;

                Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(folder);

                var game = new Game(folder.Path)
                {
                    IsLoading = true
                };
                ViewModel.Session.Games.Add(game);
                ViewModel.Session.SelectedGame = game;
                await Tasks.RefreshGameFolder(game);

                if (shellFrame.Content is BrowsePage browsePage)
                {
                    browsePage.RefreshStoreAddonStatus();
                }
                game.IsLoading = false;




            }
            else
            {
                Debug.WriteLine("Operation cancelled.");
            }


        }


        private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            UpdateTitleBarLayout(sender);
        }

        private void UpdateTitleBarLayout(CoreApplicationViewTitleBar coreTitleBar)
        {
            //Debug.WriteLine(nameof(UpdateTitleBarLayout));
            // Get the size of the caption controls area and back button 
            // (returned in logical pixels), and move your content around as necessary.
            LeftPaddingColumn.Width = new GridLength(coreTitleBar.SystemOverlayLeftInset);
            RightPaddingColumn.Width = new GridLength(coreTitleBar.SystemOverlayRightInset);
            // TitleBarButton.Margin = new Thickness(0, 0, coreTitleBar.SystemOverlayRightInset, 0);

            // Update title bar control size as needed to account for system size changes.
            AppTitleBar.Height = coreTitleBar.Height;
        }

        private void CoreTitleBar_IsVisibleChanged(CoreApplicationViewTitleBar sender, object args)
        {
            //Debug.WriteLine(nameof(CoreTitleBar_IsVisibleChanged));
            if (sender.IsVisible)
            {
                AppTitleBar.Visibility = Visibility.Visible;
            }
            else
            {
                AppTitleBar.Visibility = Visibility.Collapsed;
            }
        }

        //private async void StackPanel_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        //{
        //    var game = (sender as StackPanel).Tag as Game;
        //    Debug.WriteLine("hit "+game.AbsolutePath);

        //    var contentDialog = new ContentDialog()
        //    {
        //        Title="Set display symbol",
        //        Content="For example\r\n" +
        //        "R=Retail\r\n" +
        //        "B=Beta\r\n" +
        //        "A=Alpha\r\n" +
        //        "P=Ptr\r\n" +
        //        "C=Classic",
        //        IsPrimaryButtonEnabled=true,
        //        IsSecondaryButtonEnabled=true,
        //        PrimaryButtonText="Save",
        //        SecondaryButtonText="Cancel"
        //    };

        //    var result=await contentDialog.ShowAsync();
        //}       
    }
}
