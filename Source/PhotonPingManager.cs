using ExitGames.Client.Photon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PhotonPingManager
{
    public static int Attempts = 5;
    public static bool IgnoreInitialAttempt = true;
    public static int MaxMilliseconsPerPing = 800;
    private int PingsRunning;
    public bool UseNative;

    
    public IEnumerator PingSocket(Region region)
    {
        region.Ping = PhotonPingManager.Attempts * PhotonPingManager.MaxMilliseconsPerPing;
        this.PingsRunning++;
        PhotonPing ping;
        if (PhotonHandler.PingImplementation == typeof(PingNativeDynamic))
        {
            UnityEngine.Debug.Log("Using constructor for new PingNativeDynamic()");
            ping = new PingNativeDynamic();
        }
        else
        {
            ping = (PhotonPing)Activator.CreateInstance(PhotonHandler.PingImplementation);
        }
        float rttSum = 0f;
        int replyCount = 0;
        string cleanIpOfRegion = region.HostAndPort;
        int indexOfColon = cleanIpOfRegion.LastIndexOf(':');
        if (indexOfColon > 1)
        {
            cleanIpOfRegion = cleanIpOfRegion.Substring(0, indexOfColon);
        }
        cleanIpOfRegion = PhotonPingManager.ResolveHost(cleanIpOfRegion);
        for (int i = 0; i < PhotonPingManager.Attempts; i++)
        {
            bool overtime = false;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            try
            {
                ping.StartPing(cleanIpOfRegion);
            }
            catch (Exception ex)
            {
                Exception e = ex;
                UnityEngine.Debug.Log("catched: " + e);
                this.PingsRunning--;
                break;
            }
            while (!ping.Done())
            {
                if (sw.ElapsedMilliseconds < PhotonPingManager.MaxMilliseconsPerPing)
                {
                    yield return (object)0;
                    continue;
                }
                overtime = true;
                break;
            }
            int rtt = (int)sw.ElapsedMilliseconds;
            if ((!PhotonPingManager.IgnoreInitialAttempt || i != 0) && ping.Successful && !overtime)
            {
                rttSum += (float)rtt;
                replyCount++;
                region.Ping = (int)(rttSum / (float)replyCount);
            }
            yield return (object)new WaitForSeconds(0.1f);
        }
        this.PingsRunning--;
        yield return (object)null;
    }

    public static string ResolveHost(string hostName)
    {
        try
        {
            IPAddress[] hostAddresses = Dns.GetHostAddresses(hostName);
            if (hostAddresses.Length == 1)
            {
                return hostAddresses[0].ToString();
            }
            for (int i = 0; i < hostAddresses.Length; i++)
            {
                IPAddress address = hostAddresses[i];
                if (address != null)
                {
                    string str2 = address.ToString();
                    if (str2.IndexOf('.') >= 0)
                    {
                        return str2;
                    }
                }
            }
        }
        catch (Exception exception)
        {
            UnityEngine.Debug.Log("Exception caught! " + exception.Source + " Message: " + exception.Message);
        }
        return string.Empty;
    }

    public Region BestRegion
    {
        get
        {
            Region region = null;
            int ping = 0x7fffffff;
            foreach (Region region2 in PhotonNetwork.networkingPeer.AvailableRegions)
            {
                UnityEngine.Debug.Log("BestRegion checks region: " + region2);
                if ((region2.Ping != 0) && (region2.Ping < ping))
                {
                    ping = region2.Ping;
                    region = region2;
                }
            }
            return region;
        }
    }

    public bool Done
    {
        get
        {
            return (this.PingsRunning == 0);
        }
    }

}

