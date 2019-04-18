﻿using Addon.Activation;
using Addon.Core.Helpers;
using Addon.Core.Models;
using Addon.Logic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Addon.Services
{
    // For more information on application activation see https://github.com/Microsoft/WindowsTemplateStudio/blob/master/docs/activation.md
    internal class ActivationService
    {
        private readonly App _app;
        private readonly Lazy<UIElement> _shell;
        private readonly Type _defaultNavItem;

        public ActivationService(App app, Type defaultNavItem, Lazy<UIElement> shell = null)
        {
            _app = app;
            _shell = shell;
            _defaultNavItem = defaultNavItem;
        }

        public async Task ActivateAsync(object activationArgs)
        {
            if (IsInteractive(activationArgs))
            {
                // Initialize things like registering background task before the app is loaded
                await InitializeAsync();

                // Do not repeat app initialization when the Window already has content,
                // just ensure that the window is active
                if (Window.Current.Content == null)
                {
                    // Create a Frame to act as the navigation context and navigate to the first page
                    Window.Current.Content = _shell?.Value ?? new Frame();
                }
            }

            var activationHandler = GetActivationHandlers()
                                                .FirstOrDefault(h => h.CanHandle(activationArgs));

            if (activationHandler != null)
            {
                await activationHandler.HandleAsync(activationArgs);
            }

            if (IsInteractive(activationArgs))
            {
                var defaultHandler = new DefaultLaunchActivationHandler(_defaultNavItem);
                if (defaultHandler.CanHandle(activationArgs))
                {
                    await defaultHandler.HandleAsync(activationArgs);
                }

                // Ensure the current window is active
                Window.Current.Activate();

                // Tasks after activation
                await StartupAsync();
            }

            var storeAddons = await Tasks.LoadStoreAddons();
            Singleton<Session>.Instance.StoreAddons = new ObservableCollection<StoreAddon>(storeAddons);
            Debug.WriteLine("Loaded StoreAddons " + Singleton<Session>.Instance.StoreAddons.Count);

            var knownSubFolders = await Tasks.LoadKnownSubFolders();
            var userKnownSubFolders = await Storage.LoadKnownSubFoldersFromUser();
            if (userKnownSubFolders != null)
            {
                knownSubFolders.UnionWith(userKnownSubFolders);
            }
            Singleton<Session>.Instance.KnownSubFolders.UnionWith(knownSubFolders);

            Debug.WriteLine("Loaded knownsubfolders " + Singleton<Session>.Instance.KnownSubFolders.Count);
            Application.Current.Suspending += new SuspendingEventHandler(App_Suspending);


            ApplicationView.GetForCurrentView().Title = Singleton<Session>.Instance.SelectedGame.AbsolutePath;
            Singleton<Session>.Instance.PropertyChanged += Session_PropertyChanged;


            foreach (var item in Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Entries)
            {
             Debug.WriteLine(item.Token);
            }

        }

        private async Task InitializeAsync()
        {
            await Singleton<LiveTileService>.Instance.EnableQueueAsync();
            await ThemeSelectorService.InitializeAsync();
        }

        private async Task StartupAsync()
        {
            await ThemeSelectorService.SetRequestedThemeAsync();
            Singleton<LiveTileService>.Instance.SampleUpdate();
            await Task.CompletedTask;
        }

        private IEnumerable<ActivationHandler> GetActivationHandlers()
        {
            yield return Singleton<LiveTileService>.Instance;
        }

        private bool IsInteractive(object args)
        {
            return args is IActivatedEventArgs;
        }

        async void App_Suspending(Object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            Debug.WriteLine("OnBackgroundActivated before save");
            await Storage.SaveSession();
            await Storage.SaveKnownSubFolders();
            Debug.WriteLine("OnBackgroundActivated after save");
            deferral.Complete();
        }


        private void Session_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("SelectedGame"))
            {
                var game = Singleton<Session>.Instance.SelectedGame;
                if (game != null)
                {
                    ApplicationView.GetForCurrentView().Title = Singleton<Session>.Instance.SelectedGame.AbsolutePath;
                }
            }
        }
    }
}
