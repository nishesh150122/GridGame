using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum BlockState
{
    Vertical,
    HorizontalX,
    HorizontalZ
}
public class NewPlayerMove : MonoBehaviour
{
    [SerializeField] private float rollSpeed = 90f;
    public bool isMoving;
    private BoxCollider boxCollider;
    public BlockState currentState;
    public float verticalThreshold = 1.5f; // tweak if needed
    private void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider>();
        }
    }

    private void Update()
    {
        if (isMoving) return;
        
        if (Input.GetKeyDown(KeyCode.A)) StartCoroutine(Roll(Vector3.left));
        if (Input.GetKeyDown(KeyCode.D)) StartCoroutine(Roll(Vector3.right));
        if (Input.GetKeyDown(KeyCode.W)) StartCoroutine(Roll(Vector3.forward));
        if (Input.GetKeyDown(KeyCode.S)) StartCoroutine(Roll(Vector3.back));
    }
    void UpdateBlockState()
    {
        Vector3 up = transform.up;

        // Dot product tells us alignment with world up
        float yDot = Mathf.Abs(Vector3.Dot(up, Vector3.up));
        float xDot = Mathf.Abs(Vector3.Dot(transform.right, Vector3.up));
        float zDot = Mathf.Abs(Vector3.Dot(transform.forward, Vector3.up));

        if (yDot > 0.9f)
            currentState = BlockState.Vertical;
        else if (xDot > zDot)
            currentState = BlockState.HorizontalX;
        else
            currentState = BlockState.HorizontalZ;
    }


    public bool IsVertical()
    {
        return currentState == BlockState.Vertical;
    }
    IEnumerator Roll(Vector3 dir)
    {
        isMoving = true;
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        // Get anchor and axis
        Vector3 anchor = CalculateAnchor(dir);
        Vector3 axis = Vector3.Cross(Vector3.up, dir);

        float angleRotated = 0;
        float targetAngle = 90f;

        // Smooth roll animation
        while (angleRotated < targetAngle)
        {
            float angleThisFrame = Mathf.Min(rollSpeed * Time.deltaTime, targetAngle - angleRotated);
            transform.RotateAround(anchor, axis, angleThisFrame);
            angleRotated += angleThisFrame;
            yield return null;
        }

        if (rb != null) rb.isKinematic = false;
        UpdateBlockState();
        isMoving = false;
    }

    Vector3 CalculateAnchor(Vector3 dir)
    {
        if (boxCollider == null) return transform.position;

        // Get half extents in world space (like you did for height)
        Vector3 halfExtents = Vector3.Scale(boxCollider.size * 0.5f, transform.lossyScale);
        //Debug.Log(halfExtents);
        
        // World directions of local axes
        Vector3 right = transform.right;
        Vector3 up = transform.up;
        Vector3 forward = transform.forward;
        
        // For rolling, we need the edge that's:
        // 1. At the bottom (most negative in world Y)
        // 2. In the direction we want to roll
        
        // We'll find the point that minimizes (dot with world down) and 
        // minimizes/maximizes (dot with movement direction) appropriately
        
        Vector3 worldDown = Vector3.down;
        
        // Initialize anchor at center
        Vector3 anchorOffset = Vector3.zero;
        
        // For each local axis, determine if we should add or subtract its half extent
        // based on whether it points downward and in the movement direction
        
        // First, let's find the bottom point in the movement direction
        // This is the key insight: We want the point that's most "down + dir" relative to center
        
        // The direction we want to find the extreme point in
        Vector3 targetDirection = worldDown + dir.normalized;
        
        // For each axis, if the axis aligns with targetDirection, add its full extent
        // in that direction
        for (int i = 0; i < 3; i++)
        {
            Vector3 axis = i == 0 ? right : (i == 1 ? up : forward);
            float extent = halfExtents[i];
            
            // Project the axis onto our target direction
            float projection = Vector3.Dot(axis, targetDirection);
            Debug.Log(projection);
            
            // If the projection is positive, this axis contributes positively to our target direction
            if (projection > 0)
            {
                anchorOffset += axis * extent;
            }
            else if (projection < 0)
            {
                anchorOffset -= axis * extent;
            }
            // If projection is 0, the axis is perpendicular, so we don't move along it
        }
        
        return transform.position + anchorOffset;
    }

    // Alternative simpler method using collider bounds (works well for most cases)
    Vector3 CalculateAnchorSimple(Vector3 dir)
    {
        if (boxCollider == null) return transform.position;
        
        Bounds bounds = boxCollider.bounds;
        Vector3 center = bounds.center;
        Vector3 min = bounds.min;
        Vector3 max = bounds.max;
        
        // For each direction, find the appropriate corner
        if (dir == Vector3.left)
        {
            return new Vector3(min.x, min.y, center.z);
        }
        else if (dir == Vector3.right)
        {
            return new Vector3(max.x, min.y, center.z);
        }
        else if (dir == Vector3.forward)
        {
            return new Vector3(center.x, min.y, max.z);
        }
        else if (dir == Vector3.back)
        {
            return new Vector3(center.x, min.y, min.z);
        }
        
        return center;
    }
    
    // Your height calculation (kept for reference)
    float GetColliderHeight(BoxCollider bc)
    {
        Vector3 worldSize = Vector3.Scale(bc.size, bc.transform.lossyScale);
        Transform t = bc.transform;
        float h = Mathf.Abs(Vector3.Dot(t.right, Vector3.up)) * worldSize.x
                  + Mathf.Abs(Vector3.Dot(t.up, Vector3.up)) * worldSize.y
                  + Mathf.Abs(Vector3.Dot(t.forward, Vector3.up)) * worldSize.z;
        return h;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Goal"))
            return;

        if (IsVertical())
        {
            Debug.Log("LEVEL COMPLETE ?? (Vertical on Goal)");
          //  LevelManager.Instance.OnLevelComplete();
        }
        else
        {
            Debug.Log("Touched goal but NOT vertical ?");
        }
    }

}
