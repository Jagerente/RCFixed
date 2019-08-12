using ExitGames.Client.Photon;
using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class Extensions
{
    public static bool AlmostEquals(this float target, float second, float floatDiff)
    {
        return (Mathf.Abs((float) (target - second)) < floatDiff);
    }

    public static bool AlmostEquals(this Quaternion target, Quaternion second, float maxAngle)
    {
        return (Quaternion.Angle(target, second) < maxAngle);
    }

    public static bool AlmostEquals(this Vector2 target, Vector2 second, float sqrMagnitudePrecision)
    {
        Vector2 vector = target - second;
        return (vector.sqrMagnitude < sqrMagnitudePrecision);
    }

    public static bool AlmostEquals(this Vector3 target, Vector3 second, float sqrMagnitudePrecision)
    {
        Vector3 vector = target - second;
        return (vector.sqrMagnitude < sqrMagnitudePrecision);
    }

    public static bool Contains(this int[] target, int nr)
    {
        if (target != null)
        {
            for (int i = 0; i < target.Length; i++)
            {
                if (target[i] == nr)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public static PhotonView GetPhotonView(this GameObject go)
    {
        return go.GetComponent<PhotonView>();
    }

    public static PhotonView[] GetPhotonViewsInChildren(this GameObject go)
    {
        return go.GetComponentsInChildren<PhotonView>(true);
    }

    public static void Merge(this IDictionary target, IDictionary addHash)
    {
        if (addHash == null || target.Equals(addHash))
        {
            return;
        }
        foreach (object current in addHash.Keys)
        {
            target[current] = addHash[current];
        }
    }


    // Extensions
    public static void MergeStringKeys(this IDictionary target, IDictionary addHash)
    {
        if (addHash == null || target.Equals(addHash))
        {
            return;
        }
        foreach (object current in addHash.Keys)
        {
            if (current is string)
            {
                target[current] = addHash[current];
            }
        }
    }


    public static void StripKeysWithNullValues(this IDictionary original)
    {
        object[] array = new object[original.Count];
        int num = 0;
        foreach (object current in original.Keys)
        {
            array[num++] = current;
        }
        for (int i = 0; i < array.Length; i++)
        {
            object key = array[i];
            if (original[key] == null)
            {
                original.Remove(key);
            }
        }
    }

    public static ExitGames.Client.Photon.Hashtable StripToStringKeys(this IDictionary original)
    {
        ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
        foreach (DictionaryEntry dictionaryEntry in original)
        {
            if (dictionaryEntry.Key is string)
            {
                hashtable[dictionaryEntry.Key] = dictionaryEntry.Value;
            }
        }
        return hashtable;
    }


    public static string ToStringFull(this IDictionary origin)
    {
        return SupportClass.DictionaryToString(origin, false);
    }
}

