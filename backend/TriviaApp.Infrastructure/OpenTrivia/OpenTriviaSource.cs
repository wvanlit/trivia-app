using TriviaApp.Domain.Model;
using TriviaApp.Domain.Ports;

namespace TriviaApp.Infrastructure.OpenTrivia;

public sealed class OpenTriviaSource(ITriviaApi api) : ITriviaSource
{
    public async Task<IReadOnlyList<Category>> GetCategories(CancellationToken cancellationToken)
    {
        var response = await api.GetCategories(cancellationToken);

        return response.ToDomain();
    }

    public async Task<int> GetCategoryCount(CategoryId categoryId, CancellationToken cancellationToken)
    {
        var response = await api.GetCategoryCount(ToSourceCategoryId(categoryId), cancellationToken);

        return response.ToDomain();
    }

    public async Task<IReadOnlyList<NewQuestion>> GetQuestions(
        int amount,
        CategoryId categoryId,
        CancellationToken cancellationToken)
    {
        var token = await RequestToken(cancellationToken);
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var response = await api.GetQuestions(amount, ToSourceCategoryId(categoryId), token, cancellationToken);
            var mapped = response.ToDomain();

            if (mapped.ResponseCode == OpenTriviaResponseCode.RateLimited)
            {
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                continue;
            }

            return mapped.ResponseCode == OpenTriviaResponseCode.NoResults
                ? []
                : mapped.ResponseCode == OpenTriviaResponseCode.TokenEmpty
                ? throw new InvalidOperationException("Session token is exhausted; restart ingestion to continue.")
                : mapped.ResponseCode is OpenTriviaResponseCode.InvalidParameter or OpenTriviaResponseCode.TokenNotFound
                ? throw new InvalidOperationException($"Open Trivia API error: {mapped.ResponseCode}.")
                : mapped.Questions;
        }
    }

    private static int ToSourceCategoryId(CategoryId categoryId) =>
        checked((int)categoryId.ToInt64());

    private async Task<string> RequestToken(CancellationToken cancellationToken)
    {
        var response = await api.RequestToken(cancellationToken);
        var tokenResponse = response.ToDomain();
        return tokenResponse.ResponseCode != OpenTriviaResponseCode.Success
            ? throw new InvalidOperationException($"Token request failed: {tokenResponse.ResponseMessage}.")
            : tokenResponse.Token;
    }
}
