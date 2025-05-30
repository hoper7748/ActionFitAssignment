using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public partial class BoardController
{
    private async Task CreatePlayingBlocksAsync(int stageIdx = 0)
    {
        playingBlockParent = new GameObject("PlayingBlockParent");

        for (int i = 0; i < stageDatas[stageIdx].playingBlocks.Count; i++)
        {
            var pbData = stageDatas[stageIdx].playingBlocks[i];

            GameObject blockGroupObject = Instantiate(blockGroupPrefab, playingBlockParent.transform);
            blockGroupObject.transform.position = new Vector3(
                pbData.center.x * BoardConfig.blockDistance,
                0.33f,
                pbData.center.y * BoardConfig.blockDistance
            );

            BlockDragHandler dragHandler = blockGroupObject.GetComponent<BlockDragHandler>();
            if (dragHandler != null) dragHandler.blocks = new List<BlockObject>();

            dragHandler.uniqueIndex = pbData.uniqueIndex;
            foreach (var gimmick in pbData.gimmicks)
            {
                if (Enum.TryParse(gimmick.gimmickType, out ObjectPropertiesEnum.BlockGimmickType gimmickType))
                {
                    dragHandler.gimmickType.Add(gimmickType);
                }
            }

            int maxX = 0;
            int minX = boardWidth;
            int maxY = 0;
            int minY = boardHeight;
            foreach (var shape in pbData.shapes)
            {
                GameObject singleBlock = Instantiate(blockPrefab, blockGroupObject.transform);

                singleBlock.transform.localPosition = new Vector3(
                    shape.offset.x * BoardConfig.blockDistance,
                    0f,
                    shape.offset.y * BoardConfig.blockDistance
                );
                dragHandler.blockOffsets.Add(new Vector2(shape.offset.x, shape.offset.y));

                var renderer = singleBlock.GetComponentInChildren<SkinnedMeshRenderer>();
                if (renderer != null && pbData.colorType >= 0)
                {
                    renderer.material = testBlockMaterials[(int)pbData.colorType];
                }

                if (singleBlock.TryGetComponent(out BlockObject blockObj))
                {
                    blockObj.colorType = pbData.colorType;
                    blockObj.x = pbData.center.x + shape.offset.x;
                    blockObj.y = pbData.center.y + shape.offset.y;
                    blockObj.offsetToCenter = new Vector2(shape.offset.x, shape.offset.y);

                    if (dragHandler != null)
                        dragHandler.blocks.Add(blockObj);
                    boardBlockDic[((int)blockObj.x, (int)blockObj.y)].playingBlock = blockObj;
                    blockObj.preBoardBlockObject = boardBlockDic[((int)blockObj.x, (int)blockObj.y)];
                    if (minX > blockObj.x) minX = (int)blockObj.x;
                    if (minY > blockObj.y) minY = (int)blockObj.y;
                    if (maxX < blockObj.x) maxX = (int)blockObj.x;
                    if (maxY < blockObj.y) maxY = (int)blockObj.y;
                }
            }

            dragHandler.horizon = maxX - minX + 1;
            dragHandler.vertical = maxY - minY + 1;
        }

        await Task.Yield();
    }

}
