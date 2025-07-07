using UnityEngine;
using UnityEngine.UI;

public class ProgressBarUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;           // Drag your "Fill-Image" here
    [SerializeField] private GameObject uiContainer;    // Drag your "Canvas" here

    private void Start()
    {
        // Subscribe to global UI events
        EventManager.Instance.Subscribe<ProgressBarUpdateData>("updateProgressUI", OnUpdateProgressUI);
        EventManager.Instance.Subscribe<GameObject>("showProgressUI", OnShowUI);
        EventManager.Instance.Subscribe<GameObject>("hideProgressUI", OnHideUI);
    }

    private void OnDestroy()
    {
        // Always unsubscribe to prevent memory leaks
        EventManager.Instance.Unsubscribe<ProgressBarUpdateData>("updateProgressUI", OnUpdateProgressUI);
        EventManager.Instance.Unsubscribe<GameObject>("showProgressUI", OnShowUI);
        EventManager.Instance.Unsubscribe<GameObject>("hideProgressUI", OnHideUI);
    }

    private void OnUpdateProgressUI(ProgressBarUpdateData data)
    {
        if (data.target == this.gameObject && fillImage != null)
        {
            fillImage.fillAmount = Mathf.Clamp01(data.normalizedValue);
        }
    }

    private void OnShowUI(GameObject target)
    {
        if (target == this.gameObject && uiContainer != null)
        {
            uiContainer.SetActive(true);
        }
    }

    private void OnHideUI(GameObject target)
    {
        if (target == this.gameObject && uiContainer != null)
        {
            uiContainer.SetActive(false);
        }
    }
}

public class ProgressBarUpdateData
{
    public GameObject target;
    public float normalizedValue;

    public ProgressBarUpdateData(GameObject target, float normalizedValue)
    {
        this.target = target;
        this.normalizedValue = normalizedValue;
    }
}
