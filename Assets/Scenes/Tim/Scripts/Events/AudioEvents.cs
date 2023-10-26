using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Events
{
    public static class AudioEvents
    {

        public delegate void AudioReceived(int samples, int channels, int frequency, float[] data);
        public static AudioReceived OnAudioReceived;
    }
}