using System.Collections;
using UnityEngine;

public class PlayerMover : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float acceleration;
    [SerializeField] private float walkSpeedMultiplier;
    [SerializeField] private float sprintSpeedMultiplier;
    [SerializeField] private float crouchSpeedMultiplier;
    [SerializeField] private float frictionMultiplier;
    [SerializeField] private float moveSpeedTransition;
    private float speed;
    private float currentSpeedMultiplier;

    [HideInInspector] public CharacterController characterController;
    [HideInInspector] public Vector2 input;
    private Vector3 move;
    public bool isSprinting = false;
    private bool wasForwardLastFrame = false;
    public bool isCrouching = false;
    [HideInInspector] public float crouchBoost = 0;
    private Vector3 crouchBoostDir;
    private bool isSliding = false;
    Vector3 slideVelocity;

    [HideInInspector] public Vector3 velocity;

    [Header("Camera")]
    CameraViewer camView;

    [Header("Jumping")]
    private Vector3 jumpVelocity;
    private float gravity = -9.81f;
    public float gravityMultiplier;
    public float glidingMultiplier;
    private float jumpHeight = 3.0f;
    bool wasGroundedLastFrame = false;
    private float jumpPressTime = 0f;
    [SerializeField] private float jumpHoldThreshold = 0.1f;
    public float extraJumpHeight;

    [HideInInspector] public bool enableMovement = true;

    [Header("Sliding")]
    [SerializeField] float slideSpeed = 6f;

    [Header("Dash")]
    [SerializeField] float dashSpeed = 35f;
    [SerializeField] float dashTime = 0.2f;
    [SerializeField] float dashCooldown = 1f;

    [Header("Layers")]
    public LayerMask alllayers;

    [Header("Old Input Keys")]
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode dashKey = KeyCode.Q;
    [SerializeField] private KeyCode glideKey = KeyCode.Space;
    [SerializeField] private int aimMouseButton = 1;   // RMB
    [SerializeField] private int shootMouseButton = 0; // LMB

    bool canDash = true;
    public bool isDashing = false;
    Vector3 dashVelocity;

    Vector3 lastPos;
    Vector3 currentVelocity;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        camView = GetComponent<CameraViewer>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        ApplyGravity();

        if (!enableMovement)
        {
            characterController.Move(jumpVelocity * Time.deltaTime);
            return;
        }

        if (Input.GetKeyDown(dashKey))
            TriggerDash();

        Jump();

        Vector3 SlopeDirection = Vector3.zero;
        RaycastHit hit;
        Vector3 halfExtents = new Vector3(0.4f, 0.4f, 0.4f);

        if (Physics.BoxCast(transform.position, halfExtents, Vector3.down, out hit, Quaternion.identity, 0.5f, alllayers, QueryTriggerInteraction.Ignore))
        {
            Vector3 groundNormal = hit.normal;
        }

        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        move = transform.right * input.x + transform.forward * input.y;
        Vector3 moveDir = move.normalized;

        Vector3 slopMoveDir = Vector3.ProjectOnPlane(moveDir, hit.normal).normalized;
        if (crouchBoost > 0)
            slopMoveDir = FilterSlideInput(slopMoveDir);

        bool isMovingForward = input.y > 0;
        bool justStartedForward = isMovingForward && !wasForwardLastFrame;
        bool sprintPressedThisFrame = Input.GetKeyDown(sprintKey);
        bool sprintHeld = Input.GetKey(sprintKey);
        bool sprintReleasedThisFrame = Input.GetKeyUp(sprintKey);

        bool crouchPressedThisFrame = Input.GetKeyDown(crouchKey);
        bool crouchHeld = Input.GetKey(crouchKey);
        bool crouchReleasedThisFrame = Input.GetKeyUp(crouchKey);

        bool justStartedSprint = (sprintPressedThisFrame && isMovingForward) || (sprintHeld && justStartedForward);

        if (isSprinting && input.y < 0.5f)
            isSprinting = false;

        if (justStartedSprint || (crouchReleasedThisFrame && sprintHeld))
        {
            isSprinting = true;
            isCrouching = false;

            if (IsBlockedAbove())
            {
                isSprinting = false;
                isCrouching = true;
            }
        }
        else if (crouchPressedThisFrame || (sprintReleasedThisFrame && crouchHeld))
        {
            isSprinting = false;
            isCrouching = true;
        }
        else if (isCrouching)
        {
            if (IsBlockedAbove())
            {
                isCrouching = true;
            }
            else if (!crouchHeld)
            {
                isCrouching = false;
            }
        }
        else if ((isSprinting && !sprintHeld) || (isCrouching && !crouchHeld))
        {
            isSprinting = false;
            isCrouching = false;
        }

        bool isAiming = Input.GetMouseButton(aimMouseButton);
        bool isShootingAuto = Input.GetMouseButton(shootMouseButton);

        if (isAiming || isShootingAuto)
            isSprinting = false;

        if (!isSprinting && !isCrouching)
            currentSpeedMultiplier = walkSpeedMultiplier;

        if (isSprinting)
            currentSpeedMultiplier = Mathf.Lerp(currentSpeedMultiplier, sprintSpeedMultiplier, 4 * Time.deltaTime);

        if (isCrouching)
        {
            if (characterController.isGrounded)
            {
                currentSpeedMultiplier = Mathf.Lerp(currentSpeedMultiplier, crouchSpeedMultiplier, 4 * Time.deltaTime);

                if (!isSliding)
                {
                    if (velocity.magnitude > 6.5f)
                    {
                        crouchBoost = 10f;
                        crouchBoostDir = moveDir;
                        isSliding = true;
                    }
                }
            }
        }

        crouchBoost = Mathf.Lerp(crouchBoost, 0f, 0.7f * Time.deltaTime);

        if (crouchBoost < 1.5f || !isCrouching)
        {
            crouchBoost = 0;
            isSliding = false;
        }

        Vector3 targetVelocity = currentSpeedMultiplier * slopMoveDir;
        float deceleration = 30f;

        if (moveDir.magnitude > 0.1f)
            velocity = Vector3.MoveTowards(velocity, targetVelocity, acceleration * Time.deltaTime);
        else
            velocity = Vector3.MoveTowards(velocity, Vector3.zero, deceleration * Time.deltaTime);

        Vector3 crouchBoostVector = slopMoveDir * crouchBoost;

        Crouch();

        Vector3 finalMove = jumpVelocity - slideVelocity + velocity + crouchBoostVector;
        if (isDashing)
            finalMove = dashVelocity;

        characterController.Move(finalMove * Time.deltaTime);

        bool isGroundedNow = characterController.isGrounded;
        if (!wasGroundedLastFrame && isGroundedNow)
        {
            if (-currentVelocity.y / 100 > 0.1f)
            {
                float x = Mathf.Clamp01(-currentVelocity.y / 100f);
                float shake = x * (0.5f + x);
                TriggerCameraShake(0.1f, shake);
            }
        }

        wasGroundedLastFrame = isGroundedNow;
        wasForwardLastFrame = isMovingForward;

        currentVelocity = (transform.position - lastPos) / Time.deltaTime;
        lastPos = transform.position;
    }

    public void ApplyImpulse(Vector3 dir)
    {
        velocity = Vector3.zero;
        velocity += dir;
    }

    private bool IsBlockedAbove()
    {
        float standingHeight = 2f;
        float crouchedHeight = 1f;
        float checkDistance = (standingHeight - crouchedHeight) + 0.1f;

        Vector3 origin = transform.position + Vector3.up * crouchedHeight;
        Debug.DrawRay(origin, Vector3.up * checkDistance, Color.red);

        return Physics.Raycast(origin, Vector3.up, checkDistance);
    }

    private void TriggerDash()
    {
        if (canDash)
            StartCoroutine(Dash());
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;

        Vector3 dashDir;

        if (move.normalized.magnitude > 0.1f)
            dashDir = move.normalized;
        else
            dashDir = transform.forward;

        dashVelocity = dashDir * dashSpeed;

        yield return new WaitForSeconds(dashTime);

        dashVelocity = Vector3.zero;
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    public bool OnSlope(out Vector3 SlopeDirection, out float slopeAngle)
    {
        SlopeDirection = Vector3.zero;
        slopeAngle = 0f;
        RaycastHit hit;
        Vector3 halfExtents = new Vector3(0.3f, 0.3f, 0.3f);

        if (Physics.BoxCast(transform.position, halfExtents, Vector3.down, out hit, Quaternion.identity, 2f))
        {
            Vector3 groundNormal = hit.normal;
            SlopeDirection = Vector3.Cross(groundNormal, Vector3.Cross(Vector3.up, groundNormal)).normalized;
            slopeAngle = Vector3.Angle(groundNormal, Vector3.up);
            return true;
        }

        return false;
    }

    Vector3 FilterSlideInput(Vector3 moveDir)
    {
        float forward = Vector3.Dot(moveDir, crouchBoostDir);
        float sideways = Vector3.Dot(moveDir, Vector3.Cross(Vector3.up, crouchBoostDir));

        bool hasForwardOrBackInput = Mathf.Abs(forward) > 0.1f;

        if (!hasForwardOrBackInput)
            forward = 1f;

        float forwardStrength = Mathf.Max(0f, forward);
        float sidewaysStrength = sideways * 0.15f;

        if (forward < 0f)
            forwardStrength = -forward * 0.7f;

        return crouchBoostDir * forwardStrength +
               Vector3.Cross(Vector3.up, crouchBoostDir) * sidewaysStrength;
    }

    void ApplyGravity()
    {
        if (characterController.isGrounded)
        {
            if (jumpVelocity.y < 0)
                jumpVelocity.y = -2f;

            if (OnSlope(out Vector3 slopeDir, out float slopeAngle) && slopeAngle > characterController.slopeLimit)
                slideVelocity = slopeDir * slideSpeed;
            else
                slideVelocity = Vector3.zero;
        }

        bool isFalling = currentVelocity.y < 0;
        bool isGliding = Input.GetKey(glideKey);

        if (isFalling && isGliding)
            glidingMultiplier = 0.2f;
        else
            glidingMultiplier = 1f;

        jumpVelocity.y += gravity * gravityMultiplier * glidingMultiplier * Time.deltaTime;
    }

    public void Jump()
    {
        if (Input.GetKeyDown(jumpKey))
        {
            jumpPressTime = Time.time;
        }

        if (Input.GetKey(jumpKey) &&
            Time.time - jumpPressTime >= jumpHoldThreshold)
        {
            OnJumpHold();
        }

        if (Input.GetKeyUp(jumpKey))
        {
            float duration = Time.time - jumpPressTime;

            if (duration < jumpHoldThreshold)
                OnJumpTap();
            else
                OnJumpHoldRelease();
        }
    }

    private void OnJumpTap()
    {
        OnSlope(out Vector3 slopeDir, out float angle);

        if (characterController.isGrounded &&
            angle < characterController.slopeLimit)
        {
            jumpVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    private void OnJumpHold()
    {
        extraJumpHeight += Time.deltaTime * 3;
        extraJumpHeight = Mathf.Clamp(extraJumpHeight, 0, 5);
    }

    private void OnJumpHoldRelease()
    {
        OnSlope(out Vector3 slopeDir, out float angle);

        if (characterController.isGrounded &&
            angle < characterController.slopeLimit)
        {
            jumpVelocity.y = Mathf.Sqrt((jumpHeight + extraJumpHeight) * -2f * gravity);
            extraJumpHeight = 0;
        }
    }

    float targetHeight;
    Vector3 targetCenter;

    private void Crouch()
    {
        if (isCrouching)
        {
            targetHeight = 1;
            targetCenter = new Vector3(0, 1f / 2f, 0);
        }
        else
        {
            targetHeight = 2;
            targetCenter = new Vector3(0, 0, 0);
        }

        characterController.height = Mathf.Lerp(characterController.height, targetHeight, 10 * Time.deltaTime);
        characterController.center = Vector3.Lerp(characterController.center, targetCenter, 10 * Time.deltaTime);
    }

    public void TriggerCameraShake(float shakeDuration, float shakeMagnitude)
    {
        if (camView != null)
            camView.TriggerCameraShake(shakeDuration, shakeMagnitude);
    }
}