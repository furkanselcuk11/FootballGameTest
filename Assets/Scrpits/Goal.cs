using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    [SerializeField] private Player _scriptPlayer;
    void Start()
    {
        
    }
    void Update()
    {
        
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            if (name.Equals("GoalDetectorA"))
            {
                _scriptPlayer.IncreaseTeamBScore();
            }
            else
            {
                _scriptPlayer.IncreaseTeamAScore();
            }
            other.GetComponent<Ball>().BallResetPosition();
        }

        if (other.CompareTag("AIBall"))
        {            
            other.GetComponent<AIBall>().BallResetPosition();
        }
    }
}
