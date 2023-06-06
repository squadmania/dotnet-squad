namespace Squadmania.Squad.LogParser.Models
{
    public sealed class PlayerControllerConnectedPayload
    {
        public PlayerControllerConnectedPayload(
            string controller
        )
        {
            Controller = controller;
        }

        public string Controller { get; }
    }
}