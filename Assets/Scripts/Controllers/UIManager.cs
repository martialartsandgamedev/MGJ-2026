using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIManager:LevelEventMonoBehaviour
{
   public static UIManager Ins = null;
   
   public Image AimReticle = null;
   
   [SerializeField]
   private Transform m_defaultLayer = null;
   
   [SerializeField]
   private bool m_displayingMenu = false;
   
   [SerializeField]
   private Transform m_playerView;

   [SerializeField]
   private Transform m_focusView;
   
   //Day
   [FormerlySerializedAs("m_dayControlMenu")]
   [SerializeField]
   private CanvasGroup m_levelControlMenu = null;

   [SerializeField]
   private Image m_dayProgressCircle = null;

   [SerializeField]
   private MeshRenderer m_moonPhaseRenderer = null;
   
   private void Awake()
   {
       if (Ins == null)
       {
           Debug.LogFormat($"UI Singleton created");
           Ins = this;
       }
   }

   public void ProcessUIRequest(UIInputContext inputContext)
   {
       if (inputContext.RequestMenu)
       {
           m_displayingMenu = !m_displayingMenu;
           Debug.LogWarningFormat($"Input manager requested a menu change - menu is now {(m_displayingMenu?"showing":"not showing")}");

           if (m_displayingMenu)
           {
                //Show a menu
               InputManager.ins.ToggleInputs(false);
           }
           else
           {
               //Destroy the menu
               InputManager.ins.ToggleInputs(true);
           }
       }
   }
   
   protected override void OnLevelPrepared(Level level)
   {
       m_levelControlMenu.alpha = 1;
       m_levelControlMenu.interactable = true;
       m_levelControlMenu.blocksRaycasts = true;
   }
   
   protected override void OnlevelStarted(Level level)
   {
       m_levelControlMenu.alpha = 0;
       m_levelControlMenu.interactable = false;
       m_levelControlMenu.blocksRaycasts = false;
   }
    
   protected override void OnLevelTicked(Level level, float deltaTime)
   {
      
   }
   
   protected override void OnLevelEnded(Level level)
   {
       m_levelControlMenu.alpha = 1;
       m_levelControlMenu.interactable = true;
       m_levelControlMenu.blocksRaycasts = true;
   }
}