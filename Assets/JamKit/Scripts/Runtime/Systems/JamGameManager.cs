using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace JamKit
{
    public enum JamGameState
    {
        Playing,
        Paused,
        Won,
        Lost
    }

    [JamHelp("The referee of the level. Knows if you are playing, paused, won or lost, shows the right screen and handles Restart / Next Level / Menu. One per level.")]
    [DefaultExecutionOrder(-100)]
    [DisallowMultipleComponent]
    public class JamGameManager : JamBehaviour
    {
        public static JamGameManager Instance { get; private set; }

        /// <summary>True while the game is running (also true when there is no GameManager at all).</summary>
        public static bool IsPlaying => Instance == null || Instance.State == JamGameState.Playing;

        /// <summary>Raised on every state change.</summary>
        public static event Action<JamGameState> StateChanged;

        [Header("Screens (found automatically by name if left empty)")]
        [Tooltip("Shown when the player wins. Leave empty to auto-find an object called 'WinScreen'.")]
        public GameObject winScreen;
        [Tooltip("Shown when the player loses. Leave empty to auto-find an object called 'LoseScreen'.")]
        public GameObject loseScreen;
        [Tooltip("Shown while paused. Leave empty to auto-find an object called 'PauseMenu'.")]
        public GameObject pauseScreen;

        [Header("Levels")]
        [Tooltip("Scene loaded by the Win screen's Next button. Leave blank and the Next button is hidden (the Win screen still shows). The scene must be in Build Settings.")]
        public string nextLevelName = "";
        [Tooltip("Scene loaded by the Menu buttons.")]
        public string mainMenuName = "MainMenu";

        [Header("Keys")]
        [Tooltip("Restarts the level at any time.")]
        public KeyCode restartKey = KeyCode.R;
        [Tooltip("Pauses / resumes. Esc, P and the gamepad Start button always work too.")]
        public KeyCode pauseKey = KeyCode.Escape;

        [Header("Lives (optional)")]
        [Tooltip("ON: dying costs a life and respawns you at the last checkpoint. OFF: dying shows the Lose screen.")]
        public bool useLives = false;
        [Tooltip("How many lives the player starts with (only used when 'Use Lives' is on).")]
        [Min(1)] public int lives = 3;

        [Header("Feel")]
        [Tooltip("Seconds between reaching the goal (or dying) and the Win/Lose screen appearing. Lets the victory/death moment play.")]
        [Min(0)] public float endScreenDelay = 0.75f;

        [Header("Events")]
        public UnityEvent OnWin = new UnityEvent();
        public UnityEvent OnLose = new UnityEvent();
        public UnityEvent OnPause = new UnityEvent();
        public UnityEvent OnResume = new UnityEvent();

        public JamGameState State { get; private set; } = JamGameState.Playing;
        public int LivesLeft { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[JamKit] More than one JamGameManager in the scene. Only one is used.", this);
                enabled = false;
                return;
            }
            Instance = this;
            Time.timeScale = 1f;
            LivesLeft = lives;
            JamUISetup.EnsureEventSystem();
            if (winScreen == null) winScreen = FindByName("WinScreen");
            if (loseScreen == null) loseScreen = FindByName("LoseScreen");
            if (pauseScreen == null) pauseScreen = FindByName("PauseMenu");
            Show(winScreen, false);
            Show(loseScreen, false);
            Show(pauseScreen, false);
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                Time.timeScale = 1f;
            }
        }

        void Update()
        {
            if (JamInput.RestartPressed(restartKey))
            {
                Restart();
                return;
            }
            if (JamInput.PausePressed(pauseKey))
            {
                if (State == JamGameState.Playing) Pause();
                else if (State == JamGameState.Paused) Resume();
            }
        }

        // ------------------------------------------------------------------ Public actions (wire these to buttons and events)

        public void Win()
        {
            if (State == JamGameState.Won || State == JamGameState.Lost) return;
            SetState(JamGameState.Won);
            OnWin.Invoke();
            StartCoroutine(ShowEndScreen(winScreen, true));
        }

        public void Lose()
        {
            if (State == JamGameState.Won || State == JamGameState.Lost) return;
            SetState(JamGameState.Lost);
            OnLose.Invoke();
            StartCoroutine(ShowEndScreen(loseScreen, false));
        }

        public void Pause()
        {
            if (State != JamGameState.Playing) return;
            SetState(JamGameState.Paused);
            Time.timeScale = 0f;
            Show(pauseScreen, true);
            OnPause.Invoke();
        }

        public void Resume()
        {
            if (State != JamGameState.Paused) return;
            Show(pauseScreen, false);
            Time.timeScale = 1f;
            SetState(JamGameState.Playing);
            OnResume.Invoke();
        }

        public void TogglePause()
        {
            if (State == JamGameState.Playing) Pause();
            else Resume();
        }

        public void Restart() => JamSceneLoader.ReloadCurrent();

        public bool HasNextLevel => !string.IsNullOrWhiteSpace(nextLevelName);

        public void NextLevel()
        {
            if (HasNextLevel) JamSceneLoader.Load(nextLevelName);
            else Restart();
        }

        public void MainMenu() => JamSceneLoader.Load(mainMenuName);

        public void Quit() => JamSceneLoader.QuitGame();

        /// <summary>Called by the player's JamHealth when it reaches zero.</summary>
        public void PlayerDied(JamPlayer player)
        {
            if (State != JamGameState.Playing) return;
            if (useLives && LivesLeft > 1)
            {
                LivesLeft--;
                StartCoroutine(RespawnAfterDelay(player));
            }
            else
            {
                LivesLeft = 0;
                Lose();
            }
        }

        // ------------------------------------------------------------------ Internals

        IEnumerator RespawnAfterDelay(JamPlayer player)
        {
            yield return new WaitForSeconds(0.6f);
            if (player != null) player.Respawn(true);
        }

        IEnumerator ShowEndScreen(GameObject screen, bool won)
        {
            if (endScreenDelay > 0f) yield return new WaitForSecondsRealtime(endScreenDelay);
            Show(pauseScreen, false);
            Show(screen, true);
            if (won && screen != null)
            {
                var next = FindChild(screen.transform, "Next");
                if (next != null) next.gameObject.SetActive(HasNextLevel);
            }
            Time.timeScale = 0f;
            if (screen == null)
                Debug.Log(won ? "[JamKit] You win! (no WinScreen in the scene)" : "[JamKit] You lose! (no LoseScreen in the scene)", this);
        }

        void SetState(JamGameState s)
        {
            State = s;
            StateChanged?.Invoke(s);
        }

        static void Show(GameObject go, bool on)
        {
            if (go != null) go.SetActive(on);
        }

        static GameObject FindByName(string name)
        {
            foreach (var t in FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (t.name == name && t.gameObject.scene.IsValid()) return t.gameObject;
            return null;
        }

        static Transform FindChild(Transform root, string name)
        {
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
                if (t.name == name) return t;
            return null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            Instance = null;
            StateChanged = null;
        }
    }
}
