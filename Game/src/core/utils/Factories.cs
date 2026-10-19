using System.Dynamic;
using Game.src.core.auras;
using Game.src.core.entities.Enemies;
using Game.src.core.entities.Building;

namespace Game.src.core.utils.Factory {
    public static class EnemyFactory {
        public static Enemy CreateEnemy(EnemyType t, bool isHero) {
            switch (t) {
                case EnemyType.LightInf:
                    if (isHero) {
                        Hero h = new Hero("LightHero", 200, 2, 0.2f, 2, EnemyType.LightInf);
                    }
                    return new LightInfantry("LightUnit", 150, 1, 0.05f, 2);
                case EnemyType.HeavyInf:
                    if (isHero) {
                        return new Hero("HeavyHero", 400, 3, 0.3f, 1, EnemyType.HeavyInf);
                    }
                    return new HeavyInfantry("HeavyUnit", 300, 2, 0.1f, 1, 3.5f);
                case EnemyType.Avia:
                    if (isHero) {
                        return new Hero("AviaHero", 170, 2, 0.1f, 3, EnemyType.Avia);
                    }
                    return new Aviation("AviaUnit", 150, 1, 0.07f, 3, 4);
                default:
                    throw new ArgumentException("Unknown enemy type");
            }
        }
    }

    public static class AuraFactory {
        public static Aura CreateAura(AuraType t, Enemy source) {
            switch (t) {
                case AuraType.Max_hp:
                    return new Aura(source, 2f, AuraType.Max_hp);
                case AuraType.Mv_spd:
                    return new Aura(source, 1f, AuraType.Mv_spd);
                case AuraType.Hp_regen:
                    return new Aura(source, 3f, AuraType.Hp_regen);
                default:
                    throw new ArgumentException("Unknown aura type");
            }
        }
    }

    public static class BuildingFactory {
        public static Building InitBuilding(BuildingType type) {
            Building newBuilding = type switch {
                BuildingType.Castle => new Castle("Castle", 1, 1000, 100, 500, 5.0f, 10),
                BuildingType.Tower => new Tower("Tower", 1, 300, 3.0f, 1.0f, 100, 25),
                BuildingType.Wall => new Wall("Wall", 500, 50),
                _ => throw new ArgumentException("Unknown building type"),
            };
            return newBuilding;
        }
    }
}
