using Squadmania.Squad.Rcon.Models;
using Squadmania.Squad.Rcon.Parsers;

namespace Squadmania.Squad.Rcon.Tests;

public class ListSquadsParserTests
{
    public static readonly IEnumerable<object[]> ListSquadsTestData = new List<object[]>
    {
        new object[]
        {
            "----- Active Squads -----\nTeam ID: 1 (III Corps)\nTeam ID: 2 (60th Prince Assur Armored Brigade)\nID: 1 | Name: Squad 1 | Size: 1 | Locked: False | Creator Name: pixlcrashr | Creator Steam ID: 76561198040411592",
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
            "----- Active Squads -----\nTeam ID: 1 (III Corps)\nID: 1 | Name: Squad 1 | Size: 1 | Locked: False | Creator Name: pixlcrashr | Creator Steam ID: 76561198040411592\nTeam ID: 2 (60th Prince Assur Armored Brigade)",
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
        }
    };
    
    [Theory]
    [MemberData(nameof(ListSquadsTestData))]
    public void TestListSquads(
        string raw,
        Models.Squad[] expectedSquads)
    {
        var parser = new ListSquadsParser();

        var squads = parser.Parse(raw);

        for (var i = 0; i < expectedSquads.Length; i++)
        {
            var cur = squads[i];
            var exp = expectedSquads[i];
            
            Assert.Equal(exp, cur);
        }
    }
}