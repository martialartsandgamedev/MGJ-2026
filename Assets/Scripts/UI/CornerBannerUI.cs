using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CornerBannerUI : MonoBehaviour
{
    public enum Corner { BottomLeft, BottomRight, TopLeft, TopRight }

    [SerializeField] private RectTransform _bannerRect;
    [SerializeField] private Image _bannerImage;
    [SerializeField] private Sprite[] _sprites;
    [SerializeField] private RectTransform _speechBubble;
    [SerializeField] private TextMeshProUGUI _fishInfoText;

    [Header("Settings")]
    [SerializeField] private Corner _corner = Corner.BottomRight;
    [SerializeField] private float _slideTime = 0.4f;
    [SerializeField] private float _displayDuration = 3f;
    [SerializeField] private float _spriteSwapInterval = 0.5f;
    [SerializeField] private LeanTweenType _easeIn = LeanTweenType.easeOutBack;
    [SerializeField] private LeanTweenType _easeOut = LeanTweenType.easeInBack;

    private Vector2 _onScreenPos;
    private Vector2 _offScreenPos;
    private int _slideTween = -1;
    private Coroutine _sequenceCoroutine;

    private readonly Queue<(int playerIndex, Fish fish, int earned, PlayerScore score)> _queue = new();

    public Fish LastCaughtFish { get; private set; }
    public int LastPlayerIndex { get; private set; }
    public int LastScoreEarned { get; private set; }
    public PlayerScore LastPlayerScore { get; private set; }

    private void Awake()
    {
        Canvas.ForceUpdateCanvases();
        _bannerRect.localRotation = Quaternion.Euler(0f, 0f, GetRotationAngle());
        _onScreenPos = GetOnScreenAnchoredPos();
        _offScreenPos = GetOffScreenAnchoredPos();
        _bannerRect.anchoredPosition = _offScreenPos;
        if (_speechBubble != null)
        {
            _speechBubble.gameObject.SetActive(false);
            // Counter-rotate so the bubble is axis-aligned with the screen
            _speechBubble.localRotation = Quaternion.Euler(0f, 0f, -GetRotationAngle());
            // Reposition in banner local space so it appears along the correct screen edge
            _speechBubble.anchoredPosition = GetBubbleLocalPos();
        }
    }

    private void OnEnable()
    {
        if (ScoreManager.ins != null)
            ScoreManager.ins.OnFishCaught += OnFishCaught;
    }

    private void OnDisable()
    {
        if (ScoreManager.ins != null)
            ScoreManager.ins.OnFishCaught -= OnFishCaught;
    }

    private void OnFishCaught(int playerIndex, Fish fish, int earned, PlayerScore score)
    {
        _queue.Enqueue((playerIndex, fish, earned, score));
        if (_sequenceCoroutine == null)
            _sequenceCoroutine = StartCoroutine(ProcessQueue());
    }

    /// <summary>Call this to trigger the banner to show with current fish data.</summary>
    public void Show()
    {
        Debug.Log("Show triggered");
        if (_sequenceCoroutine == null)
            _sequenceCoroutine = StartCoroutine(ProcessQueue());
    }

    private IEnumerator ProcessQueue()
    {
        while (_queue.Count > 0)
        {
            var entry = _queue.Dequeue();
            LastPlayerIndex = entry.playerIndex;
            LastCaughtFish = entry.fish;
            LastScoreEarned = entry.earned;
            LastPlayerScore = entry.score;
            yield return BannerSequence();
        }
        _sequenceCoroutine = null;
    }

    private IEnumerator BannerSequence()
    {
        // Cancel any running slide tween
        if (LeanTween.isTweening(_slideTween))
            LeanTween.cancel(_slideTween);

        // Slide in
        _bannerRect.anchoredPosition = _offScreenPos;
        bool slideInDone = false;
        _slideTween = LeanTween.value(gameObject, _offScreenPos, _onScreenPos, _slideTime)
            .setEase(_easeIn)
            .setOnUpdate((Vector2 v) => _bannerRect.anchoredPosition = v)
            .setOnComplete(() => slideInDone = true)
            .id;

        yield return new WaitUntil(() => slideInDone);

        if (_speechBubble != null)
        {
            if (_fishInfoText != null && LastCaughtFish != null)
                _fishInfoText.text = $"{LastPlayerScore.PlayerName} caught himself a {LastCaughtFish.ResolvedID}\n+{LastScoreEarned} pts";
            _speechBubble.gameObject.SetActive(true);
        }

        // Animate between sprites
        float elapsed = 0f;
        int spriteIndex = 0;
        if (_sprites != null && _sprites.Length > 0)
            _bannerImage.sprite = _sprites[0];

        while (elapsed < _displayDuration)
        {
            elapsed += Time.deltaTime;

            if (_sprites != null && _sprites.Length > 1)
            {
                int newIndex = Mathf.FloorToInt(elapsed / _spriteSwapInterval) % _sprites.Length;
                if (newIndex != spriteIndex)
                {
                    spriteIndex = newIndex;
                    _bannerImage.sprite = _sprites[spriteIndex];
                }
            }

            yield return null;
        }

        if (_speechBubble != null)
            _speechBubble.gameObject.SetActive(false);

        // Slide out
        bool slideOutDone = false;
        _slideTween = LeanTween.value(gameObject, _onScreenPos, _offScreenPos, _slideTime)
            .setEase(_easeOut)
            .setOnUpdate((Vector2 v) => _bannerRect.anchoredPosition = v)
            .setOnComplete(() => slideOutDone = true)
            .id;

        yield return new WaitUntil(() => slideOutDone);
    }

    private Vector2 GetBubbleLocalPos()
    {
        // Desired screen-space offset: move the bubble along the screen edge away from the corner
        float d = _bannerRect.rect.width * 0.5f;
        float screenX = (_corner == Corner.BottomRight || _corner == Corner.TopRight) ? -d : d;

        // Convert screen-space offset to banner local space using the inverse rotation
        float angle = -GetRotationAngle() * Mathf.Deg2Rad;
        float cos = Mathf.Cos(angle);
        float sin = Mathf.Sin(angle);
        return new Vector2(cos * screenX, sin * screenX);
    }

    private float GetRotationAngle() => _corner switch
    {
        Corner.BottomLeft  =>  45f,
        Corner.BottomRight => -45f,
        Corner.TopLeft     => -45f,
        Corner.TopRight    =>  45f,
        _ => 0f
    };

    private Vector2 GetCornerPos()
    {
        var canvasRect = _bannerRect.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        float halfW = canvasRect.rect.width * 0.5f;
        float halfH = canvasRect.rect.height * 0.5f;
        return _corner switch
        {
            Corner.BottomLeft  => new Vector2(-halfW, -halfH),
            Corner.BottomRight => new Vector2( halfW, -halfH),
            Corner.TopLeft     => new Vector2(-halfW,  halfH),
            Corner.TopRight    => new Vector2( halfW,  halfH),
            _ => Vector2.zero
        };
    }

    private Vector2 GetInwardDiagonal() => _corner switch
    {
        Corner.BottomLeft  => new Vector2( 1f,  1f).normalized,
        Corner.BottomRight => new Vector2(-1f,  1f).normalized,
        Corner.TopLeft     => new Vector2( 1f, -1f).normalized,
        Corner.TopRight    => new Vector2(-1f, -1f).normalized,
        _ => Vector2.zero
    };

    // On-screen: banner center sits quarter of its width inward from the corner
    private Vector2 GetOnScreenAnchoredPos()
        => GetCornerPos() + GetInwardDiagonal() * (_bannerRect.rect.width * 0.1f);

    // Off-screen: banner center is one full width outside the corner
    private Vector2 GetOffScreenAnchoredPos()
        => GetCornerPos() - GetInwardDiagonal() * _bannerRect.rect.width;
}
