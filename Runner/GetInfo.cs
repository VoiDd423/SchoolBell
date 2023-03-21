using System;
using System.IO;
using System.Windows.Forms;
using System.Reflection;
using System.Diagnostics;
using System.Security.Principal;
using Microsoft.VisualBasic.Logging;

namespace Runner
{
    internal class GetInfo
    {
        //Returns what song will be played next and time when it has to play
        public static string[] GetNextSong()
        {
            string musicFilePath = GetProjectDiarectionary() + "MusicDictionary.txt";
            DateTime time;
            string[] dt;

            foreach (string line in File.ReadLines(musicFilePath))
            {
                dt = line.Split(',');
                time = DateTime.Parse(dt[0]);
                if (time.Hour > DateTime.Now.Hour || (time.Hour == DateTime.Now.Hour && time.Minute >= DateTime.Now.Minute))
                {
                    dt = line.Split(",");
                    return dt; // 0 - time, 1 - song, 2 - duracation
                }

            }
            return new string[] { "No New Time", "No New Song", "No New Duracation" };
        }

        //Returns music path to the music folder
        public static string GetMusicFolder()
        {
            return GetProjectDiarectionary() + @"MusicFiles";
        }

        public static string GetProjectDiarectionary()
        {
            string filePath = Assembly.GetExecutingAssembly().Location;
            string projectPath = filePath.Replace(@"Runner\Runner\bin\Debug\net6.0-windows\Runner.dll", "");
            return projectPath;
        }

        public static string[] GetMusicFiles(string path)
        {
            return System.IO.Directory.GetFiles(path);
        }

    }

}

    

