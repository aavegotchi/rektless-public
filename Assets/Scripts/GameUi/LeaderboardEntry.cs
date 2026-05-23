using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LeaderboardData
{

    public string PlayerName;
    public string Runs;
    public string Rekt;
    public float Score;
    public bool IsPlayer;

    public LeaderboardData(string playerName, string runs, string rekt, float score, bool isPlayer)
    {
        PlayerName = playerName;
        Runs = runs;
        Rekt = rekt;
        Score = score;
        this.IsPlayer = isPlayer;
    }
}

public class LeaderboardEntry : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI rank, playerName, runs, rekt, score;

    public void Init(int rank, LeaderboardData data)
    {
        if (data == null)
            return;

        if (data.IsPlayer)
            data.PlayerName = PlayfabManager.Instance.newName;

        this.rank.text = "#" + rank.ToString() + "  ";
        playerName.text = data.PlayerName;

        if (playerName.text == null)
            playerName.text = "NO NAME";

        runs.text = data.Runs;
        rekt.text = data.Rekt;
        score.text = data.Score.ToString();

        if (data.IsPlayer)
        {
            playerName.color = Color.yellow;
            runs.color = Color.yellow;
            rekt.color = Color.yellow;
            score.color = Color.yellow;
        }
        else
        {
            playerName.color = Color.white;
            runs.color = Color.white;
            rekt.color = Color.white;
            score.color = Color.white;
        }
    }
}
