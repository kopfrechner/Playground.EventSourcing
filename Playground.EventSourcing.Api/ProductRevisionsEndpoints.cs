using Marten;
using Microsoft.AspNetCore.Mvc;
using Playground.EventSourcing.Aggregates;

namespace Playground.EventSourcing.Api;

public static class ProductRevisionsEndpoints
{
    public static void MapProductRevisions(this WebApplication app)
    {
        var group = app.MapGroup("/products/{productId}/revision")
            .WithTags("Products");

        group.MapPost("/upload", UploadProductRevision)
            .WithName("UploadProductRevision")
            .WithOpenApi();
        
        group.MapPost("/approve", ApproveProductRevision)
            .WithName("ApproveProductRevision")
            .WithOpenApi();
        
        group.MapPost("/decline", DeclineProductRevision)
            .WithName("DeclineProductRevision")
            .WithOpenApi();
    }

    private static async Task<IResult> UploadProductRevision(IDocumentSession session, Guid productId, [FromBody] ProductRevisionUploaded uploaded)
    {
        var product = await session.Events.AggregateStreamAsync<Product>(productId);
        if (product == null) return Results.NotFound();

        var domainEvent = uploaded with { ProductId = productId };
        session.Events.Append(productId, domainEvent);
        await session.SaveChangesAsync();
        return Results.Ok();
    }

    private static async Task<IResult> ApproveProductRevision(IDocumentSession session, Guid productId, [FromBody] ProductRevisionApproved approved)
    {
        var product = await session.Events.AggregateStreamAsync<Product>(productId);
        if (product == null) return Results.NotFound();

        var domainEvent = approved with { ProductId = productId };
        session.Events.Append(productId, domainEvent);
        await session.SaveChangesAsync();
        return Results.Ok();
    }

    private static async Task<IResult> DeclineProductRevision(IDocumentSession session, Guid productId, [FromBody] ProductRevisionDeclined declined)
    {
        var product = await session.Events.AggregateStreamAsync<Product>(productId);
        if (product == null) return Results.NotFound();

        var domainEvent = declined with { ProductId = productId };
        session.Events.Append(productId, domainEvent);
        await session.SaveChangesAsync();
        return Results.Ok();
    }
}