using UnityEngine;
using UnityEngine.UI; // Still need this for UI components!
using TMPro;
using System.Collections;
public class XPBarUI : MonoBehaviour
{
    [Header("UI References")]
    public Slider xpSlider; // Changed from Image to Slider
    public Image xpFillImage;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI howMuchToLevelUp;
    
    [Header("Seetings")]
    public Color flashColor = Color.white;
    private Color originalColor;

    #region Make it Public
    public static XPBarUI XPBARGlobalScript;
    private void Awake()
    {
        if (XPBARGlobalScript == null) XPBARGlobalScript = this;
        else Destroy(gameObject);
    }
    #endregion
    private void Start()
    {
        if (xpFillImage != null) { 
        originalColor = xpFillImage.color;
        }
    }
    void Update()
    {
        
    }
  
    public void FlashBar()
    {
        StopAllCoroutines();
        StartCoroutine(FlashRoutine());
    }
    IEnumerator FlashRoutine()
    {
        xpFillImage.color = flashColor;

        yield return new WaitForSeconds(0.1f);

        xpFillImage.color = originalColor;

    }
}