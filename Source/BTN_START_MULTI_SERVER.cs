﻿using System;
using UnityEngine;

public class BTN_START_MULTI_SERVER : MonoBehaviour
{
    private void OnClick()
    {
        string text = GameObject.Find("InputServerName").GetComponent<UIInput>().label.text;
        int maxPlayers = int.Parse(GameObject.Find("InputMaxPlayer").GetComponent<UIInput>().label.text);
        int num2 = int.Parse(GameObject.Find("InputMaxTime").GetComponent<UIInput>().label.text);
        string selection = GameObject.Find("PopupListMap").GetComponent<UIPopupList>().selection;
        string str3 = !GameObject.Find("CheckboxHard").GetComponent<UICheckbox>().isChecked ? (!GameObject.Find("CheckboxAbnormal").GetComponent<UICheckbox>().isChecked ? "normal" : "abnormal") : "hard";
        string str4 = string.Empty;
        if (IN_GAME_MAIN_CAMERA.dayLight == DayLight.Day)
        {
            str4 = "day";
        }
        if (IN_GAME_MAIN_CAMERA.dayLight == DayLight.Dawn)
        {
            str4 = "dawn";
        }
        if (IN_GAME_MAIN_CAMERA.dayLight == DayLight.Night)
        {
            str4 = "night";
        }
        string unencrypted = GameObject.Find("InputStartServerPWD").GetComponent<UIInput>().label.text;
        PhotonNetwork.CreateRoom(string.Concat(new object[] { text, "`", selection, "`", str3, "`", num2, "`", str4, "`", (unencrypted.Length > 0 ? new SimpleAES().Encrypt(unencrypted) : ""), "`", UnityEngine.Random.Range(0, 0xc350) }), new RoomOptions() { isOpen = true, isVisible = true, maxPlayers = maxPlayers }, null);
    }
}

