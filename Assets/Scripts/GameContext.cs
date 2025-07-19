using UnityEngine;

[LazyInstatiate(true)]
public class GameContext : SingletonBehaviour<GameContext>
{
    [SerializeField] private GameLevelsAsset _gameLevelsAsset;
    public GameLevelsAsset GameLevelsAsset { get => _gameLevelsAsset; }

    protected override void Initialize()
    {
        dontDestroyOnload = true;
        if (_gameLevelsAsset == null)
        {
            Debug.LogError("Please put reference to Scene management settings asset!");
            LogUI.Instance.SendLogInformation("SCENE MANAGEMENT IS MISSING", LogUI.MessageType.ERROR);
        }
    }

    private void Start()
    {
        if (_gameLevelsAsset == null)
        {
            TryLoadAsset();
        }
    }

    private void TryLoadAsset()
    {
        _gameLevelsAsset = Utils.LoadScriptableObjectOfType<GameLevelsAsset>();
    }
}
