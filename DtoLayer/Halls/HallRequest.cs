namespace DtoLayer.Halls;

// Version is optional; when sent on update, the change is rejected if someone else modified the hall since it was read.
public record HallRequest(string Name, int RowCount, int SeatsPerRow, uint? Version = null);
