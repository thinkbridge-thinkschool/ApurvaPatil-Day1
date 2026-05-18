using QuotesApi.Models;
using QuotesApi.Repositories;

namespace QuotesApi.Extensions;

public static class EndpointExtensions
{
    /// <summary>
    /// Map all Quote API endpoints
    /// </summary>
    public static WebApplication MapQuoteEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/quotes")
            .WithName("Quotes");

        // GET /api/quotes - Get all quotes with pagination
        group.MapGet("/", GetAllQuotes)
            .WithName("GetAllQuotes")
            .WithDescription("Get all quotes with pagination");

        // GET /api/quotes/{id} - Get quote by ID
        group.MapGet("/{id}", GetQuoteById)
            .WithName("GetQuoteById")
            .WithDescription("Get a specific quote by ID");

        // POST /api/quotes - Create new quote
        group.MapPost("/", CreateQuote)
            .WithName("CreateQuote")
            .WithDescription("Create a new quote");

        // DELETE /api/quotes/{id} - Delete quote by ID
        group.MapDelete("/{id}", DeleteQuote)
            .WithName("DeleteQuote")
            .WithDescription("Delete a quote by ID");

        return app;
    }

    /// <summary>
    /// GET /api/quotes - Get all quotes with pagination
    /// Query params: page (default 1), size (default 10)
    /// </summary>
    private static async Task<IResult> GetAllQuotes(
        int page = 1,
        int size = 10,
        IQuoteRepository repository = default!,
        CancellationToken cancellationToken = default)
    {
        // Validate pagination parameters
        if (page < 1) page = 1;
        if (size < 1) size = 10;
        if (size > 100) size = 100; // Max 100 items per page

        var quotes = await repository.GetAllAsync(page, size, cancellationToken);
        return Results.Ok(quotes);
    }

    /// <summary>
    /// GET /api/quotes/{id} - Get quote by ID
    /// </summary>
    private static async Task<IResult> GetQuoteById(
        int id,
        IQuoteRepository repository,
        CancellationToken cancellationToken)
    {
        if (id < 1)
            return Results.BadRequest(new { error = "Invalid quote ID" });

        var quote = await repository.GetByIdAsync(id, cancellationToken);

        return quote is null
            ? Results.NotFound(new { error = "Quote not found" })
            : Results.Ok(quote);
    }

    /// <summary>
    /// POST /api/quotes - Create a new quote
    /// Body: { "author": "string", "text": "string" }
    /// </summary>
    private static async Task<IResult> CreateQuote(
        Quote quote,
        IQuoteRepository repository,
        CancellationToken cancellationToken)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(quote.Author) ||
            string.IsNullOrWhiteSpace(quote.Text))
        {
            return Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["error"] = new[] { "Author and Text are required fields" }
                });
        }

        var createdQuote = await repository.AddAsync(quote, cancellationToken);
        return Results.Created($"/api/quotes/{createdQuote.Id}", createdQuote);
    }

    /// <summary>
    /// DELETE /api/quotes/{id} - Delete a quote
    /// </summary>
    private static async Task<IResult> DeleteQuote(
        int id,
        IQuoteRepository repository,
        CancellationToken cancellationToken)
    {
        if (id < 1)
            return Results.BadRequest(new { error = "Invalid quote ID" });

        var deleted = await repository.DeleteAsync(id, cancellationToken);

        return deleted
            ? Results.NoContent()
            : Results.NotFound(new { error = "Quote not found" });
    }
}
