using System;
using System.Threading.Tasks;
using StellarDotnetSdk;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Transactions;
using StellarDotnetSdk.Claimants;
using StellarDotnetSdk.Xdr;
using Claimant = StellarDotnetSdk.Claimants.Claimant;
using Transaction = StellarDotnetSdk.Transactions.Transaction;
using Asset = StellarDotnetSdk.Assets.Asset;
using ClaimPredicate = StellarDotnetSdk.Claimants.ClaimPredicate;


namespace StellarAssetSdk;

public class AssetIssuer(Server server, Account sgIssuer, Account sgOperator)
{
    private const int ClawbackEnabled = 0x8;
    private const int RevokableEnabled = 0x2;
    private const int AuthorizeTrustline = 0x1;
    private const int NoFlags = 0x0;

    public readonly Asset Eurcv = new Eurcv(sgIssuer);
    public readonly Account SgIssuer = sgIssuer;
    public readonly Account SgOperator = sgOperator;

    public async Task<Transaction> IssueTransaction(string initalAmount = "10000000000")
    {
        var setOpts = new SetOptionsOperation().SetSetFlags(ClawbackEnabled | RevokableEnabled);
        var changeTrust = new ChangeTrustOperation(Eurcv, null, SgOperator.MuxedAccount);
        var payment = new PaymentOperation(SgOperator.MuxedAccount, Eurcv, initalAmount);
        var setTrustlineFlags =
            new SetTrustlineFlagsOperation(Eurcv, SgOperator.KeyPair, NoFlags, AuthorizeTrustline);
        // Create a new transaction builder. Is async becuase it fetches account information to set the sequence number
        var tx = await server.NewTransactionBuilder(SgIssuer);
        return tx.SetFee(1000)
            .AddOperation(setOpts)
            .AddOperation(changeTrust)
            .AddOperation(payment)
            .AddOperation(setTrustlineFlags)
            .Build();
    }

    public async Task<Transaction> TrustlineTransaction(Account account)
    {
        var changeTrust = new ChangeTrustOperation(Eurcv, null, null);
        var tx = await server.NewTransactionBuilder(account);
        return tx
            .SetFee(1000)
            .AddOperation(changeTrust)
            .Build();
    }

    public async Task<Transaction> MintTransaction(String amount)
    {
        /*
         * # Example of minting new assets to the Operator with a sandwich transaction
           stellar tx new set-trustline-flags --fee 1000 --build-only --source sg-issuer --asset $ASSET --trustor $OPERATOR_PK --set-authorize \
           | stellar tx op add payment --destination $OPERATOR_PK --asset $ASSET --amount 1000000000000 \
           | stellar tx op add set-trustline-flags --asset $ASSET --trustor $OPERATOR_PK --clear-authorize \
           | stellar tx sign --sign-with-key sg-issuer \
           | stellar tx send
         */

        var setTrustlineFlags =
            new SetTrustlineFlagsOperation(Eurcv, SgOperator.KeyPair, AuthorizeTrustline, NoFlags);
        var clearSetTrustlineFlags =
            new SetTrustlineFlagsOperation(Eurcv, SgOperator.KeyPair, NoFlags, AuthorizeTrustline);
        var payment = new PaymentOperation(SgOperator.MuxedAccount, Eurcv, amount);
        var tx = await server.NewTransactionBuilder(SgIssuer);
        return tx
            .SetFee(1000)
            .AddOperation(setTrustlineFlags)
            .AddOperation(payment)
            .AddOperation(clearSetTrustlineFlags)
            .Build();
    }

    public async Task<Transaction> Payment(Account receiver, String amount)
    {
        /*
         * # Example of minting new assets to the Operator with a sandwich transaction
           stellar tx new set-trustline-flags --fee 1000 --build-only --source sg-issuer --asset $ASSET --trustor $OPERATOR_PK --set-authorize \
           | stellar tx op add payment --destination $OPERATOR_PK --asset $ASSET --amount 1000000000000 \
           | stellar tx op add set-trustline-flags --asset $ASSET --trustor $OPERATOR_PK --clear-authorize \
           | stellar tx sign --sign-with-key sg-issuer \
           | stellar tx send
         */
        var setTrustlineFlags =
            new SetTrustlineFlagsOperation(Eurcv, SgOperator.KeyPair, AuthorizeTrustline, NoFlags);
        var clearSetTrustlineFlags =
            new SetTrustlineFlagsOperation(Eurcv, SgOperator.KeyPair, NoFlags, AuthorizeTrustline);
        var payment = new PaymentOperation(receiver.MuxedAccount, Eurcv, amount, SgOperator.MuxedAccount);
        var tx = await server.NewTransactionBuilder(SgIssuer);

        return tx
            .SetFee(1000)
            .AddOperation(setTrustlineFlags)
            .AddOperation(payment)
            .AddOperation(clearSetTrustlineFlags)
            .Build();
    }

    public async Task<Transaction> CreateClaimableBalanceTransaction(Account sender, String amount)
    {
        //Set server

        var claimableBalance =
            new CreateClaimableBalanceOperation(Eurcv, amount,
            [
                new Claimant(SgOperator.AccountId, ClaimPredicate.Unconditional()),
                new Claimant(sender.AccountId, ClaimPredicate.Unconditional())
            ]);

        var tx = await server.NewTransactionBuilder(SgIssuer);
        return tx
            .SetFee(1000)
            .AddOperation(claimableBalance)
            .Build();
    }
}