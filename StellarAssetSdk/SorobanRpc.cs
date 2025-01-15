using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using NSec.Cryptography;
using StellarDotnetSdk;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Responses;
using StellarDotnetSdk.Responses.SorobanRpc;
using StellarDotnetSdk.Soroban;
using StellarDotnetSdk.Transactions;
using Asset = StellarDotnetSdk.Assets.Asset;

namespace StellarAssetSdk;

public class Server
{
    public readonly SorobanServer RpcServer;

    public Server(SorobanServer rpcServer)
    {
        RpcServer = rpcServer;
    }

    public Server(string uri, string? bearerToken = null)
    {
        RpcServer = new SorobanServer(uri, bearerToken);
    }

    public static Server Testnet()
    {
        Network.UseTestNetwork();
        return new Server(new SorobanServer(TestnetRpcUrl()));
    }

    public static Server Local(string? uri = null)
    {
        Network.Use(new Network("Standalone Network ; February 2017"));
        var envVar = Environment.GetEnvironmentVariable("STELLAR_TEST_RPC_URL");
        Console.WriteLine($"Env:{envVar}");
        return new Server((uri ?? envVar) ?? throw new
            InvalidOperationException());
    }


    public static string TestnetRpcUrl()
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

    public async Task<GetTransactionResponse> GetTransaction(string hash)
    {
        var start = DateTime.UtcNow;
        var timeout = TimeSpan.FromSeconds(60);

        // See https://tsapps.nist.gov/publication/get_pdf.cfm?pub_id=50731
        // Is optimal exponent for exponential backoff
        var exponentialBackoff = 1.0 / (1.0 - Math.Pow(Math.E, -1.0));
        var sleepTime = TimeSpan.FromMilliseconds(2000);
        var tryCount = 1;

        while (true)
        {
            var res = await RpcServer.GetTransaction(hash);
            Console.WriteLine($"Try: {tryCount++} Status: {res.Status}");
            switch (res.Status)
            {
                case TransactionInfo.TransactionStatus.SUCCESS:
                    {
                        DebugInfo.WriteLine("tx", res);
                        return res;
                    }
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

    public async Task<GetTransactionResponse> SubmitTransaction(Transaction tx)
    {
        var result = await RpcServer.SendTransaction(tx);
        Console.WriteLine($"status: {result.Status}");
        if (result.Status == SendTransactionResponse.SendTransactionStatus.ERROR)
        {

            throw new TransactionSendFailed(result.ErrorResultXdr!);
        }

        return await GetTransaction(result.Hash);
    }

    public async Task<TransactionBuilder> NewTransactionBuilder(ITransactionBuilderAccount account)
    {
        var accountInfo = await RpcServer.GetAccount(account.AccountId);
        return new TransactionBuilder(accountInfo);
    }

    public async Task<KeyPair> GenerateAndFund()
    {
        var result = KeyPair.Random();
        var network = await RpcServer.GetNetwork();
        var url = network.FriendbotUrl.StartsWith("http://localhost")
            ? Environment.GetEnvironmentVariable("STELLAR_TEST_FRIENDBOT_URL") ?? network.FriendbotUrl
            : network.FriendbotUrl;
        string fullFriendBotUrl = $"{url}?addr={result.AccountId}";
        Console.WriteLine(fullFriendBotUrl);
        using HttpClient client = new HttpClient();
        HttpResponseMessage response = await client.GetAsync(fullFriendBotUrl);
        response.EnsureSuccessStatusCode();
        return result;
    }

    /*
     // Currently the upstream doesn't handle fetching account balances using the soroban RPC interface
     // Perhaps need to update it since there is a 'getAccount' method provided by the server which isn't
     // being used
    public async Task<Balance[]> AccountBalance(ITransactionBuilderAccount account)
    {
        var accountInfo = await rpcServer.GetAccount(account.AccountId);
        accountInfo.

    }
    */
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

public class TransactionSendFailed : Exception
{
    public TransactionSendFailed(string s)
        : base($"Transaction send failed. No pending transaction.\n{s}")
    {
    }
}