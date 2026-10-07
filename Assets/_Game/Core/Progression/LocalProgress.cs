using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PocketToys.Core.Progression
{
    [Serializable]
    public sealed class GameSettings
    {
        public bool sound = true;
        public bool music = true;
        public bool haptics = true;
        public bool motion = true;
        public bool reduceMotion;
        public bool localDiagnostics;
        public float sensitivity = 2.5f;
        public bool calibrated;
        public float neutralX, neutralY;
        public int shell;
    }

    [Serializable]
    public sealed class LevelRecord
    {
        public string id;
        public int stars;
        public float bestSeconds;
        public int bestPumps;
        public int attempts;
    }

    [Serializable]
    public sealed class SaveData
    {
        public int version = 1;
        public string lastLevel = "water_01";
        public GameSettings settings = new GameSettings();
        public List<LevelRecord> levels = new List<LevelRecord>();
        public bool tutorialSeen;

        public LevelRecord Record(string id)
        {
            var record = levels.Find(x => x.id == id);
            if (record != null) return record;
            record = new LevelRecord { id = id };
            levels.Add(record);
            return record;
        }

        public int TotalStars
        {
            get { int total = 0; foreach (var level in levels) total += level.stars; return total; }
        }
    }

    /// <summary>Versioned local JSON with atomic replacement, backup recovery and no remote storage.</summary>
    public sealed class LocalProgress
    {
        public SaveData Data { get; private set; }
        public string LastError { get; private set; }
        public string Path { get; }
        bool incompatible;

        public LocalProgress(string path)
        {
            Path = path;
            Data = Read(path) ?? Read(path + ".bak") ?? new SaveData();
        }

        SaveData Read(string path)
        {
            if (!File.Exists(path)) return null;
            try
            {
                var data = JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
                if (data == null || data.version != 1)
                {
                    incompatible = data != null && data.version > 1;
                    LastError = incompatible ? "This save belongs to a newer game version." : "Could not read save. Trying backup.";
                    return null;
                }
                data.settings = data.settings ?? new GameSettings();
                data.levels = data.levels ?? new List<LevelRecord>();
                data.levels.RemoveAll(x => x == null || string.IsNullOrEmpty(x.id));
                foreach (var record in data.levels)
                {
                    record.stars = Mathf.Clamp(record.stars, 0, 3);
                    record.attempts = Mathf.Max(0, record.attempts);
                }
                data.settings.sensitivity = Mathf.Clamp(data.settings.sensitivity, .8f, 4.5f);
                data.settings.shell = Mathf.Clamp(data.settings.shell, 0, 2);
                return data;
            }
            catch (Exception e) { LastError = "Save recovery: " + e.Message; return null; }
        }

        public bool Save()
        {
            if (incompatible) return false;
            try
            {
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path));
                string temporary = Path + ".tmp";
                File.WriteAllText(temporary, JsonUtility.ToJson(Data, true));
                if (File.Exists(Path)) File.Replace(temporary, Path, Path + ".bak");
                else File.Move(temporary, Path);
                LastError = null;
                return true;
            }
            catch (Exception e) { LastError = "Progress could not be saved: " + e.Message; return false; }
        }

        public void Complete(string id, int stars, float seconds, int pumps)
        {
            var record = Data.Record(id);
            record.stars = Mathf.Max(record.stars, Mathf.Clamp(stars, 1, 3));
            if (record.bestSeconds <= 0f || seconds < record.bestSeconds) record.bestSeconds = seconds;
            if (record.bestPumps <= 0 || pumps < record.bestPumps) record.bestPumps = pumps;
            Save();
        }

        public static int EvaluateStars(float seconds, int pumps, float silverSeconds, float goldSeconds, int goldPumps)
        {
            if (seconds <= goldSeconds && pumps <= goldPumps) return 3;
            return seconds <= silverSeconds ? 2 : 1;
        }
    }
}
