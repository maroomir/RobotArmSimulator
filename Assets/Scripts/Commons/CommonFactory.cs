using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class CommonFactory
{
    public static KinematicsCalculator RobotKinematics { get; set; }
    
    public static bool VerifyDirectory(string strPath)
    {
        if (string.IsNullOrEmpty(strPath)) return false;
        if (!Directory.Exists(strPath))
        {
            Directory.CreateDirectory(strPath);
            if (Directory.Exists(strPath))
                return true;
        }
        else
            return true;

        return false;
    }

    public static bool VerifyFilePath(string strPath, bool bCreateFile = true)
    {
        if (string.IsNullOrEmpty(strPath)) return false;
        FileInfo fi = new FileInfo(strPath);
        if (!VerifyDirectory(fi.DirectoryName))
            return false;

        try
        {
            if (!fi.Exists)
            {
                if (!bCreateFile) return false;
                FileStream fs = fi.Create();
                fs.Close();
                return true;
            }
            else
                return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        return false;
    }
    
    public static bool IsInputKeys(IEnumerable<KeyCode> pObserves)
    {
        foreach (KeyCode eKey in pObserves)
            if (Input.GetKey(eKey))
                return true;
        return false;
    }

    public static KeyCode GetInputKeys(IEnumerable<KeyCode> pObserves)
    {
        foreach (KeyCode eKey in pObserves)
            if (Input.GetKey(eKey))
                return eKey;
        return KeyCode.None;
    }

    public static float[] AddByElement(this float[] pArray1, float[] pArray2)
    {
        float[] pResultArray = new float[pArray1.Length];
        for (int i = 0; i < pResultArray.Length; i++)
            pResultArray[i] = pArray1[i] + pArray2[i];
        return pResultArray;
    }

    public static float[] SubtractByElement(this float[] pArray1, float[] pArray2)
    {
        float[] pResultArray = new float[pArray1.Length];
        for (int i = 0; i < pResultArray.Length; i++)
            pResultArray[i] = pArray1[i] + pArray2[i];
        return pResultArray;
    }

    public static GameObject[] Grouping(this GameObject[] pArray1, GameObject[] pArray2)
    {
        int nLength = pArray1.Length + pArray2.Length;
        GameObject[] pResultArray = new GameObject[nLength];
        for (int i = 0; i < nLength; i++)
        {
            pResultArray[i] = (i < pArray1.Length) ? pArray1[i] : pArray2[i - pArray1.Length];
        }

        return pResultArray;
    }
}