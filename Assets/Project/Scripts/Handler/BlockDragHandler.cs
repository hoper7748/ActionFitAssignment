
using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class BlockDragHandler : MonoBehaviour
{
    private DragInputHandler inputHandler;
    private DragMovementHandler movementHandler;
    private CollisionHandler collisionHandler;
    public int horizon = 1;
    public int vertical = 1;
    public int uniqueIndex;
    public List<ObjectPropertiesEnum.BlockGimmickType> gimmickType;
    public List<BlockObject> blocks = new List<BlockObject>();
    public List<Vector2> blockOffsets = new List<Vector2>();
    public bool Enabled = true;

    public Vector2 centerPos;
    public Camera mainCamera;
    public Rigidbody rb;
    public bool isDragging = false;
    public Vector3 offset;
    public float zDistanceToCamera;
    public float maxSpeed = 20f;
    public Outline outline;
    public Vector2 previousXY;

    // 충돌 감지 변수
    public Collider col { get; set; }
    public bool isColliding = false;
    public Vector3 lastCollisionNormal;
    public float collisionResetTime = 0.1f; // 충돌 상태 자동 해제 시간
    public float lastCollisionTime;
    public float moveSpeed = 25f;
    public float followSpeed = 30f;

    void Start()
    {
        inputHandler = new DragInputHandler(this);
        movementHandler = new DragMovementHandler(this);
        collisionHandler = new CollisionHandler(this);

        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody>();

        rb.useGravity = false;
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; // 충돌 감지 모드 향상

        outline = gameObject.AddComponent<Outline>();
        outline.OutlineMode = Outline.Mode.OutlineAll;
        outline.OutlineColor = Color.yellow;
        outline.OutlineWidth = 2f;
        outline.enabled = false;
    }

    void Update()
    {
        inputHandler.Update();
        collisionHandler.CheckAutoReset();
    }

    void FixedUpdate()
    {
        movementHandler.FixedUpdate();
    }

    void OnMouseDown() => inputHandler.OnMouseDown();
    void OnMouseUp() => inputHandler.OnMouseUp();

    void OnCollisionEnter(Collision col) => collisionHandler.OnEnter(col);
    void OnCollisionStay(Collision col) => collisionHandler.OnStay(col);
    void OnCollisionExit(Collision col) => collisionHandler.OnExit(col);

    public void ResetCollisionState() => collisionHandler.Reset();

    public Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPosition = Input.mousePosition;
        mouseScreenPosition.z = zDistanceToCamera;
        return mainCamera.ScreenToWorldPoint(mouseScreenPosition);
    }

    public void SetBlockPosition(bool mouseUp = true)
    {
        Ray ray = new Ray(transform.position, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 coordinate = hit.transform.position;

            Vector3 targetPos = new Vector3(coordinate.x, transform.position.y, coordinate.z);
            if (mouseUp) transform.position = targetPos;

            centerPos.x = Mathf.Round(transform.position.x / 0.79f);
            centerPos.y = Mathf.Round(transform.position.z / 0.79f);

            if (hit.collider.TryGetComponent(out BoardBlockObject boardBlockObject))
            {
                foreach (var blockObject in blocks)
                {
                    blockObject.SetCoordinate(centerPos);
                }
                foreach (var blockObject in blocks)
                {
                    boardBlockObject.CheckAdjacentBlock(blockObject, targetPos);
                    blockObject.CheckBelowBoardBlock(targetPos);
                }
            }
        }
        else
        {
            Debug.LogWarning("Nothing Detected");
        }
    }
    public void ReleaseInput()
    {
        if (col != null) col.enabled = false;
        isDragging = false;
        outline.enabled = false;
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;
    }

    public Vector3 GetCenterX()
    {
        if (blocks == null || blocks.Count == 0)
        {
            return Vector3.zero; // Return default value if list is empty
        }

        float minX = float.MaxValue;
        float maxX = float.MinValue;

        foreach (var block in blocks)
        {
            float blockX = block.transform.position.x;

            if (blockX < minX)
            {
                minX = blockX;
            }

            if (blockX > maxX)
            {
                maxX = blockX;
            }
        }

        // Calculate the middle value between min and max
        return new Vector3((minX + maxX) / 2f, transform.position.y, 0);
    }

    public Vector3 GetCenterZ()
    {
        if (blocks == null || blocks.Count == 0)
        {
            return Vector3.zero; // Return default value if list is empty
        }

        float minZ = float.MaxValue;
        float maxZ = float.MinValue;

        foreach (var block in blocks)
        {
            float blockZ = block.transform.position.z;

            if (blockZ < minZ)
            {
                minZ = blockZ;
            }

            if (blockZ > maxZ)
            {
                maxZ = blockZ;
            }
        }

        return new Vector3(transform.position.x, transform.position.y, (minZ + maxZ) / 2f);
    }

    private void ClearPreboardBlockObjects()
    {
        foreach (var b in blocks)
        {
            if (b.preBoardBlockObject != null)
            {
                b.preBoardBlockObject.playingBlock = null;
            }
        }
    }

    public void DestroyMove(Vector3 pos, ParticleSystem particle)
    {
        ClearPreboardBlockObjects();

        transform.DOMove(pos, 1f).SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                Destroy(particle.gameObject);
                Destroy(gameObject);
                //block.GetComponent<BlockShatter>().Shatter();
            });
    }
}