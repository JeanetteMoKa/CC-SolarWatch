using CsvHelper.Configuration.Attributes;

namespace SolarWatch.Model;

public class StateCodes
{
    [Name("state-name")] public string StateName { get; set; }

    [Name("state-code")] public string StateCode { get; set; }
}