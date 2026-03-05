using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class Run: LevelEventClass
{
    public long StartTicks;
    
    public int CurrentLevelIndex = 0;
    
    private List<Level> m_levels = null;
    public List<Level> Levels => m_levels;
    public float PragmaticRunTime { get; set; }

    //Gates
    private TaskCompletionSource<bool> m_commencePermission = null;
    
    public Task CommencePermission() => m_commencePermission.Task;
    
    public Run()
    {
     
    }

    //Call from the run controller to setup the run - this DOES NOT KICK IT OFF
    public Task Initialise()
    {
        m_levels = new List<Level>();
        StartTicks = DateTime.UtcNow.Ticks;
        
        //Setup async commencement permission
        m_commencePermission = new TaskCompletionSource<bool>();
        
        //Setup day event subscriptions
        Schedule();
        
        return Task.CompletedTask;
    }
    
    public Double RunGlobalElapsed()
    {
        var runDuration = DateTime.UtcNow.Ticks - StartTicks;
        TimeSpan elapsed = new TimeSpan(runDuration);
        
        Debug.LogFormat($"Run global elapsed time is {elapsed}");
        return elapsed.TotalSeconds; 
    }
    
    protected override async void OnLevelPrepared(Level level)
    {
       
    }
   
    protected override async void OnLevelStarted(Level level)
    {
   
    }
    
    protected override async void OnLevelTicked(Level level, float deltaTime)
    {
       
    }
   
    
    protected override async void OnLevelEnded(Level level)
    {
    
    }

    public void GrantCommencePermission()
    {
        m_commencePermission.TrySetResult(true);
    }
}

public abstract class LevelEventMonoBehaviour: MonoBehaviour
{
    protected void OnEnable()
    {
        RunController.Ins.LevelPrepareEvent.AddListener(OnLevelPrepared);
        RunController.Ins.LevelStartEvent.AddListener(OnlevelStarted);
        RunController.Ins.LevelTickEvent.AddListener(OnLevelTicked);
        RunController.Ins.levelEndEvent.AddListener(OnLevelEnded);
    }
    
    protected void OnDisable()
    {
        RunController.Ins.LevelPrepareEvent.RemoveListener(OnLevelPrepared);
        RunController.Ins.LevelStartEvent.RemoveListener(OnlevelStarted);
        RunController.Ins.LevelTickEvent.RemoveListener(OnLevelTicked);
        RunController.Ins.levelEndEvent.RemoveListener(OnLevelEnded);
    }
    
    protected abstract void OnLevelPrepared(Level level);
    protected abstract void OnlevelStarted(Level level);
    protected abstract void OnLevelTicked(Level level, float deltaTime);
    protected abstract void OnLevelEnded(Level level);
}

public abstract class LevelEventClass
{
    protected void Schedule()
    {
        RunController.Ins.LevelPrepareEvent.AddListener(OnLevelPrepared);
        RunController.Ins.LevelStartEvent.AddListener(OnLevelStarted);
        RunController.Ins.LevelTickEvent.AddListener(OnLevelTicked);
        RunController.Ins.levelEndEvent.AddListener(OnLevelEnded);
    }
    
    protected void Deschedule()
    {
        RunController.Ins.LevelPrepareEvent.RemoveListener(OnLevelPrepared);
        RunController.Ins.LevelStartEvent.RemoveListener(OnLevelStarted);
        RunController.Ins.LevelTickEvent.RemoveListener(OnLevelTicked);
        RunController.Ins.levelEndEvent.RemoveListener(OnLevelEnded);
    }
    
    protected abstract void OnLevelPrepared(Level level);
    protected abstract void OnLevelStarted(Level level);
    protected abstract void OnLevelTicked(Level level, float deltaTime);
    protected abstract void OnLevelEnded(Level level);
}