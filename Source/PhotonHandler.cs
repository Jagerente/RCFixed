﻿using ExitGames.Client.Photon;
using Photon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

internal class PhotonHandler : Photon.MonoBehaviour, IPhotonPeerListener
{
    public static bool AppQuits;
    internal static CloudRegionCode BestRegionCodeCurrently = CloudRegionCode.none;
    private int nextSendTickCount;
    private int nextSendTickCountOnSerialize;
    public static System.Type PingImplementation;
    private const string PlayerPrefsKey = "PUNCloudBestRegion";
    private static bool sendThreadShouldRun;
    public static PhotonHandler SP;
    public int updateInterval;
    public int updateIntervalOnSerialize;

    protected void Awake()
    {
        if (((SP != null) && (SP != this)) && (SP.gameObject != null))
        {
            UnityEngine.Object.DestroyImmediate(SP.gameObject);
        }
        SP = this;
        UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
        this.updateInterval = 0x3e8 / PhotonNetwork.sendRate;
        this.updateIntervalOnSerialize = 0x3e8 / PhotonNetwork.sendRateOnSerialize;
        StartFallbackSendAckThread();
    }

    public void DebugReturn(DebugLevel level, string message)
    {
        if (level == DebugLevel.ERROR)
        {
            UnityEngine.Debug.LogError(message);
        }
        else if (level == DebugLevel.WARNING)
        {
            UnityEngine.Debug.LogWarning(message);
        }
        else if ((level == DebugLevel.INFO) && (PhotonNetwork.logLevel >= PhotonLogLevel.Informational))
        {
            UnityEngine.Debug.Log(message);
        }
        else if ((level == DebugLevel.ALL) && (PhotonNetwork.logLevel == PhotonLogLevel.Full))
        {
            UnityEngine.Debug.Log(message);
        }
    }

    public static bool FallbackSendAckThread()
    {
        if (sendThreadShouldRun && (PhotonNetwork.networkingPeer != null))
        {
            PhotonNetwork.networkingPeer.SendAcksOnly();
        }
        return sendThreadShouldRun;
    }

    protected void OnApplicationQuit()
    {
        AppQuits = true;
        StopFallbackSendAckThread();
        PhotonNetwork.Disconnect();
    }

    protected void OnCreatedRoom()
    {
        PhotonNetwork.networkingPeer.SetLevelInPropsIfSynced(Application.loadedLevelName);
    }

    public void OnEvent(EventData photonEvent)
    {
    }

    protected void OnJoinedRoom()
    {
        PhotonNetwork.networkingPeer.LoadLevelIfSynced();
    }

    protected void OnLevelWasLoaded(int level)
    {
        PhotonNetwork.networkingPeer.NewSceneLoaded();
        PhotonNetwork.networkingPeer.SetLevelInPropsIfSynced(Application.loadedLevelName);
    }

    public void OnOperationResponse(OperationResponse operationResponse)
    {
    }

    public void OnStatusChanged(StatusCode statusCode)
    {
    }

    protected internal static void PingAvailableRegionsAndConnectToBest()
    {
        SP.StartCoroutine(SP.PingAvailableRegionsCoroutine(true));
    }

    
    internal IEnumerator PingAvailableRegionsCoroutine(bool connectToBest)
    {
        PhotonHandler.BestRegionCodeCurrently = CloudRegionCode.none;
        while (PhotonNetwork.networkingPeer.AvailableRegions == null)
        {
            if (PhotonNetwork.connectionStateDetailed != PeerStates.ConnectingToNameServer && PhotonNetwork.connectionStateDetailed != PeerStates.ConnectedToNameServer)
            {
                UnityEngine.Debug.LogError("Call ConnectToNameServer to ping available regions.");
                yield break;
            }
            UnityEngine.Debug.Log("Waiting for AvailableRegions. State: " + PhotonNetwork.connectionStateDetailed + " Server: " + PhotonNetwork.Server + " PhotonNetwork.networkingPeer.AvailableRegions " + (PhotonNetwork.networkingPeer.AvailableRegions != null));
            yield return (object)new WaitForSeconds(0.25f);
        }
        if (PhotonNetwork.networkingPeer.AvailableRegions == null || PhotonNetwork.networkingPeer.AvailableRegions.Count == 0)
        {
            UnityEngine.Debug.LogError("No regions available. Are you sure your appid is valid and setup?");
        }
        else
        {
            PhotonPingManager pingManager = new PhotonPingManager();
            List<Region>.Enumerator enumerator = PhotonNetwork.networkingPeer.AvailableRegions.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    Region region = enumerator.Current;
                    PhotonHandler.SP.StartCoroutine(pingManager.PingSocket(region));
                }
            }
            finally
            {
                ((IDisposable)enumerator).Dispose();
            }
            while (!pingManager.Done)
            {
                yield return (object)new WaitForSeconds(0.1f);
            }
            Region best = pingManager.BestRegion;
            PhotonHandler.BestRegionCodeCurrently = best.Code;
            PhotonHandler.BestRegionCodeInPreferences = best.Code;
            UnityEngine.Debug.Log("Found best region: " + best.Code + " ping: " + best.Ping + ". Calling ConnectToRegionMaster() is: " + connectToBest);
            if (connectToBest)
            {
                PhotonNetwork.networkingPeer.ConnectToRegionMaster(best.Code);
            }
        }
    }

    public static void StartFallbackSendAckThread()
    {
        if (!sendThreadShouldRun)
        {
            sendThreadShouldRun = true;
            SupportClass.CallInBackground(new Func<bool>(PhotonHandler.FallbackSendAckThread));
        }
    }

    public static void StopFallbackSendAckThread()
    {
        sendThreadShouldRun = false;
    }

    protected void Update()
    {
        if (PhotonNetwork.networkingPeer == null)
        {
            UnityEngine.Debug.LogError("NetworkPeer broke!");
        }
        else if ((((PhotonNetwork.connectionStateDetailed != PeerStates.PeerCreated) && (PhotonNetwork.connectionStateDetailed != PeerStates.Disconnected)) && !PhotonNetwork.offlineMode) && PhotonNetwork.isMessageQueueRunning)
        {
            for (bool flag = true; PhotonNetwork.isMessageQueueRunning && flag; flag = PhotonNetwork.networkingPeer.DispatchIncomingCommands())
            {
            }
            int num = (int) (Time.realtimeSinceStartup * 1000f);
            if (PhotonNetwork.isMessageQueueRunning && (num > this.nextSendTickCountOnSerialize))
            {
                PhotonNetwork.networkingPeer.RunViewUpdate();
                this.nextSendTickCountOnSerialize = num + this.updateIntervalOnSerialize;
                this.nextSendTickCount = 0;
            }
            num = (int) (Time.realtimeSinceStartup * 1000f);
            if (num > this.nextSendTickCount)
            {
                for (bool flag2 = true; PhotonNetwork.isMessageQueueRunning && flag2; flag2 = PhotonNetwork.networkingPeer.SendOutgoingCommands())
                {
                }
                this.nextSendTickCount = num + this.updateInterval;
            }
        }
    }

    internal static CloudRegionCode BestRegionCodeInPreferences
    {
        get
        {
            string str = PlayerPrefs.GetString("PUNCloudBestRegion", string.Empty);
            if (!string.IsNullOrEmpty(str))
            {
                return Region.Parse(str);
            }
            return CloudRegionCode.none;
        }
        set
        {
            if (value == CloudRegionCode.none)
            {
                PlayerPrefs.DeleteKey("PUNCloudBestRegion");
            }
            else
            {
                PlayerPrefs.SetString("PUNCloudBestRegion", value.ToString());
            }
        }
    }
}

