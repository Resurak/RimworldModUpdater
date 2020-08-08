﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
    }
}