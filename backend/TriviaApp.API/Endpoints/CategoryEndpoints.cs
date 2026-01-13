using TriviaApp.Domain.Model;
using TriviaApp.Domain.Queries;

using ApiResult = Microsoft.AspNetCore.Http.HttpResults.Results<
    Microsoft.AspNetCore.Http.HttpResults.Ok<TriviaApp.API.Endpoints.CategoriesResponse>,
    Microsoft.AspNetCore.Http.HttpResults.BadRequest<Microsoft.AspNetCore.Mvc.ProblemDetails>>;

namespace TriviaApp.API.Endpoints;

public static class CategoryEndpoints
{
    public static RouteGroupBuilder MapCategoryEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet(
                "/categories",
                async Task<ApiResult> (
                    GetCategoriesQueryHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    var result = await handler.Handle(new GetCategoriesQuery(), cancellationToken);

                    return result.MapToResult(CategoriesResponse.FromDomain);
                })
            .WithName("GetCategories")
            .WithSummary("Lists available trivia categories.")
            .Produces<CategoriesResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        return group;
    }
}

public sealed record CategoriesResponse(IReadOnlyList<CategoryResponse> Categories)
{
    public static CategoriesResponse FromDomain(IReadOnlyList<Category> categories)
        => new(categories.Select(CategoryResponse.FromDomain).ToArray());
}

public sealed record CategoryResponse(long CategoryId, string Name)
{
    public static CategoryResponse FromDomain(Category category)
        => new(category.Id.ToInt64(), category.Name);
}
