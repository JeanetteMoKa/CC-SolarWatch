using CsvHelper.Configuration.Attributes;

namespace SolarWatch.Model;

public class CountryCodes
{
    [Name("name")] public string Name { get; set; }

    [Name("alpha-2")] public string Alpha2 { get; set; }

    [Name("alpha-3")] public string Alpha3 { get; set; }

    [Name("country-code")] public string CountryCode { get; set; }

    [Name("iso_3166-2")] public string ISO31662 { get; set; }

    [Name("region")] public string Region { get; set; }

    [Name("sub-region")] public string SubRegion { get; set; }

    [Name("intermediate-region")] public string IntermediateRegion { get; set; }

    [Name("region-code")] public string RegionCode { get; set; }

    [Name("sub-region-code")] public string SubRegionCode { get; set; }

    [Name("intermediate-region-code")] public string IntermediateRegionCode { get; set; }
}