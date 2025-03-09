using Marten;
using Marten.Events.Projections;
using Playground.EventSourcing.Aggregates.Common;
using Playground.EventSourcing.Aggregates.Projections;
using Playground.EventSourcing.Api;
using Weasel.Core;

var builder = WebApplication.CreateBuilder(args);

// Marten konfigurieren
builder.Services.AddMarten(options =>
{
    options.Connection(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.AutoCreateSchemaObjects = AutoCreate.All;

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

// Swagger hinzufügen (optional)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Endpunkte
app.MapProducts();
app.MapProductDetails();
app.MapProductRevisions();

app.Run();