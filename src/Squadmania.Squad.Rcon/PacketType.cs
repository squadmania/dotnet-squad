﻿namespace Squadmania.Squad.Rcon
{
    public static class PacketType
    {
        public const int ServerDataAuth = 0x03;
        public const int ServerDataAuthResponse = 0x02;
        public const int ServerDataExecCommand = 0x02;
        public const int ServerDataResponseValue = 0x00;
        public const int ServerDataChatMessage = 0x02;
    }
}