namespace Squadmania.Squad.Rcon.Models
{
    public sealed class CurrentMap
    {
        public CurrentMap(
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