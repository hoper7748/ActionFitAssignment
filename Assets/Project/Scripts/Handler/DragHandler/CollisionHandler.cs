using UnityEngine;

public class CollisionHandler : MonoBehaviour
{
    private BlockDragHandler context;

    public CollisionHandler(BlockDragHandler ctx) => context = ctx;

    private void Start()
    {
        context = GetComponent<BlockDragHandler>();
    }

    public void OnEnter(Collision col) => Process(col);
    public void OnStay(Collision col) => Process(col);

    private void Process(Collision col)
    {
        if (!context.isDragging || col.contactCount == 0) return;
        if (col.gameObject.layer == LayerMask.NameToLayer("Board")) return;

        Vector3 normal = col.contacts[0].normal;
        if (Vector3.Dot(normal, Vector3.up) < 0.8f)
        {
            context.isColliding = true;
            context.lastCollisionNormal = normal;
            context.lastCollisionTime = Time.time;
        }
    }

    public void OnExit(Collision col)
    {
        if (col.contactCount == 0) return;
        Vector3 normal = col.contacts[0].normal;

        if (Vector3.Dot(normal, context.lastCollisionNormal) > 0.8f)
            Reset();
    }

    public void CheckAutoReset()
    {
        if (context.isColliding && Time.time - context.lastCollisionTime > context.collisionResetTime)
            Reset();
    }

    public void Reset()
    {
        context.isColliding = false;
        context.lastCollisionNormal = Vector3.zero;
    }
}
