using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JamKit
{
    /// <summary>
    /// Creates its own small overlay at runtime for short hint messages ("Need 3 coins") and dialogue boxes.
    /// Nothing to set up: JamGoal, JamTriggerZone and JamDialogueTrigger call it.
    /// </summary>
    public class JamMessageUI : MonoBehaviour
    {
        static JamMessageUI instance;

        TextMeshProUGUI toastText;
        CanvasGroup toastGroup;
        GameObject dialogueRoot;
        TextMeshProUGUI speakerText;
        TextMeshProUGUI bodyText;
        Coroutine toastRoutine;

        public static bool DialogueOpen { get; private set; }

        static JamMessageUI Get()
        {
            if (instance == null)
            {
                var go = new GameObject("JamMessageUI");
                instance = go.AddComponent<JamMessageUI>();
                instance.Build();
            }
            return instance;
        }

        /// <summary>Shows a short message near the bottom of the screen for a few seconds.</summary>
        public static void Toast(string message, float seconds = 2f)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            var ui = Get();
            if (ui.toastRoutine != null) ui.StopCoroutine(ui.toastRoutine);
            ui.toastRoutine = ui.StartCoroutine(ui.ToastRoutine(message, seconds));
        }

        /// <summary>Shows dialogue lines one by one. Jump / Interact / Fire / Enter advances. Calls onFinished at the end.</summary>
        public static void ShowDialogue(string speaker, string[] lines, bool pauseGame, Action onFinished)
        {
            if (lines == null || lines.Length == 0)
            {
                onFinished?.Invoke();
                return;
            }
            var ui = Get();
            ui.StartCoroutine(ui.DialogueRoutine(speaker, lines, pauseGame, onFinished));
        }

        IEnumerator ToastRoutine(string message, float seconds)
        {
            toastText.text = message;
            toastGroup.alpha = 1f;
            yield return new WaitForSecondsRealtime(seconds);
            for (float t = 0f; t < 0.3f; t += Time.unscaledDeltaTime)
            {
                toastGroup.alpha = 1f - t / 0.3f;
                yield return null;
            }
            toastGroup.alpha = 0f;
        }

        IEnumerator DialogueRoutine(string speaker, string[] lines, bool pauseGame, Action onFinished)
        {
            while (DialogueOpen) yield return null;
            DialogueOpen = true;
            dialogueRoot.SetActive(true);
            speakerText.text = speaker ?? "";
            speakerText.gameObject.SetActive(!string.IsNullOrEmpty(speaker));
            float oldScale = Time.timeScale;
            if (pauseGame) Time.timeScale = 0f;

            foreach (string line in lines)
            {
                bodyText.text = line;
                yield return null; // don't let the key that opened it also skip the first line
                while (!JamInput.AdvancePressed) yield return null;
            }

            dialogueRoot.SetActive(false);
            if (pauseGame && JamGameManager.IsPlaying) Time.timeScale = oldScale == 0f ? 1f : oldScale;
            DialogueOpen = false;
            onFinished?.Invoke();
        }

        void Build()
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            // Toast
            var toast = NewRect("Toast", transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 180), new Vector2(1200, 90));
            toastGroup = toast.gameObject.AddComponent<CanvasGroup>();
            toastGroup.alpha = 0f;
            toastGroup.blocksRaycasts = false;
            var toastBg = toast.gameObject.AddComponent<Image>();
            toastBg.color = new Color(0f, 0f, 0f, 0.65f);
            toastText = NewText("Text", toast, 40, TextAlignmentOptions.Center);

            // Dialogue
            var dlg = NewRect("Dialogue", transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 170), new Vector2(1500, 260));
            dialogueRoot = dlg.gameObject;
            var bg = dlg.gameObject.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.08f, 0.12f, 0.92f);
            var speakerRect = NewRect("Speaker", dlg, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40, -20), new Vector2(800, 60));
            speakerRect.pivot = new Vector2(0f, 1f);
            speakerText = NewText("Text", speakerRect, 40, TextAlignmentOptions.Left);
            speakerText.color = new Color(1f, 0.85f, 0.3f);
            var body = NewRect("Body", dlg, new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            body.offsetMin = new Vector2(40, 50);
            body.offsetMax = new Vector2(-40, -80);
            bodyText = NewText("Text", body, 38, TextAlignmentOptions.TopLeft);
            var hint = NewRect("Hint", dlg, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-30, 15), new Vector2(500, 40));
            hint.pivot = new Vector2(1f, 0f);
            var hintText = NewText("Text", hint, 26, TextAlignmentOptions.Right);
            hintText.text = "Space / E to continue >";
            hintText.color = new Color(1f, 1f, 1f, 0.6f);
            dialogueRoot.SetActive(false);
        }

        static RectTransform NewRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return rt;
        }

        static TextMeshProUGUI NewText(string name, Transform parent, float size, TextAlignmentOptions align)
        {
            var rt = NewRect(name, parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            rt.offsetMin = new Vector2(16, 8);
            rt.offsetMax = new Vector2(-16, -8);
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            t.fontSize = size;
            t.alignment = align;
            t.color = Color.white;
            t.textWrappingMode = TextWrappingModes.Normal;
            return t;
        }

        void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
                DialogueOpen = false;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            instance = null;
            DialogueOpen = false;
        }
    }
}
