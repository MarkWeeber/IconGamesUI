using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;

[LazyInstatiate(false)]
public class WebGLSaveManager : SingletonBehaviour<WebGLSaveManager>
{
    // JavaScript plugin for WebGL
    [DllImport("__Internal")]
    private static extern void SyncFiles();

    private const string PREFIX = "_save.dat";
    private const string PASS_PHRASE = "_+$3a0-1!d";
    [SerializeField] private bool _cipheringEnabled = false;

    private ProgressData _progressData;
    public ProgressData ProgressData { get => _progressData; }
    private bool _hasUserInteracted = false;

    protected override void Initialize()
    {
        dontDestroyOnload = true;
    }

    protected async override void LazyInstantiate()
    {
        _progressData = new ProgressData("Default");
        await SaveData();
    }

    private void Start()
    {
        Application.focusChanged += OnApplicationFocus;
        _hasUserInteracted = true;
    }

    private void OnDestroy()
    {
        Application.focusChanged -= OnApplicationFocus;
    }

    private void OnApplicationFocus(bool focus)
    {
        if (focus) _hasUserInteracted = true;
    }

    public void UnloadProfile()
    {
        _progressData = null;
    }

    public async Task SubmitProfileName(string profileName)
    {
        var (exists, existingProfile) = await ReadProfileTextAsync(profileName);
        if (exists)
        {
            if (!TryParseFromJson<ProgressData>(existingProfile, out _progressData))
            {
                _progressData = new ProgressData(profileName);
                await SaveData();
            }
        }
        else
        {
            _progressData = new ProgressData(profileName);
            await SaveData();
        }
    }

    public async Task SaveData()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer && !_hasUserInteracted)
        {
            Debug.LogWarning("Save delayed - waiting for user interaction");
            LogUI.Instance.SendLogInformation("Save delayed - waiting for user interaction", LogUI.MessageType.WARNING);
            await Task.Delay(100); // Small delay before retry
            await SaveData();
            return;
        }

        string fullPath = GetFullPath(_progressData.ProfileName);
        string data = JsonUtility.ToJson(_progressData);

        if (_cipheringEnabled)
        {
            data = StringCipher.Encrypt(data, PASS_PHRASE);
        }

        try
        {
            // Different handling for WebGL
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                LogUI.Instance.SendLogInformation("CHECK 1", LogUI.MessageType.NOTE);
                await WebGLSave(fullPath, data);
            }
            else
            {
                LogUI.Instance.SendLogInformation("CHECK 2", LogUI.MessageType.NOTE);
                await StandaloneSave(fullPath, data);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Could not save locally " + e.Message);
            LogUI.Instance.SendLogInformation("Could not save locally " + e.Message, LogUI.MessageType.ERROR);
        }
    }

    private async Task StandaloneSave(string path, string data)
    {
        using (StreamWriter writer = File.CreateText(path))
        {
            await writer.WriteAsync(data);
        }
    }

    private async Task WebGLSave(string path, string data)
    {
        // Write to virtual filesystem
        File.WriteAllText(path, data);

        // Force sync with IndexedDB
        Debug.Log("BEFORE SYNC");
        SyncFiles();
        Debug.Log("AFTER SYNC");
        // Small delay to allow sync to complete
        await Task.Delay(50);
        Debug.Log("AFTER DELAY");
        // Verify the write
        if (File.Exists(path))
        {
            string verifyData = File.ReadAllText(path);
            if (verifyData != data)
            {
                // Retry once if verification failed
                File.WriteAllText(path, data);
                SyncFiles();
                await Task.Delay(50);
            }
        }
        LogUI.Instance.SendLogInformation("CHECK 3", LogUI.MessageType.NOTE);
    }

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
        else
        {
            progressMetric.LevelId = levelId;
            progressMetric.BestTime = score;
            progressMetric.LastTime = score;
            _progressData.ProgressMetrics.Add(progressMetric);
        }

        _ = SaveData();
    }

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
    }

    private async Task<List<ProgressData>> GetProgressDataFromAllProfiles()
    {
        List<ProgressData> _result = new List<ProgressData>();

        // WebGL requires special handling for file operations
        if (Application.platform == RuntimePlatform.WebGLPlayer && !_hasUserInteracted)
        {
            Debug.LogWarning("Waiting for user interaction before file access");
            LogUI.Instance.SendLogInformation("Waiting for user interaction before file access", LogUI.MessageType.WARNING);
            await WaitForCondition(() => _hasUserInteracted);
        }

        string[] allFilesInPersistentFolder;
        try
        {
            allFilesInPersistentFolder = Directory.GetFiles(Application.persistentDataPath);
        }
        catch (Exception e)
        {
            Debug.LogError($"Could not access persistent data path: {e.Message}");
            LogUI.Instance.SendLogInformation($"Could not access persistent data path: {e.Message}", LogUI.MessageType.ERROR);
            return _result;
        }

        string[] matchingFiles = allFilesInPersistentFolder
            .Where(file => Path.GetFileName(file).Contains(PREFIX))
            .ToArray();

        if (matchingFiles.Length == 0)
        {
            Debug.LogWarning("No profile save files found");
            LogUI.Instance.SendLogInformation("No profile save files found", LogUI.MessageType.WARNING);
            return _result;
        }

        foreach (string profileFile in matchingFiles)
        {
            string profileName = Path.GetFileName(profileFile).Replace(PREFIX, "");
            var (exists, existingProfile) = await ReadProfileTextAsync(profileName);
            if (exists && TryParseFromJson(existingProfile, out ProgressData progressData))
            {
                _result.Add(progressData);
            }
        }

        return _result;
    }

    // Helper method for async waiting
    private async Task WaitForCondition(Func<bool> condition, int checkInterval = 100)
    {
        while (!condition())
        {
            await Task.Delay(checkInterval);
        }
    }

    private async Task<(bool, string)> ReadProfileTextAsync(string profileName)
    {
        string assumedFile = GetFullPath(profileName);

        if (!File.Exists(assumedFile))
        {
            return (false, string.Empty);
        }

        try
        {
            string fileText = await File.ReadAllTextAsync(assumedFile);
            return (true, fileText);
        }
        catch (Exception e)
        {
            Debug.LogError($"Could not read file {assumedFile}: {e.Message}");
            LogUI.Instance.SendLogInformation($"Could not read file {assumedFile}: {e.Message}", LogUI.MessageType.ERROR);
            return (false, string.Empty);
        }
    }

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

    private string GetFullPath(string profileName)
    {
        return Path.Combine(Application.persistentDataPath, profileName + PREFIX);
    }
}