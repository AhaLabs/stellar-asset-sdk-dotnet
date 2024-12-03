using StellarDotnetSdk;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;

namespace Stellar_dotnet_cli;

public class Info
{
    public static Server Testnet()
    {
        return new Server("https://horizon-testnet.stellar.org");
    }

    public static Asset Eurcv(Account issuer)
    {
        return Eurcv(issuer.AccountId);
    }

    public static Asset Eurcv(string issuer)
    {
        return new AssetTypeCreditAlphaNum12("EURCV", issuer);
    }
}