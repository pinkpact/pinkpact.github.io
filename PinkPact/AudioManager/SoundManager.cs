using LibVLCSharp;
using LibVLCSharp.Shared;
using System;
using System.IO;
using System.Windows.Shapes;

namespace PinkPact.Sounds
{
    /// <summary>
    /// Can access and use all audio files under PinkPact\Resources\Audio
    /// </summary>
    public class SoundManager
    {
        //Class variables
        string pathToAudio = @"e:\temp";
        char sep = System.IO.Path.DirectorySeparatorChar;
        MediaPlayer mediaPlayer = null;


        // This first function is strictly for testing purpurses
        public void StartUpAudio()
        {
            var libVLC = new LibVLC();
            var media = new Media(libVLC, "C:\\Users\\hp\\Documents\\GitHub\\PinkPact\\PinkPact\\Resources\\Audio\\TestSong.ogg", FromType.FromPath);
            var mediaPlayer = new MediaPlayer(media);
            mediaPlayer.Play();
        }

        // Here is where functions with real uses start

        // Inner workings of the SoundManager (You shouldnt use these actively)
        private string GetResourcesPath(string path = @"e:\temp")
        {
            path = Directory.GetCurrentDirectory();
            int i, j = 0;
            string[] directories = new string[34]; // I know of Split(), but it just didnt want to work :(
            for (i = 0; i < path.Length; i++)
            {
                if (path[i] == sep)
                {
                    if (directories[j] == "PinkPact")
                    {
                        i = path.Length;
                    }
                    j++;
                }
                else
                {
                    directories[j] += path[i];
                }
            }

            path = "";
            for (int t = 0; t < j; t++)
            {
                path += $"{directories[t]}{sep}";
            }
            path += $"PinkPact{sep}Resources{sep}Audio{sep}";

            return path;
        }
        //RUN THIS BEFORE PLAYING ANY SOUNDS (otherwise they wont play :P)
        public void SoundInitializer()
        {
            Core.Initialize();
            pathToAudio = GetResourcesPath();
        }
        /// <summary>
        /// Plays audio files (Duh)
        /// </summary>
        /// <param name="path">Just the to be played file name</param>
        public void PlayAudio(string path)
        {
            try
            {
                var libVLC = new LibVLC();
                var media = new Media(libVLC, pathToAudio + path, FromType.FromPath);
                mediaPlayer = new MediaPlayer(media);
                mediaPlayer.Play();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public void PauseAudio()
        {
            mediaPlayer.Pause();
        }
        public void RestartAudio()
        {
            mediaPlayer.Play();
        }
        public void StopAudio()
        {
            mediaPlayer.Stop();
        }
    }
}