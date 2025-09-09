using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

public partial class MapManager : Node
{
	public static MapManager Instance { get; private set; }

	[Export] public string MapGraphJsonPath = "res://assets/data/map_graph.json";
    [Export] public string StartMapKey = "Map_Start";
    [Export] public float EdgePadding = 0f; // เผื่อขอบจอ (ถ้าอยากให้เปลี่ยนก่อน/หลังขอบเล็กน้อย)

	private Player _player;
    private Node _mapContainer;
    private Node _currentMap;

	private Dictionary<string, (string scene, Dictionary<string, string> neighbors)> _graph = [];
	private string _currentMapKey;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Instance = this;
		GD.Print($"[MapManager] READY from Autoload. Instance = {GetInstanceId()}");
	}

	public void RegisterPlayer(Player player, Node mapContainer)
	{
		if (_player != null) return;

		_player = player;
		_mapContainer = mapContainer;

		if (_player == null || _mapContainer == null)
		{
			GD.PushError("[MapManager] RegisterPlayer received null arguments.");
			return;
		}

		LoadMapGraphFromJson();
		_currentMapKey = StartMapKey;
		LoadMap(_currentMapKey, "center");
	}

	public override void _Process(double delta)
	{
		if (_player == null || _currentMap == null) return;

		var screen = GetViewport().GetVisibleRect().Size;
		var x = _player.GlobalPosition.X;
		var y = _player.GlobalPosition.Y;

		if (x > screen.X + EdgePadding) TryMoveToDirection("right");
		else if (x < 0f - EdgePadding) TryMoveToDirection("left");
		else if (y > screen.Y + EdgePadding) TryMoveToDirection("down");
		else if (y < 0f - EdgePadding) TryMoveToDirection("up");
    }

	private void LoadMapGraphFromJson()
	{
		GD.Print($"[MapManager] Loading JSON from {MapGraphJsonPath}");
		string jsonText = FileAccess.Open(MapGraphJsonPath, FileAccess.ModeFlags.Read).GetAsText();
		var parsed = Json.ParseString(jsonText);
		if (parsed.VariantType != Variant.Type.Dictionary)
		{
			GD.PushError($"[MapManager] ❌ JSON is invalid or not a dictionary.");
			return;
		}

		var root = parsed.AsGodotDictionary();
		if (root == null)
		{
			GD.PushError($"[MapManager] ❌ Parsed JSON is not a dictionary.");
			return;
		}

		_graph.Clear();

		foreach (var key in root.Keys)
		{
			var mapKey = key.AsString();
			var mapInfo = root[mapKey].AsGodotDictionary();

			string scene = mapInfo["scene"].AsString();

			var neighbor = new Dictionary<string, string>();
			if (mapInfo.ContainsKey("neighbors"))
			{
				var n = mapInfo["neighbors"].AsGodotDictionary();
				foreach (var dirKey in n.Keys)
				{
					neighbor[dirKey.AsString()] = n[dirKey].AsString();
				}
			}

			_graph[mapKey] = (scene, neighbor);
		}
	}

	private void TryMoveToDirection(string dir)
	{
		var neighbors = _graph[_currentMapKey].neighbors;

		if (!neighbors.TryGetValue(dir, out var nextKey))
		{
			return;
		}

		_currentMapKey = nextKey;
		LoadMap(_currentMapKey, dir);
	}

	private void LoadMap(string mapKey, string fromDirection)
	{
		_currentMap?.QueueFree();
		_currentMap = null;

		var scenePath = _graph[mapKey].scene;
		var scene = GD.Load<PackedScene>(scenePath);
		if (scene == null)
		{
			GD.PushError($"Failed to load scene: {scenePath}");
			return;
		}

		_currentMap = scene.Instantiate();
		_mapContainer.CallDeferred("add_child", _currentMap);

		var tileMap = _currentMap.GetNodeOrNull<TileMapLayer>("Ground");
		Vector2I usedSize = tileMap.GetUsedRect().Size;
		Vector2 tileSize = tileMap.TileSet.TileSize;
		Vector2 totalSize = usedSize * tileSize;
		if (tileMap != null)
		{
			_player.CurrentMapSize = totalSize;
		}
		else
		{
			GD.PushWarning($"TileMap not found in map '{mapKey}'.");
			_player.CurrentMapSize = new Vector2(320, 180);
		}

		string spawnName = fromDirection switch
		{
			"right" => "SpawnFromLeft",
			"left" => "SpawnFromRight",
			"up" => "SpawnFromDown",
			"down" => "SpawnFromUp",
			_ => "SpawnCenter"
		};

		var spawn = _currentMap.GetNodeOrNull<Node2D>(spawnName);
		spawn ??= _currentMap.GetNodeOrNull<Node2D>("SpawnCenter");

		if (spawn != null)
		{
			_player.GlobalPosition = spawn.GlobalPosition;
		}
		else
		{
			GD.PushWarning($"Spawn point '{spawnName}' or 'SpawnCenter' not found in map '{mapKey}'.");
			_player.GlobalPosition = Vector2.Zero;
		}

		//NOTE: กําหนด limit ของ camera
		var camera = _player.GetNode<Camera2D>("Camera2D");
		if (camera != null)
		{
			camera.LimitLeft = 0;
			camera.LimitTop = 0;
			camera.LimitRight = (int)totalSize.X;
			camera.LimitBottom = (int)totalSize.Y;
			camera.MakeCurrent();
		}
	}

	//NOTE: เปลี่ยน map by signal
	// public void OnChangeMap(string nextMapKey)
	// {
	// 	if (string.IsNullOrEmpty(nextMapKey)) return;
	// 	if (!_graph.ContainsKey(nextMapKey))
	// 	{
	// 		GD.PushWarning($"Map key '{nextMapKey}' not found in graph.");
	// 		return;
	// 	}
	// 	_currentMapKey = nextMapKey;
	// 	LoadMap(_currentMapKey, "center");
	// }
}
