using HotChocolate.AspNetCore;
using HotChocolateTest;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<DataLoaderAccessor>();

// TODO: auth, resolver middleware, schema filtering, mutations, arguments

builder.Services
    .AddGraphQLServer()
    .ModifyOptions(options =>
    {
        options.DefaultBindingBehavior = BindingBehavior.Explicit;
    })
    .AddQueryType<QueryType>()
    .InitializeOnStartup();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapGraphQL("/gql").WithOptions(new GraphQLServerOptions
{
    Tool = {
        DisableTelemetry = true,
        Title = "My GraphQL Explorer",
        UseBrowserUrlAsGraphQLEndpoint = true
    }
});

app.MapControllers();

app.Run();
