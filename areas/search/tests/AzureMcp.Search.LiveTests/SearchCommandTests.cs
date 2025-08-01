// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using AzureMcp.Tests;
using AzureMcp.Tests.Client;
using AzureMcp.Tests.Client.Helpers;
using Xunit;

namespace AzureMcp.Search.LiveTests;

public class SearchCommandTests(LiveTestFixture liveTestFixture, ITestOutputHelper output)
    : CommandTestsBase(liveTestFixture, output),
    IClassFixture<LiveTestFixture>
{
    const string IndexName = "products";

    [Fact]
    public async Task Should_list_search_services_by_subscription_id()
    {
        Assert.NotNull(Settings.SubscriptionId);

        var result = await CallToolAsync(
            "azmcp_search_service_list",
            new()
            {
                { "subscription", Settings.SubscriptionId }
            });

        var services = result.AssertProperty("services");
        Assert.Equal(JsonValueKind.Array, services.ValueKind);
    }

    [Fact]
    public async Task Should_list_search_services_by_subscription_name()
    {
        var result = await CallToolAsync(
            "azmcp_search_service_list",
            new()
            {
                { "subscription", Settings.SubscriptionName }
            });

        var services = result.AssertProperty("services");
        Assert.Equal(JsonValueKind.Array, services.ValueKind);
    }

    [Fact]
    public async Task Should_list_search_indexes_with_service_name()
    {
        var result = await CallToolAsync(
            "azmcp_search_index_list",
            new()
            {
                { "service-name", Settings.ResourceBaseName }
            });

        var indexes = result.AssertProperty("indexes");
        Assert.Equal(JsonValueKind.Array, indexes.ValueKind);
    }

    [Fact]
    public async Task Should_get_index_details()
    {
        var result = await CallToolAsync(
            "azmcp_search_index_describe",
            new()
            {
                { "service-name", Settings.ResourceBaseName },
                { "index-name", IndexName }
            });

        var index = result.AssertProperty("index");
        Assert.Equal(JsonValueKind.Object, index.ValueKind);

        var name = index.AssertProperty("name");
        Assert.Equal(IndexName, name.GetString());
    }

    [Fact]
    public async Task Should_query_search_index()
    {
        var result = await CallToolAsync(
            "azmcp_search_index_query",
            new()
            {
                { "service-name", Settings.ResourceBaseName },
                { "index-name", IndexName },
                { "query", "*" }
            });

        Assert.NotNull(result);
        Assert.Equal(JsonValueKind.Array, result.Value.ValueKind);
        Assert.True(result.Value.GetArrayLength() > 0);
    }

    [Fact]
    public async Task Should_list_search_indexes()
    {
        var result = await CallToolAsync(
            "azmcp_search_index_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "service-name", Settings.ResourceBaseName },
                { "resource-group", Settings.ResourceGroupName }
            });

        var indexesArray = result.AssertProperty("indexes");
        Assert.Equal(JsonValueKind.Array, indexesArray.ValueKind);
    }

    [Fact]
    public async Task Should_describe_search_index()
    {
        var result = await CallToolAsync(
            "azmcp_search_index_describe",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "service-name", Settings.ResourceBaseName },
                { "resource-group", Settings.ResourceGroupName },
                { "index-name", "products" }
            });

        var index = result.AssertProperty("index");
        Assert.Equal(JsonValueKind.Object, index.ValueKind);
    }

    [Fact(Skip = "Invalid test assertion")]
    public async Task Should_query_search_index_with_documents_property()
    {
        var result = await CallToolAsync(
            "azmcp_search_index_query",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "service-name", Settings.ResourceBaseName },
                { "resource-group", Settings.ResourceGroupName },
                { "index-name", "products" },
                { "query", "*" }
            });

        // TODO: results is an array, there is no documents property
        var docs = result.AssertProperty("documents");
        Assert.Equal(JsonValueKind.Array, docs.ValueKind);
    }
}
