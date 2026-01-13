using TriviaApp.Domain.Model;
using TriviaApp.Domain.Ports;

namespace TriviaApp.Domain.Queries;

public sealed record GetCategoriesQuery;

public sealed class GetCategoriesQueryHandler(ITriviaRepository repository)
{
    public async Task<QueryResult<IReadOnlyList<Category>>> Handle(
        GetCategoriesQuery _,
        CancellationToken cancellationToken)
    {
        var categories = await repository.GetCategories(cancellationToken);

        return new QueryResult<IReadOnlyList<Category>>(categories, null);
    }
}
