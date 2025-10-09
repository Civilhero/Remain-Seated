using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class WheelchairController : MonoBehaviour
{
    [Header("Input (drag these in)")]
    public InputActionReference leftWheelAction;
    public InputActionReference rightWheelAction;
    public InputActionReference scrollAction;

    [Header("Movement Settings")]
    public float wheelForce = 100f;    // forward/back force when both wheels engaged
    public float turnForce = 50f;      // torque when one wheel is engaged
    public float maxSpeed = 5f;        // cap linear speed
    public float maxTurnSpeed = 2f;    // cap angular Y speed

    [Header("Camera Settings")]
    public Transform playerCamera;     // assign your Main Camera (child of this object)
    public float cameraFollowSmooth = 5f;

    private Rigidbody rb;
    private bool leftWheelActive;
    private bool rightWheelActive;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        // Enable input actions
        if (leftWheelAction)  leftWheelAction.action.Enable();
        if (rightWheelAction) rightWheelAction.action.Enable();
        if (scrollAction)     scrollAction.action.Enable();

        // Subscribe to button press/release
        if (leftWheelAction)
        {
            leftWheelAction.action.performed += OnLeftPressed;
            leftWheelAction.action.canceled  += OnLeftReleased;
        }
        if (rightWheelAction)
        {
            rightWheelAction.action.performed += OnRightPressed;
            rightWheelAction.action.canceled  += OnRightReleased;
        }

        // Scroll is read on performed to create a push
        if (scrollAction)
        {
            scrollAction.action.performed += OnScroll;
        }

        // RB default tuning
        rb.linearDamping = 1.5f;
        rb.angularDamping = 2f;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    private void OnDisable()
    {
        if (leftWheelAction)
        {
            leftWheelAction.action.performed -= OnLeftPressed;
            leftWheelAction.action.canceled  -= OnLeftReleased;
            leftWheelAction.action.Disable();
        }
        if (rightWheelAction)
        {
            rightWheelAction.action.performed -= OnRightPressed;
            rightWheelAction.action.canceled  -= OnRightReleased;
            rightWheelAction.action.Disable();
        }
        if (scrollAction)
        {
            scrollAction.action.performed -= OnScroll;
            scrollAction.action.Disable();
        }
    }

    private void Update()
    {
        // Smoothly align the camera yaw to the chair
        if (playerCamera)
        {
            Quaternion targetRot = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
            playerCamera.rotation = Quaternion.Slerp(
                playerCamera.rotation, targetRot, Time.deltaTime * cameraFollowSmooth
            );
        }
    }

    private void OnLeftPressed(InputAction.CallbackContext ctx)  => leftWheelActive  = true;
    private void OnLeftReleased(InputAction.CallbackContext ctx) => leftWheelActive  = false;
    private void OnRightPressed(InputAction.CallbackContext ctx) => rightWheelActive = true;
    private void OnRightReleased(InputAction.CallbackContext ctx)=> rightWheelActive = false;

    private void OnScroll(InputAction.CallbackContext ctx)
    {
        // Scroll is Vector2 (x,y). We want vertical wheel.
        Vector2 delta = ctx.ReadValue<Vector2>();
        float scrollY = delta.y;
        if (Mathf.Abs(scrollY) < 0.01f) return;

        ApplyWheelPush(scrollY);
    }

    private void ApplyWheelPush(float scroll)
    {
        // Forward cap
        if (rb.linearVelocity.magnitude > maxSpeed && leftWheelActive && rightWheelActive)
            return;

        Vector3 fwd = transform.forward;

        if (leftWheelActive && rightWheelActive)
        {
            // Move forward/back
            rb.AddForce(fwd * scroll * wheelForce, ForceMode.Force);
        }
        else if (leftWheelActive)
        {
            // Turn right (positive scroll => clockwise yaw)
            if (Mathf.Abs(rb.angularVelocity.y) < maxTurnSpeed)
                rb.AddTorque(Vector3.up * scroll * turnForce, ForceMode.Force);
        }
        else if (rightWheelActive)
        {
            // Turn left (positive scroll => counter-clockwise yaw)
            if (Mathf.Abs(rb.angularVelocity.y) < maxTurnSpeed)
                rb.AddTorque(Vector3.up * -scroll * turnForce, ForceMode.Force);
        }
    }
}
