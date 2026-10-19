using UnityEngine;
using UnityEngine.UI;
using Game.src.core.utils;
using Game.src.core.tables;
using Game.src.core.entities.Building;
using Game.src.core.utils.Factory;
using Game.src.core.entities.Terrain;
using Game.src.core.entities.Enemies;
using Game.src.Level;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine.SceneManagement;

public class LevelController : MonoBehaviour
{
    [Header("Grid params")]
    private int size = 15;
    public RectTransform gameArea;

    [Header("Cell prefabs")]
    public GameObject plains;
    public GameObject water;
    public GameObject mountains;

    [Header("Building prefabs")]
    public GameObject tower;
    public GameObject wall;
    public GameObject castl;
    public GameObject spawnr;

    [Header("Enemy prefabs")]
    public GameObject lightInf;
    public GameObject heavyInf;
    public GameObject avia;

    [Header("UI components")]
    public TMP_Text goldText;
    public TMP_Text scoreText;
    public TMP_Text hpText;
    public Button towerButton;
    public Button wallButton;
    public Button repairButton;
    public Button upgradeButton;
    public TMP_Text buildModeText;
    public TMP_Text errorText;

    private LevelManager level;
    private int[,] map;
    private List<GameObject> spriteMapBuildings = new List<GameObject>();
    private Dictionary<Enemy, GameObject> enemySprites = new Dictionary<Enemy, GameObject>();
    private float time = 0f;
    private BuilderState buildState = BuilderState.None;
    private Coroutine errorCor;

    void Start()
    {
        Debug.Log("работат");
        map = GenerateMap();
        InitBuilder();
        InitLevel();
        DrawMap();
        DrawBuildings();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Build();
        }
        
        if (level.LevelCastle.CurHp <= 0)
        {
            SceneManager.LoadScene("LossScreen");
        }
        time += Time.deltaTime;
        if (time >= 0.3f)
        {
            time = 0f;
            level.Update();
            UpdateUI();
            DrawBuildings();
            DrawEnemies();
        }
    }

    private void InitLevel()
    {
        var field = new MapMatrix<Cell>(map, Converter.IntToCell);
        var table = new EnemyPosTable();
        var castle = (Castle)BuildingFactory.InitBuilding(BuildingType.Castle);
        (int, int) castlePos = (3, 12);
        var spawner = new Spawner(7);
        (int, int) spawnerPos = (8, 1);
        var buildings = new MapMatrix<Building>(size, size);
        buildings[6, 5] = BuildingFactory.InitBuilding(BuildingType.Tower);
        level = new LevelManager(table, field, buildings, castle, castlePos, spawner, spawnerPos);
    }

    private void InitBuilder()
    {
        towerButton.onClick.AddListener(() => SwitchBuilderState(BuilderState.Tower));
        wallButton.onClick.AddListener(() => SwitchBuilderState(BuilderState.Wall));
        repairButton.onClick.AddListener(() => SwitchBuilderState(BuilderState.Repair));
        upgradeButton.onClick.AddListener(() => SwitchBuilderState(BuilderState.Upgrade));
    }

    private void Build()
    {
        Vector2 clickPos = Input.mousePosition;
        clickPos.x -= 560;
        (int, int) c = TransformCoords(clickPos);
        bool s = true;
        switch (buildState)
        {
            case BuilderState.Tower:
                s = level.CreateBuilding(BuildingType.Tower, c.Item1, c.Item2);
                break;
            case BuilderState.Wall:
                s = level.CreateBuilding(BuildingType.Wall, c.Item1, c.Item2);
                break;
            case BuilderState.Repair:
                s = level.RepairBuilding(c.Item1, c.Item2);
                break;
            case BuilderState.Upgrade:
                s = level.UpgradeBuilding(c.Item1, c.Item2);
                break;
            default:
                break;
        }
        if (!s)
        {
            ErrorBuildAlert();
        }
    }

    private void ErrorBuildAlert()
    {
        if (errorCor != null)
        {
            StopCoroutine(errorCor);
        }
        errorCor = StartCoroutine(ErrorCoroutine());
    }

    private IEnumerator ErrorCoroutine()
    {
        errorText.text = "Impossible action!";
        errorText.color = Color.red;
        errorText.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        errorText.gameObject.SetActive(false);
        errorCor = null;
    }

    private void SwitchBuilderState(BuilderState newState)
    {
        if (newState == buildState)
        {
            buildState = BuilderState.None;
            return;
        }
        buildState = newState;
    }

    private (int, int) TransformCoords(Vector2 worldCoords)
    {
        float cellSize = 48f;
        int col = (int)(worldCoords.x / cellSize);
        int row = (int)(worldCoords.y / cellSize);
        row = 14 - row;
        return (row, col);
    }
    private void UpdateUI()
    {
        goldText.text = $"Gold: {level.LevelCastle.Gold}";
        scoreText.text = $"Score: {level.Score}";
        hpText.text = $"CastleHp: {level.LevelCastle.CurHp}/{level.LevelCastle.MaxHp}";
        buildModeText.text = $"Builder Mode: {buildState}";
    }

    private int[,] GenerateMap()
    {
        int[,] matrix = new int[15, 15]
        {
            {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 3, 3, 3},
            {1, 3, 1, 1, 2, 1, 1, 1, 2, 2, 1, 1, 1, 3, 3},
            {1, 1, 1, 2, 2, 1, 1, 3, 3, 2, 3, 1, 1, 1, 3},
            {1, 1, 2, 1, 1, 1, 2, 1, 1, 3, 3, 1, 1, 1, 3},
            {1, 2, 2, 2, 1, 2, 2, 1, 2, 2, 1, 1, 1, 1, 1},
            {1, 1, 1, 1, 1, 2, 2, 2, 2, 1, 1, 3, 1, 3, 1},
            {1, 2, 1, 1, 1, 1, 2, 2, 1, 1, 3, 1, 1, 1, 1},
            {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
            {1, 1, 1, 1, 1, 1, 1, 3, 1, 1, 2, 2, 2, 3, 1},
            {1, 1, 1, 1, 3, 1, 3, 3, 1, 2, 2, 1, 3, 3, 1},
            {1, 1, 2, 1, 3, 1, 1, 3, 1, 1, 2, 2, 2, 1, 1},
            {1, 1, 1, 1, 3, 3, 1, 1, 1, 1, 1, 1, 1, 1, 1},
            {2, 2, 1, 1, 1, 1, 1, 1, 1, 2, 1, 2, 2, 1, 1},
            {1, 2, 2, 2, 2, 3, 1, 1, 1, 1, 2, 2, 1, 1, 1},
            {2, 2, 1, 1, 2, 2, 3, 1, 1, 1, 1, 1, 1, 1, 1}
        };
        return matrix;
    }

    private GameObject DrawGridObject(GameObject prefab, int x, int y, string objectName, float size=48f)
    {
        float cellSize = 48f;

        GameObject gridObject = Instantiate(prefab, gameArea);
        RectTransform rect = gridObject.GetComponent<RectTransform>();

        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);

        float posX = x * cellSize;
        float posY = -y * cellSize;
        rect.anchoredPosition = new Vector2(posX, posY);
        rect.sizeDelta = new Vector2(size, size);
        gridObject.name = objectName;
        return gridObject;
    }

    private void DrawMap()
    {
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                var t = map[y, x];
                GameObject prefab = GetCellPrefab(t);
                string name = $"Tile_{x}_{y}_{t}";
                DrawGridObject(prefab, x, y, name);
            }
        }
    }

    private void DeleteBuildingSrites()
    {
        foreach (var building in spriteMapBuildings)
        {
            if (building != null)
                Destroy(building);
        }
        spriteMapBuildings.Clear();
    }

    private void DrawBuildings()
    {
        DeleteBuildingSrites();
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                var b = level.LevelBuildings[y, x];
                if (b == null) continue;
                GameObject prefab = GetBuildingPrefab(b.Type);
                string name = $"Building_{x}_{y}_{b.Type}";
                GameObject build;
                if (b is Castle)
                {
                    build = DrawGridObject(prefab, x, y, name, 70f);
                } else
                {
                    build = DrawGridObject(prefab, x, y, name);
                }
                spriteMapBuildings.Add(build);
            }
        }
        DrawGridObject(spawnr, level.SpawnerCoord.Item2, level.SpawnerCoord.Item1, "Spawner");
    }

    private void DeleteDeadEnemies()
    {
        foreach (var enemy in enemySprites.Keys.ToList())
        {
            if (enemy.State)
            {
                Destroy(enemySprites[enemy]);
                enemySprites.Remove(enemy);
            }
        }
    }

    private void CreateEnemy(Enemy enemy, int x, int y)
    {
        GameObject prefab = GetEnemyPrefab(enemy.Type);
        GameObject newEnemySprite = DrawGridObject(prefab, x, y, $"Enemy", 32f);
        CreateOffset(newEnemySprite);

        EnemyAnimation enemyAnimation = newEnemySprite.AddComponent<EnemyAnimation>();
        enemyAnimation.Initialize(enemy);
        enemySprites[enemy] = newEnemySprite;
    }

    private void UpdateEnemy(Enemy enemy, int x, int y)
    {
        if (!enemySprites.ContainsKey(enemy))
            return;

        GameObject enemyObj = enemySprites[enemy];
        float offset = 48f * 0.3f;
        var newX = x * 48f + Random.Range(-offset, offset);
        var newY = -y * 48f + Random.Range(-offset, offset);
        Vector2 newPosition = new Vector2(newX, newY);

        EnemyAnimation enemyAnimation = enemyObj.GetComponent<EnemyAnimation>();
        enemyAnimation.MoveTo(newPosition);
    }

    private void CreateOffset(GameObject enemyObj)
    {
        RectTransform rect = enemyObj.GetComponent<RectTransform>();
        float offset = 48f * 0.3f;
        float offsetX = Random.Range(-offset, offset);
        float offsetY = Random.Range(-offset, offset);
        Vector2 currentPos = rect.anchoredPosition;
        rect.anchoredPosition = new Vector2(currentPos.x + offsetX, currentPos.y + offsetY);
    }

    private void DrawEnemies()
    {
        DeleteDeadEnemies();
        var d = level.DamagedEnemies;
        foreach (var enemy in level.Enemies)
        {
            Enemy e = enemy.Item1;
            if (!e.State)
            {
                if (enemySprites.ContainsKey(e))
                {
                    UpdateEnemy(e, enemy.Item3, enemy.Item2);
                    if (d.Contains(e))
                    {
                        EnemyAnimation enemyAnimation = enemySprites[e].GetComponent<EnemyAnimation>();
                        enemyAnimation.GetDamageAnimation();
                    }
                }
                else
                {
                    CreateEnemy(e, enemy.Item3, enemy.Item2);
                }
            }
        }
    }

    private GameObject GetCellPrefab(int type)
    {
        switch (type)
        {
            case 1: return plains;
            case 2: return water;
            case 3: return mountains;
            default: return plains;
        }
    }

    private GameObject GetBuildingPrefab(BuildingType type)
    {
        switch (type)
        {
            case BuildingType.Tower: return tower;
            case BuildingType.Wall: return wall;
            case BuildingType.Castle: return castl;
            default: return tower;
        }
    }

    private GameObject GetEnemyPrefab(EnemyType type)
    {
        switch (type)
        {
            case EnemyType.LightInf: return lightInf;
            case EnemyType.HeavyInf: return heavyInf;
            case EnemyType.Avia: return avia;
            default: return lightInf;
        }
    }
}

public enum BuilderState
{
    None,
    Tower,
    Wall,
    Repair,
    Upgrade,
}