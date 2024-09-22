using System.Collections.Generic;

namespace CompanionApp.Models;

public class CharacterRaceData
{
    public string CharacterName { get; set; }
    public List<CourseTime> CourseTimes { get; set; } = [];
}