namespace Squadmania.Squad.LogParser.Models
{
    public sealed class PlayerDamagedPayload
    {
        public string VictimName { get; }
        public float Damage { get; }
        public string AttackerName { get; }
        public string Weapon { get; }

        public PlayerDamagedPayload(
            string victimName,
            float damage,
            string attackerName,
            string weapon
        )
        {
            VictimName = victimName;
            Damage = damage;
            AttackerName = attackerName;
            Weapon = weapon;
        }
    }
}