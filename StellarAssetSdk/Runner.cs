using System;
using System.Net.Http;
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
        tx.Sign(sgOperator);
        res = await server.SubmitTransaction(tx);

        // Add trustline to Alice
        var trustlineTx = await assetIssuer.TrustlineTransaction(aliceAccount);
        trustlineTx.Sign(alice);
        res = await server.SubmitTransaction(trustlineTx);

        // Send Alice funds from sgOperator's account
        var paymentTx = await assetIssuer.Payment(aliceAccount, "10000000");
        paymentTx.Sign(sgOperator);
        res = await server.SubmitTransaction(paymentTx);
        
        // Create a claimable balance from Alice for sgOperator to be able to claim
        var createClaimableBalance = await assetIssuer.CreateClaimableBalanceTransaction(aliceAccount, "10");
        createClaimableBalance.Sign(alice);
        res = await server.SubmitTransaction(createClaimableBalance);
        createClaimableBalance = await assetIssuer.CreateClaimableBalanceTransaction(aliceAccount, "10");
        createClaimableBalance.Sign(alice);
        res = await server.SubmitTransaction(createClaimableBalance);
        
        
        var limit = 1;
        var responses = await server.ClaimableBalance(assetIssuer.Usdc, sgOperator, limit);
        
        var usdcBalanceBefore = await server.GetAccountBalanceForAsset(assetIssuer.Usdc,sgOperatorAccount.AccountId);
        Console.WriteLine(usdcBalanceBefore);

        // Claim the claimable balance from sgOperator
        var claimBalanceTx = await assetIssuer.ClaimClaimableBalanceTransaction(sgOperatorAccount, responses.Records[0].Id.HexToByteArray());
        claimBalanceTx.Sign(sgOperator);
        res = await server.SubmitTransaction(claimBalanceTx);

        responses = await responses.Links.Next.Follow();
        
        claimBalanceTx = await assetIssuer.ClaimClaimableBalanceTransaction(sgOperatorAccount, responses.Records[0].Id.HexToByteArray());
        claimBalanceTx.Sign(sgOperator);
        res = await server.SubmitTransaction(claimBalanceTx);

        var usdcBalanceAfter = await server.GetAccountBalanceForAsset(assetIssuer.Usdc,sgOperatorAccount.AccountId);
        Console.WriteLine(usdcBalanceAfter);

        if (usdcBalanceAfter == usdcBalanceBefore) {
            throw new HttpRequestException("The claimable balance hasn't been received");
        }
        
    }
}