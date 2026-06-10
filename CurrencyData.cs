using UnityEngine;

/// <summary>
/// Defines a currency type: value, visuals, animation, and pickup sound.
/// Create via: Assets --> Create --> CurrencyData Data
/// </summary>
[CreateAssetMenu(fileName = "New Currency", menuName = "CurrencyData Data")]
public class CurrencyData : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Name of this currency type (e.g. Gold, Gem).")]
    public string currencyType;
    [Tooltip("How much this currency is worth when collected.")]
    public float currencyValue;
    [Header("Visuals")]
    [Tooltip("Sprite displayed in the world.")]
    public Sprite sprite;
    [Tooltip("Animation played on the pickup object.")]
    public AnimationClip animation;
    [Header("Audio")]
    [Tooltip("Sound played when the player collects this currency.")]
    public AudioClip pickUpSoundForTheCurrency;
}
