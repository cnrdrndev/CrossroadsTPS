using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(LineRenderer))]
public class TPSController : MonoBehaviour
{
    private CharacterController controller;
    private LineRenderer lineRenderer;

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 6.0f;
    [SerializeField] private float sprintSpeed = 10.0f;
    [SerializeField] private float rotationSpeed = 15.0f;
    [SerializeField] private float gravity = -20.0f;
    [SerializeField] private float jumpHeight = 1.2f;

    [Header("Roll Settings")]
    [SerializeField] private float rollSpeed = 14.0f;
    [SerializeField] private float rollDuration = 0.4f;
    private bool isRolling = false;
    private Vector3 rollDirection;

    [Header("Grapple / Tongue Settings")]
    [SerializeField] private float maxGrappleDistance = 30f;
    [SerializeField] private float grappleSpeed = 35f;
    [SerializeField] private float exitBoostMultiplier = 1.8f;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform tongueOrigin;

    private bool isGrappling = false;
    private Vector3 grappleTargetPoint;
    private Vector3 grappleDirection;
    private Vector3 glideMomentum;

    private Vector3 velocity;
    private bool isGrounded;
    private Vector3 startPosition; // Saves your initial spawn point

    void Start()
    {
        controller = GetComponent<CharacterController>();
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = false;

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (tongueOrigin == null)
        {
            tongueOrigin = transform;
        }

        // Save the starting position for respawns
        startPosition = transform.position;
    }

    void Update()
    {
        // 1. Fall / Respawn Check (If frog drops below Y = -10)
        if (transform.position.y < -10f)
        {
            RespawnPlayer();
            return;
        }

        // 2. Ground Check
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2.0f;
        }

        // --- GRAPPLE STATE ---
        if (isGrappling)
        {
            lineRenderer.SetPosition(0, tongueOrigin.position);
            lineRenderer.SetPosition(1, grappleTargetPoint);

            Vector3 toTarget = (grappleTargetPoint - transform.position);
            
            controller.Move(grappleDirection * grappleSpeed * Time.deltaTime);

            if (Vector3.Dot(toTarget, grappleDirection) < 0f || toTarget.magnitude < 1.0f)
            {
                EndGrapple(true);
            }

            if (Input.GetMouseButtonDown(1) || Input.GetButtonDown("Jump"))
            {
                EndGrapple(false);
            }
            return;
        }

        if (Input.GetMouseButtonDown(1) && !isGrappling)
        {
            TryStartGrapple();
        }

        // --- ROLL STATE ---
        if (isRolling)
        {
            controller.Move(rollDirection * rollSpeed * Time.deltaTime);
            return; 
        }

        // 3. Get Input (WASD)
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 direction = new Vector3(moveX, 0f, moveZ).normalized;

        // 4. Sprinting (Left Shift)
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && moveZ > 0; 
        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

        // 5. Standard Camera-Relative Movement
        if (direction.magnitude >= 0.1f)
        {
            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;

            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 moveDir = (camForward * direction.z + camRight * direction.x).normalized;

            controller.Move(moveDir * currentSpeed * Time.deltaTime);

            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // 6. Roll Mechanic (Left Ctrl or 'C')
        if ((Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.C)) && isGrounded && direction.magnitude >= 0.1f)
        {
            StartCoroutine(PerformRoll());
        }

        // 7. Jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2.0f * gravity);
        }

        // 8. Glide Momentum
        if (glideMomentum.magnitude > 0.2f)
        {
            controller.Move(glideMomentum * Time.deltaTime);
            glideMomentum = Vector3.Lerp(glideMomentum, Vector3.zero, 3.5f * Time.deltaTime);
        }

        // 9. Apply Gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void TryStartGrapple()
    {
        RaycastHit hit;
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, maxGrappleDistance))
        {
            if (hit.collider.CompareTag("Grapplable"))
            {
                grappleTargetPoint = hit.point;
                isGrappling = true;
                grappleDirection = (grappleTargetPoint - transform.position).normalized;

                lineRenderer.enabled = true;
                lineRenderer.positionCount = 2;
                lineRenderer.SetPosition(0, tongueOrigin.position);
                lineRenderer.SetPosition(1, grappleTargetPoint);

                velocity = Vector3.zero;
                glideMomentum = Vector3.zero;
            }
        }
    }

    void EndGrapple(bool applyBoost)
    {
        isGrappling = false;
        lineRenderer.enabled = false;

        if (applyBoost)
        {
            glideMomentum = grappleDirection * grappleSpeed * exitBoostMultiplier;
            glideMomentum.y += 3.0f;
        }
    }

    IEnumerator PerformRoll()
    {
        isRolling = true;
        rollDirection = transform.forward;

        float timer = 0f;
        while (timer < rollDuration)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        isRolling = false;
    }

    void RespawnPlayer()
    {
        controller.enabled = false;
        transform.position = startPosition;
        controller.enabled = true;

        velocity = Vector3.zero;
        glideMomentum = Vector3.zero;
        isRolling = false;
        isGrappling = false;
        lineRenderer.enabled = false;
    }
}