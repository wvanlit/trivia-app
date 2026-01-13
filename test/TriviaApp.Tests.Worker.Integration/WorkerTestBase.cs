using Xunit;

namespace TriviaApp.Tests.Worker.Integration;

[CollectionDefinition("Worker collection")]
public sealed class WorkerTestSuite : ICollectionFixture<WorkerTestFixture>;

[Collection("Worker collection")]
public abstract class WorkerTestBase(WorkerTestFixture fixture)
{
    private readonly WorkerTestFixture _fixture = fixture;

    protected TestHarness Harness => _fixture.Harness;
}

public sealed class WorkerTestFixture : IAsyncLifetime
{
    internal TestHarness Harness { get; private set; } = null!;

    async Task IAsyncLifetime.InitializeAsync() => Harness = await TestHarness.Create();

    async Task IAsyncLifetime.DisposeAsync()
    {
        if (Harness is not null)
        {
            await Harness.DisposeAsync();
        }
    }
}
