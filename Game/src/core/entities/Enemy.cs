using System.Collections.Generic;
using Game.src.core.tables;
using Game.src.core.auras;
using Game.src.core.entities.Building;
using Game.src.core.utils.Factory;
using System.Reflection.Metadata;


namespace Game.src.core.entities.Enemies {

    public interface IAbility {}

    public interface IDodger : IAbility {
        public bool Dodge();
    }

    public interface IHitter : IAbility {
        public void HitWall(Wall wall);
    }

    public class Dodger : IDodger {
        public float dodgePossibility = 0.05f;
        public bool Dodge() {
            Random random = new();
            if (random.NextDouble() < dodgePossibility) {
                return true;
            }
            return false;
        }
    }

    public class Hitter(int dmg) : IHitter {
        private int damage = dmg;

        public void HitWall(Wall wall) {
            wall.ReceiveDamage(damage);
        } 
    }

    public enum EnemyType {
        LightInf,
        HeavyInf,
        Avia
    }


    public class Enemy {
        protected string name;
        protected int max_health;
        protected int cur_health;
        protected int hp_regen_spd;
        protected float castle_dmg_mod;
        protected int mv_speed;
        protected bool isDead = false;
        protected EnemyType type;
        protected bool isBuffed;
        public IAbility? ability;

        public int MaxHp => max_health;
        public int CurHp => cur_health;
        public float CastleDmg => castle_dmg_mod;
        public int Speed => mv_speed;
        public bool State => isDead;
        public EnemyType Type => type;
        public bool AuraStatus => isBuffed;

        public Enemy(string name, int mhp, int hrs, float dmg_mod, int spd) {
            this.name = name;
            max_health = mhp;
            cur_health = mhp;
            hp_regen_spd = hrs;
            castle_dmg_mod = dmg_mod;
            mv_speed = spd;
        }
        public void GetDamage(int damage) {
            cur_health -= damage;
            if (cur_health <= 0) {
                OnDeath();
            }
        }
        public void Heal() {
            cur_health += hp_regen_spd;
            if (cur_health > max_health) {
                cur_health = max_health;
            }
        }
        public void GetAura(int mhp = 0, int spd = 0, int rg = 0) {
            max_health += mhp;
            mv_speed += spd;
            hp_regen_spd += rg;
            isBuffed = true;
        }
        public void ResetAura(int mhp = 0, int spd = 0, int rg = 0) {
            max_health -= mhp;
            mv_speed -= spd;
            hp_regen_spd -= rg;
            isBuffed = false;
        }
        public void OnDeath() {
            isDead = true;
        }
    }

    public class LightInfantry : Enemy {
        public LightInfantry(string nam, int mhp, int hrs, float dmg_mod, int spd) : base(nam, mhp, hrs, dmg_mod, spd) {
            ability = new Dodger();
            type = EnemyType.LightInf;
        }
    }

    public class HeavyInfantry : Enemy {
        private float attack_range;

        public HeavyInfantry(string name, int mhp, int hrs, float dmg_mod, int spd, float att_rng) : base(name, mhp, hrs, dmg_mod, spd) {
            attack_range = att_rng;
            ability = new Hitter(50);
            type = EnemyType.HeavyInf;
        }
        public float Range => attack_range;
    }

    public class Aviation : Enemy {
        private int ammunition;
        public int Ammo => ammunition;
        public Aviation(string name, int mhp, int hrs, float dmg_mod, int spd, int ammo) : base(name, mhp, hrs, dmg_mod, spd) {
            ammunition = ammo;
            ability = new Hitter(200);
            type = EnemyType.Avia;
        }
        public void Shoot() { 
            ammunition -= 1;
        }
    }

    public class Hero : Enemy {
        private Aura aura;
        private int wall_dmg;

        public Hero(string name, int mhp, int hrs, float dmg_mod, int spd, EnemyType etype) : base(name, mhp, hrs, dmg_mod, spd) {
            Random random = new();
            var types = (AuraType[])Enum.GetValues(typeof(AuraType));
            AuraType atype = types[random.Next(types.Length)];

            type = etype;
            aura = AuraFactory.CreateAura(atype, this);
            switch (etype) {
                case EnemyType.LightInf:
                    ability = new Dodger();
                    break;
                case EnemyType.HeavyInf:
                    ability = new Hitter(100);
                    break;
                case EnemyType.Avia:
                    ability = new Hitter(300);
                    break;
            }
        }
        public int WallDamage => wall_dmg;
        public Aura GetHeroAura => aura;
    }
}