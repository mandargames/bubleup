using System;
using System.IO;
using NUnit.Framework;
using PocketToys.Core.Progression;
using PocketToys.Core.Services;
using PocketToys.WaterRingToss.Game;
using UnityEditor;

namespace PocketToys.Tests
{
    public sealed class GameDataTests
    {
        [Test]
        public void HapticsPrioritizeSuccessAndNeverReplayMutedFeedback()
        {
            var gate = new HapticGate();
            gate.Request(0); gate.Request(2); gate.Request(1);
            Assert.IsTrue(gate.TryTake(0, out int kind)); Assert.That(kind, Is.EqualTo(2));
            gate.Request(0); Assert.IsFalse(gate.TryTake(.01f, out _));
            gate.Request(0); Assert.IsTrue(gate.TryTake(.2f, out kind));
            gate.Request(1); Assert.IsTrue(gate.TryTake(.21f, out kind)); Assert.That(kind, Is.EqualTo(1));
            gate.Request(2); Assert.IsTrue(gate.TryTake(.22f, out kind)); Assert.That(kind, Is.EqualTo(2));
            gate.Request(2); gate.Clear(); Assert.IsFalse(gate.TryTake(2f, out _));
        }
        string directory, path;
        [SetUp] public void Setup() { directory = Path.Combine(Path.GetTempPath(), "PocketToysTest-" + Guid.NewGuid()); Directory.CreateDirectory(directory); path = Path.Combine(directory, "save.json"); }
        [TearDown] public void Teardown() { Directory.Delete(directory, true); }

        [Test]
        public void CampaignHasFiveIntroductoryAndSixNewLessons()
        {
            var campaign = AssetDatabase.LoadAssetAtPath<CampaignDefinition>("Assets/_Game/Toys/WaterRingToss/Levels/FiveAdventures.asset");
            Assert.NotNull(campaign); Assert.That(campaign.levels.Length, Is.EqualTo(11));
            Assert.IsTrue(campaign.Validate(out var reason), reason);
            Assert.That(campaign.levels[0].rings.Length, Is.EqualTo(1));
            Assert.That(campaign.levels[3].baffles.Length, Is.GreaterThan(0));
            Assert.That(campaign.levels[4].pegs[1].movement, Is.GreaterThan(0f));
        }

        [TestCase(20f, 5, 3)] [TestCase(20f, 25, 2)] [TestCase(50f, 5, 2)] [TestCase(80f, 5, 1)]
        public void StarsRewardCompletionTimeAndEfficiency(float time, int pumps, int expected)
        { Assert.That(LocalProgress.EvaluateStars(time, pumps, 60f, 30f, 12), Is.EqualTo(expected)); }

        [Test]
        public void ProgressAndSettingsSurviveReloadWithoutDowngradingBests()
        {
            var save = new LocalProgress(path); save.Data.settings.music = false; save.Data.settings.sensitivity = 3.2f;
            save.Complete("water_01", 3, 20f, 7); save.Complete("water_01", 1, 90f, 30);
            var reloaded = new LocalProgress(path);
            Assert.That(reloaded.Data.Record("water_01").stars, Is.EqualTo(3));
            Assert.That(reloaded.Data.Record("water_01").bestSeconds, Is.EqualTo(20f));
            Assert.That(reloaded.Data.Record("water_01").bestPumps, Is.EqualTo(7));
            Assert.IsFalse(reloaded.Data.settings.music); Assert.That(reloaded.Data.settings.sensitivity, Is.EqualTo(3.2f));
        }

        [Test]
        public void CorruptSaveRecoversBackup()
        {
            var save = new LocalProgress(path); save.Complete("water_01", 2, 35f, 10); save.Save();
            File.WriteAllText(path, "not a save");
            var recovered = new LocalProgress(path);
            Assert.That(recovered.Data.Record("water_01").stars, Is.EqualTo(2));
        }

        [Test]
        public void NewerSchemaIsNotOverwritten()
        {
            File.WriteAllText(path, "{\"version\":999}"); var save = new LocalProgress(path);
            Assert.IsFalse(save.Save()); Assert.That(File.ReadAllText(path), Does.Contain("999"));
        }

        [Test]
        public void CommerceCannotShowAdsDuringPlayOrWhenUnconfigured()
        {
            var commerce = new UnconfiguredCommerce();
            Assert.IsFalse(commerce.Available); Assert.IsFalse(commerce.CanOfferAd(true)); Assert.IsFalse(commerce.CanOfferAd(false));
        }

        [Test]
        public void DiagnosticsRequireOptInAndRemainLocal()
        {
            string log = Path.Combine(directory, "events.jsonl"); var analytics = new LocalAnalytics(log);
            analytics.Track("level_started", "water_01"); Assert.IsFalse(File.Exists(log));
            analytics.Enabled = true; analytics.Track("level_started", "water_01");
            Assert.That(File.ReadAllText(log), Does.Contain("water_01"));
        }
    }
}
