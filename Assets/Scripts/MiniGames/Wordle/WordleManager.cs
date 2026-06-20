using System;
using System.Collections;
using System.Collections.Generic;
using MiniGames.Base;
using UnityEngine;
using Random = UnityEngine.Random;

public class WordleManager : MiniGameBase
{
    public static WordleManager Instance { get; private set; }

    [System.Serializable]
    public class GuessRow
    {
        public SpriteRenderer[] slots;
    }

    [System.Serializable]
    public class FeedbackRow
    {
        public SpriteRenderer[] markers;
    }

    private enum FeedbackState
    {
        Rejected,
        Unstable,
        Stable
    }

    [Header("Auto Setup")]
    [SerializeField] private bool autoFindBoard = true;
    [SerializeField] private Transform colorGridRoot;
    [SerializeField] private Transform colorCheckerSlotsRoot;
    [SerializeField] private Transform colorButtonsRoot;

    [Header("Board Size")]
    [SerializeField, Min(1)] private int boardRows = 5;
    [SerializeField, Min(1)] private int boardColumns = 5;
    [SerializeField] private bool hideUnusedBoardSlots = true;

    [Header("Board")]
    public GuessRow[] guessRows;
    public FeedbackRow[] resultTexts;
    public FeedbackRow[] checkStorageRows;

    [Header("Colors")]
    public Color[] palette =
    {
        Color.green,
        Color.cyan,
        Color.blue,
        Color.darkMagenta,
        Color.violet,
        Color.red,
        Color.orange,
        Color.yellow,
    };

    [Header("Slot Colors")]
    public Color emptySlotColor = Color.white;
    public Color stableColor = Color.green;
    public Color unstableColor = Color.yellow;
    public Color rejectedColor = Color.red;
    public Color checkStorageEmptyColor = Color.black;
    
    [Header("Feedback Reveal")]
    [SerializeField, Min(0f)] private float feedbackRevealDelay = 0.15f;
    [SerializeField, Range(0f, 1f)] private float feedbackSfxVolume = 1f;
    [SerializeField] private SoundId stableSfx = SoundId.Stable;
    [SerializeField] private SoundId unstableSfx = SoundId.Unstable;
    [SerializeField] private SoundId rejectedSfx = SoundId.Rejected;

    [Header("Rules")]
    [SerializeField] private bool useWordleStyleFeedback = true;
    public bool allowRepeatedColorsInAnswer = false;
    
    private int[] answer;
    private int[] currentGuess;

    private int currentRow;
    private int currentSlot;
    private int codeLength;
    private bool gameOver;
    private bool checkingGuess;
    private ColorButon[] colorButtons = Array.Empty<ColorButon>();

    protected override void Awake()
    {
        base.Awake();

        if (Instance && Instance != this)
        {
            Debug.LogWarning($"Replacing duplicate WordleManager instance on '{Instance.name}' with '{name}'.");
        }

        Instance = this;
    }

    private void Start()
    {
        SetupBoard();
        ApplyBoardSize();

        if (!ValidateSetup())
        {
            gameOver = true;
            return;
        }

        codeLength = guessRows[0].slots.Length;
        currentGuess = new int[codeLength];

        GenerateAnswer();
        ClearBoard();
        ResetCurrentGuess();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void OnDisable()
    {
        UnsubscribeInput();
    }

    public void PickColor(int colorIndex)
    {
        if (gameOver) return;
        if (checkingGuess) return;
        if (currentRow >= guessRows.Length) return;
        if (currentSlot >= codeLength) return;
        if (colorIndex < 0 || colorIndex >= palette.Length)
        {
            Debug.LogWarning($"Color index {colorIndex} is outside the Wordle palette.");
            return;
        }

        currentGuess[currentSlot] = colorIndex;
        guessRows[currentRow].slots[currentSlot].color = palette[colorIndex];

        currentSlot++;

        if (currentSlot >= codeLength)
        {
            CheckGuess();
        }
    }

    public override void OnFocusGained(TVInputRelay relay)
    {
        base.OnFocusGained(relay);
        CacheColorButtons();

        if (inputRelay != null)
        {
            inputRelay.PointerDown += HandlePointerDown;
        }
    }

    public override void OnFocusLost()
    {
        UnsubscribeInput();
        base.OnFocusLost();
    }

    private void HandlePointerDown(Vector3 worldPos)
    {
        if (!InFocus || gameOver || checkingGuess) return;

        CacheColorButtons();
        foreach (ColorButon button in colorButtons)
        {
            if (!button || !button.gameObject.activeInHierarchy) continue;

            Collider2D buttonCollider = button.GetComponent<Collider2D>();
            if (!buttonCollider || !buttonCollider.enabled) continue;
            if (!buttonCollider.OverlapPoint(worldPos)) continue;

            PickColor(button.GetColorIndex());
            return;
        }
    }

    private void UnsubscribeInput()
    {
        if (inputRelay != null)
        {
            inputRelay.PointerDown -= HandlePointerDown;
        }
    }

    private void CacheColorButtons()
    {
        Transform root = colorButtonsRoot ? colorButtonsRoot : GetMiniGameRoot();
        colorButtons = root ? root.GetComponentsInChildren<ColorButon>(true) : Array.Empty<ColorButon>();
    }

    private void CheckGuess()
    {
        checkingGuess = true;

        int stableCount = 0;
        int unstableCount = 0;
        FeedbackState[] feedbackStates = new FeedbackState[codeLength];
        bool[] answerUsed = new bool[codeLength];
        bool[] guessUsed = new bool[codeLength];

        for (int i = 0; i < codeLength; i++)
        {
            if (currentGuess[i] != answer[i]) continue;

            stableCount++;
            feedbackStates[i] = FeedbackState.Stable;
            answerUsed[i] = true;
            guessUsed[i] = true;
        }

        for (int guessIndex = 0; guessIndex < codeLength; guessIndex++)
        {
            if (guessUsed[guessIndex]) continue;

            for (int answerIndex = 0; answerIndex < codeLength; answerIndex++)
            {
                if (answerUsed[answerIndex]) continue;
                if (currentGuess[guessIndex] != answer[answerIndex]) continue;

                unstableCount++;
                feedbackStates[guessIndex] = FeedbackState.Unstable;
                answerUsed[answerIndex] = true;
                break;
            }
        }

        int rejectedCount = codeLength - stableCount - unstableCount;
        StartCoroutine(ResolveGuess(stableCount, unstableCount, rejectedCount, feedbackStates));
    }

    private IEnumerator ResolveGuess(int stableCount, int unstableCount, int rejectedCount, FeedbackState[] feedbackStates)
    {
        yield return RevealFeedback(stableCount, unstableCount, rejectedCount);
        StoreFeedback(currentRow, stableCount, unstableCount, rejectedCount, feedbackStates);

        if (stableCount == codeLength)
        {
            gameOver = true;
            checkingGuess = false;
            Debug.Log("Wordle solved.");
            GameWin();
            yield break;
        }

        currentRow++;
        if (currentRow >= guessRows.Length)
        {
            gameOver = true;
            checkingGuess = false;
            Debug.Log("Wordle failed: no guesses left.");
            GameLose();
            yield break;
        }

        ResetCurrentGuess();
        checkingGuess = false;
    }

    private void GenerateAnswer()
    {
        answer = new int[codeLength];

        if (allowRepeatedColorsInAnswer)
        {
            for (int i = 0; i < codeLength; i++)
            {
                answer[i] = Random.Range(0, palette.Length);
            }

            return;
        }

        List<int> availableColors = new List<int>(palette.Length);
        for (int i = 0; i < palette.Length; i++)
        {
            availableColors.Add(i);
        }

        for (int i = 0; i < codeLength; i++)
        {
            int availableIndex = Random.Range(0, availableColors.Count);
            answer[i] = availableColors[availableIndex];
            availableColors.RemoveAt(availableIndex);
        }
    }

    private void ClearBoard()
    {
        foreach (GuessRow row in guessRows)
        {
            foreach (SpriteRenderer slot in row.slots)
            {
                if (slot) slot.color = emptySlotColor;
            }
        }

        ClearFeedback();
        ClearCheckStorage();
    }

    private void ClearFeedback()
    {
        foreach (FeedbackRow row in resultTexts)
        {
            foreach (SpriteRenderer marker in row.markers)
            {
                if (marker) marker.color = emptySlotColor;
            }
        }
    }

    private void ClearCheckStorage()
    {
        if (checkStorageRows == null) return;

        foreach (FeedbackRow row in checkStorageRows)
        {
            if (row.markers == null) continue;

            foreach (SpriteRenderer marker in row.markers)
            {
                if (marker) marker.color = checkStorageEmptyColor;
            }
        }
    }

    private void ResetCurrentGuess()
    {
        currentSlot = 0;

        for (int i = 0; i < currentGuess.Length; i++)
        {
            currentGuess[i] = -1;
        }
    }

    private IEnumerator RevealFeedback(int stableCount, int unstableCount, int rejectedCount)
    {
        ClearFeedback();

        yield return RevealFeedbackRow(0, stableCount, stableColor, stableSfx);
        yield return RevealFeedbackRow(1, unstableCount, unstableColor, unstableSfx);
        yield return RevealFeedbackRow(2, rejectedCount, rejectedColor, rejectedSfx);
    }

    private IEnumerator RevealFeedbackRow(int rowIndex, int count, Color color, SoundId soundId)
    {
        if (rowIndex < 0 || rowIndex >= resultTexts.Length) yield break;

        SpriteRenderer[] markers = resultTexts[rowIndex].markers;
        int revealCount = Mathf.Min(count, markers.Length);

        for (int i = 0; i < revealCount; i++)
        {
            if (!markers[i]) continue;

            markers[i].color = color;
            PlayFeedbackSfx(soundId);

            if (feedbackRevealDelay > 0f)
            {
                yield return new WaitForSeconds(feedbackRevealDelay);
            }
            else
            {
                yield return null;
            }
        }
    }

    private void PlayFeedbackSfx(SoundId soundId)
    {
        if (soundId == SoundId.None) return;

        AudioManager.Instance?.PlaySfx(soundId, feedbackSfxVolume);
    }

    private void StoreFeedback(int storageRowIndex, int stableCount, int unstableCount, int rejectedCount, FeedbackState[] feedbackStates)
    {
        if (checkStorageRows == null) return;
        if (storageRowIndex < 0 || storageRowIndex >= checkStorageRows.Length) return;

        SpriteRenderer[] markers = checkStorageRows[storageRowIndex].markers;
        if (markers == null) return;

        if (useWordleStyleFeedback)
        {
            StoreFeedbackByGuessPosition(markers, feedbackStates);
            return;
        }

        int markerIndex = 0;
        markerIndex = StoreFeedbackCells(markers, markerIndex, stableCount, stableColor);
        markerIndex = StoreFeedbackCells(markers, markerIndex, unstableCount, unstableColor);
        markerIndex = StoreFeedbackCells(markers, markerIndex, rejectedCount, rejectedColor);

        for (int i = markerIndex; i < markers.Length; i++)
        {
            if (markers[i]) markers[i].color = checkStorageEmptyColor;
        }
    }

    private void StoreFeedbackByGuessPosition(SpriteRenderer[] markers, FeedbackState[] feedbackStates)
    {
        int markerCount = Mathf.Min(markers.Length, feedbackStates.Length);

        for (int i = 0; i < markerCount; i++)
        {
            if (!markers[i]) continue;

            markers[i].color = FeedbackStateToColor(feedbackStates[i]);
        }

        for (int i = markerCount; i < markers.Length; i++)
        {
            if (markers[i]) markers[i].color = checkStorageEmptyColor;
        }
    }

    private Color FeedbackStateToColor(FeedbackState feedbackState)
    {
        switch (feedbackState)
        {
            case FeedbackState.Stable:
                return stableColor;
            case FeedbackState.Unstable:
                return unstableColor;
            default:
                return rejectedColor;
        }
    }

    private int StoreFeedbackCells(SpriteRenderer[] markers, int startIndex, int count, Color color)
    {
        int markerIndex = startIndex;
        int endIndex = Mathf.Min(startIndex + count, markers.Length);

        while (markerIndex < endIndex)
        {
            if (markers[markerIndex]) markers[markerIndex].color = color;
            markerIndex++;
        }

        return markerIndex;
    }

    private void SetupBoard()
    {
        if (!autoFindBoard) return;

        if (guessRows == null || guessRows.Length == 0)
        {
            if (!colorGridRoot) colorGridRoot = FindSceneTransform("ColorGrid");
            if (!colorGridRoot) colorGridRoot = FindSceneTransform("Guess Grid");
            if (colorGridRoot) guessRows = BuildRows(colorGridRoot, false);
        }

        if (checkStorageRows == null || checkStorageRows.Length == 0)
        {
            if (!colorGridRoot) colorGridRoot = FindSceneTransform("ColorGrid");
            if (!colorGridRoot) colorGridRoot = FindSceneTransform("Guess Grid");
            if (colorGridRoot) checkStorageRows = BuildCheckStorageRows(colorGridRoot);
        }

        if (resultTexts == null || resultTexts.Length == 0)
        {
            if (!colorCheckerSlotsRoot) colorCheckerSlotsRoot = FindSceneTransform("ColorCheckerSlots");
            if (!colorCheckerSlotsRoot) colorCheckerSlotsRoot = FindSceneTransform("ColorChekerSlots");
            if (!colorCheckerSlotsRoot) colorCheckerSlotsRoot = FindSceneTransform("Active Feedback Rows");
            if (colorCheckerSlotsRoot) resultTexts = BuildFeedbackRows(colorCheckerSlotsRoot);
        }
    }

    private void ApplyBoardSize()
    {
        int activeRows = Mathf.Max(1, boardRows);
        int activeColumns = Mathf.Max(1, boardColumns);

        if (hideUnusedBoardSlots)
        {
            SetGuessRowsVisibility(guessRows, activeRows, activeColumns);
            SetFeedbackRowsVisibility(checkStorageRows, activeRows, activeColumns);
            SetFeedbackRowsVisibility(resultTexts, resultTexts?.Length ?? 0, activeColumns);
        }

        guessRows = LimitGuessRows(guessRows, activeRows, activeColumns);
        checkStorageRows = LimitFeedbackRows(checkStorageRows, activeRows, activeColumns);
        resultTexts = LimitFeedbackRows(resultTexts, resultTexts?.Length ?? 0, activeColumns);
    }

    private GuessRow[] LimitGuessRows(GuessRow[] rows, int maxRows, int maxColumns)
    {
        if (rows == null) return null;

        int rowCount = Mathf.Min(rows.Length, maxRows);
        GuessRow[] limitedRows = new GuessRow[rowCount];

        for (int i = 0; i < rowCount; i++)
        {
            limitedRows[i] = new GuessRow
            {
                slots = LimitRenderers(rows[i]?.slots, maxColumns)
            };
        }

        return limitedRows;
    }

    private FeedbackRow[] LimitFeedbackRows(FeedbackRow[] rows, int maxRows, int maxColumns)
    {
        if (rows == null) return null;

        int rowCount = Mathf.Min(rows.Length, maxRows);
        FeedbackRow[] limitedRows = new FeedbackRow[rowCount];

        for (int i = 0; i < rowCount; i++)
        {
            limitedRows[i] = new FeedbackRow
            {
                markers = LimitRenderers(rows[i]?.markers, maxColumns)
            };
        }

        return limitedRows;
    }

    private SpriteRenderer[] LimitRenderers(SpriteRenderer[] renderers, int maxCount)
    {
        if (renderers == null) return Array.Empty<SpriteRenderer>();

        int count = Mathf.Min(renderers.Length, maxCount);
        SpriteRenderer[] limitedRenderers = new SpriteRenderer[count];
        Array.Copy(renderers, limitedRenderers, count);
        return limitedRenderers;
    }

    private void SetGuessRowsVisibility(GuessRow[] rows, int activeRows, int activeColumns)
    {
        if (rows == null) return;

        for (int rowIndex = 0; rowIndex < rows.Length; rowIndex++)
        {
            SpriteRenderer[] slots = rows[rowIndex]?.slots;
            if (slots == null) continue;

            for (int columnIndex = 0; columnIndex < slots.Length; columnIndex++)
            {
                SetRendererVisible(slots[columnIndex], rowIndex < activeRows && columnIndex < activeColumns);
            }
        }
    }

    private void SetFeedbackRowsVisibility(FeedbackRow[] rows, int activeRows, int activeColumns)
    {
        if (rows == null) return;

        for (int rowIndex = 0; rowIndex < rows.Length; rowIndex++)
        {
            SpriteRenderer[] markers = rows[rowIndex]?.markers;
            if (markers == null) continue;

            for (int columnIndex = 0; columnIndex < markers.Length; columnIndex++)
            {
                SetRendererVisible(markers[columnIndex], rowIndex < activeRows && columnIndex < activeColumns);
            }
        }
    }

    private void SetRendererVisible(SpriteRenderer spriteRenderer, bool visible)
    {
        if (!spriteRenderer) return;

        spriteRenderer.gameObject.SetActive(visible);
    }

    private GuessRow[] BuildRows(Transform root, bool ignoreTextSprites)
    {
        List<SpriteRenderer> renderers = GetBoardRenderers(root, ignoreTextSprites, true);
        List<List<SpriteRenderer>> groupedRows = GroupRenderersIntoRows(renderers);
        GuessRow[] rows = new GuessRow[groupedRows.Count];

        for (int i = 0; i < groupedRows.Count; i++)
        {
            rows[i] = new GuessRow { slots = groupedRows[i].ToArray() };
        }

        return rows;
    }

    private FeedbackRow[] BuildCheckStorageRows(Transform root)
    {
        List<Transform> storageTransforms = new List<Transform>();
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (!IsCheckStorageTransform(child)) continue;

            List<SpriteRenderer> markers = GetLeafRenderers(child);
            if (markers.Count == 0) continue;

            storageTransforms.Add(child);
        }

        storageTransforms.Sort((left, right) =>
        {
            int yCompare = right.position.y.CompareTo(left.position.y);
            if (yCompare != 0) return yCompare;

            return left.position.x.CompareTo(right.position.x);
        });

        FeedbackRow[] rows = new FeedbackRow[storageTransforms.Count];
        for (int i = 0; i < storageTransforms.Count; i++)
        {
            List<SpriteRenderer> markers = GetLeafRenderers(storageTransforms[i]);
            markers.Sort((left, right) => left.transform.position.x.CompareTo(right.transform.position.x));
            rows[i] = new FeedbackRow { markers = markers.ToArray() };
        }

        return rows;
    }

    private FeedbackRow[] BuildFeedbackRows(Transform root)
    {
        List<SpriteRenderer> renderers = GetBoardRenderers(root, true, false);
        List<List<SpriteRenderer>> groupedRows = GroupRenderersIntoRows(renderers);
        FeedbackRow[] rows = new FeedbackRow[groupedRows.Count];

        for (int i = 0; i < groupedRows.Count; i++)
        {
            rows[i] = new FeedbackRow { markers = groupedRows[i].ToArray() };
        }

        return rows;
    }

    private List<SpriteRenderer> GetBoardRenderers(Transform root, bool ignoreTextSprites, bool ignoreCheckStorage)
    {
        SpriteRenderer[] allRenderers = root.GetComponentsInChildren<SpriteRenderer>(true);
        List<SpriteRenderer> renderers = new List<SpriteRenderer>();

        foreach (SpriteRenderer spriteRenderer in allRenderers)
        {
            if (ignoreCheckStorage && IsInsideCheckStorage(spriteRenderer.transform))
            {
                continue;
            }

            if (spriteRenderer.transform.childCount > 0)
            {
                continue;
            }

            if (ignoreTextSprites && IsIgnoredBoardRendererName(spriteRenderer.name))
            {
                continue;
            }

            renderers.Add(spriteRenderer);
        }

        return renderers;
    }

    private bool IsIgnoredBoardRendererName(string objectName)
    {
        return objectName.IndexOf("Text", StringComparison.OrdinalIgnoreCase) >= 0 ||
               objectName.IndexOf("Label", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private List<SpriteRenderer> GetLeafRenderers(Transform root)
    {
        SpriteRenderer[] allRenderers = root.GetComponentsInChildren<SpriteRenderer>(true);
        List<SpriteRenderer> renderers = new List<SpriteRenderer>();

        foreach (SpriteRenderer spriteRenderer in allRenderers)
        {
            if (spriteRenderer.transform.childCount > 0) continue;
            renderers.Add(spriteRenderer);
        }

        return renderers;
    }

    private bool IsInsideCheckStorage(Transform child)
    {
        Transform current = child;
        while (current)
        {
            if (IsCheckStorageTransform(current)) return true;
            current = current.parent;
        }

        return false;
    }

    private bool IsCheckStorageTransform(Transform child)
    {
        return child.name.IndexOf("ChekStorage", StringComparison.OrdinalIgnoreCase) >= 0 ||
               child.name.IndexOf("CheckStorage", StringComparison.OrdinalIgnoreCase) >= 0 ||
               child.name.IndexOf("Check Storage Row", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private List<List<SpriteRenderer>> GroupRenderersIntoRows(List<SpriteRenderer> renderers)
    {
        renderers.Sort((left, right) =>
        {
            int yCompare = right.transform.position.y.CompareTo(left.transform.position.y);
            if (yCompare != 0) return yCompare;

            return left.transform.position.x.CompareTo(right.transform.position.x);
        });

        List<List<SpriteRenderer>> rows = new List<List<SpriteRenderer>>();
        const float rowTolerance = 0.2f;

        foreach (SpriteRenderer spriteRenderer in renderers)
        {
            bool addedToRow = false;
            float y = spriteRenderer.transform.position.y;

            foreach (List<SpriteRenderer> row in rows)
            {
                if (Mathf.Abs(row[0].transform.position.y - y) > rowTolerance) continue;

                row.Add(spriteRenderer);
                addedToRow = true;
                break;
            }

            if (!addedToRow)
            {
                rows.Add(new List<SpriteRenderer> { spriteRenderer });
            }
        }

        foreach (List<SpriteRenderer> row in rows)
        {
            row.Sort((left, right) => left.transform.position.x.CompareTo(right.transform.position.x));
        }

        return rows;
    }

    private Transform FindSceneTransform(string objectName)
    {
        Transform root = GetMiniGameRoot();
        if (root)
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == objectName) return child;
            }
        }

        Transform[] transforms = FindObjectsByType<Transform>(FindObjectsInactive.Include);
        foreach (Transform sceneTransform in transforms)
        {
            if (sceneTransform.name == objectName) return sceneTransform;
        }

        return null;
    }

    private Transform GetMiniGameRoot()
    {
        return transform.parent ? transform.parent : transform;
    }

    private bool ValidateSetup()
    {
        if (palette == null || palette.Length == 0)
        {
            Debug.LogError("WordleManager needs at least one color in the palette.");
            return false;
        }

        if (guessRows == null || guessRows.Length == 0 || guessRows[0].slots == null || guessRows[0].slots.Length == 0)
        {
            Debug.LogError("WordleManager needs guess rows. Assign them in the inspector or name the grid root 'ColorGrid'.");
            return false;
        }

        int expectedLength = guessRows[0].slots.Length;
        foreach (GuessRow row in guessRows)
        {
            if (row.slots == null || row.slots.Length != expectedLength)
            {
                Debug.LogError("Every Wordle guess row needs the same number of slots.");
                return false;
            }
        }

        if (!allowRepeatedColorsInAnswer && palette.Length < expectedLength)
        {
            Debug.LogError("The Wordle palette needs at least as many colors as slots when repeated answer colors are disabled.");
            return false;
        }

        if (resultTexts == null || resultTexts.Length < 3)
        {
            Debug.LogError("WordleManager needs three feedback rows: stable, unstable, and rejected.");
            return false;
        }

        for (int i = 0; i < resultTexts.Length; i++)
        {
            if (resultTexts[i].markers == null || resultTexts[i].markers.Length == 0)
            {
                Debug.LogError($"Wordle feedback row {i} has no marker slots.");
                return false;
            }
        }

        if (checkStorageRows != null && checkStorageRows.Length > 0 && checkStorageRows.Length < guessRows.Length)
        {
            Debug.LogWarning("WordleManager found fewer check storage rows than guess rows. Missing rows will not store feedback.");
        }

        if (checkStorageRows != null)
        {
            for (int i = 0; i < checkStorageRows.Length; i++)
            {
                if (checkStorageRows[i].markers == null) continue;
                if (checkStorageRows[i].markers.Length >= expectedLength) continue;

                Debug.LogWarning($"Wordle check storage row {i} has fewer markers than the guess length. Extra feedback will not be stored.");
            }
        }

        return true;
    }
}
