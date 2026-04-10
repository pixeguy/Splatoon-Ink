using UnityEngine;

public class CameraViewer : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public Transform cameraHolder;
    private PlayerMover playerMover;

    [Header("Mouse Look")]
    public float mouseSensitivity = 2f;
    public float mouseSmoothTime = 0.03f;
    public float minPitch = -90f;
    public float maxPitch = 90f;

    private Vector2 mouseDelta;
    [HideInInspector] public Vector2 currentMouseDelta;
    private Vector2 currentMouseDeltaVelocity;
    private float cameraPitch = 0f;

    [Header("FOV")]
    public float normalFOV = 60f;
    public float sprintFOV = 95f;
    public float fovTransitionSpeed = 5f;
    [SerializeField] private float aimFOVMultiplier = 1.25f;
    [SerializeField] private int aimMouseButton = 1; // RMB

    [Header("Camera Bob")]
    private float timer;
    private float bobbingOffset;

    [Header("Camera Shake")]
    private float shakeMagnitude = 0f;
    private float shakeTimeRemaining = 0f;
    private Vector3 originalPosition;
    private Vector3 shakeOffset;

    void Start()
    {
        playerMover = GetComponent<PlayerMover>();

        if (playerCamera == null)
            playerCamera = Camera.main;

        timer = 0f;
        bobbingOffset = 0f;

        if (playerCamera != null)
            originalPosition = playerCamera.transform.localPosition;
    }

    void Update()
    {
        Look();
    }

    public void Look()
    {
        float mouseX = Input.GetAxisRaw("Mouse X");
        float mouseY = Input.GetAxisRaw("Mouse Y");

        mouseDelta = new Vector2(mouseX, mouseY);

        currentMouseDelta = Vector2.SmoothDamp(
            currentMouseDelta,
            mouseDelta,
            ref currentMouseDeltaVelocity,
            mouseSmoothTime
        );

        // yaw
        transform.Rotate(Vector3.up * currentMouseDelta.x * mouseSensitivity);

        // pitch
        cameraPitch -= currentMouseDelta.y * mouseSensitivity;
        cameraPitch = Mathf.Clamp(cameraPitch, minPitch, maxPitch);

        Vector3 euler = cameraHolder.localEulerAngles;
        euler.x = cameraPitch;
        cameraHolder.localEulerAngles = euler;
    }

    private void LateUpdate()
    {
        UpdateFOV();
        HandleCameraBob();
        HandleCameraShake();

        if (playerCamera != null)
        {
            playerCamera.transform.localPosition =
                originalPosition + shakeOffset + new Vector3(0, bobbingOffset, 0);
        }
    }

    void UpdateFOV()
    {
        if (playerCamera == null || playerMover == null)
            return;

        float currentFOV = playerCamera.fieldOfView;
        float targetFOV = normalFOV;

        if (playerMover.crouchBoost > 0 && playerMover.isDashing)
            targetFOV = sprintFOV + 10f;
        else if (playerMover.crouchBoost > 0)
            targetFOV = sprintFOV;
        else if (playerMover.isDashing)
            targetFOV = sprintFOV;
        else
            targetFOV = normalFOV;

        if (Input.GetMouseButton(aimMouseButton))
            targetFOV /= aimFOVMultiplier;

        currentFOV = Mathf.Lerp(currentFOV, targetFOV, fovTransitionSpeed * Time.deltaTime);
        playerCamera.fieldOfView = currentFOV;
    }

    private void HandleCameraBob()
    {
        if (playerMover == null || playerCamera == null)
            return;

        if (!playerMover.characterController.isGrounded)
            return;

        if (playerMover.isSprinting)
        {
            timer += Time.deltaTime * 21f;
            bobbingOffset = Mathf.Sin(timer) * 0.04f;
        }
        else
        {
            bobbingOffset = Mathf.Lerp(bobbingOffset, 0f, Time.deltaTime);
        }
    }

    public void HandleCameraShake()
    {
        if (shakeTimeRemaining > 0)
        {
            shakeOffset = Random.insideUnitSphere * shakeMagnitude;
            shakeTimeRemaining -= Time.deltaTime;
        }
        else
        {
            shakeOffset = Vector3.zero;
        }
    }

    public void TriggerCameraShake(float shakeDuration, float shakeMagnitude)
    {
        shakeTimeRemaining = shakeDuration;
        this.shakeMagnitude = shakeMagnitude;
    }

    public float GetPitch()
    {
        return cameraPitch;
    }
}