using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PinkPact.Helpers;

using FmodAudio.Base;
using FmodAudio.DigitalSignalProcessing;
using FmodAudio;
using FmodAudio.DigitalSignalProcessing.Effects;
using System.IO;

namespace PinkPact.Playback
{
    public class Audio : IDisposable
    {
        public AudioChannel Channel => currentChannel;

        public int Duration => (int)sound.GetLength(TimeUnit.MS);

        public int CurrentTime => (int?)playingChannel?.GetPosition(TimeUnit.MS) ?? 0;

        public bool IsPaused => playingChannel?.Paused ?? false;

        public bool IsPlaying => playingChannel?.IsPlaying ?? false;

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
                if (playingChannel != null) playingChannel.Volume = (float)value;
            }
        }

        public string Alias { get; set; }

        public bool Loop
        {
            get => loop;
            set
            {
                loop = value;
                if (playingChannel is null) return;
                
                playingChannel.Mode = loop ? Mode.Loop_Normal : Mode.Loop_Off;
            }
        }

        public event PlaybackFinishedEventHandler PlaybackFinished;

        double speed = 1,
               pitch = 1,
               tempo = 1,
               volume = 0.5;

        bool loop, tracking, stopped;
        string lastChannelName;

        Sound sound;
        Dsp pitchDsp;
        Channel playingChannel;
        AudioChannel currentChannel;

        static FmodSystem AudioSystem;

        private Audio()
        {

        }

        static Audio()
        {
            AudioSystem = Fmod.CreateSystem();
            AudioSystem.Init(320);
        }

        public Audio Play(string channel = null)
        {
            if (IsPlaying)
            {
                var audio = new Audio();
                audio.sound = sound;
                audio.pitchDsp = pitchDsp;
                audio.Alias = Alias;
                audio.lastChannelName = lastChannelName;
                audio.speed = speed;
                audio.pitch = pitch;
                audio.tempo = tempo;
                audio.volume = volume;
                audio.loop = loop;

                return audio.Play(channel);
            }

            stopped = false;
            lastChannelName = channel;

            if (channel != null && AudioChannel.Channel(channel) is null) return this;

            currentChannel = channel is null ? AudioChannel.Master : AudioChannel.Channel(channel);
            currentChannel.AddAudio(this);

            playingChannel = AudioSystem.PlaySound(sound, channel is null ? default : AudioChannel.Channel(channel).Handle, paused: true);
            playingChannel.Mode = Loop ? Mode.Loop_Normal : Mode.Loop_Off;

            pitchDsp.SetParameterFloat((int)DspPitchShift.Pitch, (float)(Pitch / Speed));
            playingChannel.AddDSP(ChannelControlDSPIndex.DspHead, pitchDsp);

            playingChannel.Pitch = (float)(Tempo * Speed);
            playingChannel.Volume = (float)Volume;
            playingChannel.Paused = false;

            _ = TrackPlayback();
            return this;
        }

        public Audio Play(bool loop, string channel = null)
        {
            Loop = true;
            return Play(channel);
        }

        public Audio Play(string alias, string channel = null)
        {
            Alias = alias;
            return Play(channel);
        }

        public Audio Play(string alias, bool loop, string channel = null)
        {
            Loop = true;
            return Play(alias, channel);
        }

        public Audio Pause()
        {
            if (playingChannel != null) playingChannel.Paused ^= true;
            return this;
        }

        public async Task<Audio> Stop()
        {
            stopped = true;
            if (IsPlaying && playingChannel != null) playingChannel.Stop();
            while (tracking) await Task.Delay(1);

            OnPlaybackFinished();
            return this;
        }

        public Audio Seek(long milliseconds, SeekOrigin origin = SeekOrigin.Begin)
        {
            if (playingChannel is null) return this;

            long position = origin == SeekOrigin.Current ? MathHelper.Clamp(playingChannel.GetPosition(TimeUnit.MS) + milliseconds, 0, sound.GetLength(TimeUnit.MS)) :
                            origin == SeekOrigin.End ? MathHelper.Clamp(sound.GetLength(TimeUnit.MS) - milliseconds, 0, sound.GetLength(TimeUnit.MS)) :
                                                      MathHelper.Clamp(milliseconds, (long)0, sound.GetLength(TimeUnit.MS));

            playingChannel.SetPosition(TimeUnit.MS, (uint)position);
            return this;
        }

        public async Task<Audio> AwaitSinglePlayback()
        {
            if (playingChannel is null) return this;

            playingChannel.Mode = Mode.Loop_Off;

            while (CurrentTime < Duration - 5) await Task.Delay(1);
            return Loop ? Play(lastChannelName) : this;
        }

        public async void Dispose()
        {
            GC.SuppressFinalize(this);

            while (!stopped) await Task.Delay(1);

            playingChannel.RemoveDSP(pitchDsp);

            pitchDsp.DisconnectAll(true, true);
            pitchDsp.Dispose();
            sound.Dispose();

            GC.ReRegisterForFinalize(this);
        }

        void SyncPlaybackData()
        {
            pitchDsp.SetParameterFloat((int)DspPitchShift.Pitch, (float)(Pitch / Speed));
            playingChannel.Pitch = (float)(Tempo * Speed);
        }

        void OnPlaybackFinished()
        {
            if (Loop) return;
            
            PlaybackFinished?.Invoke(this, new PlaybackFinishedEventArgs(this, currentChannel));

            currentChannel.RemoveAudio(this);
            currentChannel = null;
        }

        async Task TrackPlayback()
        {
            if (tracking) return;

            tracking = true;
            while (!stopped && IsPlaying) await Task.Delay(1);
            tracking = false;

            if (!stopped) await Stop();
        }

        public static Audio PlayResource(string resource, bool loop = false, string channel = null)
        {
            var audio = Audio.FromResource(resource);
            audio.Loop = loop;
                
            return audio.Play(channel);
        }

        public static Audio PlayResource(string resource, string alias, bool loop = false, string channel = null)
        {
            var audio = Audio.FromResource(resource);
            audio.Loop = loop;

            return audio.Play(alias, channel);
        }

        public static Audio FromResource(string resource)
        {
            var audio = new Audio();
            var data = ResourceHelper.GetResource(resource);

            audio.sound = AudioSystem.CreateSound(data, Mode.Default | Mode.OpenMemory, new CreateSoundInfo() { Length = (uint)data.Length });

            audio.pitchDsp = AudioSystem.CreateDSPByType(DSPType.PitchShift);
            audio.pitchDsp.SetParameterFloat((int)DspPitchShift.Pitch, 1);

            return audio;
        }

        internal static FmodSystem System()
        {
            return AudioSystem;
        }
    }
}