using System;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _ctaText = null;
    private int _ctaTween = -99;
    
    
    private void OnEnable()
    {
        WorldContextEvents.Ins.RareFishSpawned.AddListener(OnRareFishSpawned);
    }

    private void OnDisable()
    {
        WorldContextEvents.Ins.RareFishSpawned.RemoveListener(OnRareFishSpawned);
    }

    private void OnRareFishSpawned(FishDefinition context)
    {
        ShowCTA(String.Format($"A {context.ID} SPAWNED!"));
    }

    private void ShowCTA(string ctaString)
    {
        _ctaText.text = ctaString;

        LeanTween.cancel(_ctaTween);
        
        _ctaText.gameObject.SetActive(true);
        
        _ctaTween = LeanTween.value(1, 1.15f, 0.2f).setEase(LeanTweenType.easeInOutCubic).setLoopPingPong(4).setOnUpdate((float val) => {
            _ctaText.transform.localScale = Vector3.one * val;
        }).setOnComplete(() => {
            _ctaText.gameObject.SetActive(false);
        }).id;
    }
}
