using System.Threading.Tasks;
using StellarDotnetSdk;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Responses;
using MuxedAccount = StellarDotnetSdk.Xdr.MuxedAccount;

namespace StellarAssetSdk;

public class AccountChecker(MuxedAccount account)
{


    //Set network and server
    /*
    public static async Task GetAccountBalance(string account)
    {
        //Set server
        Server server = new Server("https://horizon-testnet.stellar.org");

        //Generate a keypair from the account id.
        KeyPair keypair = KeyPair.FromSecretSeed(account);

        //Load the account
        AccountResponse accountResponse = await server.Accounts.Account(keypair.AccountId);

        //Get the balance
        Balance[] balances = accountResponse.Balances;

        //Show the balance
        foreach (var asset in balances)
        {
            Console.WriteLine("Asset Code: " + asset.AssetType);
            Console.WriteLine("Asset Amount: " + asset.BalanceString);
        }
    }
*/
}