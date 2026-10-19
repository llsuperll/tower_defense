using System.Collections.Generic;
using Game.src.core.entities.Enemies;


namespace Game.src.core.tables {
    public class EnemyPosTable {
        private List<(Enemy, int, int)> Enemies;

        public EnemyPosTable(List<(Enemy, int, int)> enemy) {
            Enemies = enemy;
        }
        public void AddEnemy(Enemy enemy, int x, int y) {
            Enemies.Add((enemy, x, y));
        }
        public (Enemy, int, int) GetEnemy(int n) {
            return Enemies[n];
        }

        public int Size => Enemies.Count;

        public void CleanUp() {
            Enemies.RemoveAll(enemy => enemy.Item1.State == true);
        }

        public void UpdateEnemy(int n, int row, int col) {
            if (n >= 0 && n < Enemies.Count) {
                Enemy e = Enemies[n].Item1;
                Enemies[n] = (e, row, col);
            }
        }

        public IEnumerator<(Enemy, int, int)> GetEnumerator() {
            for (int i = 0; i < Enemies.Count; i++) {
                yield return Enemies[i];
            }
        }
    }
}