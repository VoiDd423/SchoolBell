using System;
using System.Media;
using System.Reflection;
using NAudio.Wave;
using System.Runtime.InteropServices;

namespace Runner
{
    internal class Program
    {
        static SoundPlayer sp;
        [System.ComponentModel.ToolboxItem(false)]
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        static void Main(string[] args)
        {
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);

            string filePath = Assembly.GetExecutingAssembly().Location;
            string projectPath = filePath.Replace("Runner.dll", "");


            NotifyIcon icon = new NotifyIcon();
            icon.Icon = new System.Drawing.Icon(projectPath + "icon.ico");
            icon.Visible = false;

            projectPath = GetInfo.GetProjectDiarectionary();
            string existingFile = projectPath + "config.txt";
            string musicFolder = GetInfo.GetMusicFolder();
            string[] nextSong;
            string[] musicFiles = GetInfo.GetMusicFiles(musicFolder);
            Random rand = new Random();
            string song;
            DateTime playTime;
            DateTime lastPlayedTime = DateTime.Parse("1/1/0001 0:00:01 AM");

            StreamReader sr = new StreamReader(existingFile);
            string workState = sr.ReadLine();
            sr.Close();
            while (true)
            {
                if (workState.Split(",")[0] == "true")
                {
                    nextSong = GetInfo.GetNextSong();
                    if (nextSong[0] != "No New Time")
                    {
                        playTime = DateTime.Parse(nextSong[0]);
                        nextSong = GetInfo.GetNextSong();
                        song = nextSong[1] != "Random" ? nextSong[1] : musicFiles[rand.Next(0, musicFiles.Length)];

                        if (lastPlayedTime.Hour != DateTime.Now.Hour || (lastPlayedTime.Hour == DateTime.Now.Hour && lastPlayedTime.Minute != DateTime.Now.Minute))
                        {
                            if (DateTime.Now.Hour == playTime.Hour && DateTime.Now.Minute == playTime.Minute)
                            {
                                lastPlayedTime = playTime;
                                PlaySong(projectPath + @"MusicFiles\" + song + ".wav", Convert.ToInt32(nextSong[2]));
                            }
                        }
                    }
                }
                Thread.Sleep(5000);
                sr = new StreamReader(existingFile);
                workState = sr.ReadLine();
                sr.Close();
            }
        }

        static void PlaySong(string path, int duration)
        {
            using (var audioFile = new AudioFileReader(path))
            using (var outputDevice = new WaveOutEvent())
            {
                outputDevice.Init(audioFile);
                outputDevice.Play();

                // Wait for the specified duration
                System.Threading.Thread.Sleep(duration * 1000);

                outputDevice.Stop();
            }
        }

    }
}
