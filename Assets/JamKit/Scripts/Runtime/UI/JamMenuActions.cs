using UnityEngine;

namespace JamKit
{
    [JamHelp("Button actions for the UI canvas: Resume, Restart, Next Level, Main Menu, Quit, Load Scene. Already wired on the kit's buttons.")]
    public class JamMenuActions : JamBehaviour
    {
        [Tooltip("Scene loaded by the Play button on a main menu. Leave blank to load the next scene in Build Settings.")]
        public string firstLevelName = "";

        void Awake() => JamUISetup.EnsureEventSystem();

        public void Play()
        {
            if (string.IsNullOrWhiteSpace(firstLevelName)) JamSceneLoader.LoadNextInBuild();
            else JamSceneLoader.Load(firstLevelName);
        }

        public void Resume()
        {
            if (JamGameManager.Instance != null) JamGameManager.Instance.Resume();
        }

        public void Restart()
        {
            if (JamGameManager.Instance != null) JamGameManager.Instance.Restart();
            else JamSceneLoader.ReloadCurrent();
        }

        public void NextLevel()
        {
            if (JamGameManager.Instance != null) JamGameManager.Instance.NextLevel();
            else JamSceneLoader.LoadNextInBuild();
        }

        public void MainMenu()
        {
            if (JamGameManager.Instance != null) JamGameManager.Instance.MainMenu();
            else JamSceneLoader.Load("MainMenu");
        }

        public void Quit() => JamSceneLoader.QuitGame();

        public void LoadScene(string sceneName) => JamSceneLoader.Load(sceneName);
    }
}
