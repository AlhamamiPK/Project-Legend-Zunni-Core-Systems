using UnityEngine;
using Unity.UI;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using BreakInfinity;
public class CurrencyManager : MonoBehaviour
{
   
    [Header("MoneyRelatedItems")]
    [SerializeField] public TextMeshProUGUI moneyCounter;
    [SerializeField] public Image missoMoneyIcon;

    public BigDouble totalMoney;

    #region making our script public
    public static CurrencyManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else Destroy(gameObject);
        
    }
    #endregion
    private void Start()
    {
        //totalMoney = 1000;
        // The first number is your base (1). 
        // The second number is exactly how many zeros you want (let's go with 1,000 zeros!)
       totalMoney = new BigDouble(1, 300);
    }
    public void AddCurrencey (BigDouble amount)
    {
        totalMoney += amount;
        moneyCounter.text = GameManager.FormatNumber(totalMoney)+"$";
        StopAllCoroutines();
        StartCoroutine(BiggerText());

    }
    IEnumerator BiggerText()
    {
        moneyCounter.color = Color.gold;
        moneyCounter.fontSize = 22;
        yield return new WaitForSeconds(0.1f);
        moneyCounter.fontSize = 20;
        moneyCounter.color = Color.white;
        
    }


}
