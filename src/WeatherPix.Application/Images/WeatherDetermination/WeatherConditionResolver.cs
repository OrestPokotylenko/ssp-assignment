using WeatherPix.Domain.WeatherStation;

namespace WeatherPix.Application.Images.WeatherDetermination;

public class WeatherConditionResolver : IWeatherConditionResolver
{
    public WeatherConditions Resolve(WeatherStation station)
    {
        return new WeatherConditions(
            DetermineSky(station),
            DeterminePrecipitation(station),
            DetermineWind(station),
            DetermineVisibility(station),
            DetermineDayPeriod(station));
    }

    private static SkyCondition DetermineSky(WeatherStation station)
    {
        return (station.HumidityPercentage, station.AirPressure) switch
        {
            ( <= 65, >= 1018) => SkyCondition.Clear,
            ( >= 80, <= 1010) => SkyCondition.Overcast,
            _ => SkyCondition.Cloudy
        };
    }

    private static PrecipitationCondition DeterminePrecipitation(
        WeatherStation station)
    {
        if (station.HumidityPercentage < 90 ||
            station.AirPressure > 1005)
        {
            return PrecipitationCondition.None;
        }

        return station.TemperatureCelsius <= 0
            ? PrecipitationCondition.Snow
            : PrecipitationCondition.Rain;
    }

    private static WindCondition DetermineWind(
        WeatherStation station)
    {
        if (station.WindGustMetersPerSecond >= 17 ||
            station.WindSpeedMetersPerSecond >= 12)
        {
            return WindCondition.Stormy;
        }

        if (station.WindGustMetersPerSecond >= 12 ||
            station.WindSpeedMetersPerSecond >= 8)
        {
            return WindCondition.Windy;
        }

        return WindCondition.Calm;
    }

    private static VisibilityCondition DetermineVisibility(
        WeatherStation station)
    {
        return station.VisibilityMeters is <= 1000
            ? VisibilityCondition.Foggy
            : VisibilityCondition.Clear;
    }

    private static DayPeriod DetermineDayPeriod(
        WeatherStation station)
    {
        return station.MeasuredAt.Hour switch
        {
            >= 5 and < 11 => DayPeriod.Morning,
            >= 11 and < 17 => DayPeriod.Afternoon,
            >= 17 and < 21 => DayPeriod.Evening,
            _ => DayPeriod.Night
        };
    }
}