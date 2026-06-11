using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SettlersClone.UI
{
    /// <summary>
    /// Pops short toast messages onto the screen (attack warnings,
    /// construction complete, out-of-resources, etc.)
    /// Call NotificationSystem.Instance.Notify("text") from anywhere.
    /// </summary>
    public class NotificationSystem : MonoBehaviour
    {
        public static NotificationSystem Instance { get; private set; }

        [SerializeField] private GameObject    toastPrefab;   // TextMeshProUGUI + CanvasGroup
        [SerializeField] private Transform     toastContainer;
        [SerializeField] private int           maxToasts   = 5;
        [SerializeField] private float         displayTime = 3f;
        [SerializeField] private float         fadeTime    = 0.5f;

        private readonly Queue<string> queue      = new();
        private int                   activeCount;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void Notify(string message, NotifyType type = NotifyType.Info)
        {
            if (activeCount >= maxToasts) { queue.Enqueue(message); return; }
            StartCoroutine(ShowToast(message, type));
        }

        private IEnumerator ShowToast(string message, NotifyType type)
        {
            activeCount++;

            var go  = Instantiate(toastPrefab, toastContainer);
            var tmp = go.GetComponentInChildren<TextMeshProUGUI>();
            var cg  = go.GetComponent<CanvasGroup>() ?? go.AddComponent<CanvasGroup>();
            var img = go.GetComponent<Image>();

            if (tmp != null) tmp.text = message;
            if (img != null) img.color = TypeColour(type);

            cg.alpha = 0f;

            // Fade in
            float t = 0f;
            while (t < fadeTime) { t += Time.deltaTime; cg.alpha = t / fadeTime; yield return null; }
            cg.alpha = 1f;

            yield return new WaitForSeconds(displayTime);

            // Fade out
            t = fadeTime;
            while (t > 0f) { t -= Time.deltaTime; cg.alpha = t / fadeTime; yield return null; }

            Destroy(go);
            activeCount--;

            if (queue.Count > 0)
                StartCoroutine(ShowToast(queue.Dequeue(), NotifyType.Info));
        }

        private static Color TypeColour(NotifyType t) => t switch
        {
            NotifyType.Warning => new Color(0.9f, 0.6f, 0.1f, 0.9f),
            NotifyType.Danger  => new Color(0.8f, 0.1f, 0.1f, 0.9f),
            NotifyType.Success => new Color(0.1f, 0.7f, 0.2f, 0.9f),
            _                  => new Color(0.15f, 0.15f, 0.2f, 0.9f)
        };
    }

    public enum NotifyType { Info, Warning, Danger, Success }
}
