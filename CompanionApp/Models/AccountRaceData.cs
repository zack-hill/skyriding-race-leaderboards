using System.Collections.Generic;

namespace CompanionApp.Models;

public class AccountRaceData
{
    public string BattleTag { get; set; }
    public List<CharacterRaceData> CharacterRaceData { get; set; } = [];
}