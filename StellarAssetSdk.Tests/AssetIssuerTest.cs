using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace StellarAssetSdk.Tests;

[TestClass]
public class AssetIssuerTest
{
    [TestMethod]
    public async Task Friendbot()
    {
        var server = StellarAssetSdk.Server.Local();
        var res = await server.RpcServer.GetNetwork();
        Console.WriteLine(res.FriendbotUrl);
    }

    [TestMethod]
    public async Task GenerateAccount()
    {
        var server = StellarAssetSdk.Server.Local();
        var res = await server.GenerateAndFund();
        Console.WriteLine(res.AccountId);
        var account = await server.RpcServer.GetAccount(res.AccountId);
        Console.WriteLine(account.SequenceNumber);
    }

    [TestMethod]
    public async Task Mint()
    {
        var server = StellarAssetSdk.Server.Local();
        var sgIssuer = await server.GenerateAndFund();
        var sgOperator = await server.GenerateAndFund();
        var alice = await server.GenerateAndFund();
        Console.WriteLine($"{sgIssuer.AccountId}, {sgOperator.AccountId}, {alice.AccountId}");
        await Runner.Run(server, sgIssuer, sgOperator, alice, Horizon.Local());
    }
}