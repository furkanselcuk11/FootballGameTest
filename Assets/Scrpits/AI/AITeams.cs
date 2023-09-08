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
    [SerializeField] private AudioSource _soundCheer; // Sesi daha sonra ses managere ta��
    private void Awake()
    {
        _ball = GameObject.FindGameObjectWithTag("AIBall").gameObject; // Daha sonra sahaya g�re top arama yap
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
        //* teamA ve TeamB de olan oyuncular i�inde topa en yak�n olan oyuncu topa ko�sun di�erleri beklemede kals�n
        //** teamA ve TeamB de olan oyuncular i�inde beklemede kalanlar kendi etraf�nda ileri geri veya sa� sol yaps�nlar
        //*** teamA ve TeamB de olan oyuncular i�inde topa ko�mayanlar top kendi tak�m�nda ise h�c�ma, rakipte ise savunmaya ko�sunlar
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
        while (true) // Sonsuz bir d�ng� olu�turur, yani oyun bitene kadar �al���r.
        {
            AIPlayerCloserToBall();

            // 1 saniye bekleme s�resi ekleyebilirsiniz (opsiyonel)
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

        // Topa en yak�n teamA oyuncusunu bul 
        GameObject _nearstPlayerTeamA = teamA
            .OrderBy(playerAI => Vector3.Distance(playerAI.transform.position, _ball.transform.position))
            .FirstOrDefault();

        // Topa en yak�n teamB oyuncusunu bul 
        GameObject _nearstPlayerTeamB = teamB
            .OrderBy(playerAI => Vector3.Distance(playerAI.transform.position, _ball.transform.position))
            .FirstOrDefault();

        // Topa en yak�n teamA oyuncusunun yapaca�� i�lem
        if (_nearstPlayerTeamA != null)
        {
            //if (_nearstPlayerTeamA.GetComponent<AIController>().BallAttachedToPlayer == null)
            //{
            //    if (isTheBallInTeamA != 1 || isTheBallFree)
            //    {
            //        // pas ve �ut �ektikten sonra bekleme s�resine g�re tekrar aras�n
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

        // Topa en yak�n teamB oyuncusunun yapaca�� i�lem
        if (_nearstPlayerTeamB != null)
        {
            if (_nearstPlayerTeamB.GetComponent<AIController>().BallAttachedToPlayer == null)
            {
                if (isTheBallInTeamA != 1 || isTheBallFree)
                {
                    // pas ve �ut �ektikten sonra bekleme s�resine g�re tekrar aras�n
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
        // teamA ve teamB listelerini temizlemek i�in:
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
