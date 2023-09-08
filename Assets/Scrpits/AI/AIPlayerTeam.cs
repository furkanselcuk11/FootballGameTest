using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPlayerTeam : MonoBehaviour
{
    public enum Team
    {
        ATeam,
        BTeam
    }
    public enum PlayerPosition
    {
        Goalkeeper,
        Defense,
        Midfielder,
        Striker
    }

    public Team playerTeam;
    public PlayerPosition playerPosition;
}
