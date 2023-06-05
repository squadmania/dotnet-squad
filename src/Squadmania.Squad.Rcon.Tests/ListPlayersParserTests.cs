using Squadmania.Squad.Rcon.Models;
using Squadmania.Squad.Rcon.Parsers;

namespace Squadmania.Squad.Rcon.Tests;

public class ListPlayersParserTests
{
    public static readonly IEnumerable<object[]> TestData = new List<object[]>
    {
        new object[]
        {
            "----- Active Players -----\nID: 0 | SteamID: 76561198040411592 | Name: <(^^<) pixlcrashr | Team ID: 1 | Squad ID: N/A | Is Leader: False | Role: USA_Rifleman_01\n----- Recently Disconnected Players [Max of 15] -----",
            new ListPlayersResult(
                new List<Player>
                {
                    new (
                        0,
                        76561198040411592,
                        "<(^^<) pixlcrashr",
                        Team.Team1,
                        false,
                        "USA_Rifleman_01",
                        null
                    )
                },
                new List<DisconnectedPlayer>()
            )
        },
        new object[]
        {
            "----- Active Players -----\nID: 0 | SteamID: 76561198040411592 | Name: <(^^<) pixlcrashr | Team ID: 2 | Squad ID: N/A | Is Leader: False | Role: MEA_Rifleman_01\n----- Recently Disconnected Players [Max of 15] -----",
            new ListPlayersResult(
                new List<Player>
                {
                    new (
                        0,
                        76561198040411592,
                        "<(^^<) pixlcrashr",
                        Team.Team2,
                        false,
                        "MEA_Rifleman_01",
                        null
                    )
                },
                new List<DisconnectedPlayer>()
            )
        },
        new object[]
        {
            "----- Active Players -----\n----- Recently Disconnected Players [Max of 15] -----\nID: 0 | SteamID: 76561198040411592 | Since Disconnect: 00m.05s | Name: pixlcrashr",
            new ListPlayersResult(
                new List<Player>(),
                new List<DisconnectedPlayer>
                {
                    new (
                        0,
                        76561198040411592,
                        new TimeSpan(0, 0, 0, 5),
                        "pixlcrashr"
                    )
                }
            )
        },
        new object[]
        {
            "----- Active Players -----\nID: 0 | SteamID: 76561198040411592 | Name: <(^^<) pixlcrashr | Team ID: 2 | Squad ID: 1 | Is Leader: True | Role: MEA_SL_02\n----- Recently Disconnected Players [Max of 15] -----",
            new ListPlayersResult(
                new List<Player>
                {
                    new (
                        0,
                        76561198040411592,
                        "<(^^<) pixlcrashr",
                        Team.Team2,
                        true,
                        "MEA_SL_02",
                        1
                    )
                },
                new List<DisconnectedPlayer>()
            )
        }
    };

    [Theory]
    [MemberData(nameof(TestData))]
    public static void TestListPlayers(
        string raw,
        ListPlayersResult expectedResult
    )
    {
        var parser = new ListPlayersParser();

        var result = parser.Parse(raw);
        
        Assert.Equal(expectedResult.ActivePlayers, result.ActivePlayers);
        Assert.Equal(expectedResult.DisconnectedPlayers, result.DisconnectedPlayers);
    }
}