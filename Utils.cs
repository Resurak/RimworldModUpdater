﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using Serilog;

namespace RimworldModUpdater
{
    public static class Utils
    {
        public static bool IsValidGamePath(string path)
        {
            if (String.IsNullOrWhiteSpace(path) || !Directory.Exists(Path.Combine(path, "Mods")))
            {
                return false;
            }

            return true;
        }

        public static string SanitizeFilename(string txt)
        {
            string folderName = txt;
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                folderName = folderName.Replace(c, '_');
            }

            return folderName;
        }

        public static void MoveShit(string from, string to, params string[] ignores)
        {
            List<String> files = Directory.GetFiles(from, "*").ToList();
            List<String> directories = Directory.GetDirectories(from, "*").ToList();

            foreach (string path in files)
            {
                File.Move(path, Path.Combine(to, Path.GetFileName(path)));
            }

            foreach (string path in directories)
            {
                if (ignores.Contains(Path.GetFileName(path)))
                    continue;

                Directory.Move(path, Path.Combine(to, Path.GetFileName(path)));
            }
        }

        public static int CountShit(string path)
        {
            return Directory.GetFiles(path, "*", SearchOption.AllDirectories).Length;
        }

        public static void DeleteShit(string path)
        {
            List<String> files = Directory.GetFiles(path, "*").ToList();
            List<String> directories = Directory.GetDirectories(path, "*").ToList();

            foreach (string file in files)
            {
                File.Delete(file);
            }

            foreach (string dir in directories)
            {
                Directory.Delete(dir, true);
            }
        }

        public static string GetSizeTextForMods(List<BaseMod> mods)
        {
            double totalSize = 0;
            mods.ForEach(x => totalSize += x.Details?.file_size ?? 0);

            return $"{Math.Round(totalSize / 1024d / 1024d, 2)} MB";
        }

        public static string GetSizeTextForMods(List<WorkshopFileDetails> mods)
        {
            double totalSize = 0;
            mods.ForEach(x => totalSize += x?.file_size ?? 0);

            return $"{Math.Round(totalSize / 1024d / 1024d, 2)} MB";
        }

        public static async Task<bool> CheckForUpdates()
        {
            if (File.Exists(".ignoreupdates"))
            {
                Log.Information(".ignoreupdates file is present; Not checking for updates.");
                return false;
            }

            // Does the updater need an update?

            var client = new HttpClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("RimworldModUpdater/" + Settings.Version);
            client.DefaultRequestHeaders.Accept.ParseAdd("text/plain");

            var response = await client.GetAsync("https://gitcdn.xyz/repo/EnervationRIN/RimworldModUpdater/master/ReleaseVersion.txt");
            var data = await response.Content.ReadAsByteArrayAsync();

            var encoding = SteamWorkshop.GetResponseEncoding(response.Content, Encoding.UTF8);

            string str = encoding.GetString(data).Trim();

            var localVer = Version.Parse(Settings.Version);

            if (Version.TryParse(str, out var ver))
            {
                Log.Information($"Remote version is {ver.ToString()}. Local version is {localVer.ToString()}");
                if (ver > localVer)
                {
                    var result = MessageBox.Show($"There is an update available ({localVer.ToString()} => {ver.ToString()}). Do you want to open the release page?\n\nCancel to ignore updates.", "Update Available", MessageBoxButtons.YesNoCancel);
                    switch (result)
                    {
                        case DialogResult.Yes:
                            Process.Start("https://github.com/EnervationRIN/RimworldModUpdater/releases/latest");
                            break;
                        case DialogResult.No:

                            break;
                        case DialogResult.Cancel:
                            File.WriteAllText(".ignoreupdates", "");
                            break;
                    }
                }
            }

            Log.Warning("Couldn't parse remote version while checking for updates. Returned: '{0}'", str);
            return false;
        }
    }
}
