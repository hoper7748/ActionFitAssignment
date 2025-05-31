using Project.Scripts.Data_Script;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using Unity.VisualScripting.YamlDotNet.Serialization;
using UnityEditor;
using UnityEngine;
using UnityEngine.WSA;
using TextAsset = UnityEngine.TextAsset;

public class MapEditor : EditorWindow
{
    private Object levelDbObj;
    private TextAsset currentLevelDataJson;
    private StageData currentLevelDataSO;
    private int tileSize = 40;
    private Vector2 scrollPos;
    private int row, column;
    private BoardBlockData temp;
    private int selectedColorIndex = 0;

    private int selectedIndex = 0;
    private readonly string[] colorOptions = { "None" ,"Red", "Orange", "Yellow", "Gray", "Purple", "Begic", "Blue", "Green", };

    private static bool[] colorSet = { false, false, false, false, false, false, false, false, false };
    private readonly Color[] colors = {
        Color.white,
        Color.red,
        new Color(1f, 0.5f, 0f), // Orange
        Color.yellow,
        new Color(0.5f, 0.5f, 0.5f),     // Gray
        new Color(0.6f, 0f, 0.6f),       // Purple
        new Color(0.96f, 0.96f, 0.86f),  // Begic
        Color.blue,
        Color.green,
    };

    [MenuItem("Tools/Map Editor", false, 0)]
    private static void Init()
    {
        var window = GetWindow(typeof(MapEditor));
        window.titleContent = new GUIContent("Map Editor");
    }

    private void OnGUI()
    {
        Draw();
    }

    private void ColorSetting()
    {
        GUILayout.Label("사용할 색", EditorStyles.boldLabel);
        GUILayout.Space(10);

        for (int i = 1; i < colorOptions.Length; i++)
        {
            GUILayout.BeginHorizontal();

            // 색 미리보기 박스
            Color originalColor = GUI.color;
            GUI.color = colors[i];
            GUILayout.Box("", GUILayout.Width(20), GUILayout.Height(20));
            GUI.color = originalColor;

            // 색 이름과 선택 버튼
            GUILayout.Label(colorOptions[i], GUILayout.Width(100));

            if (colorSet[i])
            {
                if (GUILayout.Button("< 선택됨", EditorStyles.boldLabel))
                {
                    colorSet[i] = false;
                }
            }
            else if (GUILayout.Button("선택", GUILayout.Width(60)))
            {
                colorSet[i] = true;
            }

            GUILayout.EndHorizontal();
        }
    }
    private void InitData()
    {
        LoadSOData();
    }

    private void Draw()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        var oldLabelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 90;

        // 맵 데이터 입력

        InitData();

        GUILayout.Space(20);

        // 버튼을 지정된 좌표에 배치 (x, y, width, height)
        Rect buttonRect = new Rect(350, 0, 150, 40);
        if (currentLevelDataSO != null)
        {
            DrawEditor();
        }
        else
        {
            if (GUI.Button(buttonRect, "Stage Data 생성"))
            {
                CreateMyDataAsset();
            }
        }
        buttonRect = new Rect(550, 0, 75, 40);
        if (GUI.Button(buttonRect, "시작"))
        {
            EditorApplication.isPlaying = !EditorApplication.isPlaying;
        }
        EditorGUILayout.EndScrollView();
    }

    private void CreateMyDataAsset()
    {        // 인스턴스 생성
        StageData asset = ScriptableObject.CreateInstance<StageData>();

        // 경로 설정
        string path = "Assets/Project/Resource/Data/StageData So";
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        string uniquePath = AssetDatabase.GenerateUniqueAssetPath($"{path}/StageData.asset");

        // 에셋 생성 및 저장
        AssetDatabase.CreateAsset(asset, uniquePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // 생성된 파일을 선택
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;

        Debug.Log("ScriptableObject 생성 완료: " + uniquePath);
        levelDbObj = EditorGUILayout.ObjectField("Asset", asset, typeof(StageData), false, GUILayout.Width(340));
        currentLevelDataSO = (StageData)levelDbObj;
    }

    private void DrawEditor()
    {
        GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(700));

        var style = new GUIStyle
        {
            fontSize = 20,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };
        EditorGUILayout.LabelField("Map Edit", style);
        GUILayout.Space(10);


        GUILayout.BeginHorizontal(GUILayout.Width(300));
        EditorGUILayout.HelpBox(
            "The general settings of this level.",
            MessageType.Info);
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        // 보드 전에 세팅 할 수 있는 공간이 있으면 좋음

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("column"),
            GUILayout.Width(EditorGUIUtility.labelWidth));
        column = EditorGUILayout.IntField(column, GUILayout.Width(30));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("row"),
            GUILayout.Width(EditorGUIUtility.labelWidth));
        row = EditorGUILayout.IntField(row, GUILayout.Width(30));

        GUILayout.EndHorizontal();

        ColorSetting();

        selectedColorIndex = EditorGUILayout.Popup("색상", selectedColorIndex, colorOptions, GUILayout.Width(200));

        GUILayout.Space(10f);

        if (GUILayout.Button("Create Map", GUILayout.Width(250), GUILayout.Height(tileSize)))
        {
            CreateLevel();
        }

        NewDrawBoard();

        //DrawWall();
    
        GUILayout.EndVertical();

    }

    private void DrawWall()
    {
        
    }



    private void CreateLevel()
    {
        currentLevelDataSO.boardBlocks.Clear();
        for (int y= 0; y < row; y++)
        {
            for (int x = 0; x < column; x++)
            {
                temp = new BoardBlockData();
                temp.x = x;
                temp.y = y;
                currentLevelDataSO.boardBlocks.Add(temp);
            }
        }
    }
    
    public class EditTileData
    {
        public int row, col;
        public ColorType color;
        public List<ColorType> colorTypes;
        public int dataType;

        // wall일 경우 사용할 데이터
    }

    private void DrawBoard()
    {
        if (currentLevelDataSO.boardBlocks.Count < 0)
            return;

        // 일단 띄우는 것부터 해봐 뭐부터? 바닥부터 그려봐
        GUILayout.Space(tileSize * 0.5f);
        GUILayout.BeginHorizontal();
        GUILayout.Space(tileSize * 0.5f);

        GUILayout.EndHorizontal();
        GUILayout.Space(tileSize * 0.5f);
    }

    private void NewDrawBoard()
    {
        if (tileData.Count <= 0)
            return;

        int boardPixelWidth = (column + 4) * tileSize;

        // 버튼 UI 영역 시작 (왼쪽 상단 고정)
        GUILayout.BeginArea(new Rect(300, 100, boardPixelWidth, 9999)); // x=10으로 왼쪽 정렬
        for (int y = row + 2; y >= 0; y--)
        {
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            for (int x = 0; x <= column + 2; x++)
            {
                CreateButton(tileData[y][x]);
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndArea();
    }

    ShapeData tempShapeData;
    private Vector2Int selectedBoardPosition;
    private void CreateButton(EditTileData boardData)
    {
        GUI.color = SetGUIColor(boardData.color);

        if (boardData.row > 0 && boardData.col > 0 && boardData.col < column + 2 && boardData.row < row + 2)
        {
            if (GUILayout.Button("타일", GUILayout.Width(tileSize), GUILayout.Height(tileSize)))
            {
                Debug.Log($"좌표 {boardData.col - 1} / {boardData.row - 1}");
                if(selectedColorIndex > 0 && CheckColorLRTB(boardData.col, boardData.row))
                {
                    tempShapeData = new ShapeData();
                    selectedBoardPosition = new Vector2Int(boardData.col - 1, boardData.row - 1);
                    tempShapeData.offset -= PlayerBlocks[(ColorType)selectedColorIndex].center - selectedBoardPosition;
                    // 연결된 색이 있다면 현재 타일의 색을 체크 해야함 만약 색이 있다면 컨테이너에서 지워주고 색을 칠해줘야함.
                    if (PlayerBlocks.ContainsKey(boardData.color))
                    {
                        Debug.Log("선택된 타일에 다른 색이 존재");
                        foreach(var item in PlayerBlocks[boardData.color].shapes)
                        {
                            if(item.offset + PlayerBlocks[boardData.color].center == selectedBoardPosition)
                            {
                                PlayerBlocks[boardData.color].shapes.Remove(item);
                                break;
                            }
                        }
                    }
                    boardData.color = (ColorType)selectedColorIndex;
                    // 연결된 색이 없으면 그냥 색 칠하고 Shape에 추가;
                    if (selectedColorIndex > 0 && !PlayerBlocks[(ColorType)selectedColorIndex].shapes.Contains(tempShapeData))
                        PlayerBlocks[(ColorType)selectedColorIndex].shapes.Add(tempShapeData);
                }
                else if(selectedColorIndex == 0)
                {
                    if (PlayerBlocks.ContainsKey(boardData.color))
                    {
                        Debug.Log("선택된 타일에 다른 색이 존재");
                        foreach (var item in PlayerBlocks[boardData.color].shapes)
                        {
                            if (item.offset + PlayerBlocks[boardData.color].center == selectedBoardPosition)
                            {
                                PlayerBlocks[boardData.color].shapes.Remove(item);
                                break;
                            }
                        }
                    }
                    boardData.color = (ColorType)selectedColorIndex;
                }
            }
        }
        else if (!(boardData.row == 0 && boardData.col == 0) && !(boardData.row == 0 && boardData.col == column + 2) &&
                 !(boardData.row == row + 2 && boardData.col == 0) && !(boardData.row == row + 2 && boardData.col == column + 2))
        {
            if (GUILayout.Button("벽", GUILayout.Width(tileSize), GUILayout.Height(tileSize)))
            {
                Debug.Log($"벽 : {boardData.col} / {boardData.row}");
            }
        }
        else
        {
            GUILayout.Space(tileSize + tileSize * 0.15f) ;
        }
    }

    private bool CheckColorLRTB(int x, int y)
    {
        // 4방향 중 같은 색상과 연결이 된다면?
        // 또는 연결된 색상이 없다면?
        if(x > 0 && x < column + 2 && tileData[y][x].color == (ColorType)selectedColorIndex )
        {
            Debug.Log("FF");
            return true;
        }
        else if (x > 0 && x < column + 2 && tileData[y][x + 1].color == (ColorType)selectedColorIndex)
        {
            Debug.Log("AA");
            return true;
        }
        else if (x > 0 && x < column + 2 && tileData[y][x - 1].color == (ColorType)selectedColorIndex)
        {
            Debug.Log("BB");
            return true;
        }
        else if (y > 0 && y < row + 2 && tileData[y + 1][x ].color == (ColorType)selectedColorIndex)
        {
            Debug.Log("CC");
            return true;
        }
        else if (y > 0 && y < row + 2 && tileData[y - 1][x ].color == (ColorType)selectedColorIndex)
        {
            Debug.Log("DD");
            return true;
        }
        return false;
    }


    private Color SetGUIColor(ColorType color)
    {
        switch (color)
        {
            case ColorType.None:
                break;
            case ColorType.Red:
                return Color.red;
            case ColorType.Orange:
                return new Color(1f, 0.5f, 0f); // Orange
            case ColorType.Yellow:
                return Color.yellow;
            case ColorType.Gray:
                return Color.gray;
            case ColorType.Purple:
                return new Color(0.6f, 0f, 0.6f); // Pink
            case ColorType.Beige:
                return new Color(0.96f, 0.96f, 0.86f);
            case ColorType.Blue:
                return Color.blue;
            case ColorType.Green:
                return Color.green;
            default:
                break;
        }
        return Color.white;
    }

    public List<List<EditTileData>> tileData = new List<List<EditTileData>>();

    private void LoadSOData()
    {
        var oldDb = levelDbObj;
        
        levelDbObj = EditorGUILayout.ObjectField("Asset", levelDbObj, typeof(StageData), false, GUILayout.Width(340));
        if (levelDbObj != oldDb)
        {
            currentLevelDataSO = (StageData)levelDbObj;
            row = currentLevelDataSO.boardBlocks[currentLevelDataSO.boardBlocks.Count - 1].y;
            column = currentLevelDataSO.boardBlocks[currentLevelDataSO.boardBlocks.Count - 1].x;
            tileData.Clear();

            EditTileData tempTile = null;

            if (currentLevelDataSO != null)
            {
                int count = 0;
                for (int y = 0; y <= row + 2; y++)
                {
                    List<EditTileData> temp = new List<EditTileData>();
                    for (int x = 0; x <= column + 2; x++)
                    {
                        tempTile = new EditTileData();
                        // 일반 타일
                        if (y != 0 && x != 0 && x != column + 2 && y != row + 2)
                        {
                            tempTile.col = x;
                            tempTile.row = y;
                            tempTile.colorTypes = currentLevelDataSO.boardBlocks[count].colorType;
                            count++;
                        }
                        // 문 
                        else
                        {
                            tempTile.col = x;
                            tempTile.row = y;
                        }
                        temp.Add(tempTile);
                    }
                        tileData.Add(temp);
                }
                WallContainer.Clear();
                for (int y = 1; y < row + 2; y++)
                {
                    InitWall(0, y);
                    InitWall(column, y);
                    //tileData[y][0]
                }
                for (int x = 1; x < column + 2; x++)
                {
                    InitWall(x, 0);
                    InitWall(x, row);
                    //tileData[y][0]
                }
                PlayerBlocks.Clear();

                for(int i =0; i  < colorSet.Length; i++)
                {
                    colorSet[i] = false;
                }

                // PlayerBlock
                foreach( var pBlocks in currentLevelDataSO.playingBlocks)
                {
                    PlayerBlocks.Add(pBlocks.colorType, pBlocks);
                    colorSet[(int)pBlocks.colorType - 1] = true;

                    foreach(var shapes in pBlocks.shapes)
                    {
                        tileData[pBlocks.center.y + shapes.offset.y + 1][pBlocks.center.x + shapes.offset.x + 1].color = pBlocks.colorType;
                    }
                }
            }
        }
    }

    Dictionary<ColorType, PlayingBlockData> PlayerBlocks = new Dictionary<ColorType, PlayingBlockData>();
    //Dictionary<ColorType, List<WallData>>

    List<WallData> WallContainer = new List<WallData>();

    private void InitWall(int x, int y)
    {
        for (int i = 0; i < currentLevelDataSO.Walls.Count; i++)
        {
            //if (SetWall(currentLevelDataSO.Walls[i], x, y)) return;
            if (x == 0 || x == column)
            {
                int iy = y - 1;
                if (!WallContainer.Contains(currentLevelDataSO.Walls[i]) && SetWall(currentLevelDataSO.Walls[i], x, iy)) return;
            }
            else if (y == 0 || y == row)
            {
                int ix = x - 1;
                if (!WallContainer.Contains(currentLevelDataSO.Walls[i]) && SetWall(currentLevelDataSO.Walls[i], ix, y)) return;
            }
        }
    }

    private bool SetWall(WallData curWall ,int x ,int y)
    {

        if (x == curWall.x && y == curWall.y)
        {
            switch (curWall.WallDirection)
            {
                case ObjectPropertiesEnum.WallDirection.None:
                    break;
                case ObjectPropertiesEnum.WallDirection.Single_Up:
                case ObjectPropertiesEnum.WallDirection.Single_Down:
                    if (curWall.y == row)
                        y += 2;
                    x += 1;
                    for (int i = 0; i < curWall.length; i++)
                    {
                        tileData[y ][x + i].color = curWall.wallColor;
                    }
                    break;
                case ObjectPropertiesEnum.WallDirection.Single_Left:
                case ObjectPropertiesEnum.WallDirection.Single_Right:

                    if (curWall.x == column)
                        x += 2;
                    y += 1;
                    for (int i = 0; i < curWall.length; i++)
                    {
                        tileData[y + i][x].color = curWall.wallColor;
                    }
                    break;
                case ObjectPropertiesEnum.WallDirection.Left_Up:
                    break;
                case ObjectPropertiesEnum.WallDirection.Left_Down:
                    break;
                case ObjectPropertiesEnum.WallDirection.Right_Up:
                    break;
                case ObjectPropertiesEnum.WallDirection.Right_Down:
                    break;
                case ObjectPropertiesEnum.WallDirection.Open_Up:
                    break;
                case ObjectPropertiesEnum.WallDirection.Open_Down:
                    break;
                case ObjectPropertiesEnum.WallDirection.Open_Left:
                    break;
                case ObjectPropertiesEnum.WallDirection.Open_Right:
                    break;
                default:
                    break;
            }
            WallContainer.Add(curWall);
            return true;
        }
        return false;
    }


    private void DrawJsonData()
    {
        var oldDb = levelDbObj;
        levelDbObj = EditorGUILayout.ObjectField("Asset", levelDbObj, typeof(TextAsset), false, GUILayout.Width(340));
        if (levelDbObj != oldDb)
        {
            currentLevelDataJson = (TextAsset)levelDbObj;
            try
            {
                var check = JsonUtility.FromJson<StageData>(currentLevelDataJson.text);
                Debug.Log("유효한 데이터");
            }
            catch
            {
                Debug.LogError("유효하지 않은 데이터");
                currentLevelDataJson = null;
            }
        }
    }




}
