using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TurnBasedCore.Core.Players;
using TurnBasedCore.Core.TurnSystem;
using UnityEngine;

public class AIPanda : MonoBehaviour 
{
    public string NickName;
    public PlayerInfo Info { get; set; }

    float time;

    void Start()
    {
        Info = new PlayerInfo(-1, NickName, true, false);
    }

    void Update()
    {
        if (TurnManager.Instance.GetCurrentPlayer() == this )
        {
            if (time > 0)
            {
                time -= Time.deltaTime;
            }
            else
            {
                BattleConnector.Instance.SelectOption(Random.Range(0,4));
            }
        }
    }
}
