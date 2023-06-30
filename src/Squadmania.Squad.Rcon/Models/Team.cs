using System;

namespace Squadmania.Squad.Rcon.Models
{
    public sealed class Team : IEquatable<Team>
    {
        public TeamId Id { get; }
        public string Name { get; }
        
        public Team(
            TeamId id,
            string name
        )
        {
            Id = id;
            Name = name;
        }

        public bool Equals(
            Team? other
        )
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id && Name == other.Name;
        }

        public override bool Equals(
            object? obj
        )
        {
            return ReferenceEquals(this, obj) || obj is Team other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int)Id, Name);
        }
    }
}