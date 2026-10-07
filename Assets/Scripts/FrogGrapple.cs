using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(LineRenderer))]
public class FrogGrapple : MonoBehaviour
{
    private CharacterController controller;
    private TPSController tpsController; 
    private LineRenderer lineRenderer;

    [Header("Grapple & Slingshot Settings")]
    [SerializeField] private float maxGrappleDistance = 30f;
    [SerializeField] private float grappleSpeed = 30f;
    [SerializeField] private float exitBoostMultiplier = 1.5f; 
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform tongueOrigin; 

    private bool isGrappling = false;
    private Vector3 grappleTargetPoint;
    private Vector3 grappleDirection;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        tpsController = GetComponent<TPSController>();
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
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1) && !isGrappling)
        {
            StartGrapple();
        }

        if (isGrappling)
        {
            lineRenderer.SetPosition(0, tongueOrigin.position);
            lineRenderer.SetPosition(1, grappleTargetPoint);

            Vector3 toTarget = (grappleTargetPoint - transform.position);
            
            
            controller.Move(grappleDirection * grappleSpeed * Time.deltaTime);

            
            if (Vector3.Dot(toTarget, grappleDirection) < 0f || toTarget.magnitude < 1.0f)
            {
                ReleaseSlingshot();
            }
        }
    }

    void StartGrapple()
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

                if (tpsController != null) tpsController.enabled = false;
            }
        }
    }

    void ReleaseSlingshot()
    {
        isGrappling = false;
        lineRenderer.enabled = false; 

        
        if (tpsController != null) 
        {
            tpsController.enabled = true;
            
            
            Vector3 launchVelocity = grappleDirection * grappleSpeed * exitBoostMultiplier;
            tpsController.SendMessage("AddMomentum", launchVelocity, SendMessageOptions.DontRequireReceiver);
        }
    }
}