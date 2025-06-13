using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinkPact.Playback
{
    public class PlaybackFinishedEventArgs : EventArgs
    {
        Audio Source { get; }

        AudioChannel Channel { get; }

        public PlaybackFinishedEventArgs(Audio src, AudioChannel channel) 
        {
            Source = src;
            Channel = channel;
        }
    }
}
