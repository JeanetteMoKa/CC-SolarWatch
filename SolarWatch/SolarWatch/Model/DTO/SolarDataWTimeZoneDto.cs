using SolarWatch.Model.DbModel;

namespace SolarWatch.Model.DTO;

public class SolarDataWTimeZoneDto
{
    public SolarData SolarData { get; set; }
    public TimeZoneData TimeZoneData { get; set; }
}