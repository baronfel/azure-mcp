// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Core.Commands;
using AzureMcp.Core.Commands.Subscription;
using AzureMcp.Core.Services.Telemetry;
using AzureMcp.KeyVault.Commands;
using AzureMcp.KeyVault.Options;
using AzureMcp.KeyVault.Options.Secret;
using AzureMcp.KeyVault.Services;
using Microsoft.Extensions.Logging;

namespace AzureMcp.KeyVault.Commands.Secret;

public sealed class SecretListCommand(ILogger<SecretListCommand> logger) : SubscriptionCommand<SecretListOptions>
{
    private const string _commandTitle = "List Key Vault Secrets";
    private readonly ILogger<SecretListCommand> _logger = logger;
    private readonly Option<string> _vaultOption = KeyVaultOptionDefinitions.VaultName;

    public override string Name => "list";

    public override string Title => _commandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    public override string Description =>
        """
        List all secrets in an Azure Key Vault. This command retrieves and displays the names of all secrets
        stored in the specified vault.
        """;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_vaultOption);
    }

    protected override SecretListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.VaultName = parseResult.GetValueForOption(_vaultOption);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);

        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            context.Activity?.WithSubscriptionTag(options);

            var keyVaultService = context.GetService<IKeyVaultService>();
            var secrets = await keyVaultService.ListSecrets(
                options.VaultName!,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = secrets?.Count > 0 ?
                ResponseResult.Create(
                    new SecretListCommandResult(secrets),
                    KeyVaultJsonContext.Default.SecretListCommandResult) :
                null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred listing secrets from vault {VaultName}.", options.VaultName);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record SecretListCommandResult(List<string> Secrets);
}
