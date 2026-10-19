using Game.src.core.entities.Enemies;


namespace Game.src.core.auras {

    public enum AuraType {
        Max_hp,
        Mv_spd,
        Hp_regen,
    }

    public class Aura {
        private Enemy source;
        private float radius;
        private AuraType auraType;
        private int hpBuff = 0;
        private int spdBuff = 0;
        private int rgBuff = 0;
        public float Radius => radius;
        public Aura(Enemy src, float r, AuraType aura) {
            source = src;
            radius = r;
            auraType = aura;
            switch (auraType) {
                case AuraType.Max_hp:
                    hpBuff = 50;
                    break;
                case AuraType.Hp_regen:
                    rgBuff = 2;
                    break;
                case AuraType.Mv_spd:
                    spdBuff = 1;
                    break;
            }
        }

        public void ApplyAura(Enemy target) {
            if (target == source) return;
            target.GetAura(hpBuff, spdBuff, rgBuff);
        }
        public void ResetAura(Enemy target) {
            if (target == source) return;
            target.ResetAura(hpBuff, spdBuff, rgBuff);
        }
    }

}