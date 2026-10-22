#if UNITY_EDITOR
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelData))]
public class LevelDataEditor : Editor //logic de lam giao dien cho LevelData, ke thua tu Editor cua UnityEditor
{
    LevelData level;
    GameColor paintColor = GameColor.Red;
    BoxType boxType = BoxType.Normal;

    const int CELL_SIZE = 40;
    Dictionary<int, List<Rect>> connectRects = new();
    int swapRowA = 0;
    int swapRowB = 1;

    void OnEnable()
    {
        level = (LevelData)target;

        if (level.cells == null || level.cells.Length != level.rows * level.columns)
        {
            level.Init();
            EditorUtility.SetDirty(level);
        }
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GUILayout.Space(10); // ve khoang trong

        // ===== COLOR PALETTE =====
        GUILayout.Label("Paint Color", EditorStyles.boldLabel); // ve 1 label
        paintColor = (GameColor)GUILayout.Toolbar(
            (int)paintColor,
            System.Enum.GetNames(typeof(GameColor))
        );

        GUILayout.Space(10);

        // ===== DRAW GRID =====
        DrawGrid(); // ve grid

        GUILayout.Space(10);

        GUILayout.Label("Box", EditorStyles.boldLabel);
        paintColor = (GameColor)GUILayout.Toolbar(
            (int)paintColor,
            System.Enum.GetNames(typeof(GameColor))
        );

        GUILayout.Space(10);

        GUILayout.Label("Type", EditorStyles.boldLabel);
        boxType = (BoxType)EditorGUILayout.EnumFlagsField(boxType);

        GUILayout.Space(10);
        if (GUILayout.Button("Clear Connect"))
        {
            connectRects.Clear();
        }

        // ===== DRAW GRID =====
        DrawGrid2(); // ve grid cua box
        DrawConnectLines();
        if (GUI.changed)
        {
            EditorUtility.SetDirty(level);
        }

        GUILayout.Space(10);
        if (GUILayout.Button("Generate Level Random")) // ve button
        {
            level.GenerateRandom();
        }

        GUILayout.Space(10);

        GUILayout.Label("Swap Food Rows", EditorStyles.boldLabel);

        swapRowA = EditorGUILayout.IntField("Row A", swapRowA);
        swapRowB = EditorGUILayout.IntField("Row B", swapRowB);

        if (GUILayout.Button("Swap Row"))
        {
            level.SwapFoodRows(swapRowA, swapRowB);

            EditorUtility.SetDirty(level);

#if UNITY_EDITOR
            AssetDatabase.SaveAssets();
#endif
        }
    }

    void DrawGrid()
    {
        for (int r = 0; r < level.rows; r++)
        {
            GUILayout.BeginHorizontal();
            for (int c = 0; c < level.columns; c++)
            {
                DrawCell(r, c);
            }
            GUILayout.EndHorizontal();
        }
    }

    void DrawGrid2()
    {
        for (int r = 0; r < level.boxRows; r++)// duyet hang
        {
            GUILayout.BeginHorizontal(); /// bat dau 1 hang ngang
            for (int c = 0; c < level.boxColumns; c++) // duyet het cot de ve xong 1hang ngang
            {
                DrawCell2(r, c);
            }
            GUILayout.EndHorizontal(); // ket thuc 1 hang ngang
        }
    }

    const float GAP = 2f;

    void DrawCell(int r, int c)
    {
        FoodCell cell = level.GetCell(r, c);

        // Rect gốc (kể cả khoảng hở)
        Rect rect = GUILayoutUtility.GetRect(CELL_SIZE + GAP, CELL_SIZE + GAP);

        // Thu nhỏ lại để tạo khe hở
        rect.x += GAP * 0.5f;
        rect.y += GAP * 0.5f;
        rect.width -= GAP;
        rect.height -= GAP;

        UnityEditor.EditorGUI.DrawRect(rect, GetUIColor(cell.color));

        Event e = Event.current;
        if (rect.Contains(e.mousePosition))
        {
            if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && e.button == 0) // check vi tri chuot thao tac
            {
                cell.color = paintColor; // gan mau cua box = mau paintcolor
                e.Use();
            }
        }
    }

    void DrawCell2(int r, int c)
    {
        FoodCell cell = level.GetBox(r, c);

        Color oldColor = GUI.backgroundColor;
        GUI.backgroundColor = GetUIColor(cell.color);

        if (GUILayout.Button("", GUILayout.Width(CELL_SIZE), GUILayout.Height(CELL_SIZE)))
        {
            cell.color = paintColor;
            cell.boxType = boxType;
            cell.connectId = level.connectId;
        }

        if (Event.current.type == EventType.Repaint && cell.boxType.HasFlag(BoxType.Connect))
        {
            Rect rect = GUILayoutUtility.GetLastRect();

            if (!connectRects.ContainsKey(cell.connectId))
                connectRects[cell.connectId] = new List<Rect>();

            connectRects[cell.connectId].Add(rect);
        }
        string label = "";
        if(cell.boxType.HasFlag(BoxType.Normal))
        {
            label += "Normal\n";
        }
        if (cell.boxType.HasFlag(BoxType.Large))
        {
            label += "Large\n";
        }
        if (cell.boxType.HasFlag(BoxType.Secret))
        {
            label += "Secret\n";
        }
        if (cell.boxType.HasFlag(BoxType.Star))
        {
            label += "Star\n";
        }
        if (cell.boxType.HasFlag(BoxType.Connect))
        {
            label += "Connect\n";
        }

        GUILayout.Label(label);

        GUI.backgroundColor = oldColor;
    }

    void DrawConnectLines()
    {
        Handles.BeginGUI();
        Handles.color = Color.green;

        foreach (var pair in connectRects)
        {
            var list = pair.Value;
            if (list.Count < 2) continue;

            for (int i = 0; i < list.Count - 1; i++)
            {
                Vector2 p1 = list[i].center;
                Vector2 p2 = list[i + 1].center;

                Handles.DrawLine(p1, p2);
            }
        }

        Handles.EndGUI();
    }

    Color GetUIColor(GameColor color)// quy dinh mau cho lop enum
    {
        return color switch
        {
            GameColor.Red => new Color(0.9f, 0.2f, 0.2f),
            GameColor.Blue => new Color(0.2f, 0.6f, 1f),
            GameColor.Green => new Color(0.3f, 0.85f, 0.3f),
            GameColor.Yellow => new Color(1f, 0.9f, 0.2f),
            GameColor.Purple => new Color(0.65f, 0.35f, 0.9f),
            GameColor.Orange => new Color(1f, 0.55f, 0.15f),
            GameColor.Pink => new Color(1f, 0.4f, 0.7f),
            GameColor.White => Color.white,
            GameColor.Emerald => new Color(0.1f, 0.8f, 0.6f),
            GameColor.StrongBlue => new Color(0.05f, 0.2f, 0.9f),
            _ => Color.gray
        };
    }
}
#endif

