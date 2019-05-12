﻿using Addon.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Search;

namespace Addon.Logic
{
    public static class Tasks
    {
        public static async Task RefreshGameFolder(Game game)
        {
            game.IsLoading = true;
            var folder = await StorageFolder.GetFolderFromPathAsync(game.AbsolutePath);
            var storageFolderQueryResult = folder.CreateFolderQuery(CommonFolderQuery.DefaultQuery);
            var folders = await storageFolderQueryResult.GetFoldersAsync();

            var tasks = await Task.WhenAll(folders.Select(Toc.FolderToTocFile).Where(task => task != null));

            tasks.Where(tf => tf != null && !tf.IsKnownSubFolder)
                .Select(tf => new Core.Models.Addon(game, tf.StorageFolder.Name, tf.StorageFolder.Path)
                {
                    Version = tf.Version,
                    GameVersion = tf.GameVersion,
                    Title = tf.Title
                })
                .ToList().ForEach(game.Addons.Add);
            game.IsLoading = false;
        }

        public static async Task RefreshTocFileFor(IList<Core.Models.Addon> addons)
        {
            foreach (var addon in addons)
            {
                var folder = await StorageFolder.GetFolderFromPathAsync(addon.AbsolutePath);
                var tocFile = await Toc.FolderToTocFile(folder);
                addon.Title = tocFile.Title;
                addon.Version = tocFile.Version;
                addon.GameVersion = tocFile.GameVersion;
            }
        }

        public static async Task FindProjectUrlAndDownLoadVersionsFor(ObservableCollection<Core.Models.Addon> addons)
        {
            var tasks = addons.Select(FindProjectUrlAndDownLoadVersionsFor).ToArray();
            await Task.WhenAll(tasks);
            await Sorter.Sort(addons);
        }

        public static async Task FindProjectUrlAndDownLoadVersionsFor(Core.Models.Addon addon)
        {
            addon.Progress = 0;
            addon.Status = Core.Models.Addon.DOWNLOADING_VERSIONS;
            if (String.IsNullOrEmpty(addon.ProjectUrl))
            {
                addon.ProjectUrl = await Task.Run(() => Version.FindProjectUrlFor(addon));
            }
            addon.Downloads = await Task.Run(() => Version.DownloadVersionsFor(addon));
        }

        public static async Task UpdateAddon(Core.Models.Addon addon)
        {
            await UpdateAddon(addon, addon.SuggestedDownload);
        }

        public static async Task UpdateAddon(Core.Models.Addon addon, Download download)
        {

            addon.Message = "Downloading...";
            addon.Progress = 0;
            addon.Status = Core.Models.Addon.UPDATING;

            var file = await Task.Run(() => Update.DLWithHttpProgress(addon, download));

            if (file == null)
            {
                addon.Status = Core.Models.Addon.UNKNOWN;
                addon.Message = string.Empty;
                addon.Progress = 0;
                return;
            }

            addon.Message = "Extracting...";
            addon.Progress = 0;

            if (addon.ProjectUrl.Equals(Version.ELVUI))
            {
                var trash = await Task.Run(() => Update.UpdateAddonOld(addon, download, file));
                addon.CurrentDownload = download;
                await Update.AddSubFolders(addon, trash.Item2);

                addon.Message = string.Empty;
                await Task.Run(() => Update.Cleanup(file.Name, trash.Item1));
            }
            else
            {


                var subFolders = await Task.Run(() => Update.UpdateAddon2(addon, file));
                addon.CurrentDownload = download;
                await Update.AddSubFolders(addon, subFolders);

                addon.Message = string.Empty;
                await Task.Run(() => Update.Cleanup2(file.Name));
            }
        }

        internal static async Task Remove(Core.Models.Addon addon)
        {            
            addon.Game.Addons.Remove(addon);
            await Task.Run(() => Update.RemoveFilesFor(addon));
            Debug.WriteLine("Remove done for "+addon.FolderName);
        }

    }
}
