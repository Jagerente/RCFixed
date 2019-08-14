using System;
using System.Collections.Generic;
using System.IO;
using ExitGames.Client.Photon;
using Newtonsoft.Json;
using UnityEngine;

public class HotKey
{
    private static readonly string configPath = Application.dataPath + "/keycodes.cfg";

    public string Name { get; set; }
    public KeyCode DefaultKey { get; set; }
    public KeyCode Key { get; set; }

    private static Dictionary<string, KeyCode> KeyCodes;

    //Usage example:
    //public static HotKey Restart = new HotKey("Restart", KeyCode.G);
    //
    //if (Input.GetKeyDown(Restart.Key))
    //{
    //  ...code
    //}
    public HotKey(string name, KeyCode key)
    {
        if (KeyCodes == null)
            KeyCodes = new Dictionary<string, KeyCode>();
        KeyCodes.Add(name, key);
        Name = name;
        Key = DefaultKey = key;
        Save();
    }

    //Rebind example >OnGUI()
    //var wait = false;
    //HotKey keyToRebind;
    //
    //if (GUILayout.Button(wait ? "..." : Restart.Key.ToString()))
    //{
    //  wait = true;
    //  keyToRebind = Restart;
    //}
    //
    //var event = Event.Current;
    //if (wait && event.type == EventType.KeyDown && event.keyCode != KeyCode.None)
    //{
    //  keyToRebind.Rebind(event.keyCode);
    //  wait = false;
    //}
    public void Rebind(KeyCode key)
    {
        KeyCodes[Name] = key;
        Key = key;
    }

    public static void Save()
    {
        if (!File.Exists(configPath))
            File.Create(configPath);

        //var jsonData = JsonConvert.SerializeObject(KeyCodes);
        //File.WriteAllText(configPath, jsonData);

        var configs = string.Empty;

        foreach (var keyCode in KeyCodes)
        {
            configs += $"{keyCode.Key}:{keyCode.Value}\n";
        }

        File.WriteAllText(configPath, configs);
    }

    public static void Load()
    {
        if (File.Exists(configPath))
        {
            //try
            //{
            //    KeyCodes = JsonConvert.DeserializeObject<Dictionary<string, KeyCode>>(File.ReadAllText(configPath));
            //}
            //catch (Exception e)
            //{
            //    Debug.Log(e);
            //}
        }
    }
}

public static class HotKeys
{
    public static HotKey Restart = new HotKey("Restart", KeyCode.G);
    public static HotKey Screenshot = new HotKey("Screenshot", KeyCode.F5);

    public static void Update()
    {
        //Restarts the game and resets all stats if you are in MP
        if (Input.GetKeyDown(Restart.Key))
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
                    FengGameManagerMKII.instance.photonView.RPC("Chat", PhotonTargets.All, InRoomChat.RCLine("MasterClient has restarted the game."), "");
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

        //Captures a screenshot
        if (Input.GetKeyDown(Screenshot.Key))
        {
            var path = Application.dataPath + "/Screenshots";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            Application.CaptureScreenshot(Application.dataPath + "/Screenshoots/Screenshot_" + DateTime.Now.ToString("yyyy:mm:dd:hh:mm:ss").Replace(":", "-") + ".png");
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            Restart.Key = KeyCode.Alpha0;
        }
    }
}