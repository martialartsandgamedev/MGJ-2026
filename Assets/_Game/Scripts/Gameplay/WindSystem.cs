using System.Linq;
using UnityEngine;

public class WindSystem : MonoBehaviour
{
    private const string ExternalVelocityKey = "wind";

    private enum State
    {
        READY, ACTIVATING, ACTIVE, ENDING, COOLDOWN
    }

    [SerializeField] private WindSettings _windSettings;

    private State _state;
    private float _stateTimer;

    private void Update()
    {
        Trigger();
        _stateTimer += Time.deltaTime;

        switch (_state)
        {
            case State.READY:
                // Idling until Trigger is called
                break;

            case State.ACTIVATING:
                if (_stateTimer >= _windSettings.WarningDuration)
                {
                    EnterActive();
                }
                break;

            case State.ACTIVE:
                TickActive(_stateTimer);
                if (_stateTimer >= _windSettings.GustDuration)
                {
                    EnterEnding();
                }
                break;

            case State.ENDING:
                EnterCooldown();
                break;

            case State.COOLDOWN:
                if (_stateTimer >= _windSettings.MinimumCooldown)
                {
                    EnterReady();
                }
                break;

            default:
                break;
        }
    }

    public void Trigger()
    {
        if (_state != State.READY)
        {
            return;
        }

        EnterActivating();
    }

    private readonly Vector3 _currentWindDirection = Vector3.right;

    private void TickActive(float elapsed)
    {
        var strength = _windSettings.strengthCurve.Evaluate(elapsed) * _windSettings.MaximumStrength;
        var velocity = _currentWindDirection * strength;

        foreach (var character in GameManager.Instance.Characters.Values)
        {
            if (character == null)
            {
                continue;
            }

            character.SetExternalVelocitySource(ExternalVelocityKey, velocity);
        }

    }

    // State transitions

    private void EnterReady()
    {
        _state = State.READY;
        _stateTimer = 0f;
    }

    private void EnterActivating()
    {
        _state = State.ACTIVATING;
        _stateTimer = 0f;
    }

    private void EnterActive()
    {
        _state = State.ACTIVE;
        _stateTimer = 0f;
    }

    private void EnterEnding()
    {
        _state = State.ENDING;
        _stateTimer = 0f;

        foreach (var character in GameManager.Instance.Characters.Values)
        {
            if (character == null)
            {
                continue;
            }

            character.ClearExternalVelocitySource(ExternalVelocityKey);
        }
    }

    private void EnterCooldown()
    {
        _state = State.COOLDOWN;
        _stateTimer = 0f;
    }
}
