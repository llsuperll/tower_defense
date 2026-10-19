using Game.src.core.utils;
using Game.src.core.entities.Terrain;
using Game.src.core.entities.Enemies;
using Game.src.core.utils.Factory;
using Game.src.core.tables;
using Game.src.core.entities.Building;
using Game.src.Level;
namespace Test;

public class MapMatrixTests {
    [Fact]
    public void Constructor1() {
        var matrix = new MapMatrix<string>(3, 4);
        Assert.NotNull(matrix);
    }
    [Fact]
    public void Indexer() {
        var matrix = new MapMatrix<Cell>(2, 2);
        var testValue = new Cell(CellType.Plains);
        matrix[0, 1] = testValue;
        var result = matrix[0, 1];
        Assert.Equal(testValue, result);
    }
    [Fact]
    public void GetDims() {
        var matrix = new MapMatrix<Cell>(2, 3);
        int r = matrix.Rows, c = matrix.Cols;
        Assert.Equal(2, r);
        Assert.Equal(3, c);
    }
    [Fact]
    public void Constructor2() {
        var matr = new int[,] {
            {1, 1, 1},
            {1, 2, 1},
            {1, 1, 1},
        };
        var matrix = new MapMatrix<Cell>(matr, Converter.IntToCell);
        Assert.IsType<Cell>(matrix[0, 0]);
        Assert.Equal(CellType.Water, matrix[1, 1].Type);
        Assert.Equal(3, matrix.Rows);
    }

    [Fact]
    public void Delete() {
        var matrix = new MapMatrix<Cell>(2, 2);
        var testValue = new Cell(CellType.Plains);
        matrix[0, 1] = testValue;
        matrix.Delete(0, 1);
        Assert.Null(matrix[0, 1]);
    }
    [Fact]
    public void Iterator() {
        var matr = new int[,] {
            {1, 1, 1},
            {1, 2, 1},
            {1, 1, 1},
        };
        var matrix = new MapMatrix<Cell>(matr, Converter.IntToCell);
        foreach (Cell cell in matrix) {
            Assert.NotNull(cell);
        }
    }
    [Fact]
    public void OutOfRange() {
        var matrix = new MapMatrix<Cell>(2, 2);
        Assert.Throws<IndexOutOfRangeException>(() => {
            matrix.Delete(2, 5);
        });
        Assert.Throws<IndexOutOfRangeException>(() => {
            var t = matrix[3, 5];
        });
        Assert.Throws<IndexOutOfRangeException>(() => {
            matrix[4, 4] = null;
        });
    }
    [Fact]
    public void Resize() {
        var matr = new int[,] {
            {1, 1, 1},
            {1, 2, 1},
            {1, 1, 1},
        };
        var matrix = new MapMatrix<Cell>(matr, Converter.IntToCell);
        matrix.Resize(4, 4);
        Assert.Null(matrix[3, 3]);
        Assert.NotNull(matrix[2, 1]);
        Assert.Equal(4, matrix.Rows);

        matrix.Resize(2, 2);
        Assert.NotNull(matrix[0, 0]);
        Assert.Equal(2, matrix.Rows);
    }
    [Fact]
    public void FourNeighbors() {
        var matr = new int[,] {
            {1, 1, 1},
            {1, 2, 1},
            {1, 1, 1},
        };
        var matrix = new MapMatrix<Cell>(matr, Converter.IntToCell);
        var expected = new List<(int, int)> {
            (0, 1),
            (2, 1),
            (1, 0),
            (1, 2)
        };
        var neighbors = matrix.GetNeighbours(1, 1).ToList();

        Assert.Equal(4, neighbors.Count);
        Assert.Equal(expected.OrderBy(x => x), neighbors.OrderBy(x => x));
    }
    [Fact]
    public void ThreeNeighbors() {
        var matr = new int[,] {
            {1, 1, 1},
            {1, 2, 1},
            {1, 1, 1},
        };
        var matrix = new MapMatrix<Cell>(matr, Converter.IntToCell);
        var expected = new List<(int, int)> {
            (0, 0),
            (1, 1),
            (0, 2),
        };
        var neighbors = matrix.GetNeighbours(0, 1).ToList();

        Assert.Equal(3, neighbors.Count);
        Assert.Equal(expected.OrderBy(x => x), neighbors.OrderBy(x => x));
    }
    [Fact]
    public void TwoNeighbors() {
        var matr = new int[,] {
            {1, 1, 1},
            {1, 2, 1},
            {1, 1, 1},
        };
        var matrix = new MapMatrix<Cell>(matr, Converter.IntToCell);
        var expected = new List<(int, int)> {
            (0, 1),
            (1, 0),
        };
        var neighbors = matrix.GetNeighbours(0, 0).ToList();

        Assert.Equal(2, neighbors.Count);
        Assert.Equal(expected.OrderBy(x => x), neighbors.OrderBy(x => x));
    }
}

public class EnemyTests
{
    [Fact]
    public void HealDmgTest()
    {
        Enemy e = EnemyFactory.CreateEnemy(EnemyType.LightInf, false);
        e.Heal();
        Assert.Equal(150, e.CurHp);
        e.GetDamage(15);
        e.Heal();
        Assert.Equal(140, e.CurHp);
    }
    [Fact]
    public void AuraTest()
    {
        Hero h = (Hero)EnemyFactory.CreateEnemy(EnemyType.LightInf, true);
        Enemy e = EnemyFactory.CreateEnemy(EnemyType.LightInf, false);
        var r = h.GetHeroAura.Radius;
        h.GetHeroAura.ApplyAura(e);
        Assert.True(e.AuraStatus);
        h.GetHeroAura.ApplyAura(e);
        h.GetHeroAura.ResetAura(e);
        Assert.False(e.AuraStatus);
        h.GetHeroAura.ResetAura(e);
        h.GetHeroAura.ApplyAura(h);
        Assert.False(h.AuraStatus);
    }
    [Fact]
    public void AttributesTest()
    {
        Enemy e = EnemyFactory.CreateEnemy(EnemyType.LightInf, false);
        Assert.Equal(150, e.MaxHp);
        Assert.Equal(150, e.CurHp);
        Assert.Equal(2, e.Speed);
        Assert.Equal(1, e.HealSpeed);
        Assert.Equal(EnemyType.LightInf, e.Type);
        e.GetDamage(12412040);
        Assert.True(e.State);
    }
    [Fact]
    public void AbilitiesTest()
    {
        Enemy e = EnemyFactory.CreateEnemy(EnemyType.LightInf, false);
        Tower t = (Tower)BuildingFactory.InitBuilding(BuildingType.Tower);
        for (int i = 0; i < 100; i++) t.Shoot(e);
        Wall w = (Wall)BuildingFactory.InitBuilding(BuildingType.Wall);
        Enemy h = EnemyFactory.CreateEnemy(EnemyType.HeavyInf, false);
        ((IHitter)h.ability).HitWall(w);
        Assert.Equal(450, w.CurHp);
        Aviation a = (Aviation)EnemyFactory.CreateEnemy(EnemyType.Avia, false);
        ((IHitter)a.ability).HitWall(w);
        a.Shoot();
        Assert.Equal(3, a.Ammo);
        Assert.Equal(250, w.CurHp);
        for (int i = 0; i < 12; i++) ((IHitter)h.ability).HitWall(w);
        Assert.True(w.State);
    }
}

public class TableTests
{
    [Fact]
    public void TableAllInOne()
    {
        var t = new EnemyPosTable();
        Assert.Equal(0, t.Size);
        var tt = new List<(Enemy, int, int)> { };
        tt.Add((EnemyFactory.CreateEnemy(EnemyType.LightInf, false), 1, 1));
        var table = new EnemyPosTable(tt);
        Assert.Equal(1, table.Size);
        var e = table.GetEnemy(0);
        Assert.NotNull(e.Item1);
        table.AddEnemy(EnemyFactory.CreateEnemy(EnemyType.HeavyInf, true), 0, 0);

        table.CleanUp();
        Assert.Equal(2, table.Size);
        e.Item1.GetDamage(1000);
        table.CleanUp();
        Assert.Equal(1, table.Size);
        table.UpdateEnemy(0, 2, 2);
        Assert.Equal(2, table.GetEnemy(0).Item2);
        foreach (var _ in table) continue;
    }
}

public class LevelTests
{
    [Fact]
    public void MegaUltraTest()
    {
        var e = new EnemyPosTable();
        var m = new int[10, 10];
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                m[i, j] = 1;
            }
        }
        var map = new MapMatrix<Cell>(m, Converter.IntToCell);
        var b = new MapMatrix<Building>(10, 10);
        b[2, 2] = BuildingFactory.InitBuilding(BuildingType.Tower);
        b[1, 2] = BuildingFactory.InitBuilding(BuildingType.Tower);
        b[1, 3] = BuildingFactory.InitBuilding(BuildingType.Tower);
        b[2, 1] = BuildingFactory.InitBuilding(BuildingType.Tower);
        b[0, 0] = BuildingFactory.InitBuilding(BuildingType.Tower);
        b[0, 1] = BuildingFactory.InitBuilding(BuildingType.Tower); 
        Castle c = (Castle)BuildingFactory.InitBuilding(BuildingType.Castle);
        Spawner s = new Spawner(7);
        LevelManager level = new LevelManager(e, map, b, c, (7, 7), s, (1, 1));

        level.Update();
        Assert.Equal(110, level.LevelCastle.Gold);
        level.Update();
        Assert.Equal(3, level.Enemies.Size);
    }
}