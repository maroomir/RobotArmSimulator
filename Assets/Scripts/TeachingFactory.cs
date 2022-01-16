using System;
using System.Linq;

public interface ITeachingComponent
{
    public int AxisNum { get; }
    public float MaxSpeed { get; set; }
    public int MaxFrame { get; set; }
    public SyncRule SyncMode { get; set; }
    public float[] Contents { get; }
}

public class JointPosition : ITeachingComponent
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
    public float[] Contents { get; private set; }

    private float _fMaxSpeed = 100.0F;
    private int _fMaxFrame = 20;

    public JointPosition()
    {
        // 
    }

    public static JointPosition Home(int nAxisNum = 7) => new JointPosition
    {
        SyncMode = SyncRule.FrameSync,
        MaxFrame = 100,
        AxisNum = nAxisNum,
        Contents = Enumerable.Repeat(0.0F, nAxisNum).ToArray<float>(),
    };

    public static JointPosition FromPosition(params float[] args) => new JointPosition
    {
        SyncMode = SyncRule.FrameSync,
        MaxFrame = 100,
        Contents = args.Clone() as float[],
        AxisNum = args.Length,
    };
}