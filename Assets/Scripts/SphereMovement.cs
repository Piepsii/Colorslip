using UnityEngine;

public class SphereMovement : MonoBehaviour
{
    [SerializeField, Range(1f, 100f)]
    float maxSpeed = 10f;
    [SerializeField, Range(1f, 100f)]
    float maxAcceleration = 10f, maxAirAcceleration = 1f;
    [SerializeField, Range(2f, 10f)]
    float jumpHeight = 2f;
    [SerializeField, Range(0, 5)]
    int maxAirJumps = 1;
    [SerializeField, Range(0f, 90f)]
    float maxGroundAngle = 25f, maxStairsAngle = 50f;
    [SerializeField, Range(0f, 100f)]
    float maxSnapSpeed = 100f;
    [SerializeField, Min(0f)]
    float probeDistance = 1f;
    [SerializeField]
    LayerMask probeMask = -1, stairsMask = -1;
    [SerializeField]
    Transform playerInputSpace = default;

    Vector3 playerInput;
    Vector3 velocity, desiredVelocity;
    Vector3 contactNormal, steepNormal;
    Vector3 upAxis = Vector3.up, rightAxis, forwardAxis;

    Rigidbody body;
    bool desiredJump;
    int jumpPhase;
    int stepsSinceLastGrounded, stepsSinceLastJumped;
    float minGroundDotProduct, minStairsDotProduct;
    int groundContactCount, steepContactCount;

    bool OnGround => groundContactCount > 0;
    bool OnSteep => steepContactCount > 0;

    void OnValidate()
    {
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
        minStairsDotProduct = Mathf.Cos(maxStairsAngle * Mathf.Deg2Rad);
    }

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        OnValidate();
    }

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        playerInput = Vector3.ClampMagnitude(playerInput, 1f);
        if (playerInputSpace)
        {
            rightAxis = ProjectDirectionOnPlane(playerInputSpace.right, upAxis);
            forwardAxis = ProjectDirectionOnPlane(playerInputSpace.forward, upAxis);
        }
        else
        {
            rightAxis = ProjectDirectionOnPlane(Vector3.right, upAxis);
            forwardAxis = ProjectDirectionOnPlane(Vector3.forward, upAxis);
        }
        desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;
        desiredJump |= Input.GetButtonDown("Jump");
    }

    void FixedUpdate()
    {
        UpdateState();
        AdjustVelocity();
        if (desiredJump)
        {
            desiredJump = false;
            Jump();
        }
        body.velocity = velocity;
        ClearState();
    }

    private void UpdateState()
    {
        stepsSinceLastGrounded++;
        stepsSinceLastJumped++;
        velocity = body.velocity;
        if (OnGround || SnapToGround() || CheckSteepContacts())
        {
            if (groundContactCount > 1)
                contactNormal.Normalize();
            stepsSinceLastGrounded = 0;
            if (stepsSinceLastJumped > 1)
                jumpPhase = 0;
        }
    }
    void AdjustVelocity()
    {
        Vector3 xAxis = ProjectDirectionOnPlane(rightAxis, contactNormal);
        Vector3 zAxis = ProjectDirectionOnPlane(forwardAxis, contactNormal);

        Vector3 adjustment = Vector3.zero;
        adjustment.x = playerInput.x * maxSpeed - Vector3.Dot(velocity, xAxis);
        adjustment.y = playerInput.y * maxSpeed - Vector3.Dot(velocity, zAxis);

        float acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
        adjustment = Vector3.ClampMagnitude(adjustment, acceleration * Time.deltaTime);

        velocity += xAxis * adjustment.x + zAxis * adjustment.y;
    }

    private void ClearState()
    {
        groundContactCount = steepContactCount = 0;
        contactNormal = steepNormal = Vector3.zero;
    }

    private void Jump()
    {
        Vector3 jumpDirection;
        if (OnGround)
            jumpDirection = contactNormal;
        else if (OnSteep)
        {
            jumpDirection = steepNormal;
            jumpPhase = 0;
        }
        else if (maxAirJumps > 0 && jumpPhase <= maxAirJumps)
        {
            if(jumpPhase == 0)
                jumpPhase = 1;
            jumpDirection = Vector3.up;
        }
        else
            return;

        jumpDirection = (jumpDirection + Vector3.up).normalized;
        stepsSinceLastJumped = 0;
        jumpPhase++;
        float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
        float alignedSpeed = Vector3.Dot(velocity, jumpDirection);
        if (alignedSpeed > 0f)
            jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
        velocity += jumpDirection * jumpSpeed;
    }

    void OnCollisionEnter(Collision collision)
    {
        EvaluateCollision(collision);
    }

    void OnCollisionStay(Collision collision)
    {
        EvaluateCollision(collision);
    }

    void EvaluateCollision(Collision collision)
    {
        float minDot = GetMinDot(collision.gameObject.layer);
        for(int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;
            if(normal.y >= minDot)
            {
                groundContactCount++;
                contactNormal += normal;
            }
            else if(normal.y > -0.01f)
            {
                steepContactCount++;
                steepNormal += normal;
            }
        }
    }

    Vector3 ProjectDirectionOnPlane(Vector3 direction, Vector3 normal)
    {
        return (direction - normal * Vector3.Dot(direction, normal)).normalized;
    }

    bool CheckSteepContacts()
    {
        if(steepContactCount > 1)
        {
            steepNormal.Normalize();
            if(steepNormal.y >= minGroundDotProduct)
            {
                groundContactCount = 1;
                contactNormal = steepNormal;
                return true;
            }
        }
        return false;
    }

    bool SnapToGround()
    {
        if (stepsSinceLastGrounded > 1 || stepsSinceLastJumped <= 2)
            return false;
        float speed = velocity.magnitude;
        if (speed > maxSnapSpeed)
            return false;
        if (!Physics.Raycast(body.position, Vector3.down, out RaycastHit hit, probeDistance, probeMask))
            return false;
        if (hit.normal.y < GetMinDot(hit.collider.gameObject.layer))
            return false;

        groundContactCount = 1;
        contactNormal = hit.normal;
        float dot = Vector3.Dot(velocity, hit.normal);
        if(dot > 0f)
            velocity = (velocity - hit.normal * dot).normalized * speed;
        return true;
    }

    float GetMinDot(int layer)
    {
        return (stairsMask & (1 << layer)) == 0 ?
            minGroundDotProduct : minStairsDotProduct;
    }
}
