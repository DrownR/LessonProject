using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyChase : MonoBehaviour, IFixedUpdatable
{
    [SerializeField] private Transform player;
    private Vector3 currentPlayerPos;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float rotationSpeed = 8f;

    [Header("Climbing")]
    [SerializeField] private float wallCheckDistance = 1f;
    [SerializeField] private float climbSpeed = 3f;
    [SerializeField] private float minClimbAngle = 60f;
    [SerializeField] private float maxClimbAngle = 120f;
    [SerializeField] private float wallStickDistance = 0.5f;

    [Header("Optimization")]
    [SerializeField] private float maxUpdateStartDistortion = 0.1f;
    [SerializeField] private float updateRate = 0.2f;

    private Rigidbody rb;
    private bool isClimbing;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        UpdateManager.Instance.Register(this);
    }

    private void OnDisable()
    {
        UpdateManager.Instance.Unregister(this);
    }

    private void Start()
    {
        float offset = Random.Range(0f, maxUpdateStartDistortion);
        InvokeRepeating(nameof(UpdateAI), offset, updateRate);
    }

    // AI UPDATE (LOW FREQUENCY)
    private void UpdateAI()
    {
        if (player == null) return;

        currentPlayerPos = player.position;

        WallCheck();
    }

    //  PHYSICS LOOP
    public void ManagedFixedUpdate()
    {
        if (player == null) return;

        if (isClimbing)
            Climb();
        else
            ChasePlayer();
    }

    // CHASE PLAYER (USES CACHED POSITION)
    private void ChasePlayer()
    {
        rb.useGravity = true;

        Vector3 direction = currentPlayerPos - transform.position;
        direction.y = 0f;

        float sqrDist = direction.sqrMagnitude;

        if (sqrDist < 0.01f)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            return;
        }

        direction /= Mathf.Sqrt(sqrDist); // faster normalize

        // Rotation
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        rb.MoveRotation(Quaternion.Slerp(
            rb.rotation,
            targetRotation,
            rotationSpeed * Time.fixedDeltaTime
        ));

        // Movement
        Vector3 velocity = transform.forward * moveSpeed;
        velocity.y = rb.linearVelocity.y;

        rb.linearVelocity = velocity;
    }

    // WALL CHECK (LOW FREQUENCY)
    private void WallCheck()
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;

        if (Physics.Raycast(origin, transform.forward, out RaycastHit hit, wallCheckDistance))
        {
            // Ignore player
            if (hit.transform == player)
                return;

            float angle = Vector3.Angle(hit.normal, Vector3.up);

            if (angle > minClimbAngle && angle < maxClimbAngle)
            {
                if (!isClimbing)
                    StartClimbing(hit);
            }
            else if (isClimbing)
            {
                StopClimbing();
            }
        }
        else if (isClimbing)
        {
            StopClimbing();
        }
    }

    // CLIMBING
    private void Climb()
    {
        rb.useGravity = false;

        rb.linearVelocity = new Vector3(0, climbSpeed, 0);

        if (Physics.Raycast(transform.position + Vector3.up, transform.forward, out RaycastHit hit, wallCheckDistance))
        {
            Vector3 targetPos = hit.point + hit.normal * wallStickDistance;

            rb.MovePosition(Vector3.Lerp(
                rb.position,
                targetPos,
                15f * Time.fixedDeltaTime
            ));

            if (hit.rigidbody != null)
            {
                rb.linearVelocity += hit.rigidbody.linearVelocity;
            }
        }
    }

    private void StartClimbing(RaycastHit hit)
    {
        isClimbing = true;
        rb.useGravity = false;

        Vector3 lookDir = -hit.normal;
        lookDir.y = 0f;

        if (lookDir != Vector3.zero)
        {
            rb.MoveRotation(Quaternion.LookRotation(lookDir));
        }
    }

    private void StopClimbing()
    {
        isClimbing = false;
        rb.useGravity = true;
    }
}