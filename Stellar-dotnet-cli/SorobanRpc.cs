using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Responses.SorobanRpc;
using StellarDotnetSdk.Soroban;
using Asset = StellarDotnetSdk.Assets.Asset;

namespace Stellar_dotnet_cli;

public class TestnetServer(SorobanServer rpcServer)
{
    public static SorobanServer Server()
    {
        return new SorobanServer(RpcUrl());
    }


    public static string RpcUrl()
    {
        return "https://soroban-testnet.stellar.org";
    }

    public static string NetworkPassphrase()
    {
        return "Test SDF Network ; September 2015";
    }

    public static Asset Eurcv(Account issuer)
    {
        return Eurcv(issuer.AccountId);
    }

    public static Asset Eurcv(string issuer)
    {
        return new AssetTypeCreditAlphaNum12("EURCV", issuer);
    }

    public static async Task<GetTransactionResponse> GetTransaction(string hash, SorobanServer server)
    {
        var start = DateTime.UtcNow;
        var timeout = TimeSpan.FromSeconds(60);

        // See https://tsapps.nist.gov/publication/get_pdf.cfm?pub_id=50731
        // Is optimal exponent for exponential backoff
        var exponentialBackoff = 1.0 / (1.0 - Math.Pow(Math.E, -1.0));
        var sleepTime = TimeSpan.FromSeconds(1);

        while (true)
        {
            var res = await server.GetTransaction(hash);

            switch (res.Status)
            {
                case TransactionInfo.TransactionStatus.SUCCESS:
                    return res;

                case TransactionInfo.TransactionStatus.FAILED:
                    throw new TransactionSubmissionException(
                        $"Transaction submission failed: {res.ResultValue}");

                case TransactionInfo.TransactionStatus.NOT_FOUND:
                    break;

                default:
                    throw new UnexpectedTransactionStatusException(res.Status.ToString());
            }

            if (DateTime.UtcNow - start > timeout) throw new TransactionSubmissionTimeoutException();

            await Task.Delay(sleepTime);
            sleepTime = TimeSpan.FromSeconds(sleepTime.TotalSeconds * exponentialBackoff);
        }
    }
}

public class TransactionSubmissionException : Exception
{
    public TransactionSubmissionException(string message) : base(message)
    {
    }
}

public class UnexpectedTransactionStatusException : Exception
{
    public UnexpectedTransactionStatusException(string status)
        : base($"Unexpected transaction status: {status}")
    {
    }
}

public class TransactionSubmissionTimeoutException : Exception
{
    public TransactionSubmissionTimeoutException()
        : base("Transaction submission timed out")
    {
    }
}