using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using SpaceXLaunch.Models;
using EasyCaching.Core;
using EasyCaching.InMemory;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

var graphQLClient = new GraphQLHttpClient(builder.Configuration.GetConnectionString("Default"), new NewtonsoftJsonSerializer());

builder.Services.AddEasyCaching(options =>
{
    options.UseInMemory("inMemoryCache");
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/launches", async (string per_page = "10", string page = "1") =>
{
    int amount;
    int curr;

    if(int.TryParse(per_page, out amount))
    {
        
    }
    else
    {
        amount = 10;
    }

    if (int.TryParse(page, out curr))
    {

    }
    else
    {
        curr = 1;
    }

    if ( amount <= 0)
    {
        amount = 10;
    }

    if(curr <= 0)
    {
        curr = 1;
    }

    int start = amount * (curr - 1);

    var launchRequest = new GraphQLRequest
    {
        Query = @"
            query ($per_page: Int, $start: Int){
            launches (limit: $per_page, offset: $start){
                id
                mission_name
            }
        }",
        Variables = new
        {
            per_page = curr * amount,
            start = (curr-1) * amount
        }
    };

    var graphQLResponse = await graphQLClient.SendQueryAsync<LaunchesResponse>(launchRequest);

    return Results.Ok(graphQLResponse.Data.launches);

});

app.MapGet("/launches/{id}", async (string id, IEasyCachingProvider _provider) =>
{
    if(await _provider.ExistsAsync(id))
    {
        var cachedLaunch = await _provider.GetAsync<Launch>(id);
        return Results.Ok(cachedLaunch.Value);
    }

    var launchRequest = new GraphQLRequest
    {
        Query = @"
            query ($launchId: ID!){
              launch(id: $launchId) {
                mission_name
                launch_date_local
                rocket {
                  rocket {
                    name
                    first_flight
                    success_rate_pct
                  }
                }
              }
            }",
        Variables = new
        {
            launchId = id
        }
    };

    var graphQLResponse = await graphQLClient.SendQueryAsync<LaunchResponse>(launchRequest);

    graphQLResponse.Data.launch.cached_time = DateTime.Now;

    await _provider.SetAsync(id, graphQLResponse.Data.launch, TimeSpan.FromMinutes(60));

    return graphQLResponse.Data.launch is not null ?  Results.Ok(graphQLResponse.Data.launch) : Results.NotFound();
});

app.Run();



