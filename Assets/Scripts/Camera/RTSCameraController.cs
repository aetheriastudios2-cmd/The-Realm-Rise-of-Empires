using UnityEngine;

namespace SettlersClone.Camera
{
    // Classic RTS camera: WASD pan, Q/E rotate, scroll-wheel zoom, middle-mouse drag
    public class RTSCameraController : MonoBehaviour
    {
        [Header("Pan")]
        [SerializeField] private float panSpeed       = 20f;
        [SerializeField] private float edgeScrollSize = 20f; // pixels from edge
        [SerializeField] private bool  edgeScrollEnabled = true;

        [Header("Rotation")]
        [SerializeField] private float rotateSpeed = 80f;

        [Header("Zoom")]
        [SerializeField] private float zoomSpeed   = 10f;
        [SerializeField] private float minZoom     = 10f;
        [SerializeField] private float maxZoom     = 80f;

        [Header("Bounds")]
        [SerializeField] private Vector2 mapMin = new(-50f, -50f);
        [SerializeField] private Vector2 mapMax = new(250f, 200f);

        private UnityEngine.Camera cam;
        private Vector3 dragOrigin;
        private bool    isDragging;
        private float   currentZoom;

        private void Awake()
        {
            cam         = GetComponentInChildren<UnityEngine.Camera>();
            currentZoom = cam != null ? cam.transform.localPosition.y : 40f;
        }

        private void Update()
        {
            HandlePan();
            HandleRotation();
            HandleZoom();
            HandleMiddleMouseDrag();
            ClampPosition();
        }

        private void HandlePan()
        {
            Vector3 move = Vector3.zero;
            if (Input.GetKey(KeyCode.W) || (edgeScrollEnabled && Input.mousePosition.y >= Screen.height - edgeScrollSize))
                move += transform.forward;
            if (Input.GetKey(KeyCode.S) || (edgeScrollEnabled && Input.mousePosition.y <= edgeScrollSize))
                move -= transform.forward;
            if (Input.GetKey(KeyCode.A) || (edgeScrollEnabled && Input.mousePosition.x <= edgeScrollSize))
                move -= transform.right;
            if (Input.GetKey(KeyCode.D) || (edgeScrollEnabled && Input.mousePosition.x >= Screen.width - edgeScrollSize))
                move += transform.right;

            move.y = 0;
            if (move != Vector3.zero)
                transform.position += move.normalized * panSpeed * Time.unscaledDeltaTime;
        }

        private void HandleRotation()
        {
            float rot = 0f;
            if (Input.GetKey(KeyCode.Q)) rot = -rotateSpeed;
            if (Input.GetKey(KeyCode.E)) rot =  rotateSpeed;
            if (rot != 0f)
                transform.Rotate(Vector3.up, rot * Time.unscaledDeltaTime, Space.World);
        }

        private void HandleZoom()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) < 0.01f) return;
            currentZoom = Mathf.Clamp(currentZoom - scroll * zoomSpeed * 10f, minZoom, maxZoom);
            if (cam != null)
            {
                var lp = cam.transform.localPosition;
                cam.transform.localPosition = new Vector3(lp.x, currentZoom, lp.z);
            }
        }

        private void HandleMiddleMouseDrag()
        {
            if (Input.GetMouseButtonDown(2))
            {
                dragOrigin = Input.mousePosition;
                isDragging = true;
            }
            if (!Input.GetMouseButton(2)) { isDragging = false; return; }
            if (!isDragging) return;

            Vector3 delta = Input.mousePosition - dragOrigin;
            dragOrigin = Input.mousePosition;
            Vector3 move = new Vector3(-delta.x, 0, -delta.y) * panSpeed * 0.05f;
            transform.Translate(move, Space.Self);
        }

        private void ClampPosition()
        {
            Vector3 p = transform.position;
            p.x = Mathf.Clamp(p.x, mapMin.x, mapMax.x);
            p.z = Mathf.Clamp(p.z, mapMin.y, mapMax.y);
            transform.position = p;
        }

        // Focus camera on a world position
        public void FocusOn(Vector3 worldPos)
        {
            transform.position = new Vector3(worldPos.x, transform.position.y, worldPos.z);
        }
    }
}
