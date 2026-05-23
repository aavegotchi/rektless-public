using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class LeaderboardController : MonoBehaviour
{
    [SerializeField] RectTransform contentRect;
    [SerializeField] GameObject leaderboardEntryPrefab;
    List<LeaderboardEntry> leaderboardEntries = new();

    private void OnEnable()
    {  
        ClearLeaders();
        StartCoroutine(Co_WaitForLeaderboard()); ;
    }

    private IEnumerator Co_WaitForLeaderboard()
    {
        yield return new WaitUntil(() => PlayfabManager.Instance.LeaderboardIsDone());
        LeaderboardData[] leaders = GetLeaders();
        DisplayLeaders(leaders);
    }

    private void DisplayLeaders(LeaderboardData[] leaders)
    {
        for (int i = 0; i < leaders.Length; i++)
        {
            //Debug.Log("Creating entries");
            if (i < leaderboardEntries.Count)
            {
                leaderboardEntries[i].Init(i + 1, leaders[i]);
                leaderboardEntries[i].gameObject.SetActive(true);
                continue;
            }
            GameObject instantiated = Instantiate(leaderboardEntryPrefab, contentRect);
            LeaderboardEntry entry = instantiated.GetComponent<LeaderboardEntry>();

            entry.Init(i + 1, leaders[i]);
            leaderboardEntries.Add(entry);
        }
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, leaders.Length * 62);
    }

    private void ClearLeaders()
    {
        foreach (var entry in leaderboardEntries)
        {
            entry.gameObject.SetActive(false);
        }
        leaderboardEntries.Clear();
    }

    private LeaderboardData[] GetLeaders()
    {
        List<LeaderboardData> leaderboardDatas = new ();
        for (int i = 0; i < PlayfabManager.Instance.currentLeaderboard.Count; i++)
        {
            PlayFab.ClientModels.PlayerLeaderboardEntry entry = PlayfabManager.Instance.currentLeaderboard[i];
            if (entry == null) continue;

            var (gems, kills) = PlayfabManager.Instance.otherValues[i];

            leaderboardDatas.Add(new LeaderboardData(
                entry.DisplayName,
                gems,
                kills,
                entry.StatValue,
                entry.PlayFabId == PlayfabManager.Instance.currentPlayerID));
        }
        return leaderboardDatas.ToArray();
    }

    private LeaderboardData[] CreateRandomLeaders()
    {
        LeaderboardData[] leaders = new LeaderboardData[10];

        for (int i = 0; i < leaders.Length; i++)
        {
            string randomName = Data.Instance.CharacterProjectiles[UnityEngine.Random.Range(0, Data.Instance.CharacterProjectiles.Count)].name;
            int randomRuns = UnityEngine.Random.Range(1, 200);
            int randomRekt = UnityEngine.Random.Range(1, 200);
            float randomScore = UnityEngine.Random.Range(1, 2400);

            // Assign the new LeaderboardData instance to leaders[i]
           // leaders[i] = new LeaderboardData(randomName, randomRuns, randomRekt, randomScore);
        }

        // Order by Score in descending order and return
        return leaders.OrderByDescending(x => x.Score).ToArray();
    }

}
