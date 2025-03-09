using Marten;
using Marten.Events.Projections;
using Microsoft.AspNetCore.Mvc;
using Playground.EventSourcing.Aggregates;
using Playground.EventSourcing.Aggregates.Common;
using Playground.EventSourcing.Aggregates.Projections;
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


// Neues Product erstellen
app.MapPost("/products", async (IDocumentSession session, [FromBody] ProductAdded added) =>
{
    var product = Product.Create(added);
    session.Events.StartStream(added.ProductId, added);
    await session.SaveChangesAsync();
    return Results.Created($"/products/{added.ProductId}", added.ProductId);
})
.WithName("CreateProduct")
.WithOpenApi();

// ProductRevisionUploaded hinzufügen
app.MapPost("/products/{productId}/revision/upload", async (IDocumentSession session, Guid productId, [FromBody] ProductRevisionUploaded uploaded) =>
{
    var product = await session.Events.AggregateStreamAsync<Product>(productId);
    if (product == null) return Results.NotFound();

    var domainEvent = uploaded with { ProductId = productId };
    session.Events.Append(productId, domainEvent);
    await session.SaveChangesAsync();
    return Results.Ok();
})
.WithName("UploadProductRevision")
.WithOpenApi();

// ProductRevisionApproved hinzufügen
app.MapPost("/products/{productId}/revision/approve", async (IDocumentSession session, Guid productId, [FromBody] ProductRevisionApproved approved) =>
{
    var product = await session.Events.AggregateStreamAsync<Product>(productId);
    if (product == null) return Results.NotFound();

    var domainEvent = approved with { ProductId = productId };
    session.Events.Append(productId, domainEvent);
    await session.SaveChangesAsync();
    return Results.Ok();
})
.WithName("ApproveProductRevision")
.WithOpenApi();

// ProductRevisionDeclined hinzufügen
app.MapPost("/products/{productId}/revision/decline", async (IDocumentSession session, Guid productId, [FromBody] ProductRevisionDeclined declined) =>
{
    var product = await session.Events.AggregateStreamAsync<Product>(productId);
    if (product == null) return Results.NotFound();

    var domainEvent = declined with { ProductId = productId };
    session.Events.Append(productId, domainEvent);
    await session.SaveChangesAsync();
    return Results.Ok();
})
.WithName("DeclineProductRevision")
.WithOpenApi();

// ProductLocked hinzufügen
app.MapPost("/products/{productId}/lock", async (IDocumentSession session, Guid productId, [FromBody] ProductLocked locked) =>
{
    var product = await session.Events.AggregateStreamAsync<Product>(productId);
    if (product == null) return Results.NotFound();

    var domainEvent = locked with { ProductId = productId };
    session.Events.Append(productId, domainEvent);
    await session.SaveChangesAsync();
    return Results.Ok();
})
.WithName("LockProduct")
.WithOpenApi();

// ProductUnlocked hinzufügen
app.MapPost("/products/{productId}/unlock", async (IDocumentSession session, Guid productId, [FromBody] ProductUnlocked unlocked) =>
{
    var product = await session.Events.AggregateStreamAsync<Product>(productId);
    if (product == null) return Results.NotFound();

    var domainEvent = unlocked with { ProductId = productId };
    session.Events.Append(productId, domainEvent);
    await session.SaveChangesAsync();
    return Results.Ok();
})
.WithName("UnlockProduct")
.WithOpenApi();

// Product abfragen
app.MapGet("/products/{productId}", async (IDocumentSession session, Guid productId) =>
{
    var product = await session.Events.AggregateStreamAsync<Product>(productId);
    return product != null ? Results.Ok(product) : Results.NotFound();
})
.WithName("GetProduct")
.WithOpenApi();

// Alle Products abfragen (basierend auf Events)
app.MapGet("/products", async (IDocumentSession session) =>
{
    var products = await session.Query<Product>().ToListAsync();
    return Results.Ok(products);
})
.WithName("GetAllProducts")
.WithOpenApi();

// Revisions zur Überprüfung abfragen
app.MapGet("/products/revisions-to-review", async (IDocumentSession session) =>
{
    var revisions = await session.Query<ProductRevisionToReviewState>().ToListAsync();
    return Results.Ok(revisions);
})
.WithName("GetRevisionsToReview")
.WithOpenApi();

// Revisions-Historie abfragen
app.MapGet("/products/history", async (IDocumentSession session) =>
{
    var history = await session.Query<ProductRevisionHistoryEntry>()
        .OrderBy(h => h.EventTime)
        .ToListAsync();
    return Results.Ok(history);
})
.WithName("GetRevisionHistory")
.WithOpenApi();

// Historie eines bestimmten Products abfragen
app.MapGet("/products/{productId}/history", async (IDocumentSession session, Guid productId) =>
{
    var history = await session.Query<ProductRevisionHistoryEntry>()
        .Where(h => h.ProductId == productId)
        .OrderBy(h => h.EventTime)
        .ToListAsync();
    return Results.Ok(history);
})
.WithName("GetProductHistory")
.WithOpenApi();

app.Run();