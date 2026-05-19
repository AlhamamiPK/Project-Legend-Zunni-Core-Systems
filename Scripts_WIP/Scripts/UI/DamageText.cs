using UnityEngine;
using TMPro;
using MoreMountains.Feedbacks;
public class DamageText : MonoBehaviour
{
    [Header("References")]
    public TextMeshPro textMesh;
    public MMF_Player feelPlayer;

    [Header("Colors & Sizes")]
    public Color normalColor = Color.white;
    public Color critColor = Color.red;
    [SerializeField] public float normalSize = 20f;
    [SerializeField] public float critSize = 32f;


    public void SetupText(double damageAmount, bool isCrit)
    {
        // Set the text to the damage amount (F0 removes decimal places)
        textMesh.text = GameManager.FormatNumber(damageAmount);

        if (isCrit)
        {
            textMesh.color = critColor;
            textMesh.fontSize = critSize;
            textMesh.text = textMesh.text + "!"; // Adds a cool exclamation mark for crits
        }
        else
        {
            textMesh.color = normalColor;
            textMesh.fontSize = normalSize;
        }

        // Play the Feel feedbacks (bounce, fade, move)
        if (feelPlayer != null)
        {
            //Destroy(gameObject, 2f);
            feelPlayer.PlayFeedbacks();

            // Destroy this object after the Feel animation finishes
            Destroy(gameObject, feelPlayer.TotalDuration);
        }
    }
}
