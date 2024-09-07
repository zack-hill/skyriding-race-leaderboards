using System.Collections.Generic;

namespace CompanionApp.Models;

public class AccountRaceData
{
    public string AccountName { get; set; }
    public List<RaceTime> RaceTimes { get; set; } = new List<RaceTime>();
}