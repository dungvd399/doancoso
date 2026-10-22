using DG.Tweening;
using Ifreet.Core.Runtime.Audio;
using Ifreet.Core.Runtime.Audio.Generated;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UIElements;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    private void Awake()
    {
        Instance = this;
        if((float)Screen.width / (float)Screen.height < 0.49f)
        {
            boxDistance = new Vector2(2.85f, boxDistance.y);
        }
    }

    public Transform[] foodColumn;
    public Transform carContainer;
    public Transform mainCarPos;
    public Transform backCarPos;
    public Transform nextCarPos;
    public Transform boxContainer;
    public float foodDistance;
    public Vector2 boxDistance;
    public LevelData levelData;
    public Car mainCar;
    public Car backCar;
    public UITutorial uiTut;

    public List<Queue<Box>> boxColumnData = new();
    public List<Stack<Food>> foodColumnData = new();
    int idCar = 0;
    public bool isWin = false;
    public bool isLose = false;
    public int levelStartLoop;
    public int levelEndLoop;
    public Color[] colorRef;
    public bool isRandom = false;
    
    [Button]
    public void ChooseLevel(int level)
    {
        PlayerprefSave.Level = level;
    }

    void Start()
    {
        //LoadLevel();
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ReloadLevel();
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            PlayerprefSave.Level += 1;
            ReloadLevel();
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            PlayerprefSave.Level -= 1;
            if (PlayerprefSave.Level <= 1) PlayerprefSave.Level = 1;
            ReloadLevel();
        }
    }
#endif
    List<GameColor> colors = new List<GameColor>();
    public void LoadLevel()
    {
        isWin = false;
        isLose = false;
        int level = PlayerprefSave.Level;
        if(level <=levelEndLoop) // logic loop level 86-94
        {
            level = PlayerprefSave.Level;
        }
        else
        {
            int range = levelEndLoop - levelStartLoop + 1;
            level = levelStartLoop + (level - levelStartLoop) % range;
        }
        levelData = Resources.Load<LevelData>("Level/Level " + level.ToString()); // load scriptable obj level tu folder resource/Level
        UIMainGameplay.Instance.ChangeLevelText();
        if (isRandom)
        {
            //levelData.rows = RoundRowsToMultipleOf3(levelData.rows);
                InitCarRandom(levelData.numberOfColor);
            InitBoxRandom();
            InitBoardRandom();
            InitTutorial();
        }
        else
        {
            InitBoard();
            InitCar();
            InitBox();
            InitTutorial();
        }
    }

    //public void LoadLevelRandom()
    //{
    //    isWin = false;
    //    isLose = false;

    //    InitCarRandom();
    //    InitBoxRandom();
    //    InitBoardRandom();
    //    InitTutorial();
    //}

    public List<ColorData> GenerateColorData(int numberOfColors)
    {
        var colors = GetRandomColors(numberOfColors);

        List<ColorData> result = new();

        levelData.boxRows = Mathf.CeilToInt(((float)levelData.rows /(float) levelData.boxColumns));
        int totalSlot = levelData.boxRows * levelData.boxColumns;

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

    List<ColorData> currentData;
    List<GameColor> remainingCars = new();
    List<GameColor> carList;
    public void InitCarRandom(int cout)
    {
        colorPriority.Clear();
        Debug.Log("chuongcout" + cout);
        levelData.rows = RoundRowsToMultipleOf3(levelData.rows);
        if (cout == 0) cout = levelData.rows/3;
        currentData = GenerateColorData(cout);

        carList = new();

        foreach (var data in currentData)
        {
            for (int i = 0; i < data.carCount; i++)
            {
                carList.Add(data.color);
            }
        }

        carList = Shuffle(carList);

        // MAIN CAR
        GameObject car = PoolManager.Instance.Spawn("Car", mainCarPos.position, Quaternion.identity);
        car.transform.SetParent(carContainer);
        mainCar = car.GetComponent<Car>();
        mainCar.Setup(carList[0]);
        mainCar.canUse = true;

        // BACK CAR
        if (carList.Count > 1)
        {
            GameObject car2 = PoolManager.Instance.Spawn("Car", backCarPos.position, Quaternion.identity);
            car2.transform.SetParent(carContainer);
            backCar = car2.GetComponent<Car>();
            backCar.Setup(carList[1]);
        }

        idCar = 2;

        // lưu lại list để spawn tiếp
        remainingCars = carList;

        for (int i = 0; i < remainingCars.Count; i++)
        {
            if (!colorPriority.ContainsKey(remainingCars[i]))
            {
                colorPriority.Add(remainingCars[i], i);
            }
        }


    }
    Dictionary<GameColor, int> colorPriority = new();
    public List<GameColor> GenerateRowFoods(int rows, int columns, List<ColorData> data)
    {
        GameColor[,] grid = new GameColor[rows, columns];

        // 🔹 tạo list màu cho từng hàng
        List<GameColor> rowColors = new();

        foreach (var d in data)
        {
            int count = Mathf.Max(1, d.foodCount / columns);

            for (int i = 0; i < count; i++)
            {
                rowColors.Add(d.color);
            }
        }

        // đảm bảo đủ số hàng
        while (rowColors.Count < rows)
        {
            rowColors.Add(data[UnityEngine.Random.Range(0, data.Count)].color);
        }

        rowColors = rowColors.Take(rows).ToList();

        // shuffle hàng
        rowColors = rowColors.OrderBy(x => UnityEngine.Random.value).ToList();

        // 🎯 fill theo ROW (đúng nghĩa)
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                grid[r, c] = rowColors[r];
            }
        }

        // 🔥 convert sang list đúng thứ tự SPAWN của bạn (column-first)
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

    public void SpawnNextCarRandom()
    {
        Debug.Log("chuong" + idCar);
        if (idCar > carList.Count)
        {
            if (!isWin && !isLose)
            {
                isWin = true;
                StartCoroutine(IE_ShowWin());
            }
            return;
        }
        if (idCar < carList.Count)
        {
            GameObject car = PoolManager.Instance.Spawn("Car", backCarPos.position, Quaternion.identity);
        car.transform.SetParent(carContainer);

        backCar = car.GetComponent<Car>();
            if (isRandom)
            {
                Debug.Log("Random");
                backCar.Setup(carList[idCar]);
            }
            else
            {
                backCar.Setup(remainingCars[idCar]);
            }

            backCar.transform.position = backCarPos.position - new Vector3(5, 0, 0);
            backCar.transform.DOMove(backCarPos.position, 1).SetEase(Ease.Linear);
        }

        idCar++;
    }

    public List<GameColor> ReorderBoxes_ByCarPriority(List<GameColor> allBoxes, int rows, int columns)
    {
        // 🔹 1. tạo priority từ car
        Dictionary<GameColor, int> colorPriority = new();

        for (int i = 0; i < remainingCars.Count; i++)
        {
            if (!colorPriority.ContainsKey(remainingCars[i]))
                colorPriority.Add(remainingCars[i], i);
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

        for (int i = 0; i < remainingCars.Count; i++)
        {
            if (!colorPriority.ContainsKey(remainingCars[i]))
                colorPriority.Add(remainingCars[i], i);
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

    public void InitBoxRandom()
    {
        boxColumnData.Clear();
        boxColumnData = new List<Queue<Box>>();

        List<GameColor> allBoxes = new();

        foreach (var data in currentData)
        {
            for (int i = 0; i < data.boxCount; i++)
            {
                allBoxes.Add(data.color);
            }
        }

        allBoxes = allBoxes = ReorderBoxes_ByCarPriority(allBoxes, levelData.boxRows, levelData.boxColumns);

        int index = 0;

        for (int i = 0; i < levelData.boxColumns; i++)
        {
            Queue<Box> queue = new();

            for (int j = 0; j < levelData.boxRows; j++)
            {
                if (index >= allBoxes.Count) continue;
                if (allBoxes[index] == GameColor.Black) continue;
                GameObject box = PoolManager.Instance.Spawn("Box", Vector3.zero, Quaternion.identity);
                box.transform.SetParent(boxContainer);

                box.transform.localPosition = new Vector3(
                    (levelData.boxColumns % 2 != 0 ? i - levelData.boxColumns / 2 : i - levelData.boxColumns / 2 + 0.5f) * boxDistance.x,
                    0,
                    j == 0 ? 0 : -boxDistance.y * j - 1
                );

                Box _box = box.GetComponent<Box>();
                _box.Setup(allBoxes[index]);

                if (j == 0) _box.SetCanPlay();
                else _box.SetCannotPlay();

                _box.columnId = i;
                _box.rowId = j;

                queue.Enqueue(_box);
                ListAllBox.Add(_box);

                index++;
            }

            boxColumnData.Add(queue);
        }
    }

    public List<GameColor> GenerateRowFoods_ByCarPriority(int rows, int columns)
    {
        // 🔹 priority theo car
        Dictionary<GameColor, int> colorPriority = new();
        for (int i = 0; i < remainingCars.Count; i++)
        {
            if (!colorPriority.ContainsKey(remainingCars[i]))
                colorPriority.Add(remainingCars[i], i);
        }

        // 🔹 tạo màu cho từng hàng
        List<GameColor> rowColors = new();

        foreach (var carColor in remainingCars)
        {
            if (!rowColors.Contains(carColor))
                rowColors.Add(carColor);
        }

        while (rowColors.Count < rows)
        {
            rowColors.Add(currentData[UnityEngine.Random.Range(0, currentData.Count)].color);
        }

        rowColors = rowColors.Take(rows).ToList();

        // sort theo car priority
        rowColors = rowColors
            .OrderBy(c => colorPriority.TryGetValue(c, out var p) ? p : 999)
            .ToList();

        // 🔥 build list theo COLUMN-FIRST (QUAN TRỌNG)
        List<GameColor> result = new();

        for (int c = 0; c < columns; c++)      // column trước
        {
            for (int r = 0; r < rows; r++)     // row sau
            {
                result.Add(rowColors[r]);      // 🔥 key fix
            }
        }

        return result;
    }

    public void InitBoardRandom()
    {
        foodColumnData.Clear();
        foodColumnData = new List<Stack<Food>>();

        List<GameColor> allFoods = new();

        foreach (var data in currentData)
        {
            for (int i = 0; i < data.foodCount; i++)
            {
                allFoods.Add(data.color);
            }
        }

        allFoods = ReorderFoods_ByCarPriority(allFoods, levelData.rows, levelData.columns);
        int index = 0;

        for (int i = 0; i < levelData.columns; i++)
        {
            Stack<Food> stack = new();

            for (int j = 0; j < levelData.rows; j++)
            {
                if (index >= allFoods.Count) continue;

                GameObject food = PoolManager.Instance.Spawn("Food", Vector3.zero, Quaternion.identity);
                food.transform.SetParent(foodColumn[i]);

                food.transform.localPosition = new Vector3(0, foodDistance * (levelData.rows - j - 1), 0);

                Food _food = food.GetComponent<Food>();
                _food.Setup(allFoods[index]);
                _food.RowId = levelData.rows - j - 1;

                stack.Push(_food);

                index++;
            }

            foodColumnData.Add(stack);
        }
    }
    public int RoundRowsToMultipleOf3(int rows)
    {
        int down = rows - (rows % 3);        // làm tròn xuống
        int up = down + 3;                   // làm tròn lên

        // chọn số gần hơn
        return (rows - down <= up - rows) ? down : up;
    }

    public static List<T> Shuffle<T>(List<T> list)
    {
        return list.OrderBy(x => UnityEngine.Random.value).ToList();
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

    public void InitTutorial()
    {
        if(PlayerprefSave.Level == 1)
        {
            uiTut.gameObject.SetActive(true);
            uiTut.ShowHandClick(boxContainer.transform.position);
        }
        else
        {
            uiTut.gameObject.SetActive(false);
        }
    }

    public void InitBoard()
    {
        foodColumnData.Clear();
        foodColumnData = new List<Stack<Food>>();
        for (int i = 0; i < levelData.columns; i++)
        {
            Stack<Food> stack = new();
            for (int j = 0; j < levelData.rows; j++)
            {
                if (levelData != null)
                    if (levelData.GetCell(j, i).color != GameColor.Black)
                    {
                        GameObject food = PoolManager.Instance.Spawn("Food", Vector3.zero, Quaternion.identity);
                        food.transform.SetParent(foodColumn[i]);
                        food.transform.localPosition = new Vector3(0, foodDistance * (levelData.rows - j - 1), 0);
                        food.transform.localRotation = Quaternion.identity;
                        food.transform.localScale = Vector3.one;
                        Food _food = food.GetComponent<Food>();
                        _food.Setup(levelData.GetCell(j, i).color);
                        _food.RowId = levelData.rows - j - 1;
                        stack.Push(_food);
                    }
            }
            foodColumnData.Add(stack);
        }
    }

    public void InitCar()
    {
        GameObject car = PoolManager.Instance.Spawn("Car", mainCarPos.position, Quaternion.identity);
        car.transform.SetParent(carContainer);
        mainCar = car.GetComponent<Car>();
        mainCar.carType = levelData.cars[0].boxType;
        mainCar.starId = levelData.cars[0].starId;
        mainCar.Setup(levelData.cars[0].color);
        mainCar.canUse = true;
        GameObject car2 = PoolManager.Instance.Spawn("Car", backCarPos.position, Quaternion.identity);
        car2.transform.SetParent(carContainer);
        backCar = car2.GetComponent<Car>();
        backCar.carType = levelData.cars[1].boxType;
        backCar.starId = levelData.cars[1].starId;
        backCar.Setup(levelData.cars[1].color);
        idCar = 2;
    }
    public List<Box> ListAllBox;
    public void InitBox()
    {
        boxColumnData.Clear();
        boxColumnData = new List<Queue<Box>>();
        for (int i = 0; i < levelData.boxColumns; i++)
        {
            Queue<Box> queue = new();
            for (int j = 0; j < levelData.boxRows; j++)
            {
                if (levelData != null && j * levelData.boxColumns + i < levelData.boxs.Length)
                    if (levelData.GetBox(j, i).color != GameColor.Black)
                    {
                        GameObject box = PoolManager.Instance.Spawn("Box", Vector3.zero, Quaternion.identity);
                        box.transform.SetParent(boxContainer);
                        box.transform.localPosition = new Vector3((levelData.boxColumns % 2 != 0 ? i - levelData.boxColumns / 2 : i - levelData.boxColumns / 2 + 0.5f) * boxDistance.x, 0, j == 0 ? 0 : -boxDistance.y * j - 1);
                        box.transform.localRotation = Quaternion.identity;
                        Box _box = box.GetComponent<Box>();
                        if (levelData != null)
                        {
                            _box.boxType = levelData.GetBox(j, i).boxType;
                            _box.connectId = levelData.GetBox(j, i).connectId;
                            _box.Setup(levelData.GetBox(j, i).color);
                        }
                        if (j == 0) _box.SetCanPlay();
                        else _box.SetCannotPlay();
                            _box.columnId = i;
                        _box.rowId = j;
                        box.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
                        queue.Enqueue(_box);
                        ListAllBox.Add(_box);
                    }
            }

            boxColumnData.Add(queue);
        }
    }

    public void ActionBoxColumn(int id, bool isPlaceHolder = false) // move tat ca cac hang len 1 don vi
    {
        boxColumnData[id].Dequeue();
        foreach (Box item in boxColumnData[id]) //duyet qua tat cac hang
        {
            item.MoveUp(); //di chuyen len 1 don vi
        }
        if (boxColumnData[id].Count > 0 && !isPlaceHolder)
            boxColumnData[id].Peek().SetCanPlay2();
    }

    public void SpawnNextCar()
    {
        if (idCar >= levelData.cars.Length)
        {
            if (backCar != null)
            {
                backCar = null;
            }
            else
            {
                if (LevelManager.Instance.isWin || LevelManager.Instance.isLose) return;
                isWin = true;
                StartCoroutine(IE_ShowWin());
            }
        }
        else
        {
            GameObject car = PoolManager.Instance.Spawn("Car", backCarPos.position, Quaternion.identity);
            car.transform.SetParent(carContainer);
            backCar = car.GetComponent<Car>();
            backCar.carType = levelData.cars[idCar].boxType;
            backCar.starId = levelData.cars[idCar].starId;
            backCar.Setup(levelData.cars[idCar].color);
            backCar.transform.position = backCarPos.position - new Vector3(5, 0, 0);
            backCar.transform.DOMove(backCarPos.position, 1).SetEase(Ease.Linear);//ham di chuyen xe vao vi tri day
            idCar += 1;
        }
    }

    IEnumerator IE_ShowWin()
    {
        yield return new WaitForSeconds(0.5f);
        AudioManager.Instance.PlaySfx(AudioSfxID.sfx_complete_xoxo);
        GameController.Instance.btnReplay.interactable = false;
        UIMainGameplay.Instance.OnShowWin();
    }

    public void ClearLevel() //xoa du lieu level cu
    {

        foreach (var item in foodColumn)
        {
            DOTween.Kill(item.transform);
            item.transform.localPosition = new Vector3(item.transform.localPosition.x, 0, item.transform.localPosition.z);
        }
        if (mainCar != null)
            PoolManager.Instance.Despawn("Car", mainCar.gameObject);
        if (backCar != null)
            PoolManager.Instance.Despawn("Car", backCar.gameObject);

        foreach (var item in boxColumnData)
        {
            foreach (var item2 in item)
            {
                item2.transform.DOKill();
                PoolManager.Instance.Despawn("Box", item2.gameObject);
            }
        }
        foreach (var item in foodColumnData)
        {
            foreach (var item2 in item)
            {
                PoolManager.Instance.Despawn("Food", item2.gameObject);
            }
        }
        boxColumnData.Clear();
        foodColumnData.Clear();
        ListAllBox.Clear();
        GameController.Instance.Reset();
    }

    public void ReloadLevel() // tao lai level moi
    {
        ClearLevel(); // xoa du lieu level
        LoadLevel(); // load level
    }
}

public class ColorData
{
    public GameColor color;
    public int carCount;
    public int boxCount;
    public int foodCount;
}
