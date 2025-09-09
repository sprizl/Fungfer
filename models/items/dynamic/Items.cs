public class Items
{
    public string id { get; set; }
    public string name { get; set; }
    public string type { get; set; }       // เช่น "weapon", "consumable"
    public string effect { get; set; }     // เช่น "heal", "poison"
    public int value { get; set; }         // เช่น ค่ารักษา 50
    public int attack { get; set; }        // ใช้กับอาวุธ
}
