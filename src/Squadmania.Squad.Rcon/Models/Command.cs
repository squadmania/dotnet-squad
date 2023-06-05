namespace Squadmania.Squad.Rcon.Models
{
    public readonly struct Command
    {
        public Command(string name, string parameterDescription, string description)
        {
            Name = name;
            ParameterDescription = parameterDescription;
            Description = description;
        }

        public string Name { get; }
        public string ParameterDescription { get; }
        public string Description { get; }
    }
}