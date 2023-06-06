namespace Squadmania.Squad.LogParser.Models
{
    public sealed class DeployableDamagedPayload
    {
        public string Deployable { get; }
        public float Damage { get; }
        public string Weapon { get; }
        public string PlayerSuffix { get; }
        public string DamageType { get; }
        public float HealthRemaining { get; }

        public DeployableDamagedPayload(
            string deployable,
            float damage,
            string weapon,
            string playerSuffix,
            string damageType,
            float healthRemaining
        )
        {
            Deployable = deployable;
            Damage = damage;
            Weapon = weapon;
            PlayerSuffix = playerSuffix;
            DamageType = damageType;
            HealthRemaining = healthRemaining;
        }
    }
}