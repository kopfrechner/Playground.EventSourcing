using Marten;
using Microsoft.AspNetCore.Mvc;
using Playground.EventSourcing.Aggregates;

namespace Playground.EventSourcing.Api;

public record UploadProductRevisionRequest(string Description, string InternalDescription, string ChangeLog, DateTimeOffset UploadedAt, string UploadedBy);
public record ApproveProductRevisionRequest(string Description, string InternalDescription, string ChangeLog, string ApprovalComment, DateTimeOffset ApprovedAt, string ApprovedBy);
public record DeclineProductRevisionRequest(string DeclinedComment, DateTimeOffset DeclinedAt, string DeclinedBy);

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

    private static async Task<IResult> UploadProductRevision(IDocumentSession session, Guid productId, [FromBody] UploadProductRevisionRequest request)
    {
        var product = await session.Events.AggregateStreamAsync<Product>(productId);
        if (product == null) return Results.NotFound();

        product.ApplyEvent(new ProductRevisionUploaded(
            productId,
            request.Description,
            request.InternalDescription,
            request.ChangeLog,
            request.UploadedAt,
            request.UploadedBy));
        
        session.Events.AppendAndClearUncommitedEvents(product);
        await session.SaveChangesAsync();
        return Results.Ok();
    }

    private static async Task<IResult> ApproveProductRevision(IDocumentSession session, Guid productId, [FromBody] ApproveProductRevisionRequest request)
    {
        var product = await session.Events.AggregateStreamAsync<Product>(productId);
        if (product == null) return Results.NotFound();

        product.ApplyEvent(new ProductRevisionApproved(
            productId,
            request.Description,
            request.InternalDescription,
            request.ChangeLog,
            request.ApprovalComment,
            request.ApprovedAt,
            request.ApprovedBy));

        session.Events.AppendAndClearUncommitedEvents(product);
        await session.SaveChangesAsync();
        return Results.Ok();
    }

    private static async Task<IResult> DeclineProductRevision(IDocumentSession session, Guid productId, [FromBody] DeclineProductRevisionRequest request)
    {
        var product = await session.Events.AggregateStreamAsync<Product>(productId);
        if (product == null) return Results.NotFound();

        product.ApplyEvent(new ProductRevisionDeclined(
            productId,
            request.DeclinedComment,
            request.DeclinedAt,
            request.DeclinedBy));

        session.Events.AppendAndClearUncommitedEvents(product);
        await session.SaveChangesAsync();
        return Results.Ok();
    }
}