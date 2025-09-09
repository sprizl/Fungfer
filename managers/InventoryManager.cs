using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

public partial class InventoryManager : Node
{
	private string npcFilePath => "user://npc_data.json";
	private string itemsFilePath => "user://items_data.json";
	private string inventoryFilePath => "user://inventory_save.json";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		CreateUserSaveFile();
	}

	private void CreateUserSaveFile()
	{
		if (FileAccess.FileExists(inventoryFilePath))
		{
			return;
		}

		var inventory = new InventorySave
		{
			player_name = "New Player",
			inventory = []
		};

		string json = JsonSerializer.Serialize(inventory, new JsonSerializerOptions { WriteIndented = true });
		using var file = FileAccess.Open(inventoryFilePath, FileAccess.ModeFlags.Write);
		file.StoreString(json);
	}
}
