using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using FStudio.MatchEngine;

public class NoRedCardManager : MonoBehaviour
{
    [Header("UI Text to display No Red Card timer")]
    public TextMeshProUGUI noRedCardText;

    private void Update()
    {
        HandleInput();
        UpdateTimerText();
    }

    private void HandleInput()
    {
        // Listen for H key using new Input System.
        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.hKey.wasPressedThisFrame)
        {
            MatchManager.ActivateNoRedCardMode();
        }
    }

    private void UpdateTimerText()
    {
        if (noRedCardText == null)
        {
            return;
        }

        if (MatchManager.IsNoRedCardActive)
        {
            float remaining = MatchManager.NoRedCardRemainingTime;

            noRedCardText.gameObject.SetActive(true);
            noRedCardText.text = $"No Red Card: {remaining:0.0}s";
        }
        else
        {
            noRedCardText.gameObject.SetActive(false);
        }
    }
}
