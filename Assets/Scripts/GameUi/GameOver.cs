using System.Collections;
using TMPro;
using UnityEngine;

namespace GameUi
{
    public class GameOver : MonoBehaviour
    {
        public static readonly string HIGHSCORE_KEY = "HighScore";
        public static readonly string GEMS_KEY = "Gems";

        public GameObject topPanel;

        [Header("Rekt")] public GameObject rektPanel;
        public float rektImageDuration = 1.5f;

        [Header("High Score")] public GameObject highScorePanel;
        public TextMeshProUGUI highScoreText;
        public GameObject highScoreImage;
        public GameObject highScoreYourScoreImage;

        [Header("Name Entry")]
        public GameObject nameEntryPanel;

        [Header("Leaderboard")] public GameObject leaderboardPanel;
        [Header("Final")] 
        public GameObject finalPanel;
        public Animator finalGifAnimator;

        private StateMachine<GameOver> stateMachine;

        private void Awake()
        {
            stateMachine = new StateMachine<GameOver>(this);

            var rektState = new GameOverRektState();
            var highScoreState = new GameOverHighScoreState();
            var nameEntryState = new GameOverNameEntryState();
            var leaderboardState = new GameOverLeaderboardState();
            var finalState = new GameOverFinalState();

            stateMachine.AddState(rektState);
            stateMachine.AddState(highScoreState);
            stateMachine.AddState(nameEntryState);
            stateMachine.AddState(leaderboardState);
            stateMachine.AddState(finalState);
        }

        public void OnEnable()
        {
            stateMachine.ChangeState<GameOverRektState>();
        }

        public void OnHighScoreNextButton()
        {
            if (stateMachine.CurrentState is GameOverHighScoreState)
            {
                stateMachine.ChangeState<GameOverNameEntryState>();
            }
        }

        public void OnNameEntryNextButton()
        {
            StartCoroutine(GoLeaderboardPhase());
        }

        private IEnumerator GoLeaderboardPhase()
        {
            float timer = 1f;
            while(timer > 0f && !PlayfabManager.Instance.nameUpdated && !PlayfabManager.Instance.error)
            { 
                timer -= Time.unscaledDeltaTime;
                yield return null;
            }
            if (stateMachine.CurrentState is GameOverNameEntryState)
            {
                stateMachine.ChangeState<GameOverLeaderboardState>();
            }
        }

        public void OnLeaderboardNextButton()
        {
            if (stateMachine.CurrentState is GameOverLeaderboardState)
            {
                stateMachine.ChangeState<GameOverFinalState>();
            }
        }
    }

    public class GameOverRektState : State<GameOver>
    {
        public bool IsHighScore { get; private set; }

        public override void Enter()
        {
            owner.topPanel.SetActive(false);
            owner.rektPanel.SetActive(true);

            var previousHighScore = PlayerPrefs.GetFloat(GameOver.HIGHSCORE_KEY, 0);
            IsHighScore = Player.Instance.DistanceStatistic > previousHighScore;
            if (IsHighScore)
            {
                PlayerPrefs.SetFloat(GameOver.HIGHSCORE_KEY, Player.Instance.DistanceStatistic);
            }

            var gems = PlayerPrefs.GetInt(GameOver.GEMS_KEY, 0);
            PlayerPrefs.SetInt(GameOver.GEMS_KEY, gems + Player.Instance.GemsStatistic);
            PlayerPrefs.Save();

            owner.StartCoroutine(WaitAndExit());
        }

        private IEnumerator WaitAndExit()
        {
            yield return new WaitForSeconds(owner.rektImageDuration);
            stateMachine.ChangeState<GameOverHighScoreState>();
        }

        public override void Exit()
        {
            owner.rektPanel.SetActive(false);
        }
    }

    public class GameOverHighScoreState : State<GameOver>
    {
        public override void Enter()
        {
            owner.highScorePanel.SetActive(true);
            owner.highScoreText.text = $"{Player.Instance.DistanceStatistic:F2} BLOCKS";

            if (stateMachine.PreviousState is GameOverRektState gameOverRektState)
            {
                owner.highScoreImage.SetActive(gameOverRektState.IsHighScore);
                owner.highScoreYourScoreImage.SetActive(!gameOverRektState.IsHighScore);
            }
        }

        public override void Exit()
        {
            owner.highScorePanel.SetActive(false);
        }
    }

    public class GameOverLeaderboardState : State<GameOver>
    {
        public override void Enter()
        {
            owner.leaderboardPanel.SetActive(true);
        }

        public override void Exit()
        {
            owner.leaderboardPanel.SetActive(false);
        }
    }

    public class GameOverFinalState : State<GameOver>
    {
        public override void Enter()
        {
            owner.finalPanel.SetActive(true);
            int scoreLevel = 0;
            if (Player.Instance.DistanceStatistic > 500f)
                scoreLevel++;
            if (Player.Instance.DistanceStatistic > 1000f)
                scoreLevel++;
            owner.finalGifAnimator.SetInteger("level", scoreLevel);
        }

        public override void Exit()
        {
            owner.finalPanel.SetActive(false);
        }
    }

    public class GameOverNameEntryState : State<GameOver>
    {
        public override void Enter()
        {
            owner.nameEntryPanel.SetActive(true);
        }

        public override void Exit()
        {
            owner.nameEntryPanel.SetActive(false);
        }
    }
}