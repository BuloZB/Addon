﻿using Addon.Core.Helpers;
using Addon.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace Addon.Logic
{
    public static class Parse
    {

        public static List<Download> FromPageToDownloads(Core.Models.Addon addon, string page)
        {
            if (addon.ProjectUrl.Contains("https://wow.curseforge.com/projects/"))
            {
                return FromWowCurseForgeToDownloads(page);
            }
            else if (addon.ProjectUrl.Contains("https://www.wowace.com/projects/"))
            {
                return FromWowaceToDownloads(page);
            }
            else if (addon.ProjectUrl.Equals(Version.ELVUI))
            {
                return FromElvUiToDownloads(page);
            }
            return new List<Download>();
        }

        public static List<Download> FromWowaceToDownloads(string htmlPage)
        {
            var downloads = new List<Download>();
            int index1 = htmlPage.IndexOf("<div class=\"listing-body\">");
            int index2 = htmlPage.IndexOf("</table>");
            string data = htmlPage.Substring(index1, index2 - index1);
            var strings = data.Split("<tr class=\"project-file-list-item\">").Skip(1).ToList();

            foreach (var s in strings)
            {
                string temp = Util.Parse2(s, "<td class=\"project-file-release-type\">", "</td>");
                string release = Util.Parse2(temp, "title=\"", "\"></div>");

                string title = Util.Parse2(s, "data-name=\"", "\">");
                string fileSize = Util.Parse2(s, "<td class=\"project-file-size\">", "</td>").Trim();

                string a = Util.Parse2(s, "data-epoch=\"", "\"");
                var dateUploaded = DateTimeOffset.FromUnixTimeSeconds(long.Parse(a)).LocalDateTime;

                string gameVersion = Util.Parse2(s, "<span class=\"version-label\">", "</span>");

                string tempDL = Util.Parse2(s, "<td class=\"project-file-downloads\">", "</td>").Replace(",", "").Trim();

                long dls = long.Parse(tempDL);
                string downloadLink = Util.Parse2(s, " href=\"", "\"");

                downloads.Add(new Download(release, title, fileSize, dateUploaded, gameVersion, dls, downloadLink));
            }
            return downloads;
        }

        public static List<Download> FromWowCurseForgeToDownloads(string htmlPage)
        {
            var downloads = new List<Download>();
            int index1 = htmlPage.IndexOf("<div class=\"listing-body\">");
            int index2 = htmlPage.IndexOf("</table>");
            string data = htmlPage.Substring(index1, index2 - index1);
            var strings = data.Split("<tr class=\"project-file-list-item\">").Skip(1).ToList();

            foreach (var s in strings)
            {
                string temp = Util.Parse2(s, "<td class=\"project-file-release-type\">", "</td>");
                string release = Util.Parse2(temp, "title=\"", "\"></div>");

                string title = Util.Parse2(s, "data-name=\"", "\">");
                string fileSize = Util.Parse2(s, "<td class=\"project-file-size\">", "</td>").Trim();

                string a = Util.Parse2(s, "data-epoch=\"", "\"");
                var dateUploaded = DateTimeOffset.FromUnixTimeSeconds(long.Parse(a)).LocalDateTime;

                string gameVersion = Util.Parse2(s, "<span class=\"version-label\">", "</span>");

                string tempDL = Util.Parse2(s, "<td class=\"project-file-downloads\">", "</td>").Replace(",", "").Trim();

                long dls = long.Parse(tempDL);
                string downloadLink = Util.Parse2(s, " href=\"", "\"");

                downloads.Add(new Download(release, title, fileSize, dateUploaded, gameVersion, dls, downloadLink));
            }
            return downloads;
        }

        public static List<Download> FromElvUiToDownloads(string htmlPage)
        {
            var downloads = new List<Download>();

            string downloadDiv = Util.Parse2(htmlPage, @"<div class=""tab-pane fade in active"" id=""download"">", "</div>");
            string parsedDownloadLink = Util.Parse2(downloadDiv, @"<a href=""/", @""" ");
            string downloadLink = "https://www.tukui.org/" + parsedDownloadLink;

            string versionDiv = Util.Parse2(htmlPage, @"<div class=""tab-pane fade"" id=""version"">", "</div>");
            string version = Util.Parse2(versionDiv, @"The current version of ElvUI is <b class=""Premium"">", @"</b>");
            string dateString = Util.Parse2(versionDiv, @"and was updated on <b class=""Premium"">", @"</b>");
            var date = DateTime.Parse(dateString);

            downloads.Add(new Download("Release", version, "3.6 MB", date, "", 0, downloadLink));

            return downloads;
        }

        //public static List<string> FromPageToChanges(string htmlPage)
        //{
        //    try
        //    {
        //        string section = Util.Parse2(htmlPage, "<section class=\"project-content", "</section>");
        //        section = section.Substring(section.IndexOf("<p>"));
        //        return section.Split("<p>")
        //            .Select(line => line
        //            .Replace("</p>", "")
        //            .Replace("<br>", "\r\n")
        //            .Replace("&nbsp;", " ")
        //            .Replace("&#x27;", "'")
        //            .Replace("&quot;", "\"")
        //            .Trim()).ToList();
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.WriteLine("[ERROR] FromPageToChanges, " + e.Message);
        //        return new List<string>();
        //    }
        //}

        public static string FromPageToChanges(string htmlPage)
        {
            try
            {
                string section = Util.Parse2(htmlPage, "<section class=\"project-content", "</section>");

                section = Regex.Replace(section, "href=\".*\"", "href=\"#\"");


                return section.Substring(section.IndexOf(">") + 1);
            }
            catch (Exception e)
            {
                Debug.WriteLine("[ERROR] FromPageToChanges, " + e.Message);
                return string.Empty;
            }
        }
    }
}
