using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using UnityEngine;
using static AIPlayerTeam;
using static UnityEditor.Experimental.GraphView.GraphView;

public class AITeams : MonoBehaviour
{
    [Header("Teams")]
    public List<GameObject> teamA = new List<GameObject>();
    public List<GameObject> teamB = new List<GameObject>();
    [Space]
    [Header("Ball Reference")]
    [SerializeField] private GameObject _ball;
    public int isTheBallInTeamA = -1;   // 0= TeamA 1= TeamB
    public bool isTheBallFree;

    [Space]
    [Header("Score Settings")]    
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _goalText;
    private int _teamAScore, _teamBScore;
    private float _goalTextColorAlpha;
    [Space]
    [Header("Sonds")]
    [SerializeField] private AudioSource _soundCheer; // Sesi daha sonra ses managere taþý
    private void Awake()
    {
        _ball = GameObject.FindGameObjectWithTag("AIBall").gameObject; // Daha sonra sahaya göre top arama yap
        isTheBallInTeamA = -1;
        isTheBallFree = true;
        FindingTeamPlayers();
    }
    void Start()
    {
        StartCoroutine(ContinuousAIPlayerCloserToBall());
    }
    void Update()
    {
        //* teamA ve TeamB de olan oyuncular içinde topa en yakýn olan oyuncu topa koþsun diðerleri beklemede kalsýn
        //** teamA ve TeamB de olan oyuncular içinde beklemede kalanlar kendi etrafýnda ileri geri veya sað sol yapsýnlar
        //*** teamA ve TeamB de olan oyuncular içinde topa koþmayanlar top kendi takýmýnda ise hücüma, rakipte ise savunmaya koþsunlar
    }
    void FindingTeamPlayers()
    {
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("AIPlayer");

        foreach (GameObject player in allPlayers)
        {
            AIPlayerTeam playerTeam = player.GetComponent<AIPlayerTeam>();
            if (playerTeam != null) 
            {
                if (playerTeam.playerTeam == AIPlayerTeam.Team.ATeam)
                {
                    teamA.Add(player);
                }
                else if(playerTeam.playerTeam == AIPlayerTeam.Team.BTeam)
                {
                    teamB.Add(player);
                }
            }
        }
    }
    IEnumerator ContinuousAIPlayerCloserToBall()
    {
        while (true) // Sonsuz bir döngü oluþturur, yani oyun bitene kadar çalýþýr.
        {
            AIPlayerCloserToBall();

            // 1 saniye bekleme süresi ekleyebilirsiniz (opsiyonel)
            yield return new WaitForSeconds(1f);
        }
    }
    void AIPlayerCloserToBall()
    {
        if (_ball == null || teamA.Count == 0 ||teamB.Count == 0)
        {
            Debug.LogWarning("Top veya oyuncu listesi eksik.");
            return;
        }

        // Topa en yakýn teamA oyuncusunu bul 
        GameObject _nearstPlayerTeamA = teamA
            .OrderBy(playerAI => Vector3.Distance(playerAI.transform.position, _ball.transform.position))
            .FirstOrDefault();

        // Topa en yakýn teamB oyuncusunu bul 
        GameObject _nearstPlayerTeamB = teamB
            .OrderBy(playerAI => Vector3.Distance(playerAI.transform.position, _ball.transform.position))
            .FirstOrDefault();

        // Topa en yakýn teamA oyuncusunun yapacaðý iþlem
        if (_nearstPlayerTeamA != null)
        {
            //if (_nearstPlayerTeamA.GetComponent<AIController>().BallAttachedToPlayer == null)
            //{
            //    if (isTheBallInTeamA != 1 || isTheBallFree)
            //    {
            //        // pas ve þut çektikten sonra bekleme süresine göre tekrar arasýn
            //        _nearstPlayerTeamA.GetComponent<AIController>().CurrentState = AIController.AIPlayerState.CatchTheBall;
            //    }
            //    else if (isTheBallInTeamA == 1)
            //    {
            //        return;
            //    }

            //}
            //else if (_nearstPlayerTeamA.GetComponent<AIController>().BallAttachedToPlayer != null)
            //{
            //    return;
            //}
        }

        // Topa en yakýn teamB oyuncusunun yapacaðý iþlem
        if (_nearstPlayerTeamB != null)
        {
            if (_nearstPlayerTeamB.GetComponent<AIController>().BallAttachedToPlayer == null)
            {
                if (isTheBallInTeamA != 1 || isTheBallFree)
                {
                    // pas ve þut çektikten sonra bekleme süresine göre tekrar arasýn
                    _nearstPlayerTeamB.GetComponent<AIController>().CurrentState = AIController.AIPlayerState.CatchTheBall;
                }
                else if (isTheBallInTeamA == 1)
                {
                    return;
                }

            }
            else if(_nearstPlayerTeamB.GetComponent<AIController>().BallAttachedToPlayer != null)
            {
                return;
            }
            
        }
    }
    public void TeamsClear()
    {
        // teamA ve teamB listelerini temizlemek için:
        teamA.Clear();
        teamB.Clear();
    }

    public void IncreaseTeamAScore()
    {
        _teamAScore++;
        UpdateScoreUI();
    }
    public void IncreaseTeamBScore()
    {
        _teamBScore++;
        UpdateScoreUI();
    }
    private void UpdateScoreUI()
    {
        _soundCheer.Play();
        _scoreText.text = "Team A [" + _teamAScore.ToString() + " - " + _teamBScore.ToString() + "] Team B";
        _goalTextColorAlpha = 1f;
    }
}
