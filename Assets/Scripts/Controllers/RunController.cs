using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

[ExecuteAlways]
public class RunController : MonoBehaviour
{
    public static RunController Ins { get; private set; }
    
    private Run m_activeRun = null;
    
    [SerializeField]
    private float m_runSpeed = 1f;
    
    public UnityEvent<Level> LevelPrepareEvent;
    public UnityEvent<Level> LevelStartEvent;
    public UnityEvent<Level, float> LevelTickEvent;
    public UnityEvent<Level> levelEndEvent;
    
    private void Awake()
    {
        if (Ins != null && Ins != this)
        {
            DestroyImmediate(this); return; 
        } 
        Ins = this;
    }

    private async void Start()
    {
        SceneHandler.Ins.LoadLevelFromAddress("scene_level_000");
        await InitialiseNewRun();
    }

    
    //Setup a new run with the required parameters (difficulty, character, whatever)
    //This will NOT kick off the run
    public async Task InitialiseNewRun()
    {
        m_activeRun = new Run();
        
        //Wait for full initialisation
        await m_activeRun.Initialise();
        
        //Wait for permission to commence the run
        await m_activeRun.CommencePermission();
        
        CommenceCurrentRun();
    }

    //Kick off, or continue - the current run
    public async Task CommenceCurrentRun()
    {
        await GoNextLevel();
        Time.timeScale = 1;
    }
    
    /// <summary>
    /// Returns the currently focused level or null if none exists
    /// </summary>
    private Level GetCurrentLevel()
    {
        if (m_activeRun == null)
        {
            return null;
        }

        if (m_activeRun.CurrentLevelIndex < 0 || m_activeRun.CurrentLevelIndex >= m_activeRun.Levels.Count)
        {
            return null;

        }
            
        return m_activeRun.Levels[m_activeRun.CurrentLevelIndex];
    }

    /// <summary>
    /// Explicitly add a future level to the schedule
    /// </summary>
    public Task AddLevel()
    {
        if (m_activeRun == null)
        {
            return Task.CompletedTask;
        }

        Level newLevel = new Level();
        newLevel.Initialise();
        
        m_activeRun.Levels.Add(newLevel);
        
        return Task.CompletedTask;
    }
    
    public async Task GoToLevel(int targetLevel)
    {
        for (int i = m_activeRun.CurrentLevelIndex; i < targetLevel; i++)
        {
            m_activeRun.CurrentLevelIndex = i;
            
            Level current = GetCurrentLevel();
            
            if (current == null)
            {
                Debug.LogWarning("CURRENT LEVEL IS NULL CANNOT ADVANCE");
                return;
            }
        
            //If we got a current day that is not in a compeleted state - force complete it.
            if ((int)current.State < (int)LevelState.Prepared)
            {
                Debug.LogWarning("SIMULATING DAY: PREPARE");
                await StageCurrentLevel();
                EndCurrentLevel();
            }
        
            //If we got a current day that is not in a compeleted state - force complete it.
            else if ((int)current.State < (int)LevelState.Ended)
            {
                Debug.LogWarning("ENDING LEVEL BEFORE ADVANCE");
                EndCurrentLevel();
            }
        }
    }
    
    public async Task GoNextLevel()
    {
        Level current = GetCurrentLevel();
        
        //Deal with any day currently in progress
        if (current != null)
        {
            //This is manual and messy
            
            //If we got a current day that is not in a compeleted state - force complete it.
            if ((int)current.State < (int)LevelState.Prepared)
            {
                Debug.LogWarning("SIMULATING DAY: PREPARE");
                //This will wait for the run to do its prepare logic
                await StageCurrentLevel();
                
                //Then end
                EndCurrentLevel();
            }
        
            //If it is just waiting to be ended - end it
            else if ((int)current.State < (int)LevelState.Ended)
            {
                Debug.LogWarning("ENDING DAY BEFORE ADVANCE");
                EndCurrentLevel();
            }
        }
     
        //Advance the tracked day
        m_activeRun.CurrentLevelIndex++;
        
        //If we have no day at this point - automatically add a day
        if (m_activeRun.CurrentLevelIndex >= m_activeRun.Levels.Count)
        {
            Debug.LogWarning("There was no day in the calendar - auto adding");
            await AddLevel();
            m_activeRun.CurrentLevelIndex = m_activeRun.Levels.Count - 1;
        }

        await StageCurrentLevel();
    }
    
    //So it goes STAGE (PREPARE AND WAIT) > START/SKIP > END
    public async Task StageCurrentLevel(bool autoPrepare = false)
    {
        //Do anything you want to do in the preparation step of the level. The level will be fired out to all listeners.
        
        Level level = GetCurrentLevel();
        
        if (level == null)
        {
            Debug.LogWarning("No level to prepare.");
            return;
        }

        if (level.State != LevelState.Created)
        {
            Debug.LogWarning("Focused level is not in Created state. This should never happen and points to a critical sequencing error.");
            return;
        }
        
        //We wait for the active run to do whatever it does - when we prepare
        // await m_activeRun.OnDayPrepared(day);
        
        //After this - we can officially update the state - and fire events
        level.State = LevelState.Prepared;
        
        
        LevelPrepareEvent?.Invoke(level);
        
        //Wait for permission to start - usually via menu
        await level.StartPermission();
        
        level.State = LevelState.Started;
        LevelStartEvent?.Invoke(level);

        StartCoroutine(ProcessCurrentLevel());
    }   
    
    public async void EndCurrentLevel()
    {
        Level level = GetCurrentLevel();
        
        level.State = LevelState.Ended;
        levelEndEvent?.Invoke(level);
        
        //Wait for permission to end
        await level.EndPermission();
        
        //Go to the next day
        await GoNextLevel();
    }

    //Day commencement options
    public void CommenceRun()
    {
        m_activeRun.GrantCommencePermission();
    }
    public void StartLevel()
    {
        Level level = GetCurrentLevel();
        
        level.GrantStartPermission();
    }
    
    public void SkipLevel()
    {
        Level level = GetCurrentLevel();

        GoNextLevel();
    }
    
    public void EndLevel()
    {
        Level level = GetCurrentLevel();

        level.GrantEndPermission();
    }
    
    // =========================
    // TICKING
    // =========================

    private void TickLevel(Level level, float tick)
    {
        m_activeRun.PragmaticRunTime += tick;

        if (Random.value < level.EventChance)
        {
            Debug.Log("Day event triggered");
        }
        
        LevelTickEvent?.Invoke(level, tick);
    }
    
    private IEnumerator ProcessCurrentLevel()
    {
        Level level = GetCurrentLevel();
        
        Debug.LogFormat($"We began processing the day");
        
        while (level.State == LevelState.Started)
        {
            if (level.Elapsed() >= level.Length)
            {
                EndCurrentLevel();
                yield break;
            }

            if (Input.GetKeyUp(KeyCode.Delete))
            {
                EndCurrentLevel();
                yield break;
            }
            
            Debug.LogFormat($"Time Scale = {Time.timeScale}");
            TickLevel(level, Time.deltaTime * m_runSpeed);
            
            yield return null;
        }
    }
}

