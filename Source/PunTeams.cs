using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PunTeams : MonoBehaviour
{
    public static Dictionary<Team, List<PhotonPlayer>> PlayersPerTeam;
    public const string TeamPlayerProp = "team";

    public void OnJoinedRoom()
    {
        this.UpdateTeams();
    }

    public void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps)
    {
        this.UpdateTeams();
    }

    public void Start()
    {
        PunTeams.PlayersPerTeam = new Dictionary<PunTeams.Team, List<PhotonPlayer>>();
        Array values = Enum.GetValues(typeof(PunTeams.Team));
        foreach (object current in values)
        {
            PunTeams.PlayersPerTeam[(PunTeams.Team)((byte)current)] = new List<PhotonPlayer>();
        }
    }


    public void UpdateTeams()
    {
        Array values = Enum.GetValues(typeof(PunTeams.Team));
        foreach (object current in values)
        {
            PunTeams.PlayersPerTeam[(PunTeams.Team)((byte)current)].Clear();
        }
        for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
        {
            PhotonPlayer photonPlayer = PhotonNetwork.playerList[i];
            PunTeams.Team team = photonPlayer.GetTeam();
            PunTeams.PlayersPerTeam[team].Add(photonPlayer);
        }
    }


    public enum Team : byte
    {
        blue = 2,
        none = 0,
        red = 1
    }
}

