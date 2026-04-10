using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class MovementInput : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float gravity = 20f;

    [Header("Mouse Look")]
    public Camera playerCamera;
    public float mouseSensitivity = 2f;
    public float minPitch = -80f;
    public float maxPitch = 80f;

    [Header("Animation")]
    public float allowPlayerMovement = 0.1f;
    [Range(0, 1f)] public float StartAnimTime = 0.3f;
    [Range(0, 1f)] public float StopAnimTime = 0.15f;

    private Animator anim;
    private CharacterController controller;

    private float inputX;
    private float inputZ;
    private float verticalVel;
    public float cameraPitch;

    private void Start()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();

        if (playerCamera == null)
            playerCamera = Camera.main;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleMouseLook();
        HandleMovement();
        HandleAnimation();
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Yaw = rotate player body left/right
        transform.Rotate(Vector3.up * mouseX);

        // Pitch = rotate camera up/down
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, minPitch, maxPitch);

        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
        }
    }

    private void HandleMovement()
    {
        inputX = Input.GetAxis("Horizontal");
        inputZ = Input.GetAxis("Vertical");

        Vector3 move = (transform.right * inputX + transform.forward * inputZ).normalized;

        if (controller.isGrounded)
        {
            verticalVel = -1f;
        }
        else
        {
            verticalVel -= gravity * Time.deltaTime;
        }

        Vector3 finalMove = move * moveSpeed;
        finalMove.y = verticalVel;

        controller.Move(finalMove * Time.deltaTime);
    }

    private void HandleAnimation()
    {
        float speed = new Vector2(inputX, inputZ).sqrMagnitude;

        if (anim == null)
            return;

        // Optional if you still use this bool
        anim.SetBool("shooting", false);

        if (speed > allowPlayerMovement)
        {
            anim.SetFloat("Blend", speed, StartAnimTime, Time.deltaTime);
            anim.SetFloat("X", inputX, StartAnimTime / 3f, Time.deltaTime);
            anim.SetFloat("Y", inputZ, StartAnimTime / 3f, Time.deltaTime);
        }
        else
        {
            anim.SetFloat("Blend", speed, StopAnimTime, Time.deltaTime);
            anim.SetFloat("X", inputX, StopAnimTime / 3f, Time.deltaTime);
            anim.SetFloat("Y", inputZ, StopAnimTime / 3f, Time.deltaTime);
        }
    }
}