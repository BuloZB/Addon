﻿using AddonManager.Activation;
using AddonManager.Core.Helpers;
using AddonManager.Core.Models;
using AddonManager.Logic;
using AddonManager.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AddonManager.Services
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

            //var activationHandler = GetActivationHandlers().FirstOrDefault(h => h.CanHandle(activationArgs));

            //if (activationHandler != null)
            //{
            //    await activationHandler.HandleAsync(activationArgs);
            //}

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

           // var stopWatch= new Stopwatch();
           // stopWatch.Start();
            var addonData=await Task.Run(() => Storage.LoadAddonData());
            if (addonData!=null)
            {
                Singleton<Session>.Instance.AddonData.AddRange(addonData);
                Debug.WriteLine("AddonData count: " + Singleton<Session>.Instance.AddonData.Count);

                Singleton<Session>.Instance.StoreAddons = new ObservableCollection<StoreAddon>(await Task.Run(() => Parse.LoadStoreAddons(Singleton<Session>.Instance.AddonData)));
                Debug.WriteLine("StoreAddons count: " + Singleton<Session>.Instance.StoreAddons.Count);

            }
           // stopWatch.Stop();
           // Debug.WriteLine("Time: "+stopWatch.ElapsedMilliseconds);





            var userKnownSubFolders = await Task.Run(() => Storage.LoadKnownSubFoldersFromUser());

            if (userKnownSubFolders!=null)
            {

                Singleton<Session>.Instance.KnownSubFolders.UnionWith(userKnownSubFolders);
                Debug.WriteLine("Subfolders count: " + Singleton<Session>.Instance.KnownSubFolders.Count);

            }

            Application.Current.Suspending += new SuspendingEventHandler(App_Suspending);

            //var addonData = await Task.Run(() => Storage.LoadAddonData());

            // Singleton<Session>.Instance.AddonData.AddRange(addonData);

            // Debug.WriteLine("Addondata count: " + Singleton<Session>.Instance.AddonData.Count);

            var settings = Singleton<SettingsViewModel>.Instance;
            await settings.EnsureInstanceInitializedAsync();
            bool autoRefresh = settings.IsAutoRefreshVersions ?? false;

            if (autoRefresh)
            {
                await Tasks.FindProjectUrlAndDownLoadVersionsFor(Singleton<Session>.Instance.SelectedGame.Addons);
                await Tasks.AutoUpdate(Singleton<Session>.Instance.SelectedGame.Addons);
            }


        }

        private async Task InitializeAsync()
        {
           // await Singleton<LiveTileService>.Instance.EnableQueueAsync();
            await ThemeSelectorService.InitializeAsync();
        }

        private async Task StartupAsync()
        {
            await ThemeSelectorService.SetRequestedThemeAsync();
          //  Singleton<LiveTileService>.Instance.SampleUpdate();
            await Task.CompletedTask;
        }

        //private IEnumerable<ActivationHandler> GetActivationHandlers()
        //{
        //    yield return Singleton<LiveTileService>.Instance;
        //}

        private bool IsInteractive(object args)
        {
            return args is IActivatedEventArgs;
        }

        async void App_Suspending(Object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            await Storage.SaveSession();
            await Storage.SaveKnownSubFolders();
            Debug.WriteLine("OnBackgroundActivated after save");

            deferral.Complete();
        }



    }
}
