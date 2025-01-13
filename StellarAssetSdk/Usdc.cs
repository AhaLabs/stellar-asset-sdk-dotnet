using StellarDotnetSdk;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;

namespace StellarAssetSdk;

public class Usdc: AssetTypeCreditAlphaNum12
{
    public Usdc(ITransactionBuilderAccount issuer) : base("USDC", issuer.AccountId){}
    
    public Usdc(string issuer) : base("USDC", issuer){}
}