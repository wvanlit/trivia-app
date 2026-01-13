using Refit;
using TriviaApp.Domain.Model;

namespace TriviaApp.Infrastructure.OpenTrivia;

public interface ITriviaApi
{
    /// <summary>
    /// Requests a session token so the worker can traverse OTD results without repeats.
    /// </summary>
    [Get("/api_token.php?command=request")]
    Task<TokenResponse> RequestToken(CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves all available Open Trivia Database categories.
    /// </summary>
    [Get("/api_category.php")]
    Task<CategoryListResponse> GetCategories(CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves question counts for a single Open Trivia Database category.
    /// </summary>
    [Get("/api_count.php?category={categoryId}")]
    Task<CategoryCountResponse> GetCategoryCount(int categoryId, CancellationToken cancellationToken);

    /// <summary>
    /// Fetches questions for a category using an active session token.
    /// </summary>
    [Get("/api.php?amount={amount}&category={categoryId}&token={token}")]
    Task<QuestionResponse> GetQuestions(int amount, int categoryId, string token, CancellationToken cancellationToken);
}

public enum OpenTriviaResponseCode
{
    /// <summary>
    /// The request completed successfully.
    /// </summary>
    Success = 0,
    /// <summary>
    /// The request completed but returned no results.
    /// </summary>
    NoResults = 1,
    /// <summary>
    /// The request was rejected due to invalid parameters.
    /// </summary>
    InvalidParameter = 2,
    /// <summary>
    /// The session token was not found or expired.
    /// </summary>
    TokenNotFound = 3,
    /// <summary>
    /// The session token has returned all available questions and is exhausted.
    /// </summary>
    TokenEmpty = 4,
    /// <summary>
    /// Too many requests were made in a short time window.
    /// </summary>
    RateLimited = 5
}

public sealed record TokenResponse
{
    public OpenTriviaResponseCode ResponseCode { get; init; }

    public string ResponseMessage { get; init; } = string.Empty;

    public string Token { get; init; } = string.Empty;

    public (OpenTriviaResponseCode ResponseCode, string ResponseMessage, string Token) ToDomain() =>
        (ResponseCode, ResponseMessage, Token);
}

public sealed record CategoryListResponse
{
    public IReadOnlyList<CategoryItem> TriviaCategories { get; init; } = [];

    public IReadOnlyList<Category> ToDomain() =>
        TriviaCategories.Select(category => new Category(category.Id, category.Name)).ToArray();
}

public sealed record CategoryItem
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;
}

public sealed record CategoryCountResponse
{
    public int CategoryId { get; init; }

    public CategoryQuestionCount CategoryQuestionCount { get; init; } = new();

    public int ToDomain() => CategoryQuestionCount.TotalQuestionCount;
}

public sealed record CategoryQuestionCount
{
    public int TotalQuestionCount { get; init; }

    public int TotalEasyQuestionCount { get; init; }

    public int TotalMediumQuestionCount { get; init; }

    public int TotalHardQuestionCount { get; init; }
}

public sealed record QuestionResponse
{
    public OpenTriviaResponseCode ResponseCode { get; init; }

    public IReadOnlyList<QuestionItem> Results { get; init; } = [];

    public (OpenTriviaResponseCode ResponseCode, IReadOnlyList<NewQuestion> Questions) ToDomain() =>
        (ResponseCode, Results.Select(result => result.ToDomain()).ToArray());
}

public sealed record QuestionItem
{
    public string Type { get; init; } = string.Empty;

    public string Difficulty { get; init; } = string.Empty;

    public string Question { get; init; } = string.Empty;

    public string CorrectAnswer { get; init; } = string.Empty;

    public IReadOnlyList<string> IncorrectAnswers { get; init; } = [];

    public NewQuestion ToDomain()
    {
        var correctAnswer = new Answer(CorrectAnswer);

        var options = IncorrectAnswers
            .Select(answer => new Answer(answer))
            .Append(correctAnswer)
            .Shuffle()
            .ToArray();

        return new NewQuestion(Question, correctAnswer, options, new Difficulty(Difficulty));
    }
}
