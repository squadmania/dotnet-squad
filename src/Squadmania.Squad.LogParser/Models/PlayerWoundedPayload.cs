namespace Squadmania.Squad.LogParser.Models
{
    public sealed class PlayerWoundedPayload
    {
        public string VictimName { get; }
        public float Damage { get; }
        public string AttackerPlayerController { get; }
        public string Weapon { get; }

        public PlayerWoundedPayload(
            string victimName,
            float damage,
            string attackerPlayerController,
            string weapon
        )
        {
            VictimName = victimName;
            Damage = damage;
            AttackerPlayerController = attackerPlayerController;
            Weapon = weapon;
        }
    }
}