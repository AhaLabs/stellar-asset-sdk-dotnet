using System.Linq;
using System.Threading.Tasks;
using StellarDotnetSdk;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Xdr;
using Transaction = StellarDotnetSdk.Transactions.Transaction;

namespace StellarAssetSdk;

public interface IAssetIssuer
{
    Task<DecoratedSignature> Sign(Transaction tx, Network network);
}

public class Local(KeyPair keyPair) : IAssetIssuer
{
    public Task<DecoratedSignature> Sign(Transaction tx, Network network)
    {
        tx.Sign(keyPair);
        return Task.FromResult(tx.Signatures.Last());
    }
}