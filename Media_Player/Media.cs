using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Media_Player
{
    public class Media
    {
        public string File_Path { get; set; }
        public int Duration_length { get; set; }
        public string Description()
        {
            return Path.GetFileNameWithoutExtension(File_Path);
        }
    }
    public class Playlist
    {
        public string m_Path { get; set; }
        override public string ToString()
        {
            return Path.GetFileNameWithoutExtension(m_Path);
        }
    }
}
