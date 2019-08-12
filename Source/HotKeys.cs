﻿using ExitGames.Client.Photon;
using UnityEngine;

class HotKeys
{
    public static void Update()
    {
        //Restarts the game and resets all stats if you are in MP
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (IN_GAME_MAIN_CAMERA.gametype != GAMETYPE.SINGLE)
            {
                if (PhotonNetwork.isMasterClient)
                {
                    foreach (var player in PhotonNetwork.playerList)
                    {
                        var stats = new Hashtable
                            {
                                {PhotonPlayerProperty.kills, 0},
                                {PhotonPlayerProperty.deaths, 0},
                                {PhotonPlayerProperty.max_dmg, 0},
                                {PhotonPlayerProperty.total_dmg, 0}
                            };
                        player.SetCustomProperties(stats);
                    }

                    FengGameManagerMKII.instance.restartRC();
                    FengGameManagerMKII.instance.photonView.RPC("Chat", PhotonTargets.All, new object[] { InRoomChat.RCLine("MasterClient has restarted the game."), "" });
                }
                else
                {
                    InRoomChat.AddLineRC("Error: not masterclient");
                    return;
                }
            }
            else
            {
                FengGameManagerMKII.instance.restartGameSingle();
            }
        }
    }
}