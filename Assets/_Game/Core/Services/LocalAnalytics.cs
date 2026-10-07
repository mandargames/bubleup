using System;
using System.IO;
using UnityEngine;

namespace PocketToys.Core.Services
{
    public interface IAnalyticsService { void Track(string eventName, string levelId, float seconds = 0f, int value = 0); }

    public sealed class LocalAnalytics : IAnalyticsService
    {
        public bool Enabled;
        readonly string path;
        [Serializable] sealed class Entry { public string utc, name, level; public float seconds; public int value; }
        public LocalAnalytics(string filePath) { path = filePath; }
        public void Track(string eventName, string levelId, float seconds = 0f, int value = 0)
        {
            if (!Enabled) return;
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                if (File.Exists(path) && new FileInfo(path).Length > 512 * 1024) File.WriteAllText(path, string.Empty);
                var entry = new Entry { utc = DateTime.UtcNow.ToString("o"), name = eventName, level = levelId, seconds = seconds, value = value };
                File.AppendAllText(path, JsonUtility.ToJson(entry) + Environment.NewLine);
            }
            catch (IOException) { /* Diagnostics must never interrupt play. */ }
            catch (UnauthorizedAccessException) { }
        }
    }

    // No ads or purchases are offered until a real provider has been configured and verified.
    public interface ICommerceService
    {
        bool Available { get; }
        bool CanOfferAd(bool attemptActive);
    }
    public sealed class UnconfiguredCommerce : ICommerceService
    {
        public bool Available => false;
        public bool CanOfferAd(bool attemptActive) => false;
    }
}
