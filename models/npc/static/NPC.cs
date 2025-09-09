using System.Collections.Generic;

public class NPC
{
    public string id { get; set; }
    public string name { get; set; }
    public List<string> dialogues { get; set; }
    public Position position { get; set; }
    public List<ScheduleItem> schedule { get; set; }
}

public class Position
{
    public float x { get; set; }
    public float y { get; set; }
}

public class ScheduleItem
{
    public string time { get; set; }
    public string location { get; set; }
}