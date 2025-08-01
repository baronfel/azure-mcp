// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text.Json;
using AzureMcp.Core.Models.Command;
using AzureMcp.Core.Options;
using AzureMcp.Search.Commands.Index;
using AzureMcp.Search.Services;
using AzureMcp.Tests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace AzureMcp.Search.UnitTests.Index;

public class IndexQueryCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISearchService _searchService;
    private readonly ILogger<IndexQueryCommand> _logger;

    public IndexQueryCommandTests()
    {
        _searchService = Substitute.For<ISearchService>();
        _logger = Substitute.For<ILogger<IndexQueryCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_searchService);

        _serviceProvider = collection.BuildServiceProvider();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsResults_WhenSearchSucceeds()
    {
        // Arrange
        var serviceName = "service123";
        var indexName = "index1";
        var queryText = "test query";

        List<JsonElement> expectedResults = [
            JsonDocument.Parse(
                """
                {
                    "totalCount": 1,
                    "results": [
                        {
                            "id": "1",
                            "title": "Test Document"
                        }
                    ]
                }
                """
            ).RootElement
        ];

        _searchService
            .QueryIndex(
                Arg.Is<string>(s => s == serviceName),
                Arg.Is<string>(i => i == indexName),
                Arg.Is<string>(q => q == queryText),
                Arg.Any<RetryPolicyOptions?>())
            .Returns(expectedResults);

        var command = new IndexQueryCommand(_logger);
        var parser = new Parser(command.GetCommand());
        var args = parser.Parse($"--service-name {serviceName} --index-name {indexName} --query \"{queryText}\"");
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        Assert.Contains("totalCount", json);
        Assert.Contains("results", json);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceException()
    {
        // Arrange
        var expectedError = "Test error";
        var serviceName = "service123";
        var indexName = "index1";
        var queryText = "test query";

        _searchService
            .QueryIndex(
                Arg.Is<string>(s => s == serviceName),
                Arg.Is<string>(i => i == indexName),
                Arg.Is<string>(q => q == queryText),
                Arg.Any<RetryPolicyOptions?>())
            .ThrowsAsync(new Exception(expectedError));

        var command = new IndexQueryCommand(_logger);
        var parser = new Parser(command.GetCommand());
        var args = parser.Parse($"--service-name {serviceName} --index-name {indexName} --query \"{queryText}\"");
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
        Assert.Contains(expectedError, response.Message ?? string.Empty);
    }

    [Fact]
    public async Task ExecuteAsync_ValidatesRequiredOptions()
    {
        // Arrange
        var command = new IndexQueryCommand(_logger);
        var parser = new Parser(command.GetCommand());
        var args = parser.Parse(""); // Missing required options
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(400, response.Status);
        Assert.NotNull(response.Message);
        Assert.Contains("service-name", response.Message);
        Assert.Contains("index-name", response.Message);
        Assert.Contains("query", response.Message);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        // Arrange & Act
        var command = new IndexQueryCommand(_logger);

        // Assert
        var cmd = command.GetCommand();
        Assert.Equal("query", cmd.Name);
        Assert.NotEmpty(cmd.Description ?? string.Empty);
    }
}
