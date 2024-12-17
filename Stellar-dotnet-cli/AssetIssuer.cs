using StellarDotnetSdk;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Transactions;

namespace Stellar_dotnet_cli;

public class AssetIssuer
{
    
//Set network and server
    public static async Task<Transaction> IssueTransaction(Account sgIssuer, Account sgOperator)
    {
        //Set server
        var server = new Server("https://horizon-testnet.stellar.org");
        var clawbackEnabled = 0x8;
        var revokableEnabled = 0x2;
        var asset = Info.Eurcv(sgIssuer);
        var setOpts = new SetOptionsOperation().SetSetFlags(clawbackEnabled | revokableEnabled);
        var changeTrust = new ChangeTrustOperation(asset, null, sgOperator.MuxedAccount);
        var payment = new PaymentOperation(sgOperator.MuxedAccount, asset, "10000000000");
        uint authorize = 0x1;
        uint none = 0;
        var setTrustlineFlags = new SetTrustlineFlagsOperation(asset, sgOperator.KeyPair, none, authorize);
        var accountInfo = await server.Accounts.Account(sgIssuer.AccountId);
        var tx = new TransactionBuilder(accountInfo)
            .SetFee(1000)
            .AddOperation(setOpts)
            .AddOperation(changeTrust)
            .AddOperation(payment)
            .AddOperation(setTrustlineFlags)
            .Build();
        return tx;
    }
    
    public static async Task<Transaction> TrustlineTransaction(Account sgIssuer, Account account)
    {
        //Set server
        var server = new Server("https://horizon-testnet.stellar.org");
        var asset = Info.Eurcv(sgIssuer);
        var changeTrust = new ChangeTrustOperation(asset, null, account.MuxedAccount);
        var accountInfo = await server.Accounts.Account(account.AccountId);
        var tx = new TransactionBuilder(accountInfo)
            .SetFee(1000)
            .AddOperation(changeTrust)
            .Build();
        return tx;
    }

    public static async Task<Transaction> MintTransaction(Account sgIssuer, Account sgOperator, String amount)
    {
        var server = new Server("https://horizon-testnet.stellar.org");
        /*
         * # Example of minting new assets to the Operator with a sandwich transaction
           stellar tx new set-trustline-flags --fee 1000 --build-only --source sg-issuer --asset $ASSET --trustor $OPERATOR_PK --set-authorize \
           | stellar tx op add payment --destination $OPERATOR_PK --asset $ASSET --amount 1000000000000 \
           | stellar tx op add set-trustline-flags --asset $ASSET --trustor $OPERATOR_PK --clear-authorize \
           | stellar tx sign --sign-with-key sg-issuer \
           | stellar tx send
         */
        var asset = Info.Eurcv(sgIssuer);
        const uint none = 0;
        uint authorize = 0x1;
        var setTrustlineFlags = new SetTrustlineFlagsOperation(asset, sgOperator.KeyPair, authorize, none);
        var clearSetTrustlineFlags =  new SetTrustlineFlagsOperation(asset, sgOperator.KeyPair,none, authorize);
        var accountInfo = await server.Accounts.Account(sgIssuer.AccountId);
        var payment = new PaymentOperation(sgOperator.MuxedAccount, asset, amount);

        return new TransactionBuilder(accountInfo)
            .SetFee(1000)
            .AddOperation(setTrustlineFlags)
            .AddOperation(payment)
            .AddOperation(clearSetTrustlineFlags)
            .Build();
    }
    
    public static async Task<Transaction> Payment(Account sgIssuer, Account sgOperator, Account receiver, String amount)
    {
        var server = new Server("https://horizon-testnet.stellar.org");
        /*
         * # Example of minting new assets to the Operator with a sandwich transaction
           stellar tx new set-trustline-flags --fee 1000 --build-only --source sg-issuer --asset $ASSET --trustor $OPERATOR_PK --set-authorize \
           | stellar tx op add payment --destination $OPERATOR_PK --asset $ASSET --amount 1000000000000 \
           | stellar tx op add set-trustline-flags --asset $ASSET --trustor $OPERATOR_PK --clear-authorize \
           | stellar tx sign --sign-with-key sg-issuer \
           | stellar tx send
         */
        var asset = Info.Eurcv(sgIssuer);
        const uint none = 0;
        uint authorize = 0x1;
        var setTrustlineFlags = new SetTrustlineFlagsOperation(asset, sgOperator.KeyPair, authorize, none);
        var clearSetTrustlineFlags =  new SetTrustlineFlagsOperation(asset, sgOperator.KeyPair,none, authorize);
        var accountInfo = await server.Accounts.Account(sgIssuer.AccountId);
        var payment = new PaymentOperation(receiver.MuxedAccount, asset, amount, sgOperator.MuxedAccount);

        return new TransactionBuilder(accountInfo)
            .SetFee(1000)
            .AddOperation(setTrustlineFlags)
            .AddOperation(payment)
            .AddOperation(clearSetTrustlineFlags)
            .Build();
    }
}