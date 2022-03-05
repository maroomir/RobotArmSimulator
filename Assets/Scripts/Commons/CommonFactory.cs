using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class CommonFactory
{
    public static KinematicsCalculator RobotKinematics { get; set; }
    
    // https://github.com/maroomir/YoonFactory.Net/blob/master/YoonFile/FileFactory.cs
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

    // https://github.com/maroomir/YoonFactory.Net/blob/master/YoonFile/FileFactory.cs
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

    // https://github.com/maroomir/YoonFactory.Net/blob/master/YoonFile/FileFactory.cs
    public static bool VerifyFileExtension(string strPath, string strExt, bool bCreateFile = true)
    {
        if (!VerifyFilePath(strPath, bCreateFile)) return false;
        FileInfo pFile = new FileInfo(strPath);
        return string.Equals(pFile.Extension, strExt, StringComparison.CurrentCultureIgnoreCase);
    }
    
    public static bool IsInputKeys(IEnumerable<KeyCode> pObserves)
    {
        foreach (KeyCode eKey in pObserves)
            if (Input.GetKey(eKey))
                return true;
        return false;
    }

    public static KeyCode GetInputKey(IEnumerable<KeyCode> pObserves)
    {
        foreach (KeyCode eKey in pObserves)
            if (Input.GetKey(eKey))
                return eKey;
        return KeyCode.None;
    }
    
    public static KeyCode GetInputKeyDown(IEnumerable<KeyCode> pObserves)
    {
        foreach (KeyCode eKey in pObserves)
            if (Input.GetKeyDown(eKey))
                return eKey;
        return KeyCode.None;
    }

    public static KeyCode[] GetInputKeys(IEnumerable<KeyCode> pObserves)
    {
        List<KeyCode> pResult = new List<KeyCode>();
        foreach (KeyCode eKey in pObserves)
            if (Input.GetKey(eKey))
                pResult.Add(eKey);
        return pResult.ToArray();
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
            pResultArray[i] = pArray1[i] - pArray2[i];
        return pResultArray;
    }

    public static bool EqualByElement(this float[] pArray1, float[] pArray2)
    {
        bool bSame = true;
        for (int i = 0; i < pArray1.Length; i++)
            if (Math.Abs(pArray1[i] - pArray2[i]) > 0.0001F)
                bSame = false;

        return bSame;
    }

    public static float[] FillAll(this float[] pArray, float fValue)
    {
        for (int i = 0; i < pArray.Length; i++)
            pArray[i] = fValue;
        return pArray;
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