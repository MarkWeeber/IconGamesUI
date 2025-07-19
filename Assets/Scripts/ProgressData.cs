using System.Collections.Generic;

[System.Serializable]
public class ProgressData
{
    public string ProfileName = "";
    public List<ProgressMetric> ProgressMetrics = new List<ProgressMetric>();

    public ProgressData(string profileName)
    {
        ProfileName = profileName;
    }
}

[System.Serializable]
public struct ProgressMetric
{
    public int LevelId;
    public float BestTime;
    public float LastTime;

}