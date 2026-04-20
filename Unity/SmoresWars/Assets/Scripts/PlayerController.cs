using UnityEngine;
namespace AOTADev
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Camera")]
        [SerializeField] private Camera _camera;
        public Camera MainCamera { get => _camera; set => _camera = value; }

        [Header("Movement")]
        [SerializeField] private float _gravityAccel = -20f; // acceleration
        [SerializeField] private float acceleration = 40f; // units/s^2 (as accel)
        [SerializeField] private float maxMoveSpeed = 6f; // units/s
        [SerializeField] private float brakingFactor = 8f; // higher = stronger braking

        [Header("Jump")]
        [SerializeField] private float jumpForce = 6f; // impulse
        [SerializeField] private float groundCheckDistance = 1.6f;
        [SerializeField] private int DoubleJump = 2;


        [Header("Crouch")]
        [SerializeField] private float crouchHeight = 0.5f;
        [SerializeField] private float standingHeight = 1f;
        [SerializeField] private float crouchSpeedMultiplier = 0.5f;

        [Header("Grounding")]
        [SerializeField] private LayerMask groundMask = ~0; // what counts as ground
        [SerializeField] private int ignoreLayer = 7; // matches your original "~7" intent

        private Rigidbody _rb;
        private Vector2 _moveInput;

        private int jumpCount; // check for how many jumps
        private bool isCrouching; //True or False if player is crouching
        private Vector3 originalScale;


        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();

            if (_camera == null)
                _camera = GetComponentInChildren<Camera>();
            originalScale = transform.localScale;
        }

        private void Update()
        {
            // Character can only move left and right.
            _moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), 0f);

            // Jump press w or space
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W))
            {
                TryJump();
            }

            // Crouch hold s
            if (Input.GetKey(KeyCode.S))
            {
                if (!isCrouching) StartCrouch();
            }
            else
            {
                if (isCrouching) StopCrouch();
            }

        }


        private void FixedUpdate()
        {
            UpdateLocomotion();
        }
       
        

        private void UpdateLocomotion()
        {
            Vector3 netAccel = Vector3.zero;
            netAccel += new Vector3(0f, _gravityAccel, 0f);

            Vector3 inputWorld = transform.right * _moveInput.x;

            if (_moveInput.sqrMagnitude > 0)
                netAccel += ComputeMoveAccel(inputWorld);
            else
                netAccel += ComputeBrakeAccel();

            _rb.AddForce(netAccel, ForceMode.Acceleration);

            if (IsGrounded())
                jumpCount = 0;
        }

        private Vector3 ComputeMoveAccel(Vector3 worldMoveDir)
        {
            Vector3 desiredDir = Vector3.ProjectOnPlane(worldMoveDir, Vector3.up).normalized;
            if (desiredDir.sqrMagnitude < 0.001f) return Vector3.zero;

            float speedMultiplier = isCrouching ? crouchSpeedMultiplier : 1f;
            float accel = acceleration * speedMultiplier * (IsGrounded() ? 1f : 0.5f);
            Vector3 vH = Vector3.ProjectOnPlane(_rb.linearVelocity, Vector3.up);
            float dt = Time.fixedDeltaTime;

            Vector3 proposed = vH + desiredDir * accel * dt;

            if (proposed.magnitude > maxMoveSpeed)
                proposed = proposed.normalized * maxMoveSpeed;

            return (proposed - vH) / dt;
        }

        private Vector3 ComputeBrakeAccel()
        {
            if (!IsGrounded()) return Vector3.zero; Vector3 vH = Vector3.ProjectOnPlane(_rb.linearVelocity, Vector3.up);

            if (vH.sqrMagnitude < 0.001f) return Vector3.zero;

            float dt = Time.fixedDeltaTime;
            float maxBraking = Mathf.Min(brakingFactor, vH.magnitude / dt);

            return -vH.normalized * maxBraking;
        }

        private void TryJump()
        {
            if (jumpCount >= DoubleJump) return;

            Vector3 velocity = _rb.linearVelocity;
            float forwardBoost = _moveInput.x * 0.5f;

            _rb.linearVelocity = new Vector3(velocity.x + forwardBoost, jumpForce, 0f);

            jumpCount++;
        }

        private void StartCrouch()
        {
            isCrouching = true;
            transform.localScale = new Vector3(originalScale.x, crouchHeight, originalScale.z);
        }

        private void StopCrouch()
        {
            isCrouching = false;
            transform.localScale = new Vector3(originalScale.x, standingHeight, originalScale.z);
        }


        private bool IsGrounded()
        {
            int ignoreMask = ~(1 << ignoreLayer);
            int mask = groundMask.value & ignoreMask;

            return Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, mask, QueryTriggerInteraction.Ignore);
        }
    }
}