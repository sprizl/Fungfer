using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;

public partial class NPCManager : Node
{
	public static NPCManager Instance { get; private set; }

	private string npcDataPath = "res://assets/data/npc_data.json";
	private string npcStatePath = "user://npc_state.json";

	public List<NPCInstance> NPCList { get; private set; } = [];

	//NOTE: Set ตัวเองไว้เพื่อให้ Global เรียกใช้ได้
	public override void _EnterTree()
	{
		Instance = this;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		CreateUserSaveFile();
	}

	private void CreateUserSaveFile()
	{
		if (FileAccess.FileExists(npcStatePath))
		{
			return;
		}

		string staticJson = FileAccess.Open(npcDataPath, FileAccess.ModeFlags.Read).GetAsText();
		var staticList = JsonSerializer.Deserialize<List<NPC>>(staticJson);

		var defaultState = new List<NPCState>();
		foreach (var state in staticList)
		{
			defaultState.Add(new NPCState
			{
				id = state.id,
				affection = 0,
				quest_state = QuestState.not_started,
			});
		}

		var stateJson = JsonSerializer.Serialize(defaultState, new JsonSerializerOptions { WriteIndented = true });
		using var f = FileAccess.Open(npcStatePath, FileAccess.ModeFlags.Write);
		f.StoreString(stateJson);

		GD.Print("✅ สร้างไฟล์ npc_state.json สำเร็จ");
	}

	private void LoadNPCs()
	{
		GD.Print("🔄 กำลังโหลด NPCs...");

		string dataJson = FileAccess.Open(npcDataPath, FileAccess.ModeFlags.Read).GetAsText();
		string stateJson = FileAccess.Open(npcStatePath, FileAccess.ModeFlags.Read).GetAsText();

		var dataList = JsonSerializer.Deserialize<List<NPC>>(dataJson);
		var stateList = JsonSerializer.Deserialize<List<NPCState>>(stateJson);

		NPCList.Clear();

		foreach (var npc in dataList)
		{
			var state = stateList.FirstOrDefault(x => x.id == npc.id)
						?? new NPCState { id = npc.id, affection = 0, quest_state = QuestState.not_started };

			NPCList.Add(new NPCInstance
			{
				baseData = npc,
				saveData = state
			});
		}

		GD.Print($"✅ โหลด NPC สำเร็จทั้งหมด {NPCList.Count} ตัว");
	}

	public NPCInstance GetNPC(string id)
	{
		return NPCList.FirstOrDefault(x => x.baseData.id == id);
	}

	public void SaveNPCStates()
	{
		var states = NPCList.Select(x => x.saveData).ToList();
		string json = JsonSerializer.Serialize(states, new JsonSerializerOptions { WriteIndented = true });

		using var f = FileAccess.Open(npcStatePath, FileAccess.ModeFlags.Write);
		f.StoreString(json);

		GD.Print("✅ บันทึกสถานะ NPC สำเร็จ");		
	}
}
