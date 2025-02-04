using System.Threading.Tasks;
using System;
using StellarDotnetSdk;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Responses;
using MuxedAccount = StellarDotnetSdk.Xdr.MuxedAccount;

namespace StellarAssetSdk;

public class AccountChecker(MuxedAccount account)
{
    //Set network and server
    
    // public static async Task<Balance[]> GetAccountBalance(string account)
    // {
    //     //Set server
    //     StellarDotnetSdk.Server server = new StellarDotnetSdk.Server("https://horizon-testnet.stellar.org");

    //     //Load the account
    //     AccountResponse accountResponse = await server.Accounts.Account(account);

    //     //Get the balance
    //     Balance[] balances = accountResponse.Balances;

    //     //Show the balance
    //     foreach (var asset in balances)
    //     {
    //         Console.WriteLine("Asset Code: " + asset.AssetType);
    //         Console.WriteLine("Asset Amount: " + asset.BalanceString);
    //     }

    //     return balances;
    // }

}