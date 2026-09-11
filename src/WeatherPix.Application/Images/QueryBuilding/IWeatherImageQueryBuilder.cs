using WeatherPix.Domain.WeatherStation;

namespace WeatherPix.Application.Images.QueryBuilding;

public interface IWeatherImageQueryBuilder
{
    string Build(WeatherConditions condition);
}