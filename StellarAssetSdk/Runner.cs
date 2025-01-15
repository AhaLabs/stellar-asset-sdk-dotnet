using System;
using System.Threading.Tasks;
using dotnetstandard_bip32;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Xdr;

namespace StellarAssetSdk;

public class Runner
{
    public static async Task Run(Server server, KeyPair sgIssuer, KeyPair sgOperator, KeyPair alice)
    {
        var sgIssuerAccount = new Account(sgIssuer.AccountId, 1);
        var sgOperatorAccount = new Account(sgOperator.AccountId, 1);
        var aliceAccount = new Account(alice.AccountId, 1);
        var assetIssuer = new AssetIssuer(server, sgIssuerAccount, sgOperatorAccount);

        // Issue the asset
        var tx = await assetIssuer.IssueTransaction();
        tx.Sign(sgIssuer);
        tx.Sign(sgOperator);
        var res = await server.SubmitTransaction(tx);

        // Example of Minting more 
        tx = await assetIssuer.MintTransaction("2222222");
        tx.Sign(sgIssuer);
        res = await server.SubmitTransaction(tx);

        // Add trustline to Alice
        var trustlineTx = await assetIssuer.TrustlineTransaction(aliceAccount);
        trustlineTx.Sign(alice);
        res = await server.SubmitTransaction(trustlineTx);

        // Send Alice funds from sgOperator's account
        var paymentTx = await assetIssuer.Payment(aliceAccount, "10000000");
        paymentTx.Sign(sgIssuer);
        paymentTx.Sign(sgOperator);
        res = await server.SubmitTransaction(paymentTx);
        
        // Create a claimable balance from Alice for sgOperator to be able to claim
        var createClaimableBalance = await assetIssuer.CreateClaimableBalanceTransaction(aliceAccount, "10000000");
        createClaimableBalance.Sign(alice);
        res = await server.SubmitTransaction(createClaimableBalance);
        var txRes = TransactionResult.Decode(new XdrDataInputStream(Convert.FromBase64String(res.ResultXdr!)))!;
        var resRes = txRes.Result.Results[0]!;
        
        // Look up the ID for the claimable balance to be used to claim it.
        var balanceId = resRes.Tr.CreateClaimableBalanceResult.BalanceID.V0.InnerValue!;
        Console.WriteLine(balanceId.ToStringHex());
        
        // Claim Balance
        // TODO: use  // ClaimClaimableBalanceTransaction
        try
        {
            var claimBalance = await assetIssuer.ClaimClaimableBalanceTransaction(sgOperatorAccount, balanceId);
        }
        catch (NotImplementedException e)
        {
            Console.WriteLine(e.Message);
        }
        

    }
}