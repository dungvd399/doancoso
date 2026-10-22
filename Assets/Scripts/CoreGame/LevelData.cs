using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static Cinemachine.DocumentationSortingAttribute;

[CreateAssetMenu(menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    public int numberOfCar; // so luong car
    public int numberOfColor;
    public int rows = 5; // so hang
    public int columns = 5; // so cot

    public int boxRows = 2; //so hang cua boxx
    public int boxColumns = 3; // so cot cua box

    public int connectId = 0;

    public FoodCell[] cells;
    public FoodCell[] cars;
    public FoodCell[] boxs;
    FoodCell[] newCell;
    FoodCell[] newBox;

    private void OnValidate()
    {
        Init();
    }

    public void Init()
    {
        newCell = new FoodCell[rows * columns];
        for (int i = 0; i < newCell.Length; i++)
            newCell[i] = new FoodCell();

        for (int i = 1; i <= cells.Length; i++)
        {
            if(i <= newCell.Length && i<=cells.Length)
            newCell[newCell.Length - i].color = cells[cells.Length - i].color;
        }

        cells = newCell;

        newBox = new FoodCell[boxRows * boxColumns];
        for (int i = 0; i < newBox.Length; i++)
            newBox[i] = new FoodCell();

        for (int i = 0; i < newBox.Length; i++)
        {
            //if (i < newBox.Length && i < boxs.Length)
            //{
                if (i >= boxs.Length)
                {
                    newBox[i].color = GameColor.Black;
                }
                else
                {
                    newBox[i].color = boxs[i].color;
                    newBox[i].boxType = boxs[i].boxType;
                    newBox[i].connectId = boxs[i].connectId;
                }
            //}
        }

        boxs = newBox;
    }

    public FoodCell GetCell(int r, int c)
    {
        return cells[r * columns + c];
    }

    public FoodCell GetBox(int r, int c)
    {
        if(r * boxColumns + c < boxs.Length)
        return boxs[r * boxColumns + c];
        else
        {
            FoodCell tmp = new  FoodCell();
            tmp.color = GameColor.Black;
            return tmp;
        }
    }

    public void GenerateRandom()
    {
        Debug.Log("Generate Random Level");
        GenerateRandomLevel();
    }

    List<ColorData> currentData;
    Dictionary<GameColor, int> colorPriority = new();
    List<FoodCell> carList;

    public void GenerateRandomLevel()
    {
        InitCarRandom(numberOfColor);
        InitBoxRandom();


#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
#endif
    }

    public void InitCarRandom(int cout)
    {
        colorPriority.Clear();
        rows = numberOfCar * 3;
        if (cout == 0) cout = rows / 3;
        currentData = GenerateColorData(cout);

        carList = new();

        foreach (var data in currentData)
        {
            for (int i = 0; i < data.carCount; i++)
            {
                FoodCell tmp = new();
                tmp.color = data.color;
                carList.Add(tmp);
            }
        }

        carList = Shuffle(carList);
        cars = carList.ToArray();
    }
    List<GameColor> allBoxes = new();
    public void InitBoxRandom()
    {
        allBoxes.Clear();

        foreach (var data in currentData)
        {
            for (int i = 0; i < data.boxCount; i++)
            {
                allBoxes.Add(data.color);
            }
        }

        allBoxes = allBoxes = ReorderBoxes_ByCarPriority(allBoxes, boxRows, boxColumns);
        List<FoodCell> listBox = new List<FoodCell>();
        List<FoodCell> listFood = new List<FoodCell>();
        for (int i = 0; i < allBoxes.Count; i++)
        {
            FoodCell tmp = new FoodCell();
            FoodCell tmp2 = new FoodCell();
            tmp.color = allBoxes[i];
            tmp2.color = allBoxes[allBoxes.Count - 1- i];
            listBox.Add(tmp);
            for (int j = 0; j < columns; j++)
            {
                listFood.Add(tmp2);
            }
        }
        boxs = listBox.ToArray();
        cells = listFood.ToArray();
    }


    public List<ColorData> GenerateColorData(int numberOfColors)
    {
        var colors = GetRandomColors(numberOfColors);

        List<ColorData> result = new();

        boxRows = Mathf.CeilToInt(((float)rows / (float)boxColumns));
        int totalSlot = boxRows * boxColumns;

        // ❗ cần tối thiểu slot cho mỗi màu
        if (totalSlot < numberOfColors * 3)
        {
            Debug.LogWarning("Không đủ slot cho số màu yêu cầu!");
            numberOfColors = totalSlot / 3;
            colors = colors.Take(numberOfColors).ToList();
        }

        int usedBox = 0;

        // ✅ mỗi màu ít nhất 1 car trước
        foreach (var color in colors)
        {
            result.Add(new ColorData
            {
                color = color,
                carCount = 1,
                boxCount = 3,
                foodCount = 30
            });

            usedBox += 3;
        }

        // 🔁 distribute thêm random
        int remainSlot = totalSlot - usedBox;

        while (remainSlot >= 3)
        {
            var rand = result[UnityEngine.Random.Range(0, result.Count)];

            rand.carCount += 1;
            rand.boxCount += 3;
            rand.foodCount += 30;

            remainSlot -= 3;
        }

        return result;
    }

    public static List<GameColor> GetRandomColors(int n)
    {
        var values = Enum.GetValues(typeof(GameColor))
            .Cast<GameColor>()
            .Where(c => c != GameColor.Black) // 🔥 loại màu đen
            .ToList();

        return values
            .OrderBy(x => UnityEngine.Random.value)
            .Take(n)
            .ToList();
    }

    public static List<T> Shuffle<T>(List<T> list)
    {
        return list.OrderBy(x => UnityEngine.Random.value).ToList();
    }

    public List<GameColor> ReorderBoxes_ByCarPriority(List<GameColor> allBoxes, int rows, int columns)
    {
        // 🔹 1. tạo priority từ car
        Dictionary<GameColor, int> colorPriority = new();

        for (int i = 0; i < cars.Length; i++)
        {
            if (!colorPriority.ContainsKey(cars[i].color))
                colorPriority.Add(cars[i].color, i);
        }

        // 🔹 2. sort theo priority (giữ random nhẹ)
        var sorted = allBoxes
            .OrderBy(c => colorPriority.TryGetValue(c, out var p) ? p : 999)
            .ThenBy(_ => UnityEngine.Random.value)
            .ToList();

        // 🔥 3. build lại theo ROW (QUAN TRỌNG)
        GameColor[,] grid = new GameColor[rows, columns];

        int index = 0;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                //if()
                if (index < sorted.Count)
                {
                    grid[r, c] = sorted[index];
                    index++;
                }
            }
        }

        // 🔁 4. convert lại đúng thứ tự spawn (column-first)
        List<GameColor> result = new();
        int a = 0;
        for (int c = 0; c < rows; c++)
        {
            for (int r = 0; r < columns; r++)
            {
                a++;
                if (a <= sorted.Count)
                    result.Add(grid[c, r]);
            }
        }

        return result;
    }

    public List<GameColor> ReorderFoods_ByCarPriority(List<GameColor> allFoods, int rows, int columns)
    {
        // 🔹 1. priority từ car
        Dictionary<GameColor, int> colorPriority = new();

        for (int i = 0; i < cars.Length; i++)
        {
            if (!colorPriority.ContainsKey(cars[i].color))
                colorPriority.Add(cars[i].color, i);
        }

        // 🔹 2. sort theo priority
        var sorted = allFoods
    .OrderByDescending(c => colorPriority.TryGetValue(c, out var p) ? p : -999)
    .ThenBy(_ => UnityEngine.Random.value)
    .ToList();

        // 🔥 3. build lại theo ROW
        GameColor[,] grid = new GameColor[rows, columns];

        int index = 0;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                if (index >= sorted.Count) break;

                grid[r, c] = sorted[index];
                index++;
            }
        }

        // 🔁 4. convert lại đúng thứ tự spawn (column-first)
        List<GameColor> result = new();

        for (int c = 0; c < columns; c++)
        {
            for (int r = 0; r < rows; r++)
            {
                result.Add(grid[r, c]);
            }
        }

        return result;
    }

    public void SwapFoodRows(int rowA, int rowB)
    {
        rowA -= 1;
        rowB -= 1;
        // validate
        if (rowA < 0 || rowA >= rows || rowB < 0 || rowB >= rows)
        {
            Debug.LogError("Row index out of range!");
            return;
        }

        // same row -> nothing to do
        if (rowA == rowB)
            return;

        for (int c = 0; c < columns; c++)
        {
            int indexA = rowA * columns + c;
            int indexB = rowB * columns + c;

            // swap
            FoodCell temp = cells[indexA];
            cells[indexA] = cells[indexB];
            cells[indexB] = temp;
        }

        Debug.Log($"Swapped row {rowA} with row {rowB}");
    }
}

[System.Serializable]
public class FoodCell
{
    public GameColor color;
    public BoxType boxType;
    public int connectId;
    public int starId = 2;
}
