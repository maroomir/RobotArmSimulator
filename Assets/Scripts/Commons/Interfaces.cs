public interface ISpeedControl
{
    public int MaxFrame { get; set; }
    public float MaxSpeed { get; set; }

    public void SetPosition(float fStartPos, float fTargetPos);
    public float GetSpeed(int iX);
    public float GetPosition(int iX);
}

public interface ITeachingPoint
{
    public string Name { get; }
    public int AxisNum { get; }
    public float MaxSpeed { get; set; }
    public int MaxFrame { get; set; }
    public SyncRule SyncMode { get; set; }
    public float[] Values { get; }

    public string Print();
    public void Scan(string strValue);
}