using StellarDotnetSdk;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Xdr;
using Transaction = StellarDotnetSdk.Transactions.Transaction;

namespace Stellar_dotnet_cli;

public interface Signer
{
    Task<DecoratedSignature> Sign(Transaction tx, Network network);
}

public class Local(KeyPair keyPair) : Signer
{
    public Task<DecoratedSignature> Sign(Transaction tx, Network network)
    {
        tx.Sign(keyPair);
        return Task.FromResult(tx.Signatures.Last());
    }
}