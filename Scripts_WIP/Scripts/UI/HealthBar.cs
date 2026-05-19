using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using MoreMountains;
using MoreMountains.Feedbacks;
public class HealthBar : MonoBehaviour
{

    [Header("Health")]
    [SerializeField] Slider healthSlider;
    [SerializeField] TextMeshProUGUI healthDisplay;
    [SerializeField] Image fillHealthBAR;
    [Header("WhiteFlashThings")]
    public Material whiteMat;
    private Material defultMat;

    [SerializeField] MMF_Player healthBadFeedBack;
    [SerializeField] private float healthBarFillFlash = 0.05f;
    public static HealthBar healthBarInstance;
    private void Awake()
    {
        if (healthBarInstance == null)  healthBarInstance = this;
    }  
    void Start()
    {
        defultMat = fillHealthBAR.material;
        healthSlider.maxValue = (float)PlayerStats.Instance.maxHealth;
        healthSlider.value = (float)PlayerStats.Instance.currentHealth;
    }
    public void PlayerIsDamaged()
    {
        healthBadFeedBack.PlayFeedbacks();
        healthSlider.maxValue = (float)PlayerStats.Instance.maxHealth;
        healthSlider.value = (float)PlayerStats.Instance.currentHealth;
        healthDisplay.text = GameManager.FormatNumber(PlayerStats.Instance.currentHealth) +"/"+GameManager.FormatNumber(PlayerStats.Instance.maxHealth);
    }
    private IEnumerator FlashRoutine()
    {
        fillHealthBAR.material = whiteMat;
        yield return new WaitForSeconds(healthBarFillFlash);
        fillHealthBAR.material = defultMat;
    }
    public void PlayFlash()
    {
        StartCoroutine(FlashRoutine());
    }
    public void UpdateHealth()
    {
        healthDisplay.text = GameManager.FormatNumber(PlayerStats.Instance.currentHealth) + "/" + GameManager.FormatNumber(PlayerStats.Instance.maxHealth);
        healthSlider.maxValue = (float)PlayerStats.Instance.maxHealth;
        healthSlider.value = (float)PlayerStats.Instance.currentHealth;
    }
}
