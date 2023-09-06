using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class AITeams : MonoBehaviour
{
    [Header("Teams")]
    public List<GameObject> teamA = new List<GameObject>();
    public List<GameObject> teamB = new List<GameObject>();

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
        FindingTeamPlayers();
    }
    void Start()
    {
        
    }
    void Update()
    {
        //* teamA ve TeamB de olan oyuncular içinde topa en yakýn olan oyuncu topa koþsun diðerleri beklemede kalsýn
        //** teamA ve TeamB de olan oyuncular içinde beklemede kalanlar kendi etrafýnda ileri geri veya sað sol yapsýnlar
        //*** teamA ve TeamB de olan oyuncular içinde topa koþmayanlar top kendi takýmýnda ise hücüma, rakipte ise savunmaya koþsunlar
    }
    void FindingTeamPlayers()
    {
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("TeamB");

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
