﻿using System;

namespace Squadmania.Squad.Rcon.Models
{
    public sealed class Squad : IEquatable<Squad>
    {
        public Squad(
            int id,
            Team team,
            string teamName,
            string name,
            int size,
            string creatorName,
            ulong creatorSteamId64,
            bool isLocked
        )
        {
            Id = id;
            Team = team;
            TeamName = teamName;
            Name = name;
            Size = size;
            CreatorName = creatorName;
            CreatorSteamId64 = creatorSteamId64;
            IsLocked = isLocked;
        }
        
        public int Id { get; }
        public Team Team { get; }
        public string TeamName { get; }
        public string Name { get; }
        public int Size { get; }
        public string CreatorName { get; }
        public ulong CreatorSteamId64 { get; }
        public bool IsLocked { get; }

        public bool Equals(
            Squad? other
        )
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id && Team == other.Team && TeamName == other.TeamName && Name == other.Name && Size == other.Size && CreatorName == other.CreatorName && CreatorSteamId64 == other.CreatorSteamId64 && IsLocked == other.IsLocked;
        }

        public override bool Equals(
            object? obj
        )
        {
            return ReferenceEquals(this, obj) || obj is Squad other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, (int)Team, TeamName, Name, Size, CreatorName, CreatorSteamId64, IsLocked);
        }
    }
}