namespace WeatherPix.Domain.WeatherStation;

public sealed record WeatherConditions(
    SkyCondition Sky,
    PrecipitationCondition Precipitation,
    WindCondition Wind,
    VisibilityCondition Visibility,
    DayPeriod DayPeriod);