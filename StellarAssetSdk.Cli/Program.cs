using System;
using System.Threading.Tasks;
using StellarDotnetSdk;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Soroban;
//using StellarDotnetSdk.Xdr;
using StellarDotnetSdk.Transactions;

using StellarAssetSdk;


namespace StellarAssetSdk.Cli;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var server = Server.Testnet();
        var sgIssuer = KeyPair.FromSecretSeed(args[0]);
        var sgOperator = KeyPair.FromSecretSeed(args[1]);
        var alice = KeyPair.FromSecretSeed(args[2]);
        await Runner.Run(server, sgIssuer, sgOperator, alice, Horizon.Testnet());
    }
}