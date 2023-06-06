namespace Squadmania.Squad.LogParser.Models
{
    public sealed class PlayerDiedPayload
    {
        public PlayerDiedPayload(
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

        public string VictimName { get; }
        public float Damage { get; }
        public string AttackerPlayerController { get; }
        public string Weapon { get; }
    }
}