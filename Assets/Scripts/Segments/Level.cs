using System;
using System.Threading.Tasks;


public enum LevelState{Unknown, Created, Prepared, Started, Ended}

[Serializable]
public class Level
{
    public long StartTicks;
    
    public LevelState State = LevelState.Unknown;
    
    //A bunch of transient data that is derived from the run
    public int ThreatLevel;
    public int NPCDensity;
    public float Length;
    public float EventChance;
    
    private TaskCompletionSource<bool> m_startPermission;
    
    private TaskCompletionSource<bool> m_endPermission;
    
    public Task StartPermission() => m_startPermission.Task;
    public Task EndPermission() => m_endPermission.Task;
    
    public void Initialise()
    {
        StartTicks = DateTime.UtcNow.Ticks;
        State = LevelState.Created;
        
        //Initialise params manually for now
        Length = 240; //4 mins
        EventChance = 0.5f;
        ThreatLevel = 1;
        NPCDensity = 1;

        m_startPermission = new();
        m_endPermission = new();
    }
    
    public Double Elapsed()
    {
        var dayDuration = DateTime.UtcNow.Ticks - StartTicks;
        TimeSpan elapsed = new TimeSpan(dayDuration);
        return elapsed.TotalSeconds; 
    }
    
    public void GrantStartPermission()
    {
        m_startPermission.TrySetResult(true);
    }
    
    public void GrantEndPermission()
    {
        m_endPermission.TrySetResult(true);
    }
}
