using System;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager ins;

    /// <summary>
    /// Fired on every successful catch.
    /// Args: playerIndex, fish caught, score earned this catch, updated player total
    /// </summary>
    public event Action<int, Fish, int, PlayerScore> OnFishCaught;

    /// <summary>
    /// Fired when a player is finalized (despawned). Subscribe here to record/display their final score.
    /// The score is removed from active tracking after this fires.
    /// </summary>
    public event Action<PlayerScore> OnPlayerFinalized;

    private readonly Dictionary<int, PlayerScore> _scores = new();

    public PlayerScore SessionScore { get; private set; } = new() { PlayerName = "Session" };

    private static readonly int[] RarityWeights = { 1, 3, 6, 15 }; // Bronze, Silver, Gold, Unique
    private static readonly int[] SizeWeights   = { 1, 2, 3, 5  }; // Small, Medium, Large, Gargantuan

    private void Awake()
    {
        ins = this;
    }

    public static int CalculateFishScore(Fish fish)
        => RarityWeights[(int)fish.Rarity] * SizeWeights[(int)fish.Size];

    public void ReportCatch(int playerIndex, Fish fish)
    {
        if (!_scores.TryGetValue(playerIndex, out var score))
        {
            score = new PlayerScore
            {
                PlayerIndex = playerIndex,
                PlayerName = $"Player {playerIndex + 1}"
            };
            _scores[playerIndex] = score;
        }

        var earned = CalculateFishScore(fish);
        score.FishCount++;
        score.WeightedScore += earned;
        SessionScore.FishCount++;
        SessionScore.WeightedScore += earned;

        Debug.Log($"[ScoreManager] {score.PlayerName} caught {fish.ResolvedID} (+{earned} pts, total: {score.WeightedScore}, session: {SessionScore.WeightedScore})");

        OnFishCaught?.Invoke(playerIndex, fish, earned, score);
    }

    /// <summary>
    /// Call when a player despawns to archive their score and reset it for next spawn.
    /// Wire this to PlayerSpawner.PlayerDespawned in the Inspector.
    /// </summary>
    public void FinalizePlayer(int playerIndex)
    {
        if (!_scores.TryGetValue(playerIndex, out var score))
            return;

        Debug.Log($"[ScoreManager] Finalizing {score.PlayerName}: {score.FishCount} fish, {score.WeightedScore} pts");
        OnPlayerFinalized?.Invoke(score);
        _scores.Remove(playerIndex);
    }

    public PlayerScore GetScore(int playerIndex)
        => _scores.TryGetValue(playerIndex, out var s) ? s : null;

    public IEnumerable<PlayerScore> GetAllScores() => _scores.Values;
}
