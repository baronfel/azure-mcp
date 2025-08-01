// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Core.Areas;
using AzureMcp.Core.Commands;
using AzureMcp.Marketplace.Commands.Product;
using AzureMcp.Marketplace.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AzureMcp.Marketplace;

public class MarketplaceSetup : IAreaSetup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IMarketplaceService, MarketplaceService>();
    }

    public void RegisterCommands(CommandGroup rootGroup, ILoggerFactory loggerFactory)
    {
        // Create Marketplace command group
        var marketplace = new CommandGroup("marketplace", "Marketplace operations - Commands for managing and accessing Azure Marketplace products and offers.");
        rootGroup.AddSubGroup(marketplace);

        // Create Product subgroup
        var product = new CommandGroup("product", "Marketplace product operations - Commands for retrieving and managing marketplace products.");
        marketplace.AddSubGroup(product);

        // Register Product commands
        product.AddCommand("get", new ProductGetCommand(
            loggerFactory.CreateLogger<ProductGetCommand>()));
    }
}
