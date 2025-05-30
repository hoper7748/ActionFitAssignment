using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class BoardController
{

    public async Task CreateBoardAsync(int stageIdx = 0)
    {
        nowStageIndex = stageIdx;
        int standardBlockIndex = -1;

        // ���� ��� ����
        for (int i = 0; i < StageDatas[stageIdx].boardBlocks.Count; i++)
        {
            BoardBlockData data = StageDatas[stageIdx].boardBlocks[i];

            GameObject blockObj = Instantiate(boardBlockPrefab, boardParent.transform);
            blockObj.transform.localPosition = new Vector3(
                data.x * BoardConfig.blockDistance,
                0,
                data.y * BoardConfig.blockDistance
            );

            if (blockObj.TryGetComponent(out BoardBlockObject boardBlock))
            {
                boardBlock._ctrl = this;
                boardBlock.x = data.x;
                boardBlock.y = data.y;
                if (wallCoorInfoDic.ContainsKey((boardBlock.x, boardBlock.y)))
                {
                    for (int k = 0; k < wallCoorInfoDic[(boardBlock.x, boardBlock.y)].Count; k++)
                    {
                        boardBlock.colorType.Add(wallCoorInfoDic[(boardBlock.x, boardBlock.y)].Keys.ElementAt(k).Item2);
                        boardBlock.len.Add(wallCoorInfoDic[(boardBlock.x, boardBlock.y)].Values.ElementAt(k));

                        DestroyWallDirection dir = wallCoorInfoDic[(boardBlock.x, boardBlock.y)].Keys.ElementAt(k).Item1;
                        bool horizon = dir == DestroyWallDirection.Up || dir == DestroyWallDirection.Down;
                        boardBlock.isHorizon.Add(horizon);

                        standardBlockDic.Add((++standardBlockIndex, horizon), boardBlock);
                    }
                    boardBlock.isCheckBlock = true;
                }
                else
                {
                    boardBlock.isCheckBlock = false;
                }

                boardBlockDic.Add((data.x, data.y), boardBlock);
            }
            else
            {
                Debug.LogWarning("boardBlockPrefab�� BoardBlockObject ������Ʈ�� �ʿ��մϴ�!");
            }
        }

        // standardBlockDic���� ���� ��ġ�� ��ϵ� ����
        foreach (var kv in standardBlockDic)
        {
            BoardBlockObject boardBlockObject = kv.Value;
            for (int i = 0; i < boardBlockObject.colorType.Count; i++)
            {
                if (kv.Key.Item2) // ���� ����
                {
                    for (int j = boardBlockObject.x + 1; j < boardBlockObject.x + boardBlockObject.len[i]; j++)
                    {
                        if (boardBlockDic.TryGetValue((j, boardBlockObject.y), out BoardBlockObject targetBlock))
                        {
                            targetBlock.colorType.Add(boardBlockObject.colorType[i]);
                            targetBlock.len.Add(boardBlockObject.len[i]);
                            targetBlock.isHorizon.Add(kv.Key.Item2);
                            targetBlock.isCheckBlock = true;
                        }
                    }
                }
                else // ���� ����
                {
                    for (int k = boardBlockObject.y + 1; k < boardBlockObject.y + boardBlockObject.len[i]; k++)
                    {
                        if (boardBlockDic.TryGetValue((boardBlockObject.x, k), out BoardBlockObject targetBlock))
                        {
                            targetBlock.colorType.Add(boardBlockObject.colorType[i]);
                            targetBlock.len.Add(boardBlockObject.len[i]);
                            targetBlock.isHorizon.Add(kv.Key.Item2);
                            targetBlock.isCheckBlock = true;
                        }
                    }
                }
            }
        }

        // 3üũ ��� �׷� ����
        int checkBlockIndex = -1;
        CheckBlockGroupDic = new Dictionary<int, List<BoardBlockObject>>();

        foreach (var blockPos in boardBlockDic.Keys)
        {
            BoardBlockObject boardBlock = boardBlockDic[blockPos];

            for (int j = 0; j < boardBlock.colorType.Count; j++)
            {
                if (boardBlock.isCheckBlock && boardBlock.colorType[j] != ColorType.None)
                {
                    // �� ����� �̹� �׷쿡 �����ִ��� Ȯ��
                    if (boardBlock.checkGroupIdx.Count <= j)
                    {
                        if (boardBlock.isHorizon[j])
                        {
                            // ���� ��� Ȯ��
                            (int x, int y) leftPos = (boardBlock.x - 1, boardBlock.y);
                            if (boardBlockDic.TryGetValue(leftPos, out BoardBlockObject leftBlock) &&
                                j < leftBlock.colorType.Count &&
                                leftBlock.colorType[j] == boardBlock.colorType[j] &&
                                leftBlock.checkGroupIdx.Count > j)
                            {
                                int grpIdx = leftBlock.checkGroupIdx[j];
                                CheckBlockGroupDic[grpIdx].Add(boardBlock);
                                boardBlock.checkGroupIdx.Add(grpIdx);
                            }
                            else
                            {
                                checkBlockIndex++;
                                CheckBlockGroupDic.Add(checkBlockIndex, new List<BoardBlockObject>());
                                CheckBlockGroupDic[checkBlockIndex].Add(boardBlock);
                                boardBlock.checkGroupIdx.Add(checkBlockIndex);
                            }
                        }
                        else
                        {
                            // ���� ��� Ȯ��
                            (int x, int y) upPos = (boardBlock.x, boardBlock.y - 1);
                            if (boardBlockDic.TryGetValue(upPos, out BoardBlockObject upBlock) &&
                                j < upBlock.colorType.Count &&
                                upBlock.colorType[j] == boardBlock.colorType[j] &&
                                upBlock.checkGroupIdx.Count > j)
                            {
                                int grpIdx = upBlock.checkGroupIdx[j];
                                CheckBlockGroupDic[grpIdx].Add(boardBlock);
                                boardBlock.checkGroupIdx.Add(grpIdx);
                            }
                            else
                            {
                                checkBlockIndex++;
                                CheckBlockGroupDic.Add(checkBlockIndex, new List<BoardBlockObject>());
                                CheckBlockGroupDic[checkBlockIndex].Add(boardBlock);
                                boardBlock.checkGroupIdx.Add(checkBlockIndex);
                            }
                        }
                    }
                }
            }
        }

        await Task.Yield();

        boardWidth = boardBlockDic.Keys.Max(k => k.x);
        boardHeight = boardBlockDic.Keys.Max(k => k.y);
    }
}
