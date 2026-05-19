using UnityEngine;

[CreateAssetMenu(fileName = "New Currency", menuName = "CurrencyData Data")]
public class CurrencyData : ScriptableObject
{
    public string currencyType;
    public float currencyValue;
    public Sprite sprite;
    public AnimationClip animation;
    public AudioClip pickUpSoundForTheCurrency;
}
