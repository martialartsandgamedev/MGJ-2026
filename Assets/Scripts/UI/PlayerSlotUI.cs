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
    private int _lastBankedScore;
    private int _lastHeldCount;

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
        _lastBankedScore = 0;
        _lastHeldCount = 0;
        _emptyRoot.SetActive(false);
        _activeRoot.SetActive(true);
        if (_playerNameText != null)
            _playerNameText.text = playerName;
        if (_bankedScoreText != null)
            _bankedScoreText.text = "Bank: 0 points";
        if (_heldText != null)
            _heldText.text = "Held: 0";
    }

    public void UpdateInventory(PlayerInventory inventory)
    {
        if (_playerNameText != null)
            _playerNameText.text = _playerName;
        if (_bankedScoreText != null)
        {
            _bankedScoreText.text = $"Bank: {inventory.BankedScore} points";

            if (inventory.BankedScore > _lastBankedScore)
            {
                LeanTween.cancel(_bankedScoreText.gameObject);
                _bankedScoreText.transform.localScale = Vector3.one;
                LeanTween.scale(_bankedScoreText.gameObject, Vector3.one * 1.4f, 0.12f)
                    .setEase(LeanTweenType.easeOutBack)
                    .setOnComplete(() => LeanTween.scale(_bankedScoreText.gameObject, Vector3.one, 0.1f));
            }
        }
        _lastBankedScore = inventory.BankedScore;

        if (_heldText != null)
        {
            _heldText.text = $"Held: {inventory.HeldCount}";

            if (inventory.HeldCount < _lastHeldCount)
            {
                LeanTween.cancel(_heldText.gameObject);
                var origin = _heldText.transform.localPosition;
                LeanTween.value(_heldText.gameObject, 0f, 10f, 0.04f)
                    .setEase(LeanTweenType.easeOutQuad)
                    .setLoopPingPong(4)
                    .setOnUpdate((float x) => _heldText.transform.localPosition = origin + new Vector3(x, 0f, 0f))
                    .setOnComplete(() => _heldText.transform.localPosition = origin);
            }
        }
        _lastHeldCount = inventory.HeldCount;
    }
}
