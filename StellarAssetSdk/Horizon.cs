using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StellarDotnetSdk;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Responses;
using StellarDotnetSdk.Soroban;

namespace StellarAssetSdk;

public class Horizon
{
    private readonly StellarDotnetSdk.Server _server;

    public Horizon(StellarDotnetSdk.Server server)
    {
        _server = server;
    }

    public Horizon(string uri, string? bearerToken = null)
    {
        _server = new StellarDotnetSdk.Server(uri, bearerToken);
    }

    public static Horizon Testnet()
    {
        Network.UseTestNetwork();
        return new Horizon(new StellarDotnetSdk.Server("https://horizon-testnet.stellar.org"));
    }

    public static Horizon Local(string? uri = null)
    {
        Network.Use(new Network("Standalone Network ; February 2017"));
        var envVar = Environment.GetEnvironmentVariable("STELLAR_TEST_HORIZON_URL");
        Console.WriteLine($"Env:{envVar}");
        return new Horizon((uri ?? envVar) ?? throw new
            InvalidOperationException());
    }


    public async Task<List<ClaimableBalanceResponse>> ClaimableBalance(Asset asset, KeyPair claimant)
    {
        var res = await _server.ClaimableBalances.ForAsset(asset).ForClaimant(claimant).Execute();
        return res.Records;
    }
}