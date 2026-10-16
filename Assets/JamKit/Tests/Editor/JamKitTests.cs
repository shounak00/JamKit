using System.Linq;
using System.Reflection;
using JamKit.EditorTools;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace JamKit.Tests
{
    /// <summary>Fast Edit Mode checks for the core rules: inventory, game states, goal requirement, Validate Scene.</summary>
    public class JamKitTests
    {
        const BindingFlags Any = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        static void Call(object target, string method, params object[] args)
        {
            var m = target.GetType().GetMethods(Any).First(x => x.Name == method && x.GetParameters().Length == args.Length);
            m.Invoke(target, args);
        }

        [SetUp]
        public void SetUp()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            JamInventory.ResetAll();
        }

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = 1f;
            JamInventory.ResetAll();
        }

        // ---------------------------------------------------------------- Inventory

        [Test]
        public void Inventory_AddGetConsume()
        {
            JamInventory.Add("coin", 3);
            Assert.AreEqual(3, JamInventory.Get("coin"));
            Assert.AreEqual(3, JamInventory.Get(" COIN "), "item ids ignore case and spaces");
            Assert.IsTrue(JamInventory.TryConsume("coin", 2));
            Assert.AreEqual(1, JamInventory.Get("coin"));
            Assert.IsFalse(JamInventory.TryConsume("coin", 5), "can't consume more than you have");
            Assert.AreEqual(1, JamInventory.Get("coin"), "a failed consume removes nothing");
            JamInventory.Add("coin", -10);
            Assert.AreEqual(0, JamInventory.Get("coin"), "never negative");
        }

        [Test]
        public void Inventory_RaisesOnChanged()
        {
            string lastId = null; int lastCount = -1;
            System.Action<string, int> h = (id, n) => { lastId = id; lastCount = n; };
            JamInventory.OnChanged += h;
            JamInventory.Add("key");
            JamInventory.OnChanged -= h;
            Assert.AreEqual("key", lastId);
            Assert.AreEqual(1, lastCount);
        }

        // ---------------------------------------------------------------- Game manager

        static JamGameManager NewGameManager()
        {
            var gm = new GameObject("GameManager").AddComponent<JamGameManager>();
            gm.endScreenDelay = 0f;
            Call(gm, "Awake");
            return gm;
        }

        [Test]
        public void GameManager_StartsPlaying_ThenWins()
        {
            var gm = NewGameManager();
            Assert.AreEqual(JamGameState.Playing, gm.State);
            bool won = false;
            gm.OnWin.AddListener(() => won = true);
            gm.Win();
            Assert.AreEqual(JamGameState.Won, gm.State);
            Assert.IsTrue(won, "OnWin fires");
            gm.Lose();
            Assert.AreEqual(JamGameState.Won, gm.State, "can't lose after winning");
        }

        [Test]
        public void GameManager_PauseResume()
        {
            var gm = NewGameManager();
            gm.Pause();
            Assert.AreEqual(JamGameState.Paused, gm.State);
            Assert.AreEqual(0f, Time.timeScale);
            gm.Resume();
            Assert.AreEqual(JamGameState.Playing, gm.State);
            Assert.AreEqual(1f, Time.timeScale);
        }

        [Test]
        public void GameManager_LivesMode_KeepsPlayingUntilLastLife()
        {
            var gm = NewGameManager();
            gm.useLives = true;
            gm.lives = 2;
            Call(gm, "Awake"); // re-read lives
            var player = new GameObject("Player").AddComponent<JamPlayerTopDown2D>();
            gm.PlayerDied(player);
            Assert.AreEqual(JamGameState.Playing, gm.State, "first death costs a life");
            Assert.AreEqual(1, gm.LivesLeft);
            gm.PlayerDied(player);
            Assert.AreEqual(JamGameState.Lost, gm.State, "last life → lose");
        }

        // ---------------------------------------------------------------- Goal requirement

        [Test]
        public void Goal_LockedUntilEnoughItems()
        {
            var gm = NewGameManager();
            var player = new GameObject("Player").AddComponent<JamPlayerTopDown2D>();
            var goal = new GameObject("Goal").AddComponent<JamGoal>();
            goal.requiredItemId = "coin";
            goal.requiredAmount = 2;
            Call(goal, "Start");

            Call(goal, "HandleEnter", player.gameObject);
            Assert.AreEqual(JamGameState.Playing, gm.State, "locked with 0 coins");

            JamInventory.Add("coin", 2);
            Call(goal, "HandleEnter", player.gameObject);
            Assert.AreEqual(JamGameState.Won, gm.State, "opens with 2 coins");
        }

        [Test]
        public void Goal_IgnoresNonPlayers()
        {
            var gm = NewGameManager();
            var goal = new GameObject("Goal").AddComponent<JamGoal>();
            Call(goal, "Start");
            Call(goal, "HandleEnter", new GameObject("Rock"));
            Assert.AreEqual(JamGameState.Playing, gm.State);
        }

        // ---------------------------------------------------------------- Validate Scene

        [Test]
        public void ValidateScene_EmptySceneFails()
        {
            var checks = JamValidateWindow.Run();
            Assert.IsTrue(checks.Any(c => !c.ok && c.label.Contains("No Player")));
            Assert.IsTrue(checks.Any(c => !c.ok && c.label.Contains("way to win")));
        }

        [TestCase("Assets/JamKit/Scenes/Demo/Demo_2D_Platformer.unity")]
        [TestCase("Assets/JamKit/Scenes/Demo/Demo_2D_TopDown.unity")]
        [TestCase("Assets/JamKit/Scenes/Demo/Demo_3D_ThirdPerson.unity")]
        public void ValidateScene_DemosPass(string scene)
        {
            EditorSceneManager.OpenScene(scene);
            var failed = JamValidateWindow.Run().Where(c => !c.ok && !c.warningOnly).Select(c => c.label).ToList();
            Assert.IsEmpty(failed, "Validate Scene should be all green: " + string.Join(", ", failed));
        }
    }
}
