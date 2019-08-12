using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class LoginFengKAI : MonoBehaviour
{
    private string ChangeGuildURL = "http://aotskins.com/version/guild.php";
    private string ChangePasswordURL = "http://fenglee.com/game/aog/change_password.php";
    private string CheckUserURL = "http://aotskins.com/version/login.php";
    private string ForgetPasswordURL = "http://fenglee.com/game/aog/forget_password.php";
    public string formText = string.Empty;
    private string GetInfoURL = "http://aotskins.com/version/getinfo.php";
    public PanelLoginGroupManager loginGroup;
    public GameObject output;
    public GameObject output2;
    public GameObject panelChangeGUILDNAME;
    public GameObject panelChangePassword;
    public GameObject panelForget;
    public GameObject panelLogin;
    public GameObject panelRegister;
    public GameObject panelStatus;
    public static PlayerInfoPHOTON player;
    private static string playerGUILDName = string.Empty;
    private static string playerName = string.Empty;
    private static string playerPassword = string.Empty;
    private string RegisterURL = "http://fenglee.com/game/aog/signup_check.php";

    public void cGuild(string name)
    {
        if (playerName == string.Empty)
        {
            this.logout();
            NGUITools.SetActive(this.panelChangeGUILDNAME, false);
            NGUITools.SetActive(this.panelLogin, true);
            this.output.GetComponent<UILabel>().text = "Please sign in.";
        }
        else
        {
            base.StartCoroutine(this.changeGuild(name));
        }
    }

    
    private IEnumerator changeGuild(string name)
    {
        WWWForm form = new WWWForm();
        form.AddField("name", LoginFengKAI.playerName);
        form.AddField("guildname", name);
        WWW w = new WWW(this.ChangeGuildURL, form);
        yield return (object)w;
        if (w.error != null)
        {
            MonoBehaviour.print(w.error);
        }
        else
        {
            this.output.GetComponent<UILabel>().text = w.text;
            if (w.text.Contains("Guild name set."))
            {
                NGUITools.SetActive(this.panelChangeGUILDNAME, false);
                NGUITools.SetActive(this.panelStatus, true);
                base.StartCoroutine(this.getInfo());
            }
            w.Dispose();
        }
    }

    
    private IEnumerator changePassword(string oldpassword, string password, string password2)
    {
        WWWForm form = new WWWForm();
        form.AddField("userid", LoginFengKAI.playerName);
        form.AddField("old_password", oldpassword);
        form.AddField("password", password);
        form.AddField("password2", password2);
        WWW w = new WWW(this.ChangePasswordURL, form);
        yield return (object)w;
        if (w.error != null)
        {
            MonoBehaviour.print(w.error);
        }
        else
        {
            this.output.GetComponent<UILabel>().text = w.text;
            if (w.text.Contains("Thanks, Your password changed successfully"))
            {
                NGUITools.SetActive(this.panelChangePassword, false);
                NGUITools.SetActive(this.panelLogin, true);
            }
            w.Dispose();
        }
    }

    private void clearCOOKIE()
    {
        playerName = string.Empty;
        playerPassword = string.Empty;
    }

    public void cpassword(string oldpassword, string password, string password2)
    {
        if (playerName == string.Empty)
        {
            this.logout();
            NGUITools.SetActive(this.panelChangePassword, false);
            NGUITools.SetActive(this.panelLogin, true);
            this.output.GetComponent<UILabel>().text = "Please sign in.";
        }
        else
        {
            base.StartCoroutine(this.changePassword(oldpassword, password, password2));
        }
    }

    
    private IEnumerator ForgetPassword(string email)
    {
        WWWForm form = new WWWForm();
        form.AddField("email", email);
        WWW w = new WWW(this.ForgetPasswordURL, form);
        yield return (object)w;
        if (w.error != null)
        {
            MonoBehaviour.print(w.error);
        }
        else
        {
            this.output.GetComponent<UILabel>().text = w.text;
            w.Dispose();
            NGUITools.SetActive(this.panelForget, false);
            NGUITools.SetActive(this.panelLogin, true);
        }
        this.clearCOOKIE();
    }

    
    private IEnumerator getInfo()
    {
        WWWForm form = new WWWForm();
        form.AddField("userid", LoginFengKAI.playerName);
        form.AddField("password", LoginFengKAI.playerPassword);
        WWW w = new WWW(this.GetInfoURL, form);
        yield return (object)w;
        if (w.error != null)
        {
            MonoBehaviour.print(w.error);
        }
        else
        {
            if (w.text.Contains("Error,please sign in again."))
            {
                NGUITools.SetActive(this.panelLogin, true);
                NGUITools.SetActive(this.panelStatus, false);
                this.output.GetComponent<UILabel>().text = w.text;
                LoginFengKAI.playerName = string.Empty;
                LoginFengKAI.playerPassword = string.Empty;
            }
            else
            {
                string[] result = w.text.Split('|');
                LoginFengKAI.playerGUILDName = result[0];
                this.output2.GetComponent<UILabel>().text = result[1];
                LoginFengKAI.player.name = LoginFengKAI.playerName;
                LoginFengKAI.player.guildname = LoginFengKAI.playerGUILDName;
            }
            w.Dispose();
        }
    }

    public void login(string name, string password)
    {
        base.StartCoroutine(this.Login(name, password));
    }

    
    private IEnumerator Login(string name, string password)
    {

        WWWForm form = new WWWForm();
        form.AddField("userid", name);
        form.AddField("password", password);
        form.AddField("version", UIMainReferences.version);
        WWW w = new WWW(this.CheckUserURL, form);
        yield return (object)w;
        this.clearCOOKIE();
        if (w.error != null)
        {
            MonoBehaviour.print(w.error);
        }
        else
        {
            this.output.GetComponent<UILabel>().text = w.text;
            this.formText = w.text;
            w.Dispose();
            if (this.formText.Contains("Welcome back") && this.formText.Contains("(^o^)/~"))
            {
                NGUITools.SetActive(this.panelLogin, false);
                NGUITools.SetActive(this.panelStatus, true);
                LoginFengKAI.playerName = name;
                LoginFengKAI.playerPassword = password;
                base.StartCoroutine(this.getInfo());
            }
        }
    }

    public void logout()
    {
        this.clearCOOKIE();
        player = new PlayerInfoPHOTON();
        player.initAsGuest();
        this.output.GetComponent<UILabel>().text = "Welcome," + player.name;
    }

    
    private IEnumerator Register(string name, string password, string password2, string email)
    {
        WWWForm form = new WWWForm();
        form.AddField("userid", name);
        form.AddField("password", password);
        form.AddField("password2", password2);
        form.AddField("email", email);
        WWW w = new WWW(this.RegisterURL, form);
        yield return (object)w;
        if (w.error != null)
        {
            MonoBehaviour.print(w.error);
        }
        else
        {
            this.output.GetComponent<UILabel>().text = w.text;
            if (w.text.Contains("Final step,to activate your account, please click the link in the activation email"))
            {
                NGUITools.SetActive(this.panelRegister, false);
                NGUITools.SetActive(this.panelLogin, true);
            }
            w.Dispose();
        }
        this.clearCOOKIE();
    }

    public void resetPassword(string email)
    {
        base.StartCoroutine(this.ForgetPassword(email));
    }

    public void signup(string name, string password, string password2, string email)
    {
        base.StartCoroutine(this.Register(name, password, password2, email));
    }

    private void Start()
    {
        if (player == null)
        {
            player = new PlayerInfoPHOTON();
            player.initAsGuest();
        }
        if (playerName != string.Empty)
        {
            NGUITools.SetActive(this.panelLogin, false);
            NGUITools.SetActive(this.panelStatus, true);
            base.StartCoroutine(this.getInfo());
        }
        else
        {
            this.output.GetComponent<UILabel>().text = "Welcome," + player.name;
        }
    }
}

