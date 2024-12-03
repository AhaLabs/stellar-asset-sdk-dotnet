using StellarDotnetSdk;
using StellarDotnetSdk.Accounts;

namespace Stellar_dotnet_cli;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Network.Use(Network.Test());
        var sgIssuer = KeyPair.FromSecretSeed(args[0]);
        var sgOperator = KeyPair.FromSecretSeed(args[1]);
        var tx = await AssetIssuer.IssueTransaction(new Account(sgIssuer.AccountId, 1),
            new Account(sgOperator, 1));
        Console.WriteLine(tx.ToUnsignedEnvelopeXdrBase64());
        Console.WriteLine();
        tx.Sign(sgIssuer);
        tx.Sign(sgOperator);
        Console.WriteLine(tx.ToEnvelopeXdrBase64());
        // Console.WriteLine();
        // Console.WriteLine(tx.ToEnvelopeXdrBase64());
        var server = TestnetServer.Server();
        var result = await server.SendTransaction(tx);
        Console.WriteLine(result.Status);
        var res = await TestnetServer.GetTransaction(result.Hash, server);
        Console.WriteLine(res.Status);
        Console.WriteLine(res.TxHash);
        Console.WriteLine(res.ResultValue);
    }
}