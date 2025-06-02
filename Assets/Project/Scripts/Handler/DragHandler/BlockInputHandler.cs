using UnityEngine;

public class DragInputHandler : MonoBehaviour
{
    private BlockDragHandler context;

    public DragInputHandler(BlockDragHandler ctx) => context = ctx;

    private void Start()
    {
        context = GetComponent<BlockDragHandler>();
    }

    public void Update()
    {

    }

    public void OnMouseDown()
    {
        if (!context.Enabled) return;

        context.isDragging = true;
        context.rb.isKinematic = false;
        context.outline.enabled = true;

        context.zDistanceToCamera = Vector3.Distance(context.transform.position, context.mainCamera.transform.position);
        context.offset = context.transform.position - context.GetMouseWorldPosition();
        context.ResetCollisionState();
    }

    public void OnMouseUp()
    {
        context.isDragging = false;
        context.outline.enabled = false;
        context.rb.linearVelocity = Vector3.zero;
        context.rb.isKinematic = true;

        context.SetBlockPosition();
        context.ResetCollisionState();
    }

}
