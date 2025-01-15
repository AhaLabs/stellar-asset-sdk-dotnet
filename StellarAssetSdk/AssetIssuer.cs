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

public class AssetIssuer(
    Server server,
    Account sgIssuer,
    Account sgOperator)
{
    private const int ClawbackEnabled = 0x8;
    private const int RevokableEnabled = 0x2;
    private const int AuthorizeTrustline = 0x1;
    private const int NoFlags = 0x0;

    public readonly Asset Eurcv = new Eurcv(sgIssuer);
    public readonly Account SgIssuer = sgIssuer;
    public readonly Account SgOperator = sgOperator;

    /*
     # Example of issuing an asset
        stellar tx new set-options --fee 1000 --source sg-issuer --set-clawback-enabled --set-revocable --build-only \
        | stellar tx op add change-trust --op-source sg-operator --line $ASSET \
        | stellar tx op add payment --destination $OPERATOR_PK --asset $ASSET --amount 1 \
        | stellar tx op add set-trustline-flags --asset $ASSET --trustor $OPERATOR_PK --clear-authorize \
        | stellar tx sign --sign-with-key sg-issuer \
        | stellar tx sign --sign-with-key sg-operator \
        | stellar tx send
        */

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

    /*
    # User adds trustline
        stellar tx new change-trust --source sg-user --line $ASSET
    */
    public async Task<Transaction> TrustlineTransaction(Account account)
    {
        var changeTrust = new ChangeTrustOperation(Eurcv, null, null);
        var tx = await server.NewTransactionBuilder(account);
        return tx
            .SetFee(1000)
            .AddOperation(changeTrust)
            .Build();
    }


    /*
    # Example of sending a payment with sandwich transaction
        stellar tx new set-trustline-flags --fee 1000 --build-only --source sg-issuer --asset $ASSET --trustor $OPERATOR_PK --set-authorize \
        | stellar tx op add payment --destination $OPERATOR_PK --asset $ASSET --amount 1000000000000 \
        | stellar tx op add set-trustline-flags --asset $ASSET --trustor $OPERATOR_PK --clear-authorize \
        | stellar tx sign --sign-with-key sg-issuer \
        | stellar tx send
        */
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

    /*
     * # Send user funds from operator
       stellar tx new set-trustline-flags --fee 1000 --build-only --source sg-issuer --asset $ASSET --trustor $OPERATOR_PK --set-authorize \
       | stellar tx op add payment --op-source sg-operator --destination $USER_PK --asset $ASSET --amount 10000000000 \
       | stellar tx op add set-trustline-flags --asset $ASSET --trustor $OPERATOR_PK --clear-authorize \
       | stellar tx sign --sign-with-key sg-issuer \
       | stellar tx sign --sign-with-key sg-operator \
       | stellar tx send
     */
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

    /*
    # Next user creates a claimable balance, currently only supports unconditional claim predicate
        stellar tx new create-claimable-balance --source sg-user --asset $ASSET --amount 500 \
        --cliamants $OPERATOR_PK \
        --claimants $USER_PK
    */
    public async Task<Transaction> CreateClaimableBalanceTransaction(Account sender, String amount)
    {
        //Set server

        var claimableBalance =
            new CreateClaimableBalanceOperation(Eurcv, amount,
            [
                new Claimant(SgOperator.AccountId, ClaimPredicate.Unconditional()),
                new Claimant(sender.AccountId, ClaimPredicate.Unconditional())
            ]);

        var tx = await server.NewTransactionBuilder(sender);
        return tx
            .SetFee(1000)
            .AddOperation(claimableBalance)
            .Build();
    }

    /*
     # Then can claim the balance with the following transaction
       stellar tx new set-trustline-flags --fee 1000 --build-only --source sg-issuer --asset $ASSET --trustor $OPERATOR_PK --set-authorize \
       | stellar tx op add claim-claimable-balance --op-source sg-operator --balance-id bce769414798ff660c9535945febbecaf4995a2d9a045900404d5ad82ddc24fa  \
       | stellar tx op add set-trustline-flags --asset $ASSET --trustor $OPERATOR_PK --clear-authorize \
       | stellar tx sign --sign-with-key sg-issuer \
       | stellar tx sign --sign-with-key sg-operator \
       | stellar tx send
     */
    public async Task<Transaction> ClaimClaimableBalanceTransaction(Account receiver, byte[] claimableBalanceId)
    {
        // TODO: use example above
        throw new NotImplementedException("ClaimClaimableBalanceTransaction");
    }
}