namespace SolarWatch.Model.DbModel;

public class SolarData
{
    public int Id { get; init; }
    public DateTime Sunrise { get; set; }
    public DateTime Sunset { get; set; }

    public City City { get; set; }
}