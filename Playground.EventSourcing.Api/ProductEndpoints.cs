using Marten;
using Microsoft.AspNetCore.Mvc;
using Playground.EventSourcing.Aggregates;
using Playground.EventSourcing.Aggregates.Projections;

namespace Playground.EventSourcing.Api;

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

    private static async Task<IResult> CreateProduct(IDocumentSession session, [FromBody] ProductAdded added)
    {
        var product = Product.Create(added);
        session.Events.StartStream(added.ProductId, added);
        await session.SaveChangesAsync();
        return Results.Created($"/{added.ProductId}", added.ProductId);
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