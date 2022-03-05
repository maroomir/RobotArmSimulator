using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class JointPoint : ITeachingPoint
{
    public string Name { get; set; }
    public int AxisNum { get; set; } 
    public int FrameCount { get; set; }
    public float[] Values { get; set; }
    public FingerStatus GripStatus { get; set; }

    public JointPoint()
    {
        // https://stackoverflow.com/questions/31069962
        // Need to have an empty constructor in JointPoint class in order to deserialize it
        Debug.Log("Serialize json file to use Newtonsoft.Json");
    }

    public JointPoint(string strName)
    {
        Name = strName;
    }

    public JointPoint(string strName, int nFrameCount, params float[] args) : this(strName)
    {
        FrameCount = nFrameCount;
        Values = args.Clone() as float[];
        AxisNum = args.Length;
    }

    public static JointPoint Home(int nAxisNum = 7) => new JointPoint("Home")
    {
        FrameCount = 100,
        AxisNum = nAxisNum,
        Values = Enumerable.Repeat(0.0F, nAxisNum).ToArray<float>(),
    };

    public static JointPoint FromPosition(string strName, params float[] args) => new JointPoint(strName)
    {
        FrameCount = 100,
        Values = args.Clone() as float[],
        AxisNum = args.Length,
    };

    public ITeachingPoint Clone() => new JointPoint(Name)
    {
        FrameCount = FrameCount,
        AxisNum = AxisNum,
        Values = Values.Clone() as float[]
    };

    public void Trim(TrimMode eMode = TrimMode.Pentagram)
    {
        for (int i = 0; i < AxisNum; i++)
        {
            Values[i] %= 360.0F;
            Values[i] = eMode switch
            {
                TrimMode.Integer => (int) Values[i] * 1.0F,
                TrimMode.Binary => (int) (Values[i] / 2.0F) * 2.0F,
                TrimMode.Pentagram => (int) (Values[i] / 5.0F) * 5.0F,
                TrimMode.Decimal => (int) (Values[i] / 10.0F) * 10.0F,
                TrimMode.QuadrantAngle => (int) (Values[i] / 45.0F) * 45.0F,
                TrimMode.HalfAngle => (int) (Values[i] / 90.0F) * 90.0F,
                _ => Values[i]
            };
        }
    }

    public string Print()
    {
        string strResult = "";
        for (int i = 0; i < AxisNum; i++)
        {
            strResult += $"J{i + 1:D1}:{Values[i]:F2}";
            if (i < AxisNum - 1) strResult += ",";
        }

        return $"{Name}=({strResult})";
    }

    public static JointPoint operator +(JointPoint pPoint1, JointPoint pPoint2) =>
        new JointPoint($"{pPoint1.Name}+{pPoint2.Name}")
        {
            FrameCount = pPoint2.FrameCount,
            Values = pPoint1.Values.AddByElement(pPoint2.Values),
            AxisNum = pPoint1.AxisNum
        };

    public static JointPoint operator +(JointPoint pPoint1, CartesianPoint pPoint2)
    {
        CartesianPoint pCartPoint1 = pPoint1.ToCartesianPoint(CommonFactory.RobotKinematics);
        CartesianPoint pResultPoint = pCartPoint1 + pPoint2;
        return pResultPoint.ToJointPoint(CommonFactory.RobotKinematics, pPoint1.Values);
    }

    public static JointPoint operator -(JointPoint pPoint1, JointPoint pPoint2) =>
        new JointPoint($"{pPoint1.Name}-{pPoint2.Name}")
        {
            FrameCount = pPoint2.FrameCount,
            Values = pPoint1.Values.SubtractByElement(pPoint2.Values),
            AxisNum = pPoint1.AxisNum
        };

    public static JointPoint operator -(JointPoint pPoint1, CartesianPoint pPoint2)
    {
        CartesianPoint pCartPoint1 = pPoint1.ToCartesianPoint(CommonFactory.RobotKinematics);
        CartesianPoint pResultPoint = pCartPoint1 - pPoint2;
        return pResultPoint.ToJointPoint(CommonFactory.RobotKinematics, pPoint1.Values);
    }
}

public class CartesianPoint : ITeachingPoint
{
    public string Name { get; set; }
    public int AxisNum { get; set; }
    public int FrameCount { get; set; }
    public float[] Values { get; set; }
    public FingerStatus GripStatus { get; set; }

    public Vector3 Position =>
        (Values.Length >= 3) ? new Vector3(Values[0], Values[1], Values[2]) : new Vector3(0.0F, 0.0F, 0.0F);

    public Vector3 Rotation =>
        (Values.Length >= 6) ? new Vector3(Values[3], Values[4], Values[5]) : new Vector3(0.0F, 0.0F, 0.0F);

    public float X => (Values.Length >= 3) ? Values[0] : 0.0F;
    public float Y => (Values.Length >= 3) ? Values[1] : 0.0F;
    public float Z => (Values.Length >= 3) ?  Values[2] : 0.0F;
    public float RX => (Values.Length >= 6) ? Values[3] : 0.0F;
    public float RY => (Values.Length >= 6) ? Values[4] : 0.0F;
    public float RZ => (Values.Length >= 6) ? Values[5] : 0.0F;

    
    public CartesianPoint()
    {
        // https://stackoverflow.com/questions/31069962
        // Need to have an empty constructor in CartesianPoint class in order to deserialize it
        Debug.Log("Serialize json file to use Newtonsoft.Json");
    }
    
    public CartesianPoint(string strName)
    {
        Name = strName;
    }

    public CartesianPoint(string strName, params float[] pPoints) : this(strName, 10, pPoints)
    {
    }

    public CartesianPoint(string strName, int nFrameCount, params float[] pPoints) : this(strName)
    {
        FrameCount = nFrameCount;
        int nLength = pPoints.Length;
        switch (nLength)
        {
            case >= 3 and < 6:
                AxisNum = 3;
                Values = new float[AxisNum];
                Array.Copy(pPoints, Values, AxisNum);
                break;
            case >= 6:
                AxisNum = 6;
                Values = new float[AxisNum];
                Array.Copy(pPoints, Values, AxisNum);
                break;
        }
    }

    public CartesianPoint(string strName, int nFrameCount, Vector3 pPosition) :
        this(strName, nFrameCount, pPosition.x, pPosition.y, pPosition.z)
    {
    }
    
    public CartesianPoint(string strName, int nFrameCount, Vector3 pPosition, Vector3 pRotation) :
        this(strName, nFrameCount, pPosition.x, pPosition.y, pPosition.z, pRotation.x, pRotation.y, pRotation.z)
    {
    }

    public ITeachingPoint Clone() => new CartesianPoint(Name)
    {
        FrameCount = FrameCount,
        AxisNum = AxisNum,
        Values = Values.Clone() as float[]
    };

    public void Trim(TrimMode eMode)
    {
        for (int i = 0; i < AxisNum; i++)
        {
            Values[i] = eMode switch
            {
                TrimMode.Integer => (int)Values[i] * 1.0F,
                TrimMode.Binary => (int)(Values[i] / 2.0F) * 2.0F,
                TrimMode.Pentagram => (int)(Values[i] / 5.0F) * 5.0F,
                TrimMode.Decimal => (int)(Values[i] / 10.0F) * 10.0F,
                _ => Values[i]
            };
        }
    }

    public string Print()
    {
        return $"{Name}=(X:{X:F2},Y:{Y:F2},Z{Z:F2})";
    }

    public static CartesianPoint operator +(CartesianPoint pPoint1, CartesianPoint pPoint2) =>
        new CartesianPoint($"{pPoint1.Name}+{pPoint2.Name}")
        {
            FrameCount = pPoint2.FrameCount,
            Values = pPoint1.Values.AddByElement(pPoint2.Values),
            AxisNum = pPoint1.AxisNum
        };

    public static CartesianPoint operator -(CartesianPoint pPoint1, CartesianPoint pPoint2) =>
        new CartesianPoint($"{pPoint1.Name}-{pPoint2.Name}")
        {
            FrameCount = pPoint2.FrameCount,
            Values = pPoint1.Values.SubtractByElement(pPoint2.Values),
            AxisNum = pPoint1.AxisNum
        };
}

public static class TeachingFactory
{
    public static void SaveTeachingPoints(Dictionary<string, JointPoint> pPoints, string strPath)
    {
        if (!CommonFactory.VerifyFilePath(strPath))
        {
            Debug.LogWarning("Failed to save the file");
            return;
        }

        using StreamWriter pWriter = new StreamWriter(strPath);
        JsonSerializer pSerializer = new JsonSerializer();
        pSerializer.Serialize(pWriter, pPoints);
    }

    public static Dictionary<string, JointPoint> LoadTeachingPoints(string strPath)
    {
        if (!CommonFactory.VerifyFilePath(strPath))
        {
            Debug.LogWarning("Failed to save the file");
            return null;
        }

        using TextReader pTextReader = new StreamReader(strPath);
        JsonReader pJsonReader = new JsonTextReader(pTextReader);
        JsonSerializer pSerializer = new JsonSerializer();
        return pSerializer.Deserialize<Dictionary<string, JointPoint>>(pJsonReader);
    }

    public static CartesianPoint ToCartesianPoint(this JointPoint pSourcePoint, KinematicsCalculator pCalculator)
    {
        Vector3 pPos = pCalculator.ForwardKinematics(pSourcePoint.Values);
        return new CartesianPoint(pSourcePoint.Name, pSourcePoint.FrameCount, pPos);
    }

    public static JointPoint ToJointPoint(this CartesianPoint pSourcePoint, KinematicsCalculator pCalculator,
        float[] pPrevAngles)
    {
        float[] pAngles = pCalculator.InverseKinematics(pSourcePoint.Position, pPrevAngles);
        return new JointPoint(pSourcePoint.Name, pSourcePoint.FrameCount, pAngles);
    }
}