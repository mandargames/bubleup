using System;
using System.IO;
using NUnit.Framework;
using PocketToys.Core.Progression;
using PocketToys.Core.Services;
using PocketToys.WaterRingToss.Game;
using UnityEditor;
using UnityEngine;

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
        public void CampaignHasFiveIntroductoryAndTenNewLessons()
        {
            var campaign = AssetDatabase.LoadAssetAtPath<CampaignDefinition>("Assets/_Game/Toys/WaterRingToss/Levels/FiveAdventures.asset");
            Assert.NotNull(campaign); Assert.That(campaign.levels.Length, Is.EqualTo(15));
            Assert.IsTrue(campaign.Validate(out var reason), reason);
            Assert.That(campaign.levels[0].rings.Length, Is.EqualTo(1));
            Assert.That(campaign.levels[3].baffles.Length, Is.GreaterThan(0));
            Assert.That(campaign.levels[4].pegs[1].movement, Is.GreaterThan(0f));
        }

        [Test]
        public void AuthoredCurrentsHaveHonestBoundsAndRepeatableWarnings()
        {
            var zone = new CurrentDefinition { label = "Lift", center = Vector2.zero, size = new Vector2(2, 2), acceleration = Vector2.up * 4, period = 12, startDelay = 4.5f };
            Assert.IsTrue(zone.Validate());
            Assert.That(zone.StateAt(4.49f), Is.EqualTo(CurrentState.Rest));
            Assert.That(zone.StateAt(4.5f), Is.EqualTo(CurrentState.Warning));
            Assert.That(zone.AccelerationAt(Vector2.zero, 5f), Is.EqualTo(Vector2.zero));
            Assert.That(zone.StateAt(6f), Is.EqualTo(CurrentState.Active));
            Assert.That(zone.AccelerationAt(Vector2.zero, 6f).y, Is.EqualTo(4f));
            Assert.That(zone.AccelerationAt(new Vector2(.875f, 0), 6f).y, Is.EqualTo(2f).Within(.001f));
            Assert.That(zone.AccelerationAt(Vector2.right, 6f), Is.EqualTo(Vector2.zero));
            Assert.That(zone.AccelerationAt(Vector2.right * 1.1f, 6f), Is.EqualTo(Vector2.zero));
            Assert.That(zone.StateAt(10f), Is.EqualTo(CurrentState.Rest));
            Assert.That(zone.StateAt(16.5f), Is.EqualTo(CurrentState.Warning));
            Assert.That(zone.WarningRemaining(16.5f), Is.EqualTo(1.5f));
            zone.period = zone.startDelay = 0;
            Assert.That(zone.AccelerationAt(Vector2.zero, 100f).y, Is.EqualTo(4f));
            zone.acceleration.x = float.NaN; Assert.IsFalse(zone.Validate());
        }

        [Test]
        public void ExpansionRecipeMatchesSerializedLevelsAndWaterShaderImports()
        {
            var campaign = AssetDatabase.LoadAssetAtPath<CampaignDefinition>("Assets/_Game/Toys/WaterRingToss/Levels/FiveAdventures.asset");
            var type = Type.GetType("PocketToys.Editor.GameBuild, PocketToys.Editor", true);
            var method = type.GetMethod("InitialContent", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            var recipe = (LevelDefinition[])method.Invoke(null, null);
            Assert.That(recipe.Length, Is.EqualTo(campaign.levels.Length));
            for (int i = 5; i < recipe.Length; i++)
                Assert.That(JsonUtility.ToJson(recipe[i]), Is.EqualTo(JsonUtility.ToJson(campaign.levels[i])), "Recipe mismatch: " + campaign.levels[i].id);
            var shader = Shader.Find("PocketToys/LagoonWater");
            Assert.NotNull(shader); Assert.IsFalse(ShaderUtil.ShaderHasError(shader));
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
