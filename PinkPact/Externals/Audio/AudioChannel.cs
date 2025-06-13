using FmodAudio;
using FmodAudio.Base;
using FmodAudio.DigitalSignalProcessing;
using FmodAudio.DigitalSignalProcessing.Effects;
using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinkPact.Playback
{
    public class AudioChannel
    {
        public static AudioChannel Master => master;

        public ChannelGroupHandle Handle => channelGroup;

        public string Name 
        {
            get => name;
            set
            {
                if (IsMaster) throw new InvalidOperationException("The master channel may not be renamed."); 
                if (channels.ContainsKey(value)) throw new InvalidOperationException($"The channel name '{value}' has already been registered.");
                
                channels.Add(value, this);
                if (name != null) channels.Remove(name);
                name = value;
            }
        }

        public double Speed
        {
            get => speed;
            set
            {
                speed = value;
                SyncPlaybackData();
            }
        }

        public double Pitch
        {
            get => pitch;
            set
            {
                pitch = value;
                SyncPlaybackData();
            }
        }

        public double Tempo
        {
            get => tempo;
            set
            {
                tempo = value;
                SyncPlaybackData();
            }
        }

        public double Volume
        {
            get => volume;
            set
            {
                volume = value;
                channelGroup.Volume = (float)value;
            }
        }

        public bool IsMaster { get; }

        Dsp pitchDsp;
        HashSet<Audio> audios = new();
        ChannelGroup channelGroup;
        string name;

        double speed = 1,
               pitch = 1,
               tempo = 1,
               volume = 0.5;

        static AudioChannel master;
        static Dictionary<string, AudioChannel> channels = new();

        static AudioChannel()
        {
            master = new AudioChannel("Master", true);
        }

        private AudioChannel(string name, bool master = false)
        {
            pitchDsp = Audio.System().CreateDSPByType(DSPType.PitchShift);

            if (master)
            {
                this.name = name;
                channelGroup = Audio.System().MasterChannelGroup;
                IsMaster = true;

                channelGroup.AddDSP(ChannelControlDSPIndex.DspHead, pitchDsp);
                SyncPlaybackData();
                return;
            }

            Name = name;
            channelGroup = Audio.System().CreateChannelGroup(name);
            channelGroup.AddDSP(ChannelControlDSPIndex.DspHead, pitchDsp);
            SyncPlaybackData();
        }

        public IEnumerable<Audio> GetAudios() => audios;

        public IEnumerable<Audio> GetAudios(string alias) => audios.Where(a => a.Alias == alias);

        internal void AddAudio(Audio audio)
        {
            audios.Add(audio);
        }

        internal void RemoveAudio(Audio audio)
        {
            audios.Remove(audio);
        }

        void SyncPlaybackData()
        {
            pitchDsp.SetParameterFloat((int)DspPitchShift.Pitch, (float)(Pitch / Speed));
            channelGroup.Pitch = (float)(Tempo * Speed);
        }

        public static void RegisterChannel(string name)
        {
            _ = new AudioChannel(name);
        }

        public static AudioChannel Channel(string name)
        {
            if (channels.ContainsKey(name)) return channels[name];
            else return null;
        }
    }
}