using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

[LazyInstatiate(true)]
public class LocalSaveManager : SingletonBehaviour<LocalSaveManager>
{
    private const string PREFIX = "_save.dat";
    private const string PASS_PHRASE = "_+$3a0-1!d";
    // needed to control whether to cipher the data before saving or not
    [SerializeField] private bool _cipheringEnabled = false;

    private ProgressData _progressData;
    public ProgressData ProgressData { get => _progressData; }

    // uses singleton pattern to run some code when initialized
    protected override void Initialize()
    {
        dontDestroyOnload = true;
    }

    // lazy instantiation for testing purposes in Editor
    protected async override void LazyInstantiate()
    {
        _progressData = new ProgressData("Default");
        await SaveData();
    }

    // necessary to unload profile, when needed to enter Enter Profile Screen
    public void UnloadProfile()
    {
        _progressData = null;
    }

    // a user function to enter their name
    // in the end this script should hold a Progress Data for current player
    public async Task SubmitProfileName(string profileName)
    {
        // check if profile already exists
        var (exists, existingProfile) = await ReadProfileTextAsync(profileName);
        if (exists)
        {
            // try parsing it out
            // and if for some reason file corrupted create new save file profile, overwrite the existing one
            if (!TryParseFromJson<ProgressData>(existingProfile, out _progressData))
            {
                _progressData = new ProgressData(profileName);
                await SaveData();
            }
        }
        // if not exists then create new one
        else
        {
            _progressData = new ProgressData(profileName);
            await SaveData();
        }
    }

    // a main function to be called from other scripts
    // should they need to save data
    public async Task SaveData()
    {
        string fullPath = GetFullPath(_progressData.ProfileName);
        string data = JsonUtility.ToJson(_progressData);
        if (_cipheringEnabled)
        {
            data = StringCipher.Encrypt(data, PASS_PHRASE);
        }
        try
        {
            using (StreamWriter writer = File.CreateText(fullPath))
            {
                await writer.WriteAsync(data);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Could not save locally " + e.Message);
            LogUI.Instance.SendLogInformation("Could not save locally " + e.Message, LogUI.MessageType.ERROR);
        }
    }

    // an example of code to be called 
    // in order to save score for current player profile
    public void SubmitScore(int levelId, float score)
    {
        if (_progressData == null)
        {
            Debug.LogError("Profile not loaded");
            LogUI.Instance.SendLogInformation("Profile not loaded", LogUI.MessageType.ERROR);
            return;
        }
        int levelIndex = _progressData.ProgressMetrics.FindIndex(n => n.LevelId == levelId);
        ProgressMetric progressMetric = new ProgressMetric();
        // metric already exists
        if (levelIndex >= 0)
        {
            progressMetric = _progressData.ProgressMetrics[levelIndex];
            progressMetric.LastTime = score;
            if (progressMetric.BestTime > score)
            {
                progressMetric.BestTime = score;
            }
            _progressData.ProgressMetrics[levelIndex] = progressMetric;
        }
        // add a new metric
        else
        {
            progressMetric.LevelId = levelId;
            progressMetric.BestTime = score;
            progressMetric.LastTime = score;
            _progressData.ProgressMetrics.Add(progressMetric);
        }
        // silent calling async Task to save data
        _ = SaveData();
    }

    // another necessary function to be called
    // whenever it's necessary to see scores from all player
    // for given level ID
    public async Task<Score[]> GetScores(int levelId)
    {
        List<ProgressData> allProgressData = await GetProgressDataFromAllProfiles();
        if (allProgressData.Count == 0)
        {
            Debug.LogError("Could not get all profiles");
            LogUI.Instance.SendLogInformation("Could not get all profiles", LogUI.MessageType.ERROR);
            return default;
        }
        return allProgressData
            .SelectMany(
                        profile => profile.ProgressMetrics
                            .Where(metric => metric.LevelId == levelId)
                            .Select(
                                        metric => new Score
                                        {
                                            PlayerName = profile.ProfileName,
                                            LastTime = metric.LastTime,
                                            BestTime = metric.BestTime
                                        }
                                    )
                        ).OrderByDescending(score => score.BestTime).ToArray();
        // it is important to reverse order, as when score UIs being instantiated, they are placed last in hiearchy by default
    }

    // a helper function to get all Progress Data from other players
    // which were saved previously
    private async Task<List<ProgressData>> GetProgressDataFromAllProfiles()
    {
        List<ProgressData> _result = new List<ProgressData>();
        string[] allFilesInPersisntenFolder = Directory.GetFiles(Application.persistentDataPath);
        string[] matchingFiles = allFilesInPersisntenFolder.Where(file => Path.GetFileName(file).Contains(PREFIX)).ToArray();
        string[] profileFiles = matchingFiles.Select(Path.GetFileName).ToArray();
        if (profileFiles.Length == 0)
        {
            Debug.LogWarning("No profile save files found");
            LogUI.Instance.SendLogInformation("No profile save files found", LogUI.MessageType.WARNING);
            return _result;
        }
        foreach (string profileFile in profileFiles)
        {
            string profileName = profileFile.Replace(PREFIX, "");
            var (exists, existingProfile) = await ReadProfileTextAsync(profileName);
            if (exists)
            {
                if (TryParseFromJson(existingProfile, out ProgressData progressData))
                {
                    _result.Add(progressData);
                }
            }
        }
        return _result;
    }

    // a helper function
    private async Task<(bool, string)> ReadProfileTextAsync(string profileName)
    {
        string assumedFile = GetFullPath(profileName);
        string fileText;
        if (File.Exists(assumedFile))
        {
            using (StreamReader reader = File.OpenText(assumedFile))
            {
                fileText = await reader.ReadToEndAsync();
                return (true, fileText);
            }
        }
        return (false, string.Empty);
    }

    // a helper function to parse the obtained data
    private bool TryParseFromJson<T>(string inputData, out T outputData)
    {
        outputData = default;
        try
        {
            if (_cipheringEnabled)
            {
                string deciphered = StringCipher.Decrypt(inputData, PASS_PHRASE);
                outputData = JsonUtility.FromJson<T>(deciphered);
            }
            else
            {
                outputData = JsonUtility.FromJson<T>(inputData);
            }
            return true;
        }
        catch (Exception)
        {
            Debug.LogWarning("Save file corrupted, could not read");
            LogUI.Instance.SendLogInformation("Save file corrupted, could not read", LogUI.MessageType.WARNING);
            return false;
        }
    }

    // a helper function to get correct path to save or obtain save file
    // from file system
    private string GetFullPath(string profileName)
    {
        return Application.persistentDataPath + "/" + profileName + PREFIX;
    }
}

public struct Score
{
    public string PlayerName;
    public float LastTime;
    public float BestTime;
}
