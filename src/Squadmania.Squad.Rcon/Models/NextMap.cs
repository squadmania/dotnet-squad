namespace Squadmania.Squad.Rcon.Models
{
    public sealed class NextMap
    {
        public NextMap(
            string levelName,
            string layerName
        )
        {
            LevelName = levelName;
            LayerName = layerName;
        }

        public string LevelName { get; }
        public string LayerName { get; }
    }
}