using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Utility for triggering gamepad rumble. Call Rumble.Play() from anywhere.
/// </summary>
public static class Rumble
{
    /// <summary>
    /// Trigger rumble on a gamepad for a set duration.
    /// </summary>
    /// <param name="host">MonoBehaviour to run the coroutine on.</param>
    /// <param name="gamepad">The gamepad to rumble.</param>
    /// <param name="lowFreq">Low-frequency motor (0-1).</param>
    /// <param name="highFreq">High-frequency motor (0-1).</param>
    /// <param name="duration">Seconds to rumble.</param>
    public static void Play(MonoBehaviour host, Gamepad gamepad, float lowFreq, float highFreq, float duration)
    {
        if (gamepad == null) return;
        host.StartCoroutine(DoRumble(gamepad, lowFreq, highFreq, duration));
    }

    private static IEnumerator DoRumble(Gamepad gamepad, float lowFreq, float highFreq, float duration)
    {
        gamepad.SetMotorSpeeds(lowFreq, highFreq);
        yield return new WaitForSeconds(duration);
        gamepad.SetMotorSpeeds(0f, 0f);
    }
}
