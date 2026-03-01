using UnityEngine;

namespace AOTADev
{
    /// <summary>
    /// Stripped-down legacy-input version of your controller:
    /// - Movement (WASD, camera-relative)
    /// - Jump (Space, Input.GetKeyDown)
    /// Keeps your original force-based locomotion structure as closely as possible.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Camera")]
        [SerializeField] private Camera _camera;
        public Camera MainCamera { get => _camera; set => _camera = value; }

        [SerializeField] private float sensitivityX = 0.15f; // degrees per pixel-ish
        [SerializeField] private float sensitivityY = 0.15f;
        [SerializeField] private float minPitch = -89f;
        [SerializeField] private float maxPitch = 89f;
        [SerializeField] private bool lockCursor = true;

        private float _pitch;

        [Header("Movement")]
        [SerializeField] private float _gravityAccel = -20f;     // acceleration (m/s^2). If you use this, turn Rigidbody.useGravity OFF.
        [SerializeField] private float acceleration = 40f;       // units/s^2 (as accel)
        [SerializeField] private float maxMoveSpeed = 6f;        // units/s
        [SerializeField] private float brakingFactor = 16f;      // higher = stronger braking

        [Header("Jump")]
        [SerializeField] private float jumpForce = 6f;           // impulse
        [SerializeField] private float groundCheckDistance = 1.6f;

        [Header("Grounding")]
        [SerializeField] private LayerMask groundMask = ~0;      // what counts as ground
        [SerializeField] private int ignoreLayer = 7;            // matches your original "~7" intent

        private Rigidbody _rb;
        private Vector2 _moveInput;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();

            if (_camera == null)
                _camera = GetComponentInChildren<Camera>();

            if (lockCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void Update()
        {
            // --- OLD input system ---
            _moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

            // Jump (old style)
            if (Input.GetKeyDown(KeyCode.Space))
                TryJump();

            // Mouse look (first-person)
            float yaw = Input.GetAxis("Mouse X") * sensitivityX;
            float pitch = -Input.GetAxis("Mouse Y") * sensitivityY;

            _pitch = Mathf.Clamp(_pitch + pitch, minPitch, maxPitch);

            transform.Rotate(Vector3.up, yaw, Space.Self);
            if (MainCamera != null)
                MainCamera.transform.localEulerAngles = new Vector3(_pitch, 0f, 0f);
        }

        private void FixedUpdate()
        {
            UpdateLocomotion();
        }

        private void TryJump()
        {
            if (!IsGrounded()) return;
            _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        private void UpdateLocomotion()
        {
            Vector3 netAccel = Vector3.zero;

            // Custom gravity as acceleration (turn off Rigidbody.useGravity to avoid double gravity)
            netAccel += new Vector3(0f, _gravityAccel, 0f);

            Vector3 inputWorld = (MainCamera != null)
                ? MainCamera.transform.TransformDirection(_moveInput.x, 0f, _moveInput.y)
                : transform.TransformDirection(_moveInput.x, 0f, _moveInput.y);

            bool hasInput = _moveInput.sqrMagnitude > 1e-6f;

            if (hasInput)
                netAccel += ComputeMoveAccel(inputWorld);
            else
                netAccel += ComputeBrakeAccel();

            _rb.AddForce(netAccel, ForceMode.Acceleration);
        }

        private Vector3 ComputeMoveAccel(Vector3 worldMoveDir)
        {
            Vector3 desiredDir = Horizontal(worldMoveDir).normalized;
            if (desiredDir.sqrMagnitude < 1e-6f) return Vector3.zero;

            float accel = acceleration * (IsGrounded() ? 1f : 2f); // same �more air control� behavior as your original

            Vector3 vH = Horizontal(_rb.linearVelocity);

            float dt = Time.fixedDeltaTime;
            Vector3 proposed = vH + desiredDir * accel * dt;

            if (proposed.magnitude > maxMoveSpeed)
            {
                float keep = Mathf.Max(vH.magnitude, maxMoveSpeed);
                proposed = proposed.normalized * keep;
            }

            Vector3 requiredHorizAccel = (proposed - vH) / dt;
            return requiredHorizAccel;
        }

        private Vector3 ComputeBrakeAccel()
        {
            if (!IsGrounded()) return Vector3.zero;

            Vector3 vH = Horizontal(_rb.linearVelocity);
            if (vH.sqrMagnitude <= 1e-6f) return Vector3.zero;

            float dt = Time.fixedDeltaTime;
            float maxBraking = Mathf.Min(brakingFactor, vH.magnitude / dt);

            return -maxBraking * vH.normalized;
        }

        private static Vector3 Horizontal(Vector3 v) => Vector3.ProjectOnPlane(v, Vector3.up);

        private bool IsGrounded()
        {
            int ignoreMask = ~(1 << ignoreLayer);
            int mask = groundMask.value & ignoreMask;

            return Physics.Raycast(
                transform.position,
                Vector3.down,
                groundCheckDistance,
                mask,
                QueryTriggerInteraction.Ignore
            );
        }
    }
}
