using Marten;
using Microsoft.AspNetCore.Mvc;
using Playground.EventSourcing.Aggregates;

namespace Playground.EventSourcing.Api;

public record UploadProductRevisionRequest(string Description, string InternalDescription, string ChangeLog);
public record ApproveProductRevisionRequest(string Description, string InternalDescription, string ChangeLog, string ApprovalComment);
public record DeclineProductRevisionRequest(string DeclinedComment);

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

    private static async Task<IResult> UploadProductRevision(IDocumentSession session, ICurrentUser currentUser, Guid productId, [FromBody] UploadProductRevisionRequest request)
    {
        var product = await session.Events.AggregateStreamAsync<Product>(productId);
        if (product == null) return Results.NotFound();

        product.ApplyEvent(new ProductRevisionUploaded(
            productId,
            request.Description,
            request.InternalDescription,
            request.ChangeLog,
            DateTimeOffset.Now, 
            currentUser.FullName));
        
        session.Events.AppendAndClearUncommitedEvents(product);
        await session.SaveChangesAsync();
        return Results.Ok();
    }

    private static async Task<IResult> ApproveProductRevision(IDocumentSession session, ICurrentUser currentUser, Guid productId, [FromBody] ApproveProductRevisionRequest request)
    {
        var product = await session.Events.AggregateStreamAsync<Product>(productId);
        if (product == null) return Results.NotFound();

        product.ApplyEvent(new ProductRevisionApproved(
            productId,
            request.Description,
            request.InternalDescription,
            request.ChangeLog,
            request.ApprovalComment,
            DateTimeOffset.Now, 
            currentUser.FullName));

        session.Events.AppendAndClearUncommitedEvents(product);
        await session.SaveChangesAsync();
        return Results.Ok();
    }

    private static async Task<IResult> DeclineProductRevision(IDocumentSession session, ICurrentUser currentUser, Guid productId, [FromBody] DeclineProductRevisionRequest request)
    {
        var product = await session.Events.AggregateStreamAsync<Product>(productId);
        if (product == null) return Results.NotFound();

        product.ApplyEvent(new ProductRevisionDeclined(
            productId,
            request.DeclinedComment,
            DateTimeOffset.Now, 
            currentUser.FullName));

        session.Events.AppendAndClearUncommitedEvents(product);
        await session.SaveChangesAsync();
        return Results.Ok();
    }
}