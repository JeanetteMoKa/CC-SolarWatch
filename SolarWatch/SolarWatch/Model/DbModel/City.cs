namespace SolarWatch.Model.DbModel;

public class City
{
    public int Id { get; init; }
    public string Name { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? State { get; set; }
    public string Country { get; set; }
}