using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class JointPoint : ITeachingPoint
{
    public int AxisNum { get; private set; }

    public float MaxSpeed
    {
        get => _fMaxSpeed;
        set
        {
            SyncMode = SyncRule.Async;
            _fMaxSpeed = value;
        }
    }

    public int MaxFrame
    {
        get => _fMaxFrame;
        set
        {
            SyncMode = SyncRule.FrameSync;
            _fMaxFrame = value;
        }
    }

    public SyncRule SyncMode { get; set; }
    public float[] Values { get; private set; }

    public string Name { get; set; }

    private float _fMaxSpeed = 100.0F;
    private int _fMaxFrame = 20;

    public JointPoint()
    {
        // 
    }

    public JointPoint(float[] pPoints)
    {
        AxisNum = pPoints.Length;
        Values = pPoints;
    }

    public static JointPoint Home(int nAxisNum = 7) => new JointPoint
    {
        SyncMode = SyncRule.FrameSync,
        MaxFrame = 100,
        AxisNum = nAxisNum,
        Values = Enumerable.Repeat(0.0F, nAxisNum).ToArray<float>(),
    };

    public static JointPoint FromPosition(params float[] args) => new JointPoint
    {
        SyncMode = SyncRule.FrameSync,
        MaxFrame = 100,
        Values = args.Clone() as float[],
        AxisNum = args.Length,
    };

    public string Print()
    {
        return $"[JOINT]{Name}=" + string.Join(',', Values);
    }

    public void Scan(string strValue)
    {
        throw new NotImplementedException();
    }
}

public static class TeachingFactory
{
    public static void SaveTeachingPoints(List<ITeachingPoint> pPoints, string strPath)
    {
        FileInfo pFile = new FileInfo(strPath);
        if (!pFile.Exists)
        {
            FileStream pStream = pFile.Create();
            pStream.Close();
        }

        using StreamWriter pWriter = new StreamWriter(strPath);
        foreach (ITeachingPoint pPoint in pPoints)
        {
            string strInfo = pPoint.Print();
            pWriter.WriteLine(strInfo);
        }
    }

    public static ITeachingPoint[] LoadTeachingPoints(string strPath)
    {
        throw new NotSupportedException();
    }
}