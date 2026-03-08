using UnityEngine;

/// <summary>
/// Place this on a trigger collider to create a banking area.
/// When a player with held fish steps inside and presses Interact,
/// their held fish are banked and converted to points.
/// </summary>
public class BankingZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out PlayerInventory inventory)) return;

        inventory.NotifyEnterBankZone();

        if (inventory.HeldCount > 0 && other.TryGetComponent(out PlayerCharacter character))
            character.floatingUI.ShowPrompt("bank", 0f);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent(out PlayerInventory inventory)) return;

        inventory.NotifyExitBankZone();

        if (other.TryGetComponent(out PlayerCharacter character))
            character.floatingUI.HidePrompt();
    }
}
