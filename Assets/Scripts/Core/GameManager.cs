using UnityEngine;
using UnityEngine.Events;

public enum GameState { MainMenu, Playing, Paused, LevelUp, GameOver, Victory }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Run config")]
    public float runDuration = 1200f; // 20 minutes

    [ContextMenu("Start Run")]
    public void StartGame()
    {
        StartRun();
        Debug.Log("Run started");
    }
    public GameState State { get; private set; }
    public float RunTimer { get; private set; }
    public int PlayerLevel { get; private set; }
    public int PlayerXP { get; private set; }
    public int XPToNextLevel { get; private set; } = 5;

    public static UnityEvent<GameState> OnStateChanged = new();
    public static UnityEvent<int> OnLevelUp = new();
    public static UnityEvent OnRunStarted = new();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    void Start()
    {
        StartRun();
    }
    void Update()
    {
        if (State != GameState.Playing) return;
        RunTimer += Time.deltaTime;
        if (RunTimer >= runDuration) SetState(GameState.Victory);
    }

    public void StartRun()
    {
        RunTimer = 0f;
        PlayerLevel = 1;
        PlayerXP = 0;
        XPToNextLevel = 5;
        SetState(GameState.Playing);
        OnRunStarted.Invoke();
    }

    public void AddXP(int amount)
    {
        if (State != GameState.Playing && State != GameState.LevelUp) return;
        PlayerXP += amount;
        if (PlayerXP >= XPToNextLevel) LevelUp();
    }

    void LevelUp()
    {
        PlayerXP -= XPToNextLevel;
        PlayerLevel++;
        XPToNextLevel = Mathf.RoundToInt(XPToNextLevel * 1.25f);
        SetState(GameState.LevelUp);
        OnLevelUp.Invoke(PlayerLevel);
    }

    public void ResumePlaying() => SetState(GameState.Playing);
    public void PlayerDied() => SetState(GameState.GameOver);

    void SetState(GameState next)
    {
        State = next;
        Time.timeScale = (next == GameState.LevelUp || next == GameState.Paused) ? 0f : 1f;
        OnStateChanged.Invoke(next);
    }
}
