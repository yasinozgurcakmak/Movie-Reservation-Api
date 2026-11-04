namespace EntityLayer.Entities;

public class Hall
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int RowCount { get; set; }
    public int SeatsPerRow { get; set; }
    public uint Version { get; set; }
}
