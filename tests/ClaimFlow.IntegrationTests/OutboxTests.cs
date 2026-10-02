using ClaimFlow.BuildingBlocks.Outbox;
using ClaimFlow.Claims.Contracts;
using ClaimFlow.Claims.Domain;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace ClaimFlow.IntegrationTests;

[Collection(ApiTestGroup.Name)]
public sealed class OutboxTests(ClaimFlowApiFactory factory) : IAsyncLifetime
{
    private HttpClient _client = null!;

    public async ValueTask InitializeAsync() => _client = await factory.ClientForAsync("lea");

    public ValueTask DisposeAsync()
    {
        _client.Dispose();
        return ValueTask.CompletedTask;
    }

    [Fact]
    public async Task StateChange_WritesOutboxRow_InSameTransaction()
    {
        await factory.DrainOutboxAsync();

        var claim = await _client.DeclareAsync();
        (await _client.ActAsync(claim, ClaimAction.StartReview)).EnsureSuccessStatusCode();

        (await factory.PendingOutboxCountAsync()).ShouldBe(2);
        await factory.DrainOutboxAsync();
        (await factory.PendingOutboxCountAsync()).ShouldBe(0);
    }

    [Fact]
    public async Task CommittedChange_IsPushedToSignalRClients_OnlyAfterOutboxDelivery()
    {
        await factory.DrainOutboxAsync();
        var received = new TaskCompletionSource<ClaimChangedNotification>(TaskCreationOptions.RunContinuationsAsynchronously);
        await using var connection = new HubConnectionBuilder()
            .WithUrl(new Uri(factory.Server.BaseAddress, "/hubs/claims"), options =>
            {
                options.HttpMessageHandlerFactory = _ => factory.Server.CreateHandler();
                options.AccessTokenProvider = async () => await factory.TokenForAsync("sophie");
                options.Transports = HttpTransportType.LongPolling;
            })
            .AddJsonProtocol(options => options.PayloadSerializerOptions = ApiClient.Json)
            .Build();
        connection.On<ClaimChangedNotification>("claimChanged", notification => received.TrySetResult(notification));
        await connection.StartAsync(TestContext.Current.CancellationToken);

        var claim = await _client.DeclareAsync();
        received.Task.IsCompleted.ShouldBeFalse("nothing is pushed before the outbox delivers");

        await factory.DrainOutboxAsync();
        var notification = await received.Task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);

        notification.ClaimId.ShouldBe(claim.Id);
        notification.Status.ShouldBe(ClaimStatus.Declared);
        notification.ActorName.ShouldBe("Léa Martin");
    }

    [Fact]
    public async Task FailingHandlers_AreRetriedThenParked_WithTheirRealError_WithoutBlockingOthers()
    {
        await factory.DrainOutboxAsync();
        var synchronous = await _client.DeclareAsync();
        var timeout = await _client.DeclareAsync();
        var healthy = await _client.DeclareAsync();
        PoisonHandler.Poison(synchronous.Id, PoisonHandler.Failure.Synchronous);
        PoisonHandler.Poison(timeout.Id, PoisonHandler.Failure.Timeout);

        await factory.DrainOutboxAsync();

        var parkedSync = (await factory.OutboxMessagesForAsync(synchronous.Id)).ShouldHaveSingleItem();
        parkedSync.ProcessedAt.ShouldBeNull();
        parkedSync.Attempts.ShouldBe(OutboxMessage.MaxAttempts);
        parkedSync.LastError.ShouldBe("sync handler failure");

        var parkedTimeout = (await factory.OutboxMessagesForAsync(timeout.Id)).ShouldHaveSingleItem();
        parkedTimeout.Attempts.ShouldBe(OutboxMessage.MaxAttempts);
        parkedTimeout.LastError.ShouldBe("handler timeout");

        (await factory.OutboxMessagesForAsync(healthy.Id)).ShouldHaveSingleItem().ProcessedAt.ShouldNotBeNull();
        (await factory.PendingOutboxCountAsync()).ShouldBe(0);
    }
}
