using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using Asset = StellarDotnetSdk.Assets.AssetTypeNative;

namespace StellarAssetSdk.Tests;

[TestClass]
public class ServerTest
{
    [TestMethod]
    public async Task GetAccountBalance()
    {
        var server = StellarAssetSdk.Server.Local();
        var alice = await server.GenerateAndFund();
        var balances = await server.GetAccountBalance(alice.AccountId);
        var nativeBalance = balances[0];
        Assert.AreEqual(nativeBalance.BalanceString, "10000.0000000");
    }

}