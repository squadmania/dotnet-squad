using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Squadmania.Squad.LogParser.Models;

namespace Squadmania.Squad.LogParser.Parsers
{
    public class LogMessageParser
    {
        private static readonly Regex LogSquadMessageRegex = new Regex(@"^\[([0-9.:-]+)\]\[([0-9]*)\](LogNet|LogSquad|LogSquadTrace|LogWorld): (.*)$");

        public LogMessage? Parse(
            string value
        )
        {
            var match = LogSquadMessageRegex.Match(value);
            if (!match.Success)
            {
                return null;
            }

            var rawLog = match.Groups[4].Value;
            var type = ParseLogMessageType(match.Groups[3].Value);
            
            return new LogMessage(
                match.Groups[0].Value,
                type,
                int.Parse(match.Groups[2].Value),
                DateTime.ParseExact(
                    match.Groups[1].Value,
                    "yyyy.MM.dd-HH.mm.ss:fff",
                    CultureInfo.InvariantCulture
                ),
                rawLog,
                ParsePayload(type, rawLog)
            );
        }

        private static LogMessageType ParseLogMessageType(
            string value
        )
        {
            return value switch
            {
                "LogNet" => LogMessageType.Net,
                "LogSquad" => LogMessageType.Squad,
                "LogSquadTrace" => LogMessageType.SquadTrace,
                "LogWorld" => LogMessageType.World,
                "LogGameState" => LogMessageType.GameState,
                "LogSquadGameEvents" => LogMessageType.SquadGameEvents,
                _ => LogMessageType.None
            };
        }

        public IPayloadParser<AdminBroadcastPayload> AdminBroadcastPayloadParser { get; set; } = new AdminBroadcastPayloadParser();
        public IPayloadParser<ClientConnectedPayload> ClientConnectedPayloadParser { get; set; } = new ClientConnectedPayloadParser();
        public IPayloadParser<ClientLoginPayload> ClientLoginPayloadParser { get; set; } = new ClientLoginPayloadParser();
        public IPayloadParser<DeployableDamagedPayload> DeployableDamagedPayloadParser { get; set; } = new DeployableDamagedPayloadParser();
        public IPayloadParser<NewGamePayload> NewGamePayloadParser { get; set; } = new NewGamePayloadParser();
        public IPayloadParser<PendingConnectionDestroyedPayload> PendingConnectionDestroyedPayloadParser { get; set; } = new PendingConnectionDestroyedPayloadParser();
        public IPayloadParser<PlayerConnectedPayload> PlayerConnectedPayloadParser { get; set; } = new PlayerConnectedPayloadParser();
        public IPayloadParser<PlayerControllerConnectedPayload> PlayerControllerConnectedPayloadParser { get; set; } = new PlayerControllerConnectedPayloadParser();
        public IPayloadParser<PlayerDamagedPayload> PlayerDamagedPayloadParser { get; set; } = new PlayerDamagedPayloadParser();
        public IPayloadParser<PlayerDiedPayload> PlayerDiedPayloadParser { get; set; } = new PlayerDiedPayloadParser();
        public IPayloadParser<PlayerDisconnectedPayload> PlayerDisconnectedPayloadParser { get; set; } = new PlayerDisconnectedPayloadParser();
        public IPayloadParser<PlayerPossessPayload> PlayerPossessPayloadParser { get; set; } = new PlayerPossessPayloadParser();
        public IPayloadParser<PlayerRevivedPayload> PlayerRevivedPayloadParser { get; set; } = new PlayerRevivedPayloadParser();
        public IPayloadParser<PlayerUnPossessPayload> PlayerUnPossessPayloadParser { get; set; } = new PlayerUnPossessPayloadParser();
        public IPayloadParser<PlayerWoundedPayload> PlayerWoundedPayloadParser { get; set; } = new PlayerWoundedPayloadParser();
        public IPayloadParser<RoundEndedPayload> RoundEndedPayloadParser { get; set; } = new RoundEndedPayloadParser();
        public IPayloadParser<RoundTicketsPayload> RoundTicketsPayloadParser { get; set; } = new RoundTicketsPayloadParser();
        public IPayloadParser<RoundWinnerPayload> RoundWinnerPayloadParser { get; set; } = new RoundWinnerPayloadParser();
        public IPayloadParser<ServerTickRatePayload> ServerTickRatePayloadParser { get; set; } = new ServerTickRatePayloadParser();

        protected virtual IReadOnlyCollection<IPayloadParser> PayloadParsers => new IPayloadParser[]
        {
            AdminBroadcastPayloadParser,
            ClientConnectedPayloadParser,
            ClientLoginPayloadParser,
            DeployableDamagedPayloadParser,
            NewGamePayloadParser,
            PendingConnectionDestroyedPayloadParser,
            PlayerConnectedPayloadParser,
            PlayerControllerConnectedPayloadParser,
            PlayerDamagedPayloadParser,
            PlayerDiedPayloadParser,
            PlayerDisconnectedPayloadParser,
            PlayerPossessPayloadParser,
            PlayerRevivedPayloadParser,
            PlayerUnPossessPayloadParser,
            PlayerWoundedPayloadParser,
            RoundEndedPayloadParser,
            RoundTicketsPayloadParser,
            RoundWinnerPayloadParser,
            ServerTickRatePayloadParser
        };
        
        private object? ParsePayload(
            LogMessageType type,
            string rawLog
        )
        {
            foreach (var payloadParser in PayloadParsers)
            {
                if (type != payloadParser.LogMessageType)
                {
                    continue;
                }

                var payload = payloadParser.Parse(rawLog);
                if (payload != null)
                {
                    return payload;
                }
            }

            return null;
        }
    }
}