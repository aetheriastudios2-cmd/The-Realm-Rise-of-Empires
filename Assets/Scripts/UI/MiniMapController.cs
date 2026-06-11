using UnityEngine;
using UnityEngine.UI;

namespace SettlersClone.UI
{
    /// <summary>
    /// Renders a top-down orthographic view of the map onto a RenderTexture
    /// displayed in the HUD corner as a minimap.
    /// Click on the minimap to pan the main camera to that position.
    /// </summary>
    [RequireComponent(typeof(RawImage))]
    public class MiniMapController : MonoBehaviour
    {
        [Header("Minimap Camera")]
        [SerializeField] private Camera minimapCam;
        [SerializeField] private int    textureSize = 256;

        [Header("Click-to-pan")]
        [SerializeField] private Camera.RTSCameraController mainCamRig;

        private RenderTexture rt;
        private RawImage      rawImage;
        private RectTransform rectTransform;

        private void Awake()
        {
            rawImage      = GetComponent<RawImage>();
            rectTransform = GetComponent<RectTransform>();
        }

        private void Start()
        {
            if (minimapCam == null) return;

            rt = new RenderTexture(textureSize, textureSize, 16) { name = "MinimapRT" };
            minimapCam.targetTexture = rt;
            rawImage.texture         = rt;
        }

        private void Update()
        {
            if (!Input.GetMouseButtonDown(0)) return;

            // Check if click is within minimap rect
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rectTransform, Input.mousePosition, null, out Vector2 local))
                return;

            Rect r = rectTransform.rect;
            if (!r.Contains(local)) return;

            // Convert minimap UV to world position
            float u = (local.x - r.xMin) / r.width;
            float v = (local.y - r.yMin) / r.height;

            if (minimapCam == null || mainCamRig == null) return;

            // Minimap cam is orthographic top-down — reconstruct world pos from UV
            float camOrthoH = minimapCam.orthographicSize;
            float camOrthoW = camOrthoH * minimapCam.aspect;
            Vector3 camPos  = minimapCam.transform.position;

            float wx = camPos.x + (u - 0.5f) * camOrthoW * 2f;
            float wz = camPos.z + (v - 0.5f) * camOrthoH * 2f;

            mainCamRig.FocusOn(new Vector3(wx, 0f, wz));
        }
    }
}
