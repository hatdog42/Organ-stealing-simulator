using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class OrganControler : MonoBehaviour
{
    [System.Serializable]
    private class ContainerFeedback
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite normalSprite;
        [SerializeField] private Sprite hoverSprite;
        [SerializeField] private Sprite clickedSprite;
        [SerializeField] private SoundId clickSound = SoundId.None;
        [SerializeField, Range(0f, 1f)] private float clickSoundVolume = 1f;

        public void Initialize(SoundId fallbackClickSound)
        {
            if (!spriteRenderer) return;
            if (!normalSprite) normalSprite = spriteRenderer.sprite;
            if (clickSound == SoundId.None) clickSound = fallbackClickSound;

            ShowNormal();
        }

        public void ShowNormal()
        {
            SetSprite(normalSprite);
        }

        public void ShowHover()
        {
            SetSprite(hoverSprite ? hoverSprite : normalSprite);
        }

        public void ShowClicked()
        {
            SetSprite(clickedSprite ? clickedSprite : hoverSprite ? hoverSprite : normalSprite);
        }

        public void PlayClickSound()
        {
            if (clickSound == SoundId.None || !AudioManager.Instance) return;

            AudioManager.Instance.PlaySfx(clickSound, clickSoundVolume);
        }

        private void SetSprite(Sprite sprite)
        {
            if (!spriteRenderer || !sprite) return;

            spriteRenderer.sprite = sprite;
        }
    }

    private enum OrganContainerChoice
    {
        None,
        OrganBox,
        MopBucket
    }

    private Camera _camera;
    private SpriteRenderer _spriteRenderer;
    [SerializeField]private string nextScene;
    [SerializeField]private LayerMask clickableMask;
    [SerializeField] private ContainerFeedback organBoxFeedback;
    [SerializeField] private ContainerFeedback mopBucketFeedback;
    [SerializeField, Min(0f)] private float clickFeedbackDuration = 0.15f;

    private bool _choiceMade;
    private OrganContainerChoice _hoveredChoice;
    
    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if(!_camera) _camera = Camera.main;
        organBoxFeedback?.Initialize(SoundId.Icebox);
        mopBucketFeedback?.Initialize(SoundId.MopBucket);
    }

    private void Update()
    {
        if (PauseMenueControler.IsPaused) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        
        Vector3 world = _camera.ScreenToWorldPoint(mousePos);
        world.z = 0f;
        transform.position = world;

        if (!_choiceMade)
        {
            SetHoveredChoice(GetContainerChoiceAt(world));
        }

        if (!Mouse.current.leftButton.wasPressedThisFrame) return;
        if (_choiceMade) return;

        OrganContainerChoice choice = _hoveredChoice;
        if (choice == OrganContainerChoice.None) return;

        _choiceMade = true;
        _spriteRenderer.enabled = false;
        ApplyClickedFeedback(choice);

        if (choice == OrganContainerChoice.OrganBox)
        {
            StartCoroutine(ChooseAfterFeedback(OrganBoxChosen));
        }
        else if (choice == OrganContainerChoice.MopBucket)
        {
            StartCoroutine(ChooseAfterFeedback(MopBucketChosen));
        }
    }

    private void OnDisable()
    {
        SetHoveredChoice(OrganContainerChoice.None);
    }

    private OrganContainerChoice GetContainerChoiceAt(Vector2 world)
    {
        Collider2D[] hits = Physics2D.OverlapPointAll(world, clickableMask);

        foreach (Collider2D hit in hits)
        {
            if (!hit || hit.transform.IsChildOf(transform)) continue;

            OrganContainerChoice choice = GetContainerChoice(hit.transform);
            if (choice != OrganContainerChoice.None) return choice;
        }

        return OrganContainerChoice.None;
    }

    private void SetHoveredChoice(OrganContainerChoice choice)
    {
        if (_hoveredChoice == choice) return;

        GetFeedback(_hoveredChoice)?.ShowNormal();
        _hoveredChoice = choice;
        GetFeedback(_hoveredChoice)?.ShowHover();
    }

    private void ApplyClickedFeedback(OrganContainerChoice choice)
    {
        ContainerFeedback feedback = GetFeedback(choice);
        if (feedback == null) return;

        feedback.ShowClicked();
        feedback.PlayClickSound();
    }

    private ContainerFeedback GetFeedback(OrganContainerChoice choice)
    {
        return choice switch
        {
            OrganContainerChoice.OrganBox => organBoxFeedback,
            OrganContainerChoice.MopBucket => mopBucketFeedback,
            _ => null
        };
    }

    private IEnumerator ChooseAfterFeedback(System.Action choose)
    {
        if (clickFeedbackDuration > 0f)
        {
            yield return new WaitForSecondsRealtime(clickFeedbackDuration);
        }

        choose?.Invoke();
    }

    private static OrganContainerChoice GetContainerChoice(Transform hitTransform)
    {
        for (Transform current = hitTransform; current; current = current.parent)
        {
            if (current.CompareTag("OrganBox")) return OrganContainerChoice.OrganBox;
            if (current.CompareTag("MopBucket")) return OrganContainerChoice.MopBucket;
        }

        return OrganContainerChoice.None;
    }

    private void OrganBoxChosen()
    {
        ApplyChoice(stoleOrgans: false, choseOrganBox: true);
    }

    private void MopBucketChosen()
    {
        ApplyChoice(stoleOrgans: true, choseOrganBox: false);
    }

    private void ApplyChoice(bool stoleOrgans, bool choseOrganBox)
    {
        if (HealthBars.Instance)
        {
            HealthBars.Instance.ApplyKilledPatient(stoleOrgans);
            HealthBars.Instance.bChooseOrganBox = choseOrganBox;
        }
        else
        {
            Debug.LogError($"{nameof(OrganControler)} could not find {nameof(HealthBars)} while applying organ choice.", this);
        }

        LoadNextScene();
    }

    private void LoadNextScene()
    {
        if (SceneController.Instance)
        {
            SceneController.Instance.LoadNextOrLoop();
            return;
        }

        if (!string.IsNullOrWhiteSpace(nextScene))
        {
            SceneManager.LoadScene(nextScene);
            return;
        }

        Debug.LogError($"{nameof(OrganControler)} cannot continue because no {nameof(SceneController)} exists and nextScene is empty.", this);
        _choiceMade = false;
        if (_spriteRenderer) _spriteRenderer.enabled = true;
    }
}
