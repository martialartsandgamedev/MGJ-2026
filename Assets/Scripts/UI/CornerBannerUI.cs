using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CornerBannerUI : MonoBehaviour
{
    [SerializeField] private RectTransform _bannerRect;
    [SerializeField] private Image _bannerImage;
    [SerializeField] private Sprite[] _sprites;
    [SerializeField] private RectTransform _speechBubble;
    [SerializeField] private TextMeshProUGUI _fishInfoText;

    [Header("Settings")]
    [SerializeField] private float _slideTime = 0.4f;
    [SerializeField] private float _displayDuration = 3f;
    [SerializeField] private float _spriteSwapInterval = 0.5f;
    [SerializeField] private float _yBuffer = 200f;
    [SerializeField] private LeanTweenType _easeIn = LeanTweenType.easeOutBack;
    [SerializeField] private LeanTweenType _easeOut = LeanTweenType.easeInBack;

    private Vector2 _onScreenPos;
    private Vector2 _offScreenPos;
    private int _slideTween = -1;
    private Coroutine _sequenceCoroutine;

    private readonly Queue<(int playerIndex, Fish fish, int earned, PlayerScore score)> _queue = new();
    private readonly Queue<string> _textQueue = new();

    public Fish LastCaughtFish { get; private set; }
    public int LastPlayerIndex { get; private set; }
    public int LastScoreEarned { get; private set; }
    public PlayerScore LastPlayerScore { get; private set; }

    private void Awake()
    {
        Canvas.ForceUpdateCanvases();
        // Start hidden off the right side; position will be randomized per-sequence
        var canvasRect = _bannerRect.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        float halfW = canvasRect.rect.width * 0.5f;
        _bannerRect.anchoredPosition = new Vector2(halfW + _bannerRect.rect.width, 0f);
        if (_speechBubble != null)
            _speechBubble.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (ScoreManager.ins != null)
            ScoreManager.ins.OnFishCaught += OnFishCaught;
        PlayerInventory.OnBanked += OnBanked;
    }

    private void OnDisable()
    {
        if (ScoreManager.ins != null)
            ScoreManager.ins.OnFishCaught -= OnFishCaught;
        PlayerInventory.OnBanked -= OnBanked;
    }

    private void OnBanked(string playerName, int amount)
    {
        _textQueue.Enqueue($"{playerName} banked\n+{amount} pts!");
        if (_sequenceCoroutine == null)
            _sequenceCoroutine = StartCoroutine(ProcessQueue());
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
        while (_queue.Count > 0 || _textQueue.Count > 0)
        {
            if (_queue.Count > 0)
            {
                var entry = _queue.Dequeue();
                LastPlayerIndex = entry.playerIndex;
                LastCaughtFish = entry.fish;
                LastScoreEarned = entry.earned;
                LastPlayerScore = entry.score;
                yield return BannerSequence(null);
            }
            else
            {
                yield return BannerSequence(_textQueue.Dequeue());
            }
        }
        _sequenceCoroutine = null;
    }

    private void SetupRandomSide()
    {
        bool rightSide = Random.value > 0.5f;

        // No rotation — flip the banner horizontally for the left side
        _bannerRect.localRotation = Quaternion.identity;
        _bannerRect.localScale = new Vector3(rightSide ? 1f : -1f, 1f, 1f);

        // Counter-flip the text so it stays readable on both sides
        if (_fishInfoText != null)
            _fishInfoText.transform.localScale = new Vector3(rightSide ? 1f : -1f, 1f, 1f);

        var canvasRect = _bannerRect.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        float halfW = canvasRect.rect.width * 0.5f;
        float halfH = canvasRect.rect.height * 0.5f;
        float randomY = Random.Range(-halfH + _yBuffer, halfH - _yBuffer);

        float edgeX = rightSide ? halfW : -halfW;
        float inward = rightSide ? -1f : 1f;
        _onScreenPos = new Vector2(edgeX + inward * _bannerRect.rect.width * 0.3f, randomY);
        _offScreenPos = new Vector2(edgeX - inward * _bannerRect.rect.width, randomY);

    }

    private IEnumerator BannerSequence(string overrideText = null)
    {
        SetupRandomSide();

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
            if (_fishInfoText != null)
                _fishInfoText.text = overrideText ?? $"{LastPlayerScore.PlayerName} caught himself a {LastCaughtFish.ResolvedID}\n+{LastScoreEarned} pts";
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
}
