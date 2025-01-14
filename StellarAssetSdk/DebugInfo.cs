using System;
using System.Diagnostics;
using StellarDotnetSdk.Responses.SorobanRpc;
using StellarDotnetSdk.Xdr;

namespace StellarAssetSdk;

using System.Text.Json;

public static class DebugInfo
{
    public static void WriteLine(string name, object obj)
    {
        string debugOutput = JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
        Debug.WriteLine($"{name}: {debugOutput}");
    }
    
    public static void WriteLine(string name, GetTransactionResponse res)
    {
        // byte[] decodedBytes = Convert.FromBase64String(res.ResultXdr!);
        // var stream = new XdrDataInputStream(decodedBytes);
        // var txResult = TransactionResult.Decode(stream);
        var link =
            $"https://lab.stellar.org/xdr/view?$=network$id=testnet&label=Testnet&horizonUrl=https:////horizon-testnet.stellar.org&rpcUrl=https:////soroban-testnet.stellar.org&passphrase=Test%20SDF%20Network%20/;%20September%202015;&xdr$blob={res.ResultXdr}&type=TransactionResult;;";
        Debug.WriteLine($"{name}: hash: {res.TxHash}, link:\n{link}");
        // WriteLine(name, txResult);
    }
}