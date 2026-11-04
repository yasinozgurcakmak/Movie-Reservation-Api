using BusinessLayer.Validators.Reservations;
using DtoLayer.Reservations;

namespace MovieReservationAPI.Tests;

public class ReservationRequestValidatorTests
{
    private readonly ReservationRequestValidator _validator = new();

    [Theory]
    [InlineData(10, true)]
    [InlineData(11, false)]
    public void Limits_seats_per_request(int seatCount, bool isValid)
    {
        var seats = Enumerable.Range(1, seatCount).Select(n => $"A{n}").ToList();

        var result = _validator.Validate(new ReservationRequest(1, seats));

        Assert.Equal(isValid, result.IsValid);
    }

    [Fact]
    public void Rejects_duplicate_seats_regardless_of_case_and_spacing()
    {
        var result = _validator.Validate(new ReservationRequest(1, ["A1", " a1 "]));

        Assert.False(result.IsValid);
    }
}
