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
    private readonly string[] colorOptions = { "Green", "Red", "Blue", "Yellow", "Orange", "Pink" };
    private Color[] colors = {
        Color.green,
        Color.red,
        Color.blue,
        Color.yellow,
        new Color(1f, 0.5f, 0f), // Orange
        new Color(1f, 0.4f, 0.7f) // Pink
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
    private void InitData()
    {
        LoadSOData();
    }

    private void Draw()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        var oldLabelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 90;

        // �� ������ �Է�

        InitData();

        GUILayout.Space(20);

        // ��ư�� ������ ��ǥ�� ��ġ (x, y, width, height)
        Rect buttonRect = new Rect(350, 0, 150, 40);
        if (currentLevelDataSO != null)
        {
            DrawEditor();
        }
        else
        {
            if (GUI.Button(buttonRect, "Stage Data ����"))
            {
                CreateMyDataAsset();
            }
        }
        buttonRect = new Rect(550, 0, 75, 40);
        if (GUI.Button(buttonRect, "����"))
        {
            EditorApplication.isPlaying = !EditorApplication.isPlaying;
        }
        EditorGUILayout.EndScrollView();
    }

    private void CreateMyDataAsset()
    {        // �ν��Ͻ� ����
        StageData asset = ScriptableObject.CreateInstance<StageData>();

        // ��� ����
        string path = "Assets/Project/Resource/Data/StageData So";
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        string uniquePath = AssetDatabase.GenerateUniqueAssetPath($"{path}/StageData.asset");

        // ���� ���� �� ����
        AssetDatabase.CreateAsset(asset, uniquePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // ������ ������ ����
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;

        Debug.Log("ScriptableObject ���� �Ϸ�: " + uniquePath);
        levelDbObj = EditorGUILayout.ObjectField("Asset", asset, typeof(StageData), false, GUILayout.Width(340));
        currentLevelDataSO = (StageData)levelDbObj;
    }

    private void DrawEditor()
    {
        GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(500));

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

        // ���� ���� ���� �� �� �ִ� ������ ������ ����

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


        selectedColorIndex = EditorGUILayout.Popup("����", selectedColorIndex, colorOptions, GUILayout.Width(200));

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

        // wall�� ��� ����� ������
    }

    private void DrawBoard()
    {
        if (currentLevelDataSO.boardBlocks.Count < 0)
            return;

        // �ϴ� ���� �ͺ��� �غ� ������? �ٴں��� �׷���
        GUILayout.Space(tileSize * 0.5f);
        GUILayout.BeginHorizontal();
        GUILayout.Space(tileSize * 0.5f);

        GUILayout.EndHorizontal();
        GUILayout.Space(tileSize * 0.5f);
    }

    private void NewDrawBoard()
    {
        if (tileData.Count < 0)
            return;
        // �ϴ� ���� �ͺ��� �غ� ������? �ٴں��� �׷���
        GUILayout.Space(tileSize * 0.5f);
        GUILayout.BeginHorizontal();
        GUILayout.Space(tileSize * 0.5f);
        for(int x = 0; x <= column+ 2; x++)
        {
            GUILayout.BeginVertical();   // �� ������ ����
            for (int y = row + 2; y >= 0; y--)
            {
                CreateButton(tileData[y][x]);
            }
            GUILayout.EndHorizontal(); // ���� ������ �ݱ�
        }
        GUILayout.EndHorizontal();  
        GUILayout.Space(tileSize * 0.5f);
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
                return new Color(1f, 0.4f, 0.7f); // Pink
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

    private void CreateButton(EditTileData boardData)
    {
        GUI.color = SetGUIColor(boardData.color);
        if (boardData.row > 0 && boardData.col > 0 && boardData.col < column + 2 && boardData.row < row + 2)
        {
            if (GUILayout.Button("Ÿ��", GUILayout.Width(tileSize), GUILayout.Height(tileSize)))
            {
                // �� �����µ� ���ǿ� �°� ���� ��������.
                // ��, �Ʒ�, ������ ���� 
                if (boardData.row > 0 && boardData.col > 0 && boardData.col < column + 2 && boardData.row < row + 2)
                    Debug.Log($"��ǥ {boardData.col - 1} / {boardData.row - 1}");
                else
                    Debug.Log($"�� : {boardData.col} / {boardData.row}");
            }
        }
        else if(!(boardData.row == 0 && boardData.col == 0) && !(boardData.row == 0 && boardData.col == column + 2) && 
            !(boardData.row == row + 2 && boardData.col == 0) && !(boardData.row == row + 2 && boardData.col == column + 2))
        {
            if (GUILayout.Button("��", GUILayout.Width(tileSize), GUILayout.Height(tileSize)))
            {
                if (boardData.row > 0 && boardData.col > 0 && boardData.col < column + 2 && boardData.row < row + 2)
                    Debug.Log($"��ǥ {boardData.col - 1} / {boardData.row - 1}");
                else
                    Debug.Log($"�� : {boardData.col} / {boardData.row}");
            }
        }
        else
        {
            GUILayout.Space(tileSize + tileSize * 0.1f);
        }
    }

    private void CheckColorLRTB(int x, int y)
    {
        // 4���� �� ���� ����� ������ �ȴٸ�?
        // �Ǵ� ����� ������ ���ٸ�?
        //if (tileData[y][x])
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
                        // �Ϲ� Ÿ��
                        if (y != 0 && x != 0 && x != column + 2 && y != row + 2)
                        {
                            tempTile.col = x;
                            tempTile.row = y;
                            tempTile.colorTypes = currentLevelDataSO.boardBlocks[count].colorType;
                            count++;
                        }
                        // �� 
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

                // PlayerBlock
                foreach( var pBlocks in currentLevelDataSO.playingBlocks)
                {
                    PlayerBlocks.Add(pBlocks.colorType, pBlocks.shapes);

                    foreach(var shapes in pBlocks.shapes)
                    {
                        tileData[pBlocks.center.y + shapes.offset.y + 1][pBlocks.center.x + shapes.offset.x + 1].color = pBlocks.colorType;
                    }
                }
            }
        }
    }

    Dictionary<ColorType, List<ShapeData>> PlayerBlocks = new Dictionary<ColorType, List<ShapeData>>();
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
                Debug.Log("��ȿ�� ������");
            }
            catch
            {
                Debug.LogError("��ȿ���� ���� ������");
                currentLevelDataJson = null;
            }
        }
    }




}
