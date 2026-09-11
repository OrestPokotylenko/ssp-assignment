using WeatherPix.Domain.WeatherStation;

namespace WeatherPix.Application.Images.WeatherDetermination;

public interface IWeatherConditionResolver
{
    WeatherConditions Resolve(WeatherStation station);
}