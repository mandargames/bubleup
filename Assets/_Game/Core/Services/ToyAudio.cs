using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace PocketToys.Core.Services
{
    /// <summary>Original, deterministic sound synthesis with separate music, effects and semantic haptics.</summary>
    public sealed class ToyAudio : MonoBehaviour
    {
        AudioSource[] voices;
        AudioSource music;
        AudioClip pump, tap, chime, success, soundtrack;
        bool soundOn = true, hapticsOn = true;
        int voice;
        float lastContact, lastHaptic;
        #if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")] static extern void PocketToysHaptic(int kind);
        #endif

        void Awake()
        {
            voices = new AudioSource[8];
            for (int i = 0; i < voices.Length; i++) { voices[i] = gameObject.AddComponent<AudioSource>(); voices[i].playOnAwake = false; }
            music = gameObject.AddComponent<AudioSource>(); music.playOnAwake = false; music.loop = true; music.volume = .17f;
            pump = GentleBubbles();
            tap = Synthesize("Soft plastic contact", .09f, 1);
            chime = Synthesize("Ring landing", .48f, 2);
            success = Synthesize("Completion chord", 1.4f, 3);
            soundtrack = Music(); music.clip = soundtrack;
        }
        public void Configure(bool sound, bool musicEnabled, bool haptics)
        {
            soundOn = sound; hapticsOn = haptics;
            if (!sound) foreach (var source in voices) source.Stop();
            if (musicEnabled && !music.isPlaying) music.Play();
            if (!musicEnabled) music.Stop();
        }
        void Play(AudioClip clip, float volume = .7f, float pitch = 1f, float pan = 0f)
        {
            if (!soundOn) return;
            var source = voices[voice++ % voices.Length]; source.clip = clip;
            source.volume = volume; source.pitch = pitch; source.panStereo = pan; source.Play();
        }
        public void Pump(bool left) { Play(pump, .52f, .97f + (voice % 4) * .025f, left ? -.12f : .12f); Haptic(0); }
        public void Contact()
        {
            if (Time.unscaledTime - lastContact < .12f) return;
            lastContact = Time.unscaledTime; Play(tap, .3f);
        }
        public void Catch(int count) { Play(chime, .62f, 1f + count * .05f); Haptic(1); }
        public void Click() { Play(tap, .3f, 1.4f); }
        public void Success() { Play(success, .56f); Haptic(2); }

        void Haptic(int kind)
        {
            if (!hapticsOn || Time.unscaledTime - lastHaptic < .1f) return;
            lastHaptic = Time.unscaledTime;
            #if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = unity.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var constants = new AndroidJavaClass("android.view.HapticFeedbackConstants"))
                {
                    int feedback = constants.GetStatic<int>(kind == 2 ? "LONG_PRESS" : kind == 1 ? "CONTEXT_CLICK" : "KEYBOARD_TAP");
                    activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                    {
                        try
                        {
                        using (var callbackUnity = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                        using (var callbackActivity = callbackUnity.GetStatic<AndroidJavaObject>("currentActivity"))
                        using (var window = callbackActivity.Call<AndroidJavaObject>("getWindow"))
                        using (var view = window.Call<AndroidJavaObject>("getDecorView")) view.Call<bool>("performHapticFeedback", feedback);
                        }
                        catch (AndroidJavaException) { }
                    }));
                }
            }
            catch (AndroidJavaException) { /* Devices may decline haptics; gameplay continues. */ }
            #elif UNITY_IOS && !UNITY_EDITOR
            PocketToysHaptic(kind);
            #endif
        }

        static AudioClip GentleBubbles()
        {
            const int rate = 22050;
            var data = new float[(int)(rate * .48f)];
            float[] starts = { .008f, .067f, .146f, .248f };
            float[] frequencies = { 440f, 620f, 510f, 790f };
            for (int bubble = 0; bubble < starts.Length; bubble++)
            for (int i = (int)(starts[bubble] * rate); i < data.Length; i++)
            {
                float t = (float)i / rate - starts[bubble];
                if (t < 0f) continue;
                float attack = 1f - Mathf.Exp(-t * 260f);
                float envelope = attack * Mathf.Exp(-t * (25f + bubble * 3f));
                float phase = 2f * Mathf.PI * frequencies[bubble] * (t + .85f * t * t);
                // Rounded resonant drops: no broadband hiss or percussive click.
                data[i] += Mathf.Sin(phase) * envelope * (.24f - bubble * .025f);
            }
            for (int i = 0; i < data.Length; i++) data[i] *= Mathf.Clamp01((data.Length - 1 - i) / (rate * .025f));
            var clip = AudioClip.Create("Gentle bubble cluster", data.Length, 1, rate, false); clip.SetData(data, 0); return clip;
        }

        static AudioClip Synthesize(string name, float duration, int kind)
        {
            const int rate = 22050; var data = new float[(int)(rate * duration)];
            var random = new System.Random(131 + kind); float noise = 0f;
            float[] notes = { 523.25f, 659.25f, 783.99f, 1046.5f };
            for (int i = 0; i < data.Length; i++)
            {
                float t = (float)i / rate, u = t / duration;
                noise = noise * .87f + ((float)random.NextDouble() * 2f - 1f) * .13f;
                float value;
                if (kind == 1) value = (noise * .3f + Mathf.Sin(t * 2f * Mathf.PI * 820f) * .1f) * Mathf.Exp(-t * 55f);
                else if (kind == 2) value = (Mathf.Sin(t * 2f * Mathf.PI * 784f) * .24f + Mathf.Sin(t * 2f * Mathf.PI * 1176f) * .09f) * Mathf.Exp(-t * 10f) * Mathf.Min(1f, t * 200f);
                else
                {
                    value = 0f;
                    for (int n = 0; n < notes.Length; n++)
                    {
                        float nt = t - n * .11f;
                        if (nt > 0f) value += Mathf.Sin(nt * 2f * Mathf.PI * notes[n]) * Mathf.Exp(-nt * 4f) * Mathf.Min(1f, nt * 70f) * .13f;
                    }
                }
                data[i] = value * Mathf.Min(1f, (duration - t) * 100f);
            }
            var clip = AudioClip.Create(name, data.Length, 1, rate, false); clip.SetData(data, 0); return clip;
        }

        static AudioClip Music()
        {
            const int rate = 22050, seconds = 24; var data = new float[rate * seconds];
            float[] notes = { 261.63f, 329.63f, 392f, 493.88f, 293.66f, 349.23f, 440f, 523.25f };
            for (int i = 0; i < data.Length; i++)
            {
                float t = (float)i / rate; int beat = Mathf.FloorToInt(t / 1.5f); float nt = t % 1.5f;
                float f = notes[(beat * 3) % notes.Length];
                float pluck = Mathf.Sin(nt * Mathf.PI * 2f * f) * Mathf.Exp(-nt * 2.5f) * Mathf.Min(1f, nt * 40f) * .12f;
                float pad = (Mathf.Sin(t * 2f * Mathf.PI * 130.813f) + Mathf.Sin(t * 2f * Mathf.PI * 196f)) * .025f;
                float fade = Mathf.Min(1f, Mathf.Min(t, seconds - t));
                data[i] = (pluck + pad) * fade;
            }
            var clip = AudioClip.Create("Pocket lullaby - original", data.Length, 1, rate, false); clip.SetData(data, 0); return clip;
        }
        void OnDestroy() { Destroy(pump); Destroy(tap); Destroy(chime); Destroy(success); Destroy(soundtrack); }
    }
}
