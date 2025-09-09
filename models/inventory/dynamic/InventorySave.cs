using System.Collections.Generic;

public class InventorySave
{
    public string player_name { get; set; }
    public List<InventorySlot> inventory { get; set; }
}

public class InventorySlot
{
    public string item_id { get; set; }
    public int amount { get; set; }
}
