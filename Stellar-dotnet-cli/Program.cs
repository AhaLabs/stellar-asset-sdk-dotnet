using StellarDotnetSdk;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Soroban;
//using StellarDotnetSdk.Xdr;
using StellarDotnetSdk.Transactions;


namespace Stellar_dotnet_cli;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Network.Use(Network.Test());
        var server = TestnetServer.Server();
        var sgIssuer = KeyPair.FromSecretSeed(args[0]);
        var sgIssuerAccount = new Account(sgIssuer.AccountId, 1);
        var sgOperator = KeyPair.FromSecretSeed(args[1]);
        var sgOperatorAccount = new Account(sgOperator.AccountId, 1);
        var alice = KeyPair.FromSecretSeed(args[2]);
        var aliceAccount = new Account(alice.AccountId, 1);

        var tx = await AssetIssuer.IssueTransaction(sgIssuerAccount, sgOperatorAccount);
        //Console.WriteLine(tx.ToUnsignedEnvelopeXdrBase64());
        Console.WriteLine();
        tx.Sign(sgIssuer);
        tx.Sign(sgOperator);
        Console.WriteLine(tx.ToEnvelopeXdrBase64());
        await SubmitTransaction(tx, server);
        
        // Add trustline to Alice
        var trustlineTx = await AssetIssuer.TrustlineTransaction(sgIssuerAccount, aliceAccount);
        trustlineTx.Sign(alice);
        await SubmitTransaction(trustlineTx, server);
        
        var paymentTx = await AssetIssuer.Payment(sgIssuerAccount, sgOperatorAccount, aliceAccount, "10000000");
        paymentTx.Sign(sgIssuer);
        paymentTx.Sign(sgOperator);
        await SubmitTransaction(paymentTx, server);
        
    }

    private static async Task SubmitTransaction(Transaction tx, SorobanServer server)
    {
        var result = await server.SendTransaction(tx);
        Console.WriteLine(result.Status);
        var res = await TestnetServer.GetTransaction(result.Hash, server);
        Console.WriteLine(res.Status);
        Console.WriteLine(res.TxHash);
        Console.WriteLine(res.ResultValue);
    }
}