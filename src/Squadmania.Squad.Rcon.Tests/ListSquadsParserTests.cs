using Squadmania.Squad.Rcon.Models;
using Squadmania.Squad.Rcon.Parsers;

namespace Squadmania.Squad.Rcon.Tests;

public class ListSquadsParserTests
{
    public static readonly IEnumerable<object[]> ListSquadsTestData = new List<object[]>
    {
        new object[]
        {
            @"----- Active Squads -----
Team ID: 1 (III Corps)
Team ID: 2 (60th Prince Assur Armored Brigade)
ID: 1 | Name: Squad 1 | Size: 1 | Locked: False | Creator Name: pixlcrashr | Creator Steam ID: 76561198040411592",
            new Models.Squad[]
            {
                new(
                    1,
                    Team.Team2,
                    "60th Prince Assur Armored Brigade",
                    "Squad 1",
                    1,
                    "pixlcrashr",
                    76561198040411592,
                    false
                )
            }
        },
        new object[]
        {
            @"----- Active Squads -----
Team ID: 1 (III Corps)
ID: 1 | Name: Squad 1 | Size: 1 | Locked: False | Creator Name: pixlcrashr | Creator Steam ID: 76561198040411592
Team ID: 2 (60th Prince Assur Armored Brigade)",
            new Models.Squad[]
            {
                new(
                    1,
                    Team.Team1,
                    "III Corps",
                    "Squad 1",
                    1,
                    "pixlcrashr",
                    76561198040411592,
                    false
                )
            }
        },
        new object[]
        {
            @"----- Active Squads -----
Team ID: 1 (III Corps)
ID: 1 | Name: Command Squad | Size: 1 | Locked: False | Creator Name: pixlcrashr | Creator Steam ID: 76561198040411592
ID: 2 | Name: Command Squad | Size: 1 | Locked: True | Creator Name: Kable | Creator Steam ID: 76561197997976981
ID: 3 | Name: Squad 3 | Size: 1 | Locked: False | Creator Name: egonder | Creator Steam ID: 76561198041657641
Team ID: 2 (60th Prince Assur Armored Brigade)",
            new Models.Squad[]
            {
                new(
                    1,
                    Team.Team1,
                    "III Corps",
                    "Command Squad",
                    1,
                    "pixlcrashr",
                    76561198040411592,
                    false
                ),
                new(
                    2,
                    Team.Team1,
                    "III Corps",
                    "Command Squad",
                    1,
                    "Kable",
                    76561197997976981,
                    true
                ),
                new(
                    3,
                    Team.Team1,
                    "III Corps",
                    "Squad 3",
                    1,
                    "egonder",
                    76561198041657641,
                    false
                ),
            }
        }
    };
    
    [Theory]
    [MemberData(nameof(ListSquadsTestData))]
    public void TestListSquads(
        string raw,
        Models.Squad[] expectedSquads)
    {
        var parser = new ListSquadsParser();

        var squads = parser.Parse(raw).ToArray();
        
        Assert.Equal(expectedSquads, squads);
    }
}