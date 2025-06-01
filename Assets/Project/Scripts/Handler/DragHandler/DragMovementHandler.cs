using UnityEngine;

public class DragMovementHandler : MonoBehaviour
{
    private BlockDragHandler context;

    public DragMovementHandler(BlockDragHandler ctx) => context = ctx;

    private void Start()
    {
        context = GetComponent<BlockDragHandler>();
    }

    public void FixedUpdate()
    {
        if (!context.Enabled || !context.isDragging) return;

        Vector3 mouseWorldPos = context.GetMouseWorldPosition();
        Vector3 targetPos = mouseWorldPos + context.offset;
        Vector3 moveVec = targetPos - context.transform.position;

        Vector3 velocity = context.isColliding
            ? Vector3.ProjectOnPlane(moveVec, context.lastCollisionNormal) * context.moveSpeed
            : moveVec * context.followSpeed;

        if (velocity.magnitude > context.maxSpeed)
            velocity = velocity.normalized * context.maxSpeed;

        context.rb.linearVelocity = Vector3.Lerp(context.rb.linearVelocity, velocity, Time.fixedDeltaTime * 10f);
    }
}
