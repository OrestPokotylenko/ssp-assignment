using WeatherPix.Domain.WeatherStation;

namespace WeatherPix.Application.Images.QueryBuilding;

public class WeatherImageQueryBuilder : IWeatherImageQueryBuilder
{
    public string Build(WeatherConditions conditions)
    {
        var parts = new List<string>();

        if (conditions.Visibility == VisibilityCondition.Foggy)
            parts.Add("foggy");

        if (conditions.Wind == WindCondition.Windy)
            parts.Add("windy");

        if (conditions.Wind == WindCondition.Stormy)
            parts.Add("stormy");

        if (conditions.Precipitation == PrecipitationCondition.Rain)
            parts.Add("rainy");

        if (conditions.Precipitation == PrecipitationCondition.Snow)
            parts.Add("snowy");

        parts.Add(conditions.Sky switch
        {
            SkyCondition.Clear => "clear sky",
            SkyCondition.Cloudy => "cloudy",
            SkyCondition.Overcast => "overcast",
            _ => "weather"
        });

        parts.Add(conditions.DayPeriod.ToString().ToLowerInvariant());
        parts.Add("landscape");

        return string.Join(" ", parts);
    }
}