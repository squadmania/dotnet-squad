using Squadmania.Squad.Rcon.Models;
using Squadmania.Squad.Rcon.Parsers;

namespace Squadmania.Squad.Rcon.Tests;

public class ListPlayersParserTests
{
    public static readonly IEnumerable<object[]> TestData = new List<object[]>
    {
        new object[]
        {
            @"----- Active Players -----
ID: 0 | SteamID: 76561198040411592 | Name: <(^^<) pixlcrashr | Team ID: 1 | Squad ID: N/A | Is Leader: False | Role: USA_Rifleman_01
----- Recently Disconnected Players [Max of 15] -----",
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
            @"----- Active Players -----
ID: 0 | SteamID: 76561198040411592 | Name: <(^^<) pixlcrashr | Team ID: 2 | Squad ID: N/A | Is Leader: False | Role: MEA_Rifleman_01
----- Recently Disconnected Players [Max of 15] -----",
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
            @"----- Active Players -----
----- Recently Disconnected Players [Max of 15] -----
ID: 0 | SteamID: 76561198040411592 | Since Disconnect: 00m.05s | Name: pixlcrashr",
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
            @"----- Active Players -----
ID: 0 | SteamID: 76561198040411592 | Name: <(^^<) pixlcrashr | Team ID: 2 | Squad ID: 1 | Is Leader: True | Role: MEA_SL_02
----- Recently Disconnected Players [Max of 15] -----",
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
        },
        new object[]
        {
            @"----- Active Players -----
ID: 1 | SteamID: 76561197997976981 | Name: Kable | Team ID: 1 | Squad ID: 1 | Is Leader: True | Role: USA_Recruit
ID: 3 | SteamID: 76561198041657641 | Name: egonder | Team ID: 1 | Squad ID: 3 | Is Leader: True | Role: USA_SL_01
ID: 0 | SteamID: 76561198040411592 | Name: <(^^<) pixlcrashr | Team ID: 1 | Squad ID: 1 | Is Leader: False | Role: USA_Rifleman_01
----- Recently Disconnected Players [Max of 15] -----
ID: 2 | SteamID: 76561197971138466 | Since Disconnect: 03m.13s | Name: Mithrim",
            new ListPlayersResult(
                new List<Player>
                {
                    new(
                        1,
                        76561197997976981,
                        "Kable",
                        Team.Team1,
                        true,
                        "USA_Recruit",
                        1
                    ),
                    new(
                        3,
                        76561198041657641,
                        "egonder",
                        Team.Team1,
                        true,
                        "USA_SL_01",
                        3
                    ),
                    new(
                        0,
                        76561198040411592,
                        "<(^^<) pixlcrashr",
                        Team.Team1,
                        false,
                        "USA_Rifleman_01",
                        1
                    )
                },
                new List<DisconnectedPlayer>
                {
                    new (
                        2,
                        76561197971138466,
                        new TimeSpan(0, 0, 3, 13),
                        "Mithrim"
                    )
                })
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