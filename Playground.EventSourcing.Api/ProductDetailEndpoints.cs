using Marten;
using Microsoft.AspNetCore.Mvc;
using Playground.EventSourcing.Aggregates;
using Playground.EventSourcing.Aggregates.Projections;

namespace Playground.EventSourcing.Api;

public static class ProductDetailEndpoints
{
    public static void MapProductDetails(this WebApplication app)
    {
        var group = app.MapGroup("/products/{productId}")
            .WithTags("Products");
        
        group.MapPost("/lock", LockProduct)
            .WithName("LockProduct")
            .WithOpenApi();
        
        group.MapPost("/unlock", UnlockProduct)
            .WithName("UnlockProduct")
            .WithOpenApi();
        
        group.MapGet("/", GetProduct)
            .WithName("GetProduct")
            .WithOpenApi();
        
        group.MapGet("/revisions-to-review", GetRevisionsToReview)
            .WithName("GetRevisionsToReview")
            .WithOpenApi();

        group.MapGet("/history", GetProductHistory)
            .WithName("GetProductHistory")
            .WithOpenApi();
    }

    private static async Task<IResult> LockProduct(IDocumentSession session, Guid productId, [FromBody] ProductLocked locked)
    {
        var product = await session.Events.AggregateStreamAsync<Product>(productId);
        if (product == null) return Results.NotFound();

        var domainEvent = locked with { ProductId = productId };
        session.Events.Append(productId, domainEvent);
        await session.SaveChangesAsync();
        return Results.Ok();
    }

    private static async Task<IResult> UnlockProduct(IDocumentSession session, Guid productId, [FromBody] ProductUnlocked unlocked)
    {
        var product = await session.Events.AggregateStreamAsync<Product>(productId);
        if (product == null) return Results.NotFound();

        var domainEvent = unlocked with { ProductId = productId };
        session.Events.Append(productId, domainEvent);
        await session.SaveChangesAsync();
        return Results.Ok();
    }

    private static async Task<IResult> GetProduct(IDocumentSession session, Guid productId)
    {
        var product = await session.Events.AggregateStreamAsync<Product>(productId);
        return product != null ? Results.Ok(product) : Results.NotFound();
    }

    private static async Task<IResult> GetRevisionsToReview(IDocumentSession session)
    {
        var revisions = await session.Query<ProductRevisionToReviewState>().ToListAsync();
        return Results.Ok(revisions);
    }

    private static async Task<IResult> GetProductHistory(IDocumentSession session, Guid productId)
    {
        var history = await session.Query<ProductRevisionHistoryEntry>()
            .Where(h => h.ProductId == productId)
            .OrderBy(h => h.EventTime)
            .ToListAsync();
        return Results.Ok(history);
    }
}