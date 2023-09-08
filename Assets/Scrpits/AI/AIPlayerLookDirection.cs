using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AIController;
using UnityStandardAssets.Vehicles.Ball;
using static UnityEngine.InputManagerEntry;
using System;

public class AIPlayerLookDirection : MonoBehaviour
{
    [Space]
    [Header("AI Player Look Direction Settings")]
    [SerializeField] private Transform _ball;
    [SerializeField] private Transform _teamAGoal;
    [SerializeField] private Transform _teamBGoal;
    Transform _defenceToDirection, _attackToDirection;

    private AIPlayerTeam.Team _playerTeam;  // oyunucunun takýmý

    private void Awake()
    {
        _ball = GameObject.FindWithTag("AIBall").gameObject.transform;
    }
    void Start()
    {
        AIPlayerTeam aiPlayerTeam = GetComponent<AIPlayerTeam>();
        if (aiPlayerTeam != null)
        {
            _playerTeam = aiPlayerTeam.playerTeam;
        }
        else
        {
            return;
        }
    }
    void Update()
    {
       
    }
    private void FixedUpdate()
    {
        if (_ball != null)
            AILookDirection();
    }

    void AILookDirection()
    {
        //if (_ball != null && _ballAttachedToPlayer == null)
        //{
        //    // Hedef top belirlenmiþse ve oyuncu topa baðlý deðilse Topa doðru bak
        //    Vector3 lookAtPosition = _ball.position;
        //    lookAtPosition.y = transform.position.y;
        //    transform.LookAt(lookAtPosition);
        //}
        //else if (_ball != null && _ballAttachedToPlayer != null)
        //{
        //    if (!_isShortPass && !_isLongPass)
        //    {
        //        // Hedef top belirlenmiþse ve oyuncu topa baðlý ise kaleye doðru bak
        //        Vector3 lookAtPosition = _goal.position;
        //        lookAtPosition.y = transform.position.y;
        //        transform.LookAt(lookAtPosition);
        //    }
        //    else if (_canPass && _isShortPass || _isLongPass)
        //    {
        //        // Oyunucuya bak
        //        Vector3 lookAtPosition = _targetPlayer.position;
        //        lookAtPosition.y = transform.position.y;
        //        transform.LookAt(lookAtPosition);
        //    }
        //}
        
        // -------


        // þart koy pas ve þut enumu açtýktan sonra
        // * top oyuncuda deðilse: ise topa bak,
        // ** top oyuncuda ise: pas veriyorsa hedef oyunucya bak, þut çekiyorsa kaleye bak

        if (this._playerTeam == AIPlayerTeam.Team.ATeam)
        {
            _defenceToDirection = _teamAGoal; // A Team Defense Goal
            _attackToDirection = _teamBGoal; // A Team Attack Goal
        }
        else if (this._playerTeam == AIPlayerTeam.Team.BTeam)
        {
            _defenceToDirection = _teamBGoal;   // B Team Defense Goal
            _attackToDirection = _teamAGoal;     // B Team Attack Goal
        }

        Vector3 lookAtBallPosition = _ball.position;
        lookAtBallPosition.y = transform.position.y;
        Vector3 lookAtDefencePosition = _defenceToDirection.position;
        lookAtDefencePosition.y = transform.position.y;
        Vector3 lookAtAttackPosition = _attackToDirection.position;
        lookAtAttackPosition.y = transform.position.y;

        switch (this.gameObject.GetComponent<AIController>().CurrentState)
        {
            case AIPlayerState.Idle:
                transform.LookAt(lookAtBallPosition);
                break;
            case AIPlayerState.FreeMove:
                transform.LookAt(lookAtAttackPosition);
                break;
            case AIPlayerState.PositionMove:
                transform.LookAt(lookAtAttackPosition);
                break;
            case AIPlayerState.CatchTheBall:
                transform.LookAt(lookAtBallPosition);
                break;
            case AIPlayerState.Defense:
                transform.LookAt(lookAtDefencePosition);
                break;
            case AIPlayerState.Attack:
                transform.LookAt(lookAtAttackPosition);
                break;
            default:
                Debug.LogError("Geçersiz durum!");
                break;
        }
    }
}
