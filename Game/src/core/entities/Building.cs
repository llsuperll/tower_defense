using Game.src.core.utils.Factory;
using Game.src.core.entities.Enemies;

namespace Game.src.core.entities.Building {
    public enum BuildingType {
        Castle,
        Tower,
        Wall,
    }

    public class Building {
        protected string name;
        protected int max_health;
        protected int cur_health;
        protected int cost;
        protected bool isDestroyed = false;
        protected BuildingType type;

        public Building(string name, int hp, int cost) {
            max_health = hp;
            cur_health = hp;
            this.name = name;
            this.cost = cost;
        }

        public int CurHp => cur_health;
        public int MaxHp => max_health;
        public BuildingType Type => type;
        public bool State => isDestroyed;
        public int Cost => cost;

        public void SetCost(int cost) {
            this.cost = cost;
        }
        public void ReceiveDamage(int dmg) {
            cur_health -= dmg;
            if (cur_health <= 0) {
                isDestroyed = true;
            }
        }
        public virtual void Repair() {
            cur_health += (int)0.1 * max_health;
            if (cur_health > max_health) {
                cur_health = max_health;
            }
        }
    }

    public class Wall : Building {
        public Wall(string name, int hp, int cost) : base(name, hp, cost) { }
    }

    public class Castle : Building {
        private int gold_income;
        private float repair_mod;
        private int level;
        private int gold;

        public Castle(string name, int lvl, int hp, int gold, int cost, float rpr_mod, int income) : base(name, hp, cost) {
            level = lvl;
            this.gold = gold;
            repair_mod = rpr_mod;
            gold_income = income;
        }
        public int Gold => gold;
        public int Income => gold_income;
        public float RepairMod => repair_mod;
        public int Level => level;
        public void LevelUp() {
            gold_income += 1;
            level += 1;
            max_health += 200;
        }
        public void SpendGold(int gold) {
            this.gold -= gold;
        }
        public override void Repair() {
            cur_health += (int)repair_mod * max_health;
            if (cur_health > max_health) {
                cur_health = max_health;
            }
        }
        public void ProduceGold(int gold_amount) {
            gold += gold_amount;
        }
    }

    public class Tower : Building {
        private int level;
        private float fire_range;
        private float fire_rate;
        private int damage;

        public Tower(string name, int lvl, int hp, float rng, float rate, int cost, int dmg) : base(name, hp, cost) {
            level = lvl;
            fire_range = rng;
            fire_rate = rate;
            damage = dmg;
        }

        public float FireRate => fire_rate;
        public float FireRange => fire_range;
        public int Damage => damage;

        public void LevelUp() {
            fire_range += 0.5f;
            fire_rate += 0.2f;
            damage += 2;
            level += 1;
            cost += 10;
        }
        public void Shoot(Enemy enemy) {
            if (enemy.ability is IDodger dodger) {
                if (!dodger.Dodge()) {
                    enemy.GetDamage(damage);
                }
            } else {
                enemy.GetDamage(damage);
            }
        }
    }

    public class Spawner {
        private int waveTimeSpawn;
        private int wave;
        public Spawner(int time) {
            waveTimeSpawn = time;
            wave = 1;
        }
        private (float, float, float) WaveTemplate() {
            if (wave <= 5) {
                return (100f, 0f, 0f);
            } else if (wave <= 10) {
                float heavy = 0.1f + (wave - 5) * 0.04f;
                float light = 1 - heavy;
                return (light, heavy, 0f);
            } else if (wave <= 15) {
                float avia = (wave - 10) * 0.06f;
                float heavy = 0.3f;
                float light = 1 - heavy - avia;
                return (light, heavy, avia);
            } else if (wave <= 20) {
                return (0.4f, 0.35f, 0.25f);
            } else {
                var template1 = (0.3f, 0.4f, 0.3f);
                var template2 = (0.2f, 0.2f, 0.6f);
                var template3 = (0.25f, 0.5f, 0.25f);
                Random rand = new();
                var templates = new List<(float, float, float)> {template1, template2, template3};
                return templates[rand.Next(0, templates.Count)];
            }
        }
        public int Time => waveTimeSpawn;
        public int Wave => wave;
        public List<Enemy> CreateEnemies() {
            var enemies = new List<Enemy> { };
            int N_units = 7 + wave * 2;
            var template = WaveTemplate();
            int light = (int)Math.Round(N_units * template.Item1);
            int heavy = (int)Math.Round(N_units * template.Item2);
            int avia = (int)Math.Round(N_units * template.Item3);
            int hero = 0;

            if (wave > 15 && wave <= 20) {
                hero = 1;
            } else if (wave > 20) {
                hero = 2 + (wave - 20) / 5;
            }

            enemies.AddRange(Enumerable.Repeat(EnemyFactory.CreateEnemy(EnemyType.LightInf, false), light));
            enemies.AddRange(Enumerable.Repeat(EnemyFactory.CreateEnemy(EnemyType.HeavyInf, false), heavy));
            enemies.AddRange(Enumerable.Repeat(EnemyFactory.CreateEnemy(EnemyType.Avia, false), avia));

            var types = (EnemyType[])Enum.GetValues(typeof(EnemyType));
            Random random = new();
            for (int i = 0; i < hero; i++) {
                EnemyType type = types[random.Next(types.Length)];
                enemies.Add(EnemyFactory.CreateEnemy(type, true));
            }
            wave += 1;
            return enemies;
        }
    }
}