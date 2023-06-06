namespace Squadmania.Squad.LogParser.Models
{
    public sealed class NewGamePayload
    {
        public NewGamePayload(
            string dlc,
            string mapClassname,
            string layerClassname
        )
        {
            Dlc = dlc;
            MapClassname = mapClassname;
            LayerClassname = layerClassname;
        }

        public string Dlc { get; }
        public string MapClassname { get; }
        public string LayerClassname { get; }
    }
}