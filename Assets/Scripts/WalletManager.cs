using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class WalletManager : MonoBehaviourSingleton<WalletManager>
{

    private string _walletAddress;

    [Preserve]
    public string WalletAddress => _walletAddress;

    [Preserve]
    public void SetWalletAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            Debug.LogWarning("WalletManager received an empty wallet address.");
            return;
        }

        var normalizedAddress = address.Trim();
        if (!string.IsNullOrEmpty(_walletAddress) && _walletAddress == normalizedAddress)
        {
            Debug.Log("WalletManager received the same wallet address; skipping duplicate login.");
            return;
        }

        _walletAddress = normalizedAddress;
        PlayerPrefs.SetString("WalletAddress", _walletAddress);
        PlayerPrefs.Save();
        Debug.Log($"Wallet address set to: {_walletAddress}");
        PlayfabManager.Instance.LoginWithWallet(_walletAddress);
    }

}
