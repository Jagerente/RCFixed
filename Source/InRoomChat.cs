using ExitGames.Client.Photon;
using System;
using System.Collections.Generic;
using UnityEngine;

public class InRoomChat : Photon.MonoBehaviour
{
    private bool AlignBottom = true;
    internal static InRoomChat Chat;
    public static readonly string ChatRPC = "Chat";
    public static Rect GuiRect = new Rect(0f, 100f, 300f, 470f);
    public static Rect GuiRect2 = new Rect(30f, 575f, 300f, 25f);
    private string inputLine = string.Empty;
    public bool IsVisible = true;
    public static List<string> messages = new List<string>();
    private Vector2 scrollPos = Vector2.zero;

    public void AddLine(string newLine)
    {
        messages.Add(newLine);
    }

    public void AddLineRC(string newline)
    {
        messages.Add(RCLine(newline));
    }

    public static void Message(string str)
    {
        messages.Add(str);
    }

    private void Awake()
    {
        Chat = this;
    }

    private void commandSwitch(string[] args)
    {
        switch (args[0])
        {
            case "pos":
                FengGameManagerMKII.ShowPos();
                break;

            case "ban":
                {
                    int num8 = Convert.ToInt32(args[1]);
                    if (num8 == PhotonNetwork.player.ID)
                    {
                        this.AddLine("Error:can't kick yourself.");
                    }
                    else if (!(FengGameManagerMKII.OnPrivateServer || PhotonNetwork.isMasterClient))
                    {
                        FengGameManagerMKII.instance.photonView.RPC("Chat", PhotonTargets.All, new object[] { "/kick #" + Convert.ToString(num8), LoginFengKAI.player.name });
                    }
                    else
                    {
                        foreach (PhotonPlayer player3 in PhotonNetwork.playerList)
                        {
                            if (num8 == player3.ID)
                            {
                                if (FengGameManagerMKII.OnPrivateServer)
                                {
                                    FengGameManagerMKII.instance.kickPlayerRC(player3, true, "");
                                }
                                else if (PhotonNetwork.isMasterClient)
                                {
                                    FengGameManagerMKII.instance.kickPlayerRC(player3, true, "");
                                    FengGameManagerMKII.instance.photonView.RPC("Chat", PhotonTargets.All, new object[] { RCLine(RCextensions.returnStringFromObject(player3.customProperties[PhotonPlayerProperty.name]) + " has been banned from the server!"), string.Empty });
                                }
                            }
                        }
                        if (PhotonPlayer.Find(num8)==null)
                        {
                            this.AddLine("error:no such player.");
                        }
                    }
                }
                return;

            case "cloth":
                AddLine(ClothFactory.GetDebugInfo());
                return;

            case "aso":
                if (!PhotonNetwork.isMasterClient)
                {
                    AddLineRC("Error: not masterclient");
                    return;
                }
                switch (args[1])
                {
                    case "kdr":
                        RCSettings.asoPreservekdr = RCSettings.asoPreservekdr == 0 ? 1 : 0;
                        AddLineRC("KDRs will " + (RCSettings.asoPreservekdr == 1 ? " " : "not ") +"be preserved from disconnects.");
                        break;
                    case "racing":
                        RCSettings.racingStatic= RCSettings.racingStatic == 0 ? 1 : 0;
                        AddLineRC("Racing will " + (RCSettings.asoPreservekdr == 1 ? " " : "not ") + "end on finish.");
                        break;
                }
                return;

            case "pause":
            case "unpause":
                if (!PhotonNetwork.isMasterClient)
                {
                    AddLineRC("Error: not masterclient");
                    return;
                }
                FengGameManagerMKII.instance.SetPause();
                return;

            case "checklevel":
                foreach (PhotonPlayer player in PhotonNetwork.playerList)
                {
                    AddLine(RCextensions.returnStringFromObject(player.customProperties[PhotonPlayerProperty.currentLevel]));
                }
                return;
            case "isrc":
                AddLineRC((FengGameManagerMKII.masterRC ? "is" : "not") + " RC");
                return;

            case "ignorelist":
                foreach (int id in FengGameManagerMKII.ignoreList)
                {
                    AddLine(id.ToString());
                }
                return;

            case "room":
                if (!PhotonNetwork.isMasterClient)
                {
                    AddLineRC("Error: not masterclient");
                    return;
                }
                int roomValue = Convert.ToInt32(args[2]);
                switch (args[1])
                {
                    case "max":
                        PhotonNetwork.room.maxPlayers = roomValue;
                        FengGameManagerMKII.instance.photonView.RPC("Chat", PhotonTargets.All, new object[] { RCLine("Max players changed to " + roomValue + "!"), "" });
                        break;
                    case "time":
                        FengGameManagerMKII.instance.addTime(roomValue);
                        FengGameManagerMKII.instance.photonView.RPC("Chat", PhotonTargets.All, new object[] { RCLine(roomValue + " seconds added to the clock."), "" });
                        break;
                }
                return;

            case "resetkd":
                PhotonNetwork.player.SetCustomProperties(new Hashtable() { { "kills", 0 }, { "deaths", 0 }, { "max_dmg", 0 }, { "total_dmg", 0 } });
                AddLineRC("Your stats have been reset.");
                return;

            case "resetkdall":
                {
                    if (!PhotonNetwork.isMasterClient)
                    {
                        AddLine("Error: not masterclient");
                        return;
                    }
                    Hashtable hash = new Hashtable() { { "kills", 0 }, { "deaths", 0 }, { "max_dmg", 0 }, { "total_dmg", 0 } };
                    foreach (PhotonPlayer player in PhotonNetwork.playerList)
                    {
                        player.SetCustomProperties(hash);
                    }
                    FengGameManagerMKII.instance.photonView.RPC("Chat", PhotonTargets.All, new object[] { RCLine("All stats have been reset"), "" });
                }
                return;

            case "revive":
                {
                    PhotonPlayer player = PhotonPlayer.Find(Convert.ToInt32(args[1]));
                    FengGameManagerMKII.instance.photonView.RPC("respawnHeroInNewRound", player, new object[0]);
                    AddLineRC("Player [" + player.ID + "] has been revived");
                }
                return;

            case "reviveall":
                FengGameManagerMKII.instance.photonView.RPC("respawnHeroInNewRound", PhotonTargets.All, new object[0]);
                FengGameManagerMKII.instance.photonView.RPC("Chat", PhotonTargets.All, new object[] { RCLine("All player have been revived"), "" });
                return;

            case "pm":
                {
                    PhotonPlayer player = PhotonPlayer.Find(Convert.ToInt32(args[1]));
                    string msg = "";
                    for(int i = 2; i < args.Length; i++)
                    {
                        msg += args[i] + (i == args.Length-1 ? "" : " ");
                    }
                    string myName = RCextensions.returnStringFromObject(PhotonNetwork.player.customProperties["name"]).hexColor();
                    string sendName = "";
                    switch (RCextensions.returnIntFromObject(PhotonNetwork.player.customProperties["RCteam"]))
                    {
                        case 1:
                            sendName = "<color=cyan>" + myName + "</color>";
                            break;
                        case 2:
                            sendName = "<color=magenta>" + myName + "</color>";
                            break;
                        default:
                            sendName = myName;
                            break;
                    }
                    FengGameManagerMKII.instance.photonView.RPC("ChatPM", player, new object[] { sendName, msg });
                    AddLine(RCLine("Sent PM [" + player.ID + "] " + msg));
                }
                return;
                    
            case "team":
                if(RCSettings.teamMode != 1)
                {
                    AddLineRC("Error: teams are locked or disabled");
                    return;
                }
                int teamValue = 0;
                string newTeamName = "Individuals";
                switch (args[1])
                {
                    case "0":
                    case "individual":
                        break;
                    case "1":
                    case "cyan":
                        teamValue = 1;
                        newTeamName = "Cyan";
                        break;
                    case "2":
                    case "magenta":
                        teamValue = 2;
                        newTeamName = "Magenta";
                        break;
                    default:
                        AddLineRC("Error: invalid team code/name.(use 0, 1, 2)");
                        return;
                }
                FengGameManagerMKII.instance.photonView.RPC("setTeamRPC", PhotonNetwork.player, new object[] { teamValue });
                AddLineRC("You have joined to team " + newTeamName);
                foreach (object obj in FengGameManagerMKII.instance.getPlayers())
                {
                    HERO her = (HERO)obj;
                    if (her.photonView.isMine)
                    {
                        her.markDie();
                        her.photonView.RPC("netDie2", PhotonTargets.All, new object[] { -1, "Team Switch" });
                        break;
                    }
                }
                return;

            case "kick":
                { 
                    int num8 = Convert.ToInt32(args[1]);
                    if (num8 == PhotonNetwork.player.ID)
                    {
                        this.AddLine("error:can't kick yourself.");
                    }
                    else if (!(FengGameManagerMKII.OnPrivateServer || PhotonNetwork.isMasterClient))
                    {
                        FengGameManagerMKII.instance.photonView.RPC("Chat", PhotonTargets.All, new object[] { "/kick #" + Convert.ToString(num8), LoginFengKAI.player.name });
                    }
                    else
                    {
                        foreach (PhotonPlayer player3 in PhotonNetwork.playerList)
                        {
                            if (num8 == player3.ID)
                            {
                                if (FengGameManagerMKII.OnPrivateServer)
                                {
                                    FengGameManagerMKII.instance.kickPlayerRC(player3, false, "");
                                }
                                else if (PhotonNetwork.isMasterClient)
                                {
                                    FengGameManagerMKII.instance.kickPlayerRC(player3, false, "");
                                    FengGameManagerMKII.instance.photonView.RPC("Chat", PhotonTargets.All, new object[] { RCLine(RCextensions.returnStringFromObject(player3.customProperties[PhotonPlayerProperty.name]) + " has been kicked from the server!"), string.Empty });
                                }
                            }
                        }
                        if (PhotonPlayer.Find(num8) == null)
                        {
                            this.AddLine("error:no such player.");
                        }
                    }
                }
                return;

            case "restart":
                if (!PhotonNetwork.isMasterClient)
                {
                    AddLineRC("Error: not masterclient");
                    return;
                }
                FengGameManagerMKII.instance.restartGame(false);
                FengGameManagerMKII.instance.photonView.RPC("Chat", PhotonTargets.All, new object[] { RCLine("MasterClient has restarted the game."), "" });
                return;

            case "specmode":
                if (((int)FengGameManagerMKII.settings[0xf5]) == 0)
                {
                    FengGameManagerMKII.settings[0xf5] = 1;
                    FengGameManagerMKII.instance.EnterSpecMode(true);
                    this.AddLineRC("You have entered spectator mode.");
                }
                else
                {
                    FengGameManagerMKII.settings[0xf5] = 0;
                    FengGameManagerMKII.instance.EnterSpecMode(false);
                    this.AddLineRC("You have exited spectator mode.");
                }
                return;

            case "fov":
                int num6 = Convert.ToInt32(args[1]);
                Camera.main.fieldOfView = num6;
                this.AddLineRC("Field of vision set to " + num6.ToString() + ".");
                return;

            case "colliders":
                int num7 = 0;
                foreach (TITAN titan in FengGameManagerMKII.instance.getTitans())
                {
                    if (titan.myTitanTrigger.isCollide)
                    {
                        num7++;
                    }
                }
                AddLine(num7.ToString());
                return;

            case "spectate":
                {
                    int num8 = Convert.ToInt32(args[1]);
                    foreach (GameObject obj5 in GameObject.FindGameObjectsWithTag("Player"))
                    {
                        if (obj5.GetPhotonView().owner.ID == num8)
                        {
                            Camera.main.GetComponent<IN_GAME_MAIN_CAMERA>().setMainObject(obj5, true, false);
                            Camera.main.GetComponent<IN_GAME_MAIN_CAMERA>().setSpectorMode(false);
                        }
                    }
                }
                return;

            case "rules":
                {
                    AddLineRC("Currently activated gamemodes:");
                    if (RCSettings.bombMode > 0)
                    {
                        this.AddLineRC("Bomb mode is on.");
                    }
                    if (RCSettings.teamMode > 0)
                    {
                        if (RCSettings.teamMode == 1)
                        {
                            this.AddLineRC("Team mode is on (no sort).");
                        }
                        else if (RCSettings.teamMode == 2)
                        {
                            this.AddLineRC("Team mode is on (sort by size).");
                        }
                        else if (RCSettings.teamMode == 3)
                        {
                            this.AddLineRC("Team mode is on (sort by skill).");
                        }
                    }
                    if (RCSettings.pointMode > 0)
                    {
                        this.AddLineRC("Point mode is on (" + Convert.ToString(RCSettings.pointMode) + ").");
                    }
                    if (RCSettings.disableRock > 0)
                    {
                        this.AddLineRC("Punk Rock-Throwing is disabled.");
                    }
                    if (RCSettings.spawnMode > 0)
                    {
                        this.AddLineRC("Custom spawn rate is on (" + RCSettings.nRate.ToString("F2") + "% Normal, " + RCSettings.aRate.ToString("F2") + "% Abnormal, " + RCSettings.jRate.ToString("F2") + "% Jumper, " + RCSettings.cRate.ToString("F2") + "% Crawler, " + RCSettings.pRate.ToString("F2") + "% Punk");
                    }
                    if (RCSettings.explodeMode > 0)
                    {
                        this.AddLineRC("Titan explode mode is on (" + Convert.ToString(RCSettings.explodeMode) + ").");
                    }
                    if (RCSettings.healthMode > 0)
                    {
                        this.AddLineRC("Titan health mode is on (" + Convert.ToString(RCSettings.healthLower) + "-" + Convert.ToString(RCSettings.healthUpper) + ").");
                    }
                    if (RCSettings.infectionMode > 0)
                    {
                        this.AddLineRC("Infection mode is on (" + Convert.ToString(RCSettings.infectionMode) + ").");
                    }
                    if (RCSettings.damageMode > 0)
                    {
                        this.AddLineRC("Minimum nape damage is on (" + Convert.ToString(RCSettings.damageMode) + ").");
                    }
                    if (RCSettings.moreTitans > 0)
                    {
                        this.AddLineRC("Custom titan # is on (" + Convert.ToString(RCSettings.moreTitans) + ").");
                    }
                    if (RCSettings.sizeMode > 0)
                    {
                        this.AddLineRC("Custom titan size is on (" + RCSettings.sizeLower.ToString("F2") + "," + RCSettings.sizeUpper.ToString("F2") + ").");
                    }
                    if (RCSettings.banEren > 0)
                    {
                        this.AddLineRC("Anti-Eren is on. Using Titan eren will get you kicked.");
                    }
                    if (RCSettings.waveModeOn == 1)
                    {
                        this.AddLineRC("Custom wave mode is on (" + Convert.ToString(RCSettings.waveModeNum) + ").");
                    }
                    if (RCSettings.friendlyMode > 0)
                    {
                        this.AddLineRC("Friendly-Fire disabled. PVP is prohibited.");
                    }
                    if (RCSettings.pvpMode > 0)
                    {
                        if (RCSettings.pvpMode == 1)
                        {
                            this.AddLineRC("AHSS/Blade PVP is on (team-based).");
                        }
                        else if (RCSettings.pvpMode == 2)
                        {
                            this.AddLineRC("AHSS/Blade PVP is on (FFA).");
                        }
                    }
                    if (RCSettings.maxWave > 0)
                    {
                        this.AddLineRC("Max Wave set to " + RCSettings.maxWave.ToString());
                    }
                    if (RCSettings.horseMode > 0)
                    {
                        this.AddLineRC("Horses are enabled.");
                    }
                    if (RCSettings.ahssReload > 0)
                    {
                        this.AddLineRC("AHSS Air-Reload disabled.");
                    }
                    if (RCSettings.punkWaves > 0)
                    {
                        this.AddLineRC("Punk override every 5 waves enabled.");
                    }
                    if (RCSettings.endlessMode > 0)
                    {
                        this.AddLineRC("Endless Respawn is enabled (" + RCSettings.endlessMode.ToString() + " seconds).");
                    }
                    if (RCSettings.globalDisableMinimap > 0)
                    {
                        this.AddLineRC("Minimap are disabled.");
                    }
                    if (RCSettings.motd != string.Empty)
                    {
                        this.AddLineRC("MOTD:" + RCSettings.motd);
                    }
                    if (RCSettings.deadlyCannons > 0)
                    {
                        this.AddLineRC("Cannons will kill humans.");
                    }
                }
                return;

            default:
                return;
        }
    }

    public void OnGUI()
    {
        int num4;
        if (!this.IsVisible || (PhotonNetwork.connectionStateDetailed != PeerStates.Joined))
        {
            return;
        }
        if (Event.current.type == EventType.KeyDown)
        {
            if ((((Event.current.keyCode == KeyCode.Tab) || (Event.current.character == '\t')) && !IN_GAME_MAIN_CAMERA.isPausing) && (FengGameManagerMKII.inputRC.humanKeys[InputCodeRC.chat] != KeyCode.Tab))
            {
                Event.current.Use();
                goto Label_219C;
            }
        }
        else if ((Event.current.type == EventType.KeyUp) && (((Event.current.keyCode != KeyCode.None) && (Event.current.keyCode == FengGameManagerMKII.inputRC.humanKeys[InputCodeRC.chat])) && (GUI.GetNameOfFocusedControl() != "ChatInput")))
        {
            this.inputLine = string.Empty;
            GUI.FocusControl("ChatInput");
            goto Label_219C;
        }
        if ((Event.current.type == EventType.KeyDown) && ((Event.current.keyCode == KeyCode.KeypadEnter) || (Event.current.keyCode == KeyCode.Return)))
        {
            if (!string.IsNullOrEmpty(this.inputLine))
            {
                string str2;
                if (this.inputLine == "\t")
                {
                    this.inputLine = string.Empty;
                    GUI.FocusControl(string.Empty);
                    return;
                }
                if (FengGameManagerMKII.RCEvents.ContainsKey("OnChatInput"))
                {
                    string key = (string) FengGameManagerMKII.RCVariableNames["OnChatInput"];
                    if (FengGameManagerMKII.stringVariables.ContainsKey(key))
                    {
                        FengGameManagerMKII.stringVariables[key] = this.inputLine;
                    }
                    else
                    {
                        FengGameManagerMKII.stringVariables.Add(key, this.inputLine);
                    }
                    ((RCEvent)FengGameManagerMKII.RCEvents["OnChatInput"]).checkEvent();
                }
                if (!this.inputLine.StartsWith("/"))
                {
                    str2 = RCextensions.returnStringFromObject(PhotonNetwork.player.customProperties[PhotonPlayerProperty.name]).hexColor();
                    if (str2 == string.Empty)
                    {
                        str2 = RCextensions.returnStringFromObject(PhotonNetwork.player.customProperties[PhotonPlayerProperty.name]);
                        if (PhotonNetwork.player.customProperties[PhotonPlayerProperty.RCteam] != null)
                        {
                            if (RCextensions.returnIntFromObject(PhotonNetwork.player.customProperties[PhotonPlayerProperty.RCteam]) == 1)
                            {
                                str2 = "<color=#00FFFF>" + str2 + "</color>";
                            }
                            else if (RCextensions.returnIntFromObject(PhotonNetwork.player.customProperties[PhotonPlayerProperty.RCteam]) == 2)
                            {
                                str2 = "<color=#FF00FF>" + str2 + "</color>";
                            }
                        }
                    }
                    object[] parameters = new object[] { this.inputLine, str2 };
                    FengGameManagerMKII.instance.photonView.RPC("Chat", PhotonTargets.All, parameters);
                }
                else
                {
                    commandSwitch(this.inputLine.Remove(0, 1).Split(' '));
                }
                this.inputLine = string.Empty;
                GUI.FocusControl(string.Empty);
                return;
            }
            this.inputLine = "\t";
            GUI.FocusControl("ChatInput");
        }
    Label_219C:
        GUI.SetNextControlName(string.Empty);
        GUILayout.BeginArea(GuiRect);
        GUILayout.FlexibleSpace();
        string text = string.Empty;
        if (messages.Count < 15)
        {
            for (num4 = 0; num4 < messages.Count; num4++)
            {
                text = text + messages[num4] + "\n";
            }
        }
        else
        {
            for (int i = messages.Count - 15; i < messages.Count; i++)
            {
                text = text + messages[i] + "\n";
            }
        }
        GUILayout.Label(text, new GUILayoutOption[0]);
        GUILayout.EndArea();
        GUILayout.BeginArea(GuiRect2);
        GUILayout.BeginHorizontal(new GUILayoutOption[0]);
        GUI.SetNextControlName("ChatInput");
        this.inputLine = GUILayout.TextField(this.inputLine, new GUILayoutOption[0]);
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    public static string RCLine(string line)
    {
        return "<color=#FFC000>" + line + "</color>";
    }

    public void setPosition()
    {
        if (this.AlignBottom)
        {
            GuiRect = new Rect(0f, (float) (Screen.height - 500), 300f, 470f);
            GuiRect2 = new Rect(30f, (float) ((Screen.height - 300) + 0x113), 300f, 25f);
        }
    }

    public void Start()
    {
        this.setPosition();
    }
}

