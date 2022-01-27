using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using UnityEngine;

public class JointPoint : ITeachingPoint
{
    public string Name { get; set; }
    public int AxisNum { get; private set; }
    public int FrameCount { get; set; }
    public float[] Values { get; private set; }
    
    public JointPoint(string strName)
    {
        Name = strName;
    }

    public JointPoint(string strName, params float[] pPoints) : this(strName)
    {
        AxisNum = pPoints.Length;
        Values = pPoints;
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

    public string Print()
    {
        return $"[JOINT]{Name}=" + string.Join(',', Values);
    }

    public static JointPoint operator +(JointPoint pPoint1, JointPoint pPoint2) =>
        new JointPoint($"{pPoint1.Name}+{pPoint2.Name}")
        {
            FrameCount = pPoint1.FrameCount,
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
            FrameCount = pPoint1.FrameCount,
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
    public int AxisNum { get; private set; }
    public int FrameCount { get; set; }
    public float[] Values { get; private set; }

    public Vector3 Position =>
        (Values.Length >= 3) ? new Vector3(Values[0], Values[1], Values[2]) : new Vector3(0.0F, 0.0F, 0.0F);

    public Vector3 Rotation =>
        (Values.Length >= 6) ? new Vector3(Values[3], Values[4], Values[5]) : new Vector3(0.0F, 0.0F, 0.0F);

    public float X => Values[0];
    public float Y => Values[1];
    public float Z => Values[2];
    public float RX => Values[3];
    public float RY => Values[4];
    public float RZ => Values[5];

    public CartesianPoint(string strName)
    {
        Name = strName;
    }

    public CartesianPoint(string strName, params float[] pPoints) : this(strName)
    {
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
            default:
                break;
        }
    }

    public CartesianPoint(string strName, Vector3 pPosition) :
        this(strName, pPosition.x, pPosition.y, pPosition.z)
    {
    }

    public CartesianPoint(string strName, Vector3 pPosition, Vector3 pRotation) :
        this(strName, pPosition.x, pPosition.y, pPosition.z, pRotation.x, pRotation.y, pRotation.z)
    {
    }

    public ITeachingPoint Clone() => new CartesianPoint(Name)
    {
        FrameCount = FrameCount,
        AxisNum = AxisNum,
        Values = Values.Clone() as float[]
    };

    public string Print()
    {
        return $"[CART]{Name}=" + string.Join(',', Values);
    }

    public static CartesianPoint operator +(CartesianPoint pPoint1, JointPoint pPoint2)
    {
        JointPoint pJointPoint1 = pPoint1.ToJointPoint(CommonFactory.RobotKinematics);
        JointPoint pResultPoint = pJointPoint1 + pPoint2;
        return pResultPoint.ToCartesianPoint(CommonFactory.RobotKinematics);
    }

    public static CartesianPoint operator +(CartesianPoint pPoint1, CartesianPoint pPoint2) =>
        new CartesianPoint($"{pPoint1.Name}+{pPoint2.Name}")
        {
            FrameCount = pPoint1.FrameCount,
            Values = pPoint1.Values.AddByElement(pPoint2.Values),
            AxisNum = pPoint1.AxisNum
        };

    public static CartesianPoint operator -(CartesianPoint pPoint1, JointPoint pPoint2)
    {
        JointPoint pJointPoint1 = pPoint1.ToJointPoint(CommonFactory.RobotKinematics);
        JointPoint pResultPoint = pJointPoint1 - pPoint2;
        return pResultPoint.ToCartesianPoint(CommonFactory.RobotKinematics);
    }

    public static CartesianPoint operator -(CartesianPoint pPoint1, CartesianPoint pPoint2) =>
        new CartesianPoint($"{pPoint1.Name}-{pPoint2.Name}")
        {
            FrameCount = pPoint1.FrameCount,
            Values = pPoint1.Values.SubtractByElement(pPoint2.Values),
            AxisNum = pPoint1.AxisNum
        };
}

public static class TeachingFactory
{
    public static void SaveTeachingPoints(List<ITeachingPoint> pPoints, string strPath)
    {
        if (!CommonFactory.VerifyFilePath(strPath))
        {
            Debug.LogWarning("Failed to save the file");
            return;
        }

        using (StreamWriter pWriter = new StreamWriter(strPath))
        {
            JsonSerializer pSerializer = new JsonSerializer();
            pSerializer.Serialize(pWriter, pPoints);
        }
    }

    public static ITeachingPoint[] LoadTeachingPoints(string strPath)
    {
        throw new NotSupportedException();
    }

    public static CartesianPoint ToCartesianPoint(this JointPoint pSourcePoint, KinematicsCalculator pCalculator)
    {
        Vector3 pPos = pCalculator.ForwardKinematics(pSourcePoint.Values);
        return new CartesianPoint(pSourcePoint.Name, pPos);
    }

    public static JointPoint ToJointPoint(this CartesianPoint pSourcePoint, KinematicsCalculator pCalculator)
    {
        float[] pAngles = pCalculator.InverseKinematics(pSourcePoint.Position);
        return new JointPoint(pSourcePoint.Name, pAngles);
    }

    public static JointPoint ToJointPoint(this CartesianPoint pSourcePoint, KinematicsCalculator pCalculator, float[] pPrevAngles)
    {
        float[] pAngles = pCalculator.InverseKinematics(pSourcePoint.Position, pPrevAngles);
        return new JointPoint(pSourcePoint.Name, pAngles);
    }
}