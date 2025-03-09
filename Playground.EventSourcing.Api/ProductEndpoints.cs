using Marten;
using Microsoft.AspNetCore.Mvc;
using Playground.EventSourcing.Aggregates;
using Playground.EventSourcing.Aggregates.Projections;

namespace Playground.EventSourcing.Api;

public record CreateProductRequest(string Alias);

public static class ProductEndpoints
{
    public static void MapProducts(this WebApplication app)
    {
        var group = app.MapGroup("/products")
            .WithTags("Products");

        group.MapPost("", CreateProduct)
            .WithName("CreateProduct")
            .WithOpenApi();

        group.MapGet("", GetAllProducts)
            .WithName("GetAllProducts")
            .WithOpenApi();
        
        group.MapGet("/history", GetRevisionHistory)
            .WithName("GetRevisionHistory")
            .WithOpenApi();
    }

    private static async Task<IResult> CreateProduct(IDocumentSession session, ICurrentUser currentUser, [FromBody] CreateProductRequest request)
    {
        var productId = Guid.NewGuid();
        var product = Product.Create(new ProductAdded(productId, request.Alias, DateTimeOffset.Now, currentUser.FullName));
        session.Events.AppendAndClearUncommitedEvents(product);
        await session.SaveChangesAsync();
        return Results.Created($"/{productId}", productId);
    }

    private static async Task<IResult> GetAllProducts(IDocumentSession session)
    {
        var products = await session.Query<Product>().ToListAsync();
        return Results.Ok(products);
    }

    private static async Task<IResult> GetRevisionHistory(IDocumentSession session)
    {
        var history = await session.Query<ProductRevisionHistoryEntry>()
            .OrderBy(h => h.EventTime)
            .ToListAsync();
        return Results.Ok(history);
    }
}