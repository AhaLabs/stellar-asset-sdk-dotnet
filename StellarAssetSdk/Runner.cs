using System;
using System.Threading.Tasks;
using StellarDotnetSdk.Accounts;

namespace StellarAssetSdk;

public class Runner
{
    public static async Task Run(Server server, KeyPair sgIssuer, KeyPair sgOperator, KeyPair alice)
    {
        var sgIssuerAccount = new Account(sgIssuer.AccountId, 1);
        var sgOperatorAccount = new Account(sgOperator.AccountId, 1);
        var aliceAccount = new Account(alice.AccountId, 1);
        var assetIssuer = new AssetIssuer(server, sgIssuerAccount, sgOperatorAccount);

        var tx = await assetIssuer.IssueTransaction();
        tx.Sign(sgIssuer);
        tx.Sign(sgOperator);
        var res = await server.SubmitTransaction(tx);
        
        tx = await assetIssuer.MintTransaction("2222222");
        tx.Sign(sgIssuer);
        res = await server.SubmitTransaction(tx);
        

        // Add trustline to Alice
        
        var trustlineTx = await assetIssuer.TrustlineTransaction(aliceAccount);
        trustlineTx.Sign(alice);        
        res = await server.SubmitTransaction(trustlineTx);
        
        var paymentTx = await assetIssuer.Payment(aliceAccount, "10000000");
        paymentTx.Sign(sgIssuer);
        paymentTx.Sign(sgOperator);
        res = await server.SubmitTransaction(paymentTx);
        
    }
}