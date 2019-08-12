﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace Media_Player
{
    public class Media
    {
        public int Order { get; set; }
        public string File_Path { get; set; }
        public int Duration_length { get; set; }
        public string Description
        {
            get
            {
                return Path.GetFileNameWithoutExtension(File_Path);
            }
        }
        public string DurationString
        {
            get
            {
                string durationStr;
                TimeSpan result = TimeSpan.FromSeconds(Duration_length);
                if (Duration_length >= 3600)
                    durationStr = result.ToString("hh':'mm':'ss");
                else durationStr = result.ToString("mm':'ss");
                return durationStr;
            }
        }
    }
    public class Playlist
    {
        public BindingList<Media> mediaList = new BindingList<Media>();
        public string m_Path { get; set; }
        override public string ToString()
        {
            return Path.GetFileNameWithoutExtension(m_Path);
        }
        public void Shuffle()
        {
            Random rng = new Random();
            int n = mediaList.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                Media value = mediaList[k];
                mediaList[k] = mediaList[n];
                mediaList[n] = value;
            }
        }
        public void ShuffleOff()
        {
            for (int i = 0; i < mediaList.Count; i++)
                for (int j = i + 1; j < mediaList.Count; j++)
                {
                    if (mediaList[i].Order > mediaList[j].Order)
                    {
                        Media temp = mediaList[i];
                        mediaList[i] = mediaList[j];
                        mediaList[j] = temp;
                    }
                }
        }
    }
}