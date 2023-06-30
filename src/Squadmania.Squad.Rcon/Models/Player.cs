﻿using System;

namespace Squadmania.Squad.Rcon.Models
{
    public sealed class Player : IEquatable<Player>
    {
        public Player(
            int id,
            ulong steamId64,
            string name,
            TeamId teamId,
            bool isLeader,
            string roleClass,
            int? squadId = null
        )
        {
            Id = id;
            SteamId64 = steamId64;
            Name = name;
            TeamId = teamId;
            IsLeader = isLeader;
            RoleClass = roleClass;
            SquadId = squadId;
        }

        public int Id { get; }
        public ulong SteamId64 { get; }
        public string Name { get; }
        public TeamId TeamId { get; }
        public bool IsLeader { get; }
        public string RoleClass { get; }
        public int? SquadId { get; }

        public bool Equals(
            Player? other
        )
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id && SteamId64 == other.SteamId64 && Name == other.Name && TeamId == other.TeamId && IsLeader == other.IsLeader && RoleClass == other.RoleClass && SquadId == other.SquadId;
        }

        public override bool Equals(
            object? obj
        )
        {
            return ReferenceEquals(this, obj) || obj is Player other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, SteamId64, Name, (int)TeamId, IsLeader, RoleClass, SquadId);
        }
    }
}