using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class TypewriterBase : MonoBehaviour
{
    [Header("Typing")]
    [SerializeField, Range(0.001f, 0.1f)] protected float charDelay = 0.02f;
    [SerializeField, Range(0f, 1f)] protected float spaceDelayMultiplier = 0.35f;
    [SerializeField, Min(0f)] protected float commaPause = 0.12f;
    [SerializeField, Min(0f)] protected float sentencePause = 0.22f;
    [SerializeField, Min(0f)] protected float ellipsisPause = 0.35f;
    [SerializeField, Min(0f)] protected float lineBreakPause = 0.25f;

    [Header("Advance")]
    [SerializeField, Min(0f)] protected float advancePromptDelay = 0.5f;

    protected Coroutine Typing;

    protected TypewriterSettings CurrentTypewriterSettings => new(
        charDelay,
        spaceDelayMultiplier,
        commaPause,
        sentencePause,
        ellipsisPause,
        lineBreakPause,
        advancePromptDelay);

    protected IEnumerator TypeText(TMP_Text targetText, string line)
    {
        yield return TypeText(targetText, line, CurrentTypewriterSettings);
    }

    protected IEnumerator TypeText(TMP_Text targetText, string line, TypewriterSettings settings)
    {
        if (!targetText) yield break;

        string text = line ?? string.Empty;
        bool skipTyping = false;
        targetText.text = "";

        for (int i = 0; i < text.Length; i++)
        {
            yield return WaitWhilePaused();

            if (WasAdvanceClickPressed())
            {
                skipTyping = true;
            }

            if (skipTyping)
            {
                targetText.text = text;
                yield break;
            }

            targetText.text += text[i];
            float delay = GetTypingDelay(text, i, settings);

            yield return WaitForTypingDelayOrSkip(delay, () => skipTyping = true);
            if (skipTyping)
            {
                targetText.text = text;
                yield break;
            }
        }

        targetText.text = text;
    }

    protected IEnumerator WaitForAdvancePromptDelay()
    {
        yield return WaitForAdvancePromptDelay(advancePromptDelay);
    }

    protected IEnumerator WaitForAdvancePromptDelay(float delay)
    {
        float elapsed = 0f;
        while (elapsed < delay)
        {
            if (!PauseMenueControler.IsPaused)
            {
                WasAdvanceClickPressed();
                elapsed += Time.unscaledDeltaTime;
            }

            yield return null;
        }
    }

    protected static IEnumerator WaitForAdvanceClick()
    {
        yield return null;

        while (true)
        {
            if (PauseMenueControler.IsPaused)
            {
                yield return null;
                continue;
            }

            if (WasAdvanceClickPressed())
            {
                yield break;
            }

            yield return null;
        }
    }

    protected static IEnumerator WaitWhilePaused()
    {
        while (PauseMenueControler.IsPaused)
        {
            yield return null;
        }
    }

    protected static float GetTypingDelay(string line, int characterIndex, TypewriterSettings settings)
    {
        if (string.IsNullOrEmpty(line) || characterIndex < 0 || characterIndex >= line.Length)
        {
            return settings.CharDelay;
        }

        char current = line[characterIndex];
        char previous = characterIndex > 0 ? line[characterIndex - 1] : '\0';
        char next = characterIndex + 1 < line.Length ? line[characterIndex + 1] : '\0';

        if (current == '\n') return settings.LineBreakPause;
        if (current == ' ') return settings.CharDelay * settings.SpaceDelayMultiplier;
        if (current == ',' || current == ';' || current == ':') return settings.CommaPause;

        if (current == '.')
        {
            if (next == '.') return settings.CharDelay;
            if (previous == '.') return settings.EllipsisPause;
            return settings.SentencePause;
        }

        if (current == '!' || current == '?') return settings.SentencePause;

        return settings.CharDelay;
    }

    protected static bool WasAdvanceClickPressed()
    {
        return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
    }

    private static IEnumerator WaitForTypingDelayOrSkip(float seconds, System.Action skipTyping)
    {
        float elapsed = 0f;
        while (elapsed < seconds)
        {
            if (PauseMenueControler.IsPaused)
            {
                yield return null;
                continue;
            }

            if (WasAdvanceClickPressed())
            {
                skipTyping?.Invoke();
                yield break;
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
    }
}

public readonly struct TypewriterSettings
{
    public readonly float CharDelay;
    public readonly float SpaceDelayMultiplier;
    public readonly float CommaPause;
    public readonly float SentencePause;
    public readonly float EllipsisPause;
    public readonly float LineBreakPause;
    public readonly float AdvancePromptDelay;

    public TypewriterSettings(
        float charDelay,
        float spaceDelayMultiplier,
        float commaPause,
        float sentencePause,
        float ellipsisPause,
        float lineBreakPause,
        float advancePromptDelay)
    {
        CharDelay = charDelay;
        SpaceDelayMultiplier = spaceDelayMultiplier;
        CommaPause = commaPause;
        SentencePause = sentencePause;
        EllipsisPause = ellipsisPause;
        LineBreakPause = lineBreakPause;
        AdvancePromptDelay = advancePromptDelay;
    }
}
