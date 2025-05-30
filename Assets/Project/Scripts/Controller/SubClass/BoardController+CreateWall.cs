using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public partial class BoardController
{
    private async Task CreateCustomWalls(int stageIdx)
    {
        if (stageIdx < 0 || stageIdx >= stageDatas.Length || stageDatas[stageIdx].Walls == null)
        {
            Debug.LogError($"��ȿ���� ���� �������� �ε����̰ų� �� �����Ͱ� �����ϴ�: {stageIdx}");
            return;
        }

        GameObject wallsParent = new GameObject("CustomWallsParent");

        wallsParent.transform.SetParent(boardParent.transform);
        wallCoorInfoDic = new Dictionary<(int x, int y), Dictionary<(DestroyWallDirection, ColorType), int>>();

        foreach (var wallData in stageDatas[stageIdx].Walls)
        {
            Quaternion rotation;

            // �⺻ ��ġ ���
            var position = new Vector3(
                wallData.x * BoardConfig.blockDistance,
                0f,
                wallData.y * BoardConfig.blockDistance);

            DestroyWallDirection destroyDirection = DestroyWallDirection.None;
            bool shouldAddWallInfo = false;
            //await Task.Delay(100);
            // �� ����� ������ ���� ��ġ�� ȸ�� ����
            switch (wallData.WallDirection)
            {
                case ObjectPropertiesEnum.WallDirection.Single_Up:
                    position.z += 0.5f;
                    rotation = Quaternion.Euler(0f, 180f, 0f);
                    shouldAddWallInfo = true;
                    destroyDirection = DestroyWallDirection.Up;
                    break;

                case ObjectPropertiesEnum.WallDirection.Single_Down:
                    position.z -= 0.5f;
                    rotation = Quaternion.identity;
                    shouldAddWallInfo = true;
                    destroyDirection = DestroyWallDirection.Down;
                    break;

                case ObjectPropertiesEnum.WallDirection.Single_Left:
                    position.x -= 0.5f;
                    rotation = Quaternion.Euler(0f, 90f, 0f);
                    shouldAddWallInfo = true;
                    destroyDirection = DestroyWallDirection.Left;
                    break;

                case ObjectPropertiesEnum.WallDirection.Single_Right:
                    position.x += 0.5f;
                    rotation = Quaternion.Euler(0f, -90f, 0f);
                    shouldAddWallInfo = true;
                    destroyDirection = DestroyWallDirection.Right;
                    break;

                case ObjectPropertiesEnum.WallDirection.Left_Up:
                    // ���� �� �𼭸�
                    position.x -= 0.5f;
                    position.z += 0.5f;
                    rotation = Quaternion.Euler(0f, 180f, 0f);
                    break;

                case ObjectPropertiesEnum.WallDirection.Left_Down:
                    // ���� �Ʒ� �𼭸�
                    position.x -= 0.5f;
                    position.z -= 0.5f;
                    rotation = Quaternion.identity;
                    break;

                case ObjectPropertiesEnum.WallDirection.Right_Up:
                    // ������ �� �𼭸�
                    position.x += 0.5f;
                    position.z += 0.5f;
                    rotation = Quaternion.Euler(0f, 270f, 0f);
                    break;

                case ObjectPropertiesEnum.WallDirection.Right_Down:
                    // ������ �Ʒ� �𼭸�
                    position.x += 0.5f;
                    position.z -= 0.5f;
                    rotation = Quaternion.Euler(0f, 0f, 0f);
                    break;

                case ObjectPropertiesEnum.WallDirection.Open_Up:
                    // ������ ���� ��
                    position.z += 0.5f;
                    rotation = Quaternion.Euler(0f, 180f, 0f);
                    break;

                case ObjectPropertiesEnum.WallDirection.Open_Down:
                    // �Ʒ����� ���� ��
                    position.z -= 0.5f;
                    rotation = Quaternion.identity;
                    break;

                case ObjectPropertiesEnum.WallDirection.Open_Left:
                    // ������ ���� ��
                    position.x -= 0.5f;
                    rotation = Quaternion.Euler(0f, 90f, 0f);
                    break;

                case ObjectPropertiesEnum.WallDirection.Open_Right:
                    // �������� ���� ��
                    position.x += 0.5f;
                    rotation = Quaternion.Euler(0f, -90f, 0f);
                    break;

                default:
                    Debug.LogError($"�������� �ʴ� �� ����: {wallData.WallDirection}");
                    continue;
            }

            if (shouldAddWallInfo && wallData.wallColor != ColorType.None)
            {
                var pos = (wallData.x, wallData.y);
                var wallInfo = (destroyDirection, wallData.wallColor);

                if (!wallCoorInfoDic.ContainsKey(pos))
                {
                    Dictionary<(DestroyWallDirection, ColorType), int> wallInfoDic =
                        new Dictionary<(DestroyWallDirection, ColorType), int> { { wallInfo, wallData.length } };
                    wallCoorInfoDic.Add(pos, wallInfoDic);
                }
                else
                {
                    wallCoorInfoDic[pos].Add(wallInfo, wallData.length);
                }
            }

            // ���̿� ���� ��ġ ���� (����/���� ���� ����)
            if (wallData.length > 1)
            {
                // ���� ���� �߾� ��ġ ���� (Up, Down ����)
                if (wallData.WallDirection == ObjectPropertiesEnum.WallDirection.Single_Up ||
                    wallData.WallDirection == ObjectPropertiesEnum.WallDirection.Single_Down ||
                    wallData.WallDirection == ObjectPropertiesEnum.WallDirection.Open_Up ||
                    wallData.WallDirection == ObjectPropertiesEnum.WallDirection.Open_Down)
                {
                    // x������ �߾����� �̵�
                    position.x += (wallData.length - 1) * BoardConfig.blockDistance * 0.5f;
                }
                // ���� ���� �߾� ��ġ ���� (Left, Right ����)
                else if (wallData.WallDirection == ObjectPropertiesEnum.WallDirection.Single_Left ||
                         wallData.WallDirection == ObjectPropertiesEnum.WallDirection.Single_Right ||
                         wallData.WallDirection == ObjectPropertiesEnum.WallDirection.Open_Left ||
                         wallData.WallDirection == ObjectPropertiesEnum.WallDirection.Open_Right)
                {
                    // z������ �߾����� �̵�
                    position.z += (wallData.length - 1) * BoardConfig.blockDistance * 0.5f;
                }
            }

            // �� ������Ʈ ����, isOriginal = false
            // prefabIndex�� length-1 (�� ������ �迭�� �ε���)
            if (wallData.length - 1 >= 0 && wallData.length - 1 < wallPrefabs.Length)
            {
                GameObject wallObj = Instantiate(wallPrefabs[wallData.length - 1], wallsParent.transform);
                wallObj.transform.position = position;
                wallObj.transform.rotation = rotation;
                WallObject wall = wallObj.GetComponent<WallObject>();
                wall.SetWall(wallMaterials[(int)wallData.wallColor], wallData.wallColor != ColorType.None);
                walls.Add(wallObj);
            }
            else
            {
                Debug.LogError($"������ �ε��� ������ ���: {wallData.length - 1}, ��� ������ ������: 0-{wallPrefabs.Length - 1}");
            }
        }

        await Task.Yield();
    }
}
