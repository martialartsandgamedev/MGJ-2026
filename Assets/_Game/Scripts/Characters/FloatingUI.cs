using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public enum ControllerType { PC, Xbox, PlayStation }

[System.Serializable]
public struct ButtonPromptData
{
    public string   ActionId;
    public Sprite[] PCSprites;
    public Sprite[] XboxSprites;
    public Sprite[] PlayStationSprites;

    public readonly Sprite[] GetSprites(ControllerType type) => type switch
    {
        ControllerType.Xbox        => XboxSprites,
        ControllerType.PlayStation => PlayStationSprites,
        _                          => PCSprites
    };
}

/// <summary>
/// World-space UI canvas component that displays contextual button prompts.
/// Call Init(devices) when the player is spawned, then ShowPrompt/HidePrompt
/// from game logic to display the appropriate sprite for the current controller type.
/// Multi-sprite prompts cycle through their frames at m_cycleInterval seconds per frame.
/// </summary>
public class FloatingUI : MonoBehaviour
{
    [SerializeField] private GameObject         m_promptRoot;
    [SerializeField] private Image              m_promptImage;
    [SerializeField] private ButtonPromptData[] m_prompts;
    [SerializeField] private float              m_cycleInterval = 0.35f;

    private ControllerType m_controllerType = ControllerType.PC;
    private string         m_currentActionId;
    private Coroutine      m_cycleCoroutine;
    private Coroutine      m_hideCoroutine;

    private void Start()
    {
        transform.forward = Camera.main.transform.forward;
    }

    /// <summary>Call once after the player's input devices are assigned.</summary>
    public void Init(InputDevice[] devices)
    {
        m_controllerType = DetectControllerType(devices);
        RefreshPrompt();
    }

    private void Update()
    {
       
    }

    public void ShowPrompt(string actionId, float duration = 0f)
    {
        m_currentActionId = actionId;
        StopHide();
        RefreshPrompt();
        if (duration > 0f)
            m_hideCoroutine = StartCoroutine(HideAfterDelay(duration));
    }

    public void HidePrompt()
    {
        m_currentActionId = null;
        StopHide();
        StopCycle();
        if (m_promptRoot != null)
            m_promptRoot.SetActive(false);
    }

    private void RefreshPrompt()
    {
        StopCycle();

        if (string.IsNullOrEmpty(m_currentActionId) || m_promptRoot == null)
        {
            if (m_promptRoot != null)
                m_promptRoot.SetActive(false);
            return;
        }

        foreach (var prompt in m_prompts)
        {
            if (prompt.ActionId != m_currentActionId) continue;

            var sprites = prompt.GetSprites(m_controllerType);
            if (sprites == null || sprites.Length == 0)
            {
                m_promptRoot.SetActive(false);
                return;
            }

            m_promptRoot.SetActive(true);

            if (sprites.Length == 1)
            {
                if (m_promptImage != null) m_promptImage.sprite = sprites[0];
            }
            else
            {
                m_cycleCoroutine = StartCoroutine(CycleSprites(sprites));
            }
            return;
        }

        // No matching entry found — hide
        m_promptRoot.SetActive(false);
    }

    private IEnumerator CycleSprites(Sprite[] sprites)
    {
        int i = 0;
        while (true)
        {
            if (m_promptImage != null) m_promptImage.sprite = sprites[i];
            i = (i + 1) % sprites.Length;
            yield return new WaitForSeconds(m_cycleInterval);
        }
    }

    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HidePrompt();
    }

    private void StopHide()
    {
        if (m_hideCoroutine == null) return;
        StopCoroutine(m_hideCoroutine);
        m_hideCoroutine = null;
    }

    private void StopCycle()
    {
        if (m_cycleCoroutine == null) return;
        StopCoroutine(m_cycleCoroutine);
        m_cycleCoroutine = null;
    }

    private static ControllerType DetectControllerType(InputDevice[] devices)
    {
        foreach (var device in devices)
        {
            if (device is Keyboard)
                return ControllerType.PC;

            if (device is Gamepad)
            {
                var layout = device.layout.ToLowerInvariant();
                var mfr    = (device.description.manufacturer ?? string.Empty).ToLowerInvariant();

                if (layout.Contains("dualshock") || layout.Contains("dualsense") ||
                    mfr.Contains("sony"))
                    return ControllerType.PlayStation;

                // XInput / Xbox or unknown gamepad defaults to Xbox-style prompts
                return ControllerType.Xbox;
            }
        }

        return ControllerType.PC;
    }
}
