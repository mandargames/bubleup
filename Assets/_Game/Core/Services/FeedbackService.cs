using UnityEngine;

namespace PocketToys.Core.Services
{
    public interface IHapticsService
    {
        void LightTap();
        void MediumImpact();
        void Success();
        void Warning();
    }

    public interface IAudioService
    {
        void Pump();
        void Contact();
        void Success();
    }

    /// <summary>Vendor-free spike adapter. Native semantic haptics can replace this later.</summary>
    public sealed class FeedbackService : MonoBehaviour, IHapticsService, IAudioService
    {
        public bool SoundEnabled { get; set; } = true;
        public bool HapticsEnabled { get; set; } = true;
        AudioSource[] pool;
        AudioClip pump, contact, success;
        int nextVoice;
        float lastVibration = -10f;

        void Awake()
        {
            pool = new AudioSource[4];
            for (var i = 0; i < pool.Length; i++)
            {
                pool[i] = gameObject.AddComponent<AudioSource>();
                pool[i].playOnAwake = false;
                pool[i].spatialBlend = 0f;
            }
            pump = Tone("Pump", 150f, .11f);
            contact = Tone("Contact", 540f, .045f);
            success = Tone("Success", 880f, .24f);
        }

        static AudioClip Tone(string name, float frequency, float seconds)
        {
            const int rate = 22050;
            var samples = new float[Mathf.CeilToInt(seconds * rate)];
            for (var i = 0; i < samples.Length; i++)
            {
                float t = (float)i / rate;
                float envelope = Mathf.Sin(Mathf.PI * i / samples.Length) * Mathf.Exp(-t * 12f);
                samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * .2f;
            }
            var clip = AudioClip.Create(name, samples.Length, 1, rate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        void Play(AudioClip clip)
        {
            if (!SoundEnabled) return;
            var source = pool[nextVoice++ % pool.Length];
            source.clip = clip;
            source.Play();
        }

        void Vibrate()
        {
            if (!HapticsEnabled || !Application.isMobilePlatform || Time.unscaledTime - lastVibration < .15f) return;
            lastVibration = Time.unscaledTime;
            #if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
            #endif
        }

        public void Pump() { Play(pump); }
        public void Contact() { Play(contact); }
        public void LightTap() { Vibrate(); }
        public void MediumImpact() { Vibrate(); }
        public void Warning() { Vibrate(); }
        public void Success() { Vibrate(); Play(success); }

        void OnDestroy()
        {
            Destroy(pump);
            Destroy(contact);
            Destroy(success);
        }
    }
}
