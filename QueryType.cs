namespace HotChocolateTest
{
    public class QueryType : ObjectType<Query>
    {
        protected override void Configure(IObjectTypeDescriptor<Query> descriptor)
        {
            descriptor
                .Field("WeatherList").Type<ListType<WeatherForecastType>>()
                .Resolve(x => x.Parent<Query>().Get());
        }
    }
}
