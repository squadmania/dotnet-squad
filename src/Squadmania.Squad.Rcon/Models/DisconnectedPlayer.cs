using System;

namespace Squadmania.Squad.Rcon.Models
{
    public sealed class DisconnectedPlayer : IEquatable<DisconnectedPlayer>
    {
        public int Id { get; }
        public ulong SteamId64 { get; }
        public TimeSpan DisconnectedSince { get; }
        public string Name { get; }

        public DisconnectedPlayer(
            int id,
            ulong steamId64,
            TimeSpan disconnectedSince,
            string name
        )
        {
            Id = id;
            SteamId64 = steamId64;
            DisconnectedSince = disconnectedSince;
            Name = name;
        }

        public bool Equals(
            DisconnectedPlayer? other
        )
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id && SteamId64 == other.SteamId64 && DisconnectedSince.Equals(other.DisconnectedSince) && Name == other.Name;
        }

        public override bool Equals(
            object? obj
        )
        {
            return ReferenceEquals(this, obj) || obj is DisconnectedPlayer other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, SteamId64, DisconnectedSince, Name);
        }
    }
}