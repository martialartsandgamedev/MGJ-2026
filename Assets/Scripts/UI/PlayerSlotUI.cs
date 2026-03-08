using TMPro;
using UnityEngine;

public class PlayerSlotUI : MonoBehaviour
{
    [SerializeField] private GameObject _emptyRoot;
    [SerializeField] private TextMeshProUGUI _slotLabel;

    [SerializeField] private GameObject _activeRoot;
    [SerializeField] private TextMeshProUGUI _playerNameText;
    [SerializeField] private TextMeshProUGUI _bankedScoreText;
    [SerializeField] private TextMeshProUGUI _heldText;

    private string _playerName;

    public void SetEmpty(int slotNumber)
    {
        _emptyRoot.SetActive(true);
        _activeRoot.SetActive(false);
        if (_slotLabel != null)
            _slotLabel.text = $"PLAYER {slotNumber + 1}";
    }

    public void SetActive(string playerName)
    {
        _playerName = playerName;
        _emptyRoot.SetActive(false);
        _activeRoot.SetActive(true);
        if (_playerNameText != null)
            _playerNameText.text = playerName;
        if (_bankedScoreText != null)
            _bankedScoreText.text = "0 pts banked";
        if (_heldText != null)
            _heldText.text = "0 held";
    }

    public void UpdateInventory(PlayerInventory inventory)
    {
        if (_playerNameText != null)
            _playerNameText.text = _playerName;
        if (_bankedScoreText != null)
            _bankedScoreText.text = $"{inventory.BankedScore} pts banked";
        if (_heldText != null)
            _heldText.text = $"{inventory.HeldCount} held ({inventory.HeldScore} pts)";
    }
}
