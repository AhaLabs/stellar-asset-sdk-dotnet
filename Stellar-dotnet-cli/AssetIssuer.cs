using StellarDotnetSdk;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Transactions;

namespace Stellar_dotnet_cli;

public class AssetIssuer
{
//Set network and server
    public static async Task<Transaction> IssueTransaction(Account sg_issuer, Account sg_operator)
    {
        //Set server
        var server = new Server("https://horizon-testnet.stellar.org");
        var clawbackEnabled = 0x8;
        var revokableEnabled = 0x2;
        var asset = Info.Eurcv(sg_issuer);
        var setOpts = new SetOptionsOperation().SetSetFlags(clawbackEnabled | revokableEnabled);
        var changeTrust = new ChangeTrustOperation(asset, null, sg_operator.MuxedAccount);
        var payment = new PaymentOperation(sg_operator.MuxedAccount, asset, "1");
        uint authorize = 0x1;
        uint none = 0;
        var setTrustlineFlags = new SetTrustlineFlagsOperation(asset, sg_operator.KeyPair, none, authorize);
        var accountInfo = await server.Accounts.Account(sg_issuer.AccountId);
        var tx = new TransactionBuilder(accountInfo)
            .SetFee(1000)
            .AddOperation(setOpts)
            .AddOperation(changeTrust)
            .AddOperation(payment)
            .AddOperation(setTrustlineFlags)
            .Build();
        return tx;
    }
}