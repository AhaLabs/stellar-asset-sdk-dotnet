using StellarDotnetSdk;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;

namespace StellarAssetSdk;

public class Eurcv : AssetTypeCreditAlphaNum12
{
    public Eurcv(ITransactionBuilderAccount issuer) : base("EURCV", issuer.AccountId) { }

    public Eurcv(string issuer) : base("EURCV", issuer) { }
}