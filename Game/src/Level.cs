using Game.src.core.entities.Building;
using Game.src.core.entities.Enemies;
using Game.src.core.entities.Terrain;
using Game.src.core.utils;
using Game.src.core.tables;
using Game.src.core.utils.Factory;


namespace Level {
    public static class GameConstants {
        public const int CASTLE_BASE_HP = 1000;
        public const int CASTLE_BASE_COST = 500;
        public const int TOWER_BASE_HP = 300;
        public const int TOWER_BASE_COST = 100;
        public const int WALL_BASE_HP = 500;
        public const int WALL_BASE_COST = 50;

        public const float LIGHT_INFANTRY_DODGE = 0.05f;
        public const int HEAVY_INFANTRY_WALL_DAMAGE = 10;

        public const int STARTING_GOLD = 100;
        public const float REPAIR_SPEED = 5.0f;
        public const int REPAIR_COST = 25;
    }

    public class LevelManager {
        private EnemyPosTable EnemiesTable;
        private MapMatrix<Cell> Field;
        private MapMatrix<Building> Buildings;
        private Spawner EnemySpawner;
        private Castle MainCastle;
        private (int, int) CastleCoords;
        private (int, int) SpawnerCoords;
        private int GameTicks = 0;

        public void Update() {
            SpawnEnemies();
            ApplyAuras();
            ResetAuras();
            CheckEnemyInCastle();
            CleanUpEnemies();
            CleanUpBuildings();
            MoveEnemies();
            TowerUpdate();
            MainCastle.ProduceGold(MainCastle.Income);
            GameTicks++;
        }
        public LevelManager(EnemyPosTable enemies, MapMatrix<Cell> fd, MapMatrix<Building> builds, Castle castl, (int x, int y) castle_point) {
            EnemiesTable = enemies;
            Field = fd;
            Buildings = builds;
            EnemySpawner = new Spawner(7);
            MainCastle = castl;
            CastleCoords = castle_point;
            Buildings[CastleCoords.Item1, CastleCoords.Item2] = MainCastle;
        }
        private static void SaveMatrix<T>(MapMatrix<T> matrix, string filename, Func<T, string> converter) where T : class {
            using StreamWriter writer = new StreamWriter(filename);
            int r = matrix.Rows;
            int c = matrix.Cols;
            for (int i = 0; i < r; i++) {
                for (int j = 0; j < c; j++) {
                    writer.Write(converter(matrix[i, j]));
                }
                writer.WriteLine();
            }
        }
        private void SaveGameState(string filename) {
            using StreamWriter writer = new StreamWriter(filename);
            writer.WriteLine($"{GameTicks}");
            writer.WriteLine($"{EnemySpawner.Time}");
            writer.WriteLine($"{EnemySpawner.Wave}");
            writer.WriteLine($"{CastleCoords.Item1},{CastleCoords.Item2}");
            writer.WriteLine($"{SpawnerCoords.Item1},{SpawnerCoords.Item2}");
            writer.WriteLine($"{MainCastle.Gold},{MainCastle.Cost},{MainCastle.CurHp},{MainCastle.MaxHp},{MainCastle.Level},{MainCastle.RepairMod}");

        }
        public void SaveGame() {
            SaveMatrix(Field, "../Saves/field.txt", cell => Converter.CellToInt(cell).ToString());
            SaveMatrix(Buildings, "../Saves/buildings.txt", building => Converter.BuildingToInt(building).ToString());
            SaveGameState("../Saves/game.txt");
        }

        public bool CreateBuilding(BuildingType type, int x, int y) {
            if (x < 0 || x >= Field.Cols || y >= Field.Rows || y < 0) return false;
            if (Buildings[x, y] != null || Field[x, y].Type != CellType.Plains) return false;

            Building newBuilding = BuildingFactory.InitBuilding(type);
            if (MainCastle.Gold < newBuilding.Cost) return false;
            MainCastle.SpendGold(newBuilding.Cost);
            Buildings[x, y] = newBuilding;
            return true;
        }
        public bool RepairBuilding(int x, int y) {
            if (x < 0 || x >= Field.Cols || y >= Field.Rows || y < 0) return false;
            if (Buildings[x, y] == null) return false;
            if (MainCastle.Gold < GameConstants.REPAIR_COST) return false;
            if (Buildings[x, y].CurHp == Buildings[x, y].MaxHp) return false;

            MainCastle.SpendGold(GameConstants.REPAIR_COST);
            Buildings[x, y].Repair();
            return true;
        }

        public bool UpgradeBuilding(int x, int y) {
            if (x < 0 || x >= Field.Cols || y >= Field.Rows || y < 0) return false;
            if (Buildings[x, y] is not Tower tower) return false;
            if (MainCastle.Gold < tower.Cost) return false;

            MainCastle.SpendGold(tower.Cost);

            return true;
        }

        private void CleanUpBuildings() {
            for (int i = 0; i < Buildings.Rows; i++) {
                for (int j = 0; j < Buildings.Cols; j++) {
                    if (Buildings[i, j]?.State == true) {
                        Buildings.Delete(i, j);
                    }
                }
            }
        }

        private static float Dist((int x, int y) point1, (int x, int y) point2) {
            int dx = point1.x - point2.x;
            int dy = point1.y - point2.y;
            float d = (float)Math.Sqrt(dx * dx + dy * dy);
            return d;
        }

        private (Enemy?, int, int) FindClosestEnemy(int row, int col) {
            float minDist = 12402104214;
            (Enemy?, int, int) closest = (null, 0, 0);
            foreach (var enemy in EnemiesTable) {
                float q = Dist((enemy.Item2, enemy.Item3), (row, col));
                if (q < minDist) {
                    minDist = q;
                    closest = enemy;
                }
            }
            return closest;
        }

        private static bool IsWithinRange((int x, int y) target_pos, (int x, int y) enemy_pos, float range) {
            float d = Dist(target_pos, enemy_pos);
            return d <= range;
        }

        private void TowerUpdate() {
            for (int i = 0; i < Buildings.Rows; i++) {
                for (int j = 0; j < Buildings.Cols; j++) {
                    if (Buildings[i, j] is Tower tower) {
                        (Enemy?, int, int) target = FindClosestEnemy(i, j);
                        if (target.Item1 != null && IsWithinRange((i, j), (target.Item2, target.Item3), tower.FireRange)) {
                            for (int k = 0; k < tower.FireRate; k++) tower.Shoot(target.Item1);
                        }
                    }
                }
            }
        }

        private List<(int, int)> PathFinder(Enemy enemy, (int, int) start, (int, int) end) {
            bool[,] visited = new bool[Field.Rows, Field.Cols];
            var queue = new Queue<List<(int, int)>>();
            queue.Enqueue([start]);
            visited[start.Item1, start.Item2] = true;
            while (queue.Count > 0) {
                var path = queue.Dequeue();
                var cur_cell = path[path.Count - 1];
                if (cur_cell == end) {
                    return path;
                }

                foreach (var neighbour in Field.GetNeighbours(cur_cell.Item1, cur_cell.Item2)) {
                    if (!visited[neighbour.row, neighbour.col] && CanPassCell(enemy, neighbour)) {
                        visited[neighbour.row, neighbour.col] = true;
                        var new_path = new List<(int, int)>(path) { neighbour };
                        queue.Enqueue(new_path);
                    }
                }
            }
            return [];
        }

        private bool CanPassCell(Enemy enemy, (int x, int y) point) {
            var cell = Field[point.x, point.y];
            var building = Buildings[point.x, point.y];
            switch (enemy.Type) {
                case EnemyType.LightInf:
                    return (building == null || building.Type == BuildingType.Castle) && cell.Type == CellType.Plains;
                case EnemyType.HeavyInf:
                    return (building == null || building.Type == BuildingType.Wall ||
                            building.Type == BuildingType.Castle) && (cell.Type == CellType.Plains);
                case EnemyType.Avia:
                    return cell.Type != CellType.Mountains;
                default:
                    return false;
            }
        }

        private void MoveEnemies() {
            for (int i = 0; i < EnemiesTable.Size; i++) {
                var enemy = EnemiesTable.GetEnemy(i);
                var path = PathFinder(enemy.Item1, (enemy.Item2, enemy.Item3), CastleCoords);
                switch (enemy.Item1.Type) {
                    case EnemyType.HeavyInf:
                        if (AttackWallInRange(enemy)) continue;
                        break;
                    case EnemyType.Avia:
                        AttackWallInRange(enemy);
                        break;
                }
                if (path.Count > 0) {
                    int d = Math.Min(path.Count, enemy.Item1.Speed);
                    var new_pos = path[d - 1];
                    EnemiesTable.UpdateEnemy(i, new_pos.Item1, new_pos.Item2);
                }
            }
        }

        private bool AttackWallInRange((Enemy e, int x, int y) enemy) {
            for (int i = 0; i < Buildings.Rows; i++) {
                for (int j = 0; j < Buildings.Cols; j++) {
                    if (Buildings[i, j] is Wall wall) {
                        if (enemy.e is HeavyInfantry inf) {
                            if (!IsWithinRange((i, j), (enemy.x, enemy.y), inf.Range)) return false;
                            if (inf.ability == null) return false;
                            ((IHitter)inf.ability).HitWall(wall);
                            return true;
                        } else if (enemy.e is Aviation avia) {
                            if (!IsWithinRange((i, j), (enemy.x, enemy.y), 0)) return false;
                            if (avia.ability == null) return false;
                            ((IHitter)avia.ability).HitWall(wall);
                            avia.Shoot();
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private void CheckEnemyInCastle() {
            foreach (var enemy in EnemiesTable) {
                if ((enemy.Item2, enemy.Item3) == CastleCoords) {
                    MainCastle.ReceiveDamage((int)Math.Round(enemy.Item1.CastleDmg * enemy.Item1.CurHp));
                    enemy.Item1.GetDamage(100000000);
                }
            }
        }

        private void ApplyAuras() {
            foreach (var enemy in EnemiesTable) {
                if (enemy.Item1 is Hero hero && !hero.State) {
                    foreach (var target in EnemiesTable) {
                        if (IsWithinRange((target.Item2, target.Item3), (enemy.Item2, enemy.Item3), hero.GetHeroAura.Radius) &&
                            target.Item1.AuraStatus == false) {
                            hero.GetHeroAura.ApplyAura(target.Item1);
                        }
                    }
                }
            }
        }
        private void ResetAuras() {
            foreach (var enemy in EnemiesTable) {
                if (enemy.Item1 is Hero hero) {
                    foreach (var target in EnemiesTable) {
                        if ((!IsWithinRange((target.Item2, target.Item3), (enemy.Item2, enemy.Item3),
                            hero.GetHeroAura.Radius) || hero.State) && target.Item1.AuraStatus) {
                            hero.GetHeroAura.ResetAura(target.Item1);
                        }
                    }
                }
            }
        }

        private void SpawnEnemies() {
            if (GameTicks % EnemySpawner.Time == 0) {
                var new_enemies = EnemySpawner.CreateEnemies();
                foreach (Enemy e in new_enemies) {
                    EnemiesTable.AddEnemy(e, SpawnerCoords.Item1, SpawnerCoords.Item2);
                }
            }
        }

        private void CleanUpEnemies() {
            int bounty = 0;
            foreach (var enemy in EnemiesTable) {
                if (enemy.Item1.State) {
                    bounty += 10;
                }
            }
            MainCastle.ProduceGold(bounty);
            EnemiesTable.CleanUp();
        }
    }
}
