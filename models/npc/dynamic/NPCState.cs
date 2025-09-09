using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class NPCState
{
    public string id { get; set; }             // ต้อง match กับ NPC.id
    public int affection { get; set; }         // 0 - 100
    public QuestState quest_state { get; set; }    // not_started, started, done
}


public enum QuestState
{
	not_started,
	started,
	done
}