using PlayFab;
using PlayFab.ClientModels;
using PlayFab.ProgressionModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Thirdweb.Unity;
using Unity.VisualScripting;
using UnityEngine;

public class PlayfabManager : MonoBehaviourSingletonPersistent<PlayfabManager>
{
    public LeaderboardDefinition leaderboardDefinition;
    public List<PlayerLeaderboardEntry> currentLeaderboard = new();
    public (string gems, string kills)[] otherValues;
    public bool error;
    Coroutine leaderboardCoroutine;
    bool _leaderboardLoadComplete;
    bool _leaderboardLoadFailed;
    bool _pipelineStepDone;
    int _pendingExtraStats;
    public string currentPlayerID;
    public bool nameUpdated;
    public string oldName;
    public string newName;
    public string walletAddress;
    private string _lastLoginWalletId;
    private string _currentLevelKey;

    public override void Awake()
    {
        base.Awake();
    }

    public async void ConnectToWallet()
    {
        var options = new WalletOptions(provider: WalletProvider.ReownWallet, chainId: 1);
        var wallet = await ThirdwebManager.Instance.ConnectWallet(options);

        walletAddress = await wallet.GetAddress();
        Debug.Log($"Wallet address: {walletAddress}");

        if (walletAddress != null && walletAddress.Length > 0)
            PlayerPrefs.SetString("WalletAddress", walletAddress);

        LoginWithWallet(walletAddress);

        // if (walletAddress != null && walletAddress.Length > 0)
        // {
        //     var dataRequest = new UpdateUsserDataRequest
        //     {
        //         Data = new Dictionary<string, string>
        //         {
        //             { "WalletAddress", walletAddress }
        //         },
        //         Permission = UserDataPermission.Public
        //     };
        //
        //     PlayFabClientAPI.UpdateUserData(dataRequest, resultCallback => { Debug.Log("successfuldataUpdate"); },
        //         OnError);
        // }
    }

    void OnLeaderboardStepError(PlayFabError playFabError)
    {
        Debug.LogError($"[PlayFab] Leaderboard pipeline error: {playFabError.GenerateErrorReport()}");
        _leaderboardLoadFailed = true;
        _leaderboardLoadComplete = true;
        _pipelineStepDone = true;
    }

    public void LoginWithWallet(string walletID)
    {
        if (string.IsNullOrWhiteSpace(walletID))
        {
            Debug.LogWarning("[PlayFab] LoginWithWallet called with empty wallet ID.");
            return;
        }

        walletID = walletID.Trim();
        if (!string.IsNullOrEmpty(_lastLoginWalletId) && _lastLoginWalletId == walletID && !string.IsNullOrEmpty(currentPlayerID))
        {
            Debug.Log($"[PlayFab] Already logged in with wallet ID: {walletID}");
            return;
        }

        _lastLoginWalletId = walletID;
        Debug.Log($"[PlayFab] Logging in with wallet ID: {walletID}");
        var request = new LoginWithCustomIDRequest
        {
            CustomId = walletID,
            CreateAccount = true,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerProfile = true
            }
        };
        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnError);
    }

    public void LoginAsGuest()
    {
        string customId = new UniqueID().ID;
        Debug.Log($"[PlayFab] Logging in as guest with ID: {customId}");

        var request = new LoginWithCustomIDRequest
        {
            CustomId = customId,

            CreateAccount = true,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerProfile = true
            }
        };
        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnError);
    }

    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log($"[PlayFab] ✓ Login success — PlayFabId: {result.PlayFabId}, NewlyCreated: {result.NewlyCreated}");

        string playerName = null;
        if (result.InfoResultPayload.PlayerProfile != null)
            playerName = result.InfoResultPayload.PlayerProfile.DisplayName;
        newName = playerName;
        oldName = playerName;
        currentPlayerID = result.PlayFabId;

        if (PlayerPrefs.GetString("WalletAddress", string.Empty) != string.Empty)
        {
            if (walletAddress != null && walletAddress.Length > 0)
            {
                var dataRequest = new UpdateUserDataRequest
                {
                    Data = new Dictionary<string, string>
                    {
                        { "WalletAddress", walletAddress }
                    },
                    Permission = UserDataPermission.Public
                };

                PlayFabClientAPI.UpdateUserData(dataRequest, resultCallback => { Debug.Log("successfuldataUpdate"); },
                    OnError);
            }
        }
    }

    private void OnError(PlayFabError error)
    {
        this.error = true;
        Debug.LogError($"[PlayFab] ✗ Error — {error.GenerateErrorReport()}");
    }

    public void SendEmptyLeaderboard()
    {
        SendLeaderboard(0, 0, 0, "default");
    }

    public void SendLeaderboard(int distance, int numberRekt, int gems, string levelKey)
    {
        if (leaderboardCoroutine != null)
            StopCoroutine(leaderboardCoroutine);

        currentLeaderboard.Clear();
        otherValues = new (string gems, string kills)[0];
        _leaderboardLoadComplete = false;
        _leaderboardLoadFailed = false;
        _pendingExtraStats = 0;
        leaderboardCoroutine = StartCoroutine(Co_SendLeaderboard(distance, numberRekt, gems, levelKey));
    }

    IEnumerator Co_SendLeaderboard(int distance, int numberRekt, int gems, string levelKey)
    {
        string distanceStatName = "MaxDistance_" + levelKey;
        string rektStatName = "MaxRekt_" + levelKey;
        _currentLevelKey = levelKey;
        error = false;

        Debug.Log($"[PlayFab] Starting leaderboard update — Distance: {distance}, Rekt: {numberRekt}, Gems: {gems}");

        var playerDataRequest = new GetPlayerStatisticsRequest
        {
            StatisticNames = new List<string> { distanceStatName, rektStatName, "MaxGems" }
        };

        int currentMaxDistance = 0, currentMaxRekt = 0, currentMaxGems = 0;

        Debug.Log("[PlayFab] Step 1/5 — GetPlayerStatistics...");
        _pipelineStepDone = false;
        PlayFabClientAPI.GetPlayerStatistics(playerDataRequest,
            resultCallback =>
            {
                foreach (var stat in resultCallback.Statistics)
                {
                    if (stat.StatisticName == distanceStatName)
                        currentMaxDistance = stat.Value;
                    if (stat.StatisticName == rektStatName)
                        currentMaxRekt = stat.Value;
                    if (stat.StatisticName == "MaxGems")
                        currentMaxGems = stat.Value;
                }

                Debug.Log($"[PlayFab] ✓ Got stats — {distanceStatName}: {currentMaxDistance}, {rektStatName}: {currentMaxRekt}, MaxGems: {currentMaxGems}");
                _pipelineStepDone = true;
            },
            OnLeaderboardStepError);

        yield return new WaitUntil(() => _pipelineStepDone);
        if (_leaderboardLoadFailed) yield break;

        // Only update if new values are higher
        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<PlayFab.ClientModels.StatisticUpdate>
            {
                new()
                {
                    StatisticName = distanceStatName,
                    Value = Mathf.Max(distance, currentMaxDistance)
                },
                new()
                {
                    StatisticName = rektStatName,
                    Value = Mathf.Max(numberRekt, currentMaxRekt)
                },
                new()
                {
                    StatisticName = "MaxGems",
                    Value = Mathf.Max(gems, currentMaxGems)
                }
            }
        };

        Debug.Log("[PlayFab] Step 2/5 — UpdatePlayerStatistics...");
        _pipelineStepDone = false;
        PlayFabClientAPI.UpdatePlayerStatistics(request, result =>
        {
            Debug.Log("[PlayFab] ✓ Stats updated successfully");
            _pipelineStepDone = true;
        }, OnLeaderboardStepError);

        yield return new WaitForSeconds(.5f);
        yield return new WaitUntil(() => _pipelineStepDone);
        if (_leaderboardLoadFailed) yield break;

        Debug.Log("[PlayFab] Step 3/5 — Re-fetching player stats to confirm...");
        var playerDataRequest2 = new GetPlayerStatisticsRequest
        {
            StatisticNames = new List<string> { distanceStatName, rektStatName, "MaxGems" }
        };

        _pipelineStepDone = false;
        PlayFabClientAPI.GetPlayerStatistics(playerDataRequest2,
            resultCallback =>
            {
                foreach (var stat in resultCallback.Statistics)
                {
                    if (stat.StatisticName == distanceStatName)
                        currentMaxDistance = stat.Value;
                    if (stat.StatisticName == rektStatName)
                        currentMaxRekt = stat.Value;
                    if (stat.StatisticName == "MaxGems")
                        currentMaxGems = stat.Value;
                }

                Debug.Log($"[PlayFab] ✓ Confirmed stats — {distanceStatName}: {currentMaxDistance}, {rektStatName}: {currentMaxRekt}, MaxGems: {currentMaxGems}");
                _pipelineStepDone = true;
            },
            OnLeaderboardStepError);

        yield return new WaitUntil(() => _pipelineStepDone);
        if (_leaderboardLoadFailed) yield break;

        Debug.Log("[PlayFab] Step 4/5 — UpdateUserData (public gems & rekt)...");
        var dataRequest = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                { rektStatName, currentMaxRekt.ToString() },
                { "MaxGems", currentMaxGems.ToString() },
            },
            Permission = UserDataPermission.Public
        };

        _pipelineStepDone = false;
        PlayFabClientAPI.UpdateUserData(dataRequest, resultCallback =>
            {
                Debug.Log("[PlayFab] ✓ User data updated successfully");
                _pipelineStepDone = true;
            },
            OnLeaderboardStepError);

        yield return new WaitUntil(() => _pipelineStepDone);
        if (_leaderboardLoadFailed) yield break;

        Debug.Log($"[PlayFab] Step 5/5 — GetLeaderboard (top 100 by {distanceStatName})...");
        var boardRequest = new GetLeaderboardRequest
        {
            StatisticName = distanceStatName,
            StartPosition = 0,
            MaxResultsCount = 100,
        };

        PlayFabClientAPI.GetLeaderboard(boardRequest, OnLeaderboardGet, OnLeaderboardStepError);
    }

    void OnLeaderboardUpdate(UpdatePlayerStatisticsResult result)
    {
        Debug.Log("Successful Leaderboard Update Sent!");
    }

    void GetLeaderboard(string distanceStatName)
    {
        var boardRequest = new GetLeaderboardRequest
        {
            StatisticName = distanceStatName,
            StartPosition = 0,
            MaxResultsCount = 100,
        };

        PlayFabClientAPI.GetLeaderboard(boardRequest, OnLeaderboardGet, OnError);
    }

    void OnLeaderboardGet(GetLeaderboardResult result)
    {
        Debug.Log($"[PlayFab] ✓ Leaderboard received — {result.Leaderboard.Count} entries");
        foreach (var item in result.Leaderboard)
            Debug.Log($"[PlayFab]   #{item.Position + 1} | {item.DisplayName} | Distance: {item.StatValue} | ID: {item.PlayFabId}");

        currentLeaderboard = result.Leaderboard;
        otherValues = new (string, string)[result.Leaderboard.Count];
        _pendingExtraStats = result.Leaderboard.Count;

        if (_pendingExtraStats == 0)
        {
            _leaderboardLoadComplete = true;
            return;
        }

        for (int i = 0; i < currentLeaderboard.Count; i++)
            GetAdditionalStats(currentLeaderboard[i], i);
    }

    public bool LeaderboardIsDone()
    {
        return _leaderboardLoadComplete || _leaderboardLoadFailed;
    }

    void OnExtraStatsLoaded(int index)
    {
        _pendingExtraStats--;
        if (_pendingExtraStats <= 0)
            _leaderboardLoadComplete = true;
    }


    public void UpdateName(string name)
    {
        error = false;
        nameUpdated = false;
        if (name == null || name == string.Empty)
        {
            nameUpdated = true;
            return;
        }

        var request = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = name
        };
        PlayFabClientAPI.UpdateUserTitleDisplayName(request, updateFinished =>
            {
                OnUpdateDisplayName(updateFinished);
                nameUpdated = true;
                oldName = newName;
                newName = updateFinished.DisplayName;
            },
            OnError);
    }

    private void OnUpdateDisplayName(UpdateUserTitleDisplayNameResult result) => Debug.Log(result.DisplayName);


    public void GetAdditionalStats(PlayerLeaderboardEntry entry, int index)
    {
        string rektKey = "MaxRekt_" + _currentLevelKey;
        var dataRequest = new GetUserDataRequest
        {
            PlayFabId = entry.PlayFabId,
        };

        PlayFabClientAPI.GetUserData(dataRequest, dataResult =>
            {
                var data = dataResult.Data;

                otherValues[index].gems = data.ContainsKey("MaxGems") ? data["MaxGems"].Value : "0";
                otherValues[index].kills = data.ContainsKey(rektKey) ? data[rektKey].Value : "0";

                Debug.Log($"[PlayFab] ✓ Extra stats for {entry.DisplayName} — Gems: {otherValues[index].gems}, Rekt: {otherValues[index].kills}");
                OnExtraStatsLoaded(index);
            },
            error =>
            {
                Debug.LogError($"[PlayFab] ✗ Failed to get extra stats for {entry.DisplayName}: {error.GenerateErrorReport()}");
                otherValues[index].gems = "0";
                otherValues[index].kills = "0";
                OnExtraStatsLoaded(index);
            });
    }
}
