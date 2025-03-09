using Marten;
using Marten.Events.Projections;
using Playground.EventSourcing.Aggregates.Common;
using Playground.EventSourcing.Aggregates.Projections;
using Playground.EventSourcing.Api;
using Scalar.AspNetCore;
using Weasel.Core;

var builder = WebApplication.CreateBuilder(args);

// Marten
builder.Services.AddMarten(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.Connection(connectionString!);
    options.AutoCreateSchemaObjects = AutoCreate.CreateOrUpdate;
    
    options.UseSystemTextJsonForSerialization();
    
    var eventTypes = typeof(IDomainEvent).Assembly.GetTypes()
        .Where(t => typeof(IDomainEvent).IsAssignableFrom(t) && t is { IsClass: true, IsAbstract: false })
        .ToList();

    foreach (var type in eventTypes)
    {
        options.Events.AddEventType(type);
    }

    // Projektionen registrieren
    options.Projections.Add<ProductRevisionToReviewProjection>(ProjectionLifecycle.Inline);
    options.Projections.Add<ProductRevisionHistoryProjection>(ProjectionLifecycle.Inline);
});

builder.Services.AddScoped<ICurrentUser, FakeCurrentUser>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Endpoints
app.MapProducts();
app.MapProductDetails();
app.MapProductRevisions();

app.Run();