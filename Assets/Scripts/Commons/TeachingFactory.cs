using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class JointPoint : ITeachingPoint
{
    public string Name { get; set; }
    public int AxisNum { get; private set; }
    public int FrameCount { get; set; }
    public float[] Values { get; private set; }

    public JointPoint()
    {
        // 
    }

    public JointPoint(params float[] pPoints)
    {
        AxisNum = pPoints.Length;
        Values = pPoints;
    }

    public static JointPoint Home(int nAxisNum = 7) => new JointPoint
    {
        Name = "Home",
        FrameCount = 100,
        AxisNum = nAxisNum,
        Values = Enumerable.Repeat(0.0F, nAxisNum).ToArray<float>(),
    };

    public static JointPoint FromPosition(string strName, params float[] args) => new JointPoint
    {
        Name = strName,
        FrameCount = 100,
        Values = args.Clone() as float[],
        AxisNum = args.Length,
    };

    public static JointPoint FromPosition(params float[] args) => new JointPoint
    {
        FrameCount = 100,
        Values = args.Clone() as float[],
        AxisNum = args.Length,
    };

    public string Print()
    {
        return $"[JOINT]{Name}=" + string.Join(',', Values);
    }
}

public class CartesianPoint : ITeachingPoint
{
    public string Name { get; set; }
    public int AxisNum { get; private set; }
    public int FrameCount { get; set; }
    public float[] Values { get; private set; }
    public Vector3 Position => (Values.Length >= 3) ? new Vector3(Values[0], Values[1], Values[2]) : new Vector3(0.0F, 0.0F, 0.0F);
    public Vector3 Rotation => (Values.Length >= 6) ? new Vector3(Values[3], Values[4], Values[5]) : new Vector3(0.0F, 0.0F, 0.0F);

    public float X => Values[0];
    public float Y => Values[1];
    public float Z => Values[2];
    public float RX => Values[3];
    public float RY => Values[4];
    public float RZ => Values[5];

    public CartesianPoint()
    {
        //
    }

    public CartesianPoint(params float[] pPoints)
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

    public CartesianPoint(Vector3 pPosition) :
        this(pPosition.x, pPosition.y, pPosition.z)
    {
    }

    public CartesianPoint(Vector3 pPosition, Vector3 pRotation) :
        this(pPosition.x, pPosition.y, pPosition.z, pRotation.x, pRotation.y, pRotation.z)
    {
    }

    public string Print()
    {
        return $"[CART]{Name}=" + string.Join(',', Values);
    }
}

public static class TeachingFactory
{
    public static void SaveTeachingPoints(List<ITeachingPoint> pPoints, string strPath)
    {
        if (!CommonFunctions.VerifyFilePath(strPath))
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
        return new CartesianPoint(pPos);
    }

    public static JointPoint ToJointPoint(this CartesianPoint pSourcePoint, KinematicsCalculator pCalculator)
    {
        float[] pAngles = pCalculator.InverseKinematics(pSourcePoint.Position);
        return new JointPoint(pAngles);
    }

    public static JointPoint ToJointPoint(this CartesianPoint pSourcePoint, KinematicsCalculator pCalculator, float[] pPrevAngles)
    {
        float[] pAngles = pCalculator.InverseKinematics(pSourcePoint.Position, pPrevAngles);
        return new JointPoint(pAngles);
    }
}