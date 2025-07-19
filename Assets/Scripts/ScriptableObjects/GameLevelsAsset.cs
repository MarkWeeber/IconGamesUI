using UnityEngine;

[CreateAssetMenu(fileName = "Game Levels Asset", menuName = "Custom Assets/Game Levels Asset")]
public class GameLevelsAsset : ScriptableObject
{
    public int MainSceneBuildIndex = 0;
    public LevelSceneInfo[] LevelSceneInfos;
}

[System.Serializable]
public struct LevelSceneInfo
{
    public string GameName;
    public int LevelSceneBuildIndex;
    public int LevelId;
    public string InfoMessage;
    public int LevelStageCount;
}