using EntityLayer.Entities;

namespace BusinessLayer.Common;

public static class SeatLayout
{
    public static string ToRowLabel(int rowIndex) => ((char)('A' + rowIndex)).ToString();

    public static string ToSeatCode(string rowLabel, int seatNumber) => $"{rowLabel}{seatNumber}";

    public static bool Exists(Hall hall, string seatCode)
    {
        var rowIndex = seatCode[0] - 'A';
        return rowIndex >= 0 && rowIndex < hall.RowCount
            && int.TryParse(seatCode[1..], out var seatNumber)
            && seatNumber >= 1 && seatNumber <= hall.SeatsPerRow;
    }
}
