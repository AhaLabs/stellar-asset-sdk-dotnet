using StellarDotnetSdk;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Soroban;
//using StellarDotnetSdk.Xdr;
using StellarDotnetSdk.Transactions;

using StellarAssetSdk;


namespace StellarAssetSdk.Console;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Network.Use(Network.Test());
        var server = Server.Testnet();
        var sgIssuer = KeyPair.FromSecretSeed(args[0]);
        var sgIssuerAccount = new Account(sgIssuer.AccountId, 1);
        var sgOperator = KeyPair.FromSecretSeed(args[1]);
        var sgOperatorAccount = new Account(sgOperator.AccountId, 1);
        var alice = KeyPair.FromSecretSeed(args[2]);
        var aliceAccount = new Account(alice.AccountId, 1);
        
        var assetIssuer = new AssetIssuer(server, sgIssuerAccount, sgOperatorAccount);

        // var tx = await assetIssuer.IssueTransaction();
        // Console.WriteLine();
        // tx.Sign(sgIssuer);
        // tx.Sign(sgOperator);
        // Console.WriteLine(tx.ToEnvelopeXdrBase64());
        // var res = await server.SubmitTransaction(tx);
        // Console.WriteLine(res.Status);
        // Console.WriteLine(res.TxHash);
        
        // Add trustline to Alice
        var trustlineTx = await assetIssuer.TrustlineTransaction(aliceAccount);
        trustlineTx.Sign(alice);
        Console.WriteLine(trustlineTx.ToEnvelopeXdrBase64());
        var res = await server.SubmitTransaction(trustlineTx);
        Console.WriteLine(res.Status);
        Console.WriteLine(res.TxHash);
        
        var paymentTx = await assetIssuer.Payment(aliceAccount, "10000000");
        paymentTx.Sign(sgIssuer);
        paymentTx.Sign(sgOperator);
        res = await server.SubmitTransaction(paymentTx);
        Console.WriteLine(res.Status);
        Console.WriteLine(res.TxHash);
        
    }
}