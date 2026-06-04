using BreakInfinity;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handles infinite-scaling shop mechanics using BigDouble (BreakInfinity).
/// Uses a DRY architecture for dynamic multiplier calculations (x1, x10, x100, Max)
/// and cleanly isolates UI updates from backend mathematical logic.
/// </summary>
public class ShopManager : MonoBehaviour
{
    #region Inspector Fields

    [Header ("Runtime")]
    [Tooltip("Checks if the curser is touching the shop")]
    public static bool isHoveringShop {  get; private set; }

    [Space(5)]
    [Header("Custom Cursors")]
    public Sprite normalCursor;
    public Sprite shopCursor;
    public Vector2 cursorHotspot = Vector2.zero;

    [Space(5)]
    [Header("UI")]
    [SerializeField] TextMeshProUGUI attackPriceDisplay;
    [SerializeField] TextMeshProUGUI hpPriceDisplay;
    [SerializeField] TextMeshProUGUI critDamagePriceDisplay;
    [SerializeField] TextMeshProUGUI speedOfPlayerDisplay;
    [SerializeField] TextMeshProUGUI displayAllStatsOnTheRight;
    [SerializeField] public GameObject shopPanel;

    #endregion
    #region Prices

    private BigDouble attackPrice;
    private BigDouble hpPrice;
    private BigDouble critDamagePrice;
    private BigDouble speedOfPlayer;

    #endregion

    #region  Growth Rates

    private BigDouble attackGrowthRate = 1.07;
    private BigDouble hpGrowthRate = 1.07;
    private BigDouble critGrowthRate = 1.20;
    private BigDouble speedGrowthRate = 1.04;
    private BigDouble flatTax = 5;

    #endregion
   
    #region ENUMS - Multiplers

    [Space(5)]
    [Header("ENMUS")]
    public BuyMultiplier currentMultiplier = BuyMultiplier.x1;
    public enum BuyMultiplier { x1, x10, x100, Max }// No ; for some reason
    #endregion

    #region Unity Lifecycle

    // Awake: Wire up component references before Start initializes values.

    private void Awake()
    {
        TextMeshProUGUI[] allTexts = shopPanel.GetComponentsInChildren<TextMeshProUGUI>(true);

        foreach (TextMeshProUGUI textComponent in allTexts)
        {

            if (attackPriceDisplay == null && textComponent.gameObject.name == "AttackPrice")
            {
                attackPriceDisplay = textComponent;
            }
            else if (hpPriceDisplay == null && textComponent.gameObject.name == "HealthPrice")
            {
                hpPriceDisplay = textComponent;
            }
            else if (critDamagePriceDisplay == null && textComponent.gameObject.name == "CritPrice")
            {
                critDamagePriceDisplay = textComponent;
            }
            else if (speedOfPlayerDisplay == null && textComponent.gameObject.name == "SpeedPrice")
            {
                speedOfPlayerDisplay = textComponent;
            }
            else if (displayAllStatsOnTheRight == null && textComponent.gameObject.name == "StatsDisplayText")
            {
                displayAllStatsOnTheRight = textComponent;
            }
        }

    }
    // Start: All references are ready — initialize prices and UI.

    private void Start()
    {
        if (!PlayersSavedData.instance.didWeUseTheShopPrior)
        {
            attackPrice = 3;
            hpPrice = 1;
            critDamagePrice = 10;
            speedOfPlayer = 9;
            PlayersSavedData.instance.savedAttackPrice = attackPrice;
            PlayersSavedData.instance.savedHpPrice = hpPrice;
            PlayersSavedData.instance.savedCritPrice = critDamagePrice;
            PlayersSavedData.instance.savedSpeedPrice = speedOfPlayer;
        }
        else 
        {
            attackPrice = PlayersSavedData.instance.savedAttackPrice;
            hpPrice = PlayersSavedData.instance.savedHpPrice;
            critDamagePrice = PlayersSavedData.instance.savedCritPrice;
            speedOfPlayer = PlayersSavedData.instance.savedSpeedPrice;
        }

        DisplayAllStatsOnRight();
        attackPriceDisplay.text = GameManager.FormatNumber(attackPrice);
        hpPriceDisplay.text = GameManager.FormatNumber(hpPrice);
        critDamagePriceDisplay.text = GameManager.FormatNumber(critDamagePrice);
        speedOfPlayerDisplay.text = GameManager.FormatNumber(speedOfPlayer);
        SetupPanelHoverEvents();

    }


    #endregion

    #region Hover Events
    private void SetupPanelHoverEvents()
    {
        // 1. Get or add an EventTrigger to the shopPanel
        EventTrigger trigger = shopPanel.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = shopPanel.AddComponent<EventTrigger>();
        }

        // 2. Create the "Mouse Enter" event
        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((data) => { OnShopHoverEnter(); });
        trigger.triggers.Add(enterEntry);

        // 3. Create the "Mouse Exit" event
        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((data) => { OnShopHoverExit(); });
        trigger.triggers.Add(exitEntry);
    }
    public void OnShopHoverEnter()
    {
        isHoveringShop = true;
        CustomMouse.Instance.imageForMyMouse.sprite = shopCursor;
    }

    // Triggered by the EventTrigger when mouse leaves shopPanel
    public void OnShopHoverExit()
    {
        isHoveringShop = false;
        CustomMouse.Instance.imageForMyMouse.sprite = normalCursor;
    }

    #endregion

    #region Trigger Enter/Exit
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            shopPanel.SetActive(true);
        }
    }
    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            shopPanel.SetActive(false);
            OnShopHoverExit();
        }

    }
    #endregion
    
    #region Math Core
    public BigDouble CalculateCostForAmount(BigDouble currentPrice, BigDouble growthRate, BigDouble amount)
    {
        if (currentPrice <= 0) return 0;
        if (amount == 1) return currentPrice;
        return currentPrice * (BigDouble.Pow(growthRate, amount) - 1) / (growthRate - 1);
    }
    public BigDouble CalculateMaxAffordableAmount(BigDouble currentPrice, BigDouble growthRate, BigDouble totalMoney)
    {

        if (totalMoney < currentPrice) return 0;

        BigDouble insideLog = (totalMoney * (growthRate - 1) / currentPrice) + 1;

        // Use BigDouble.Log and BigDouble.Floor instead of System.Math
        BigDouble n = BigDouble.Log(insideLog, growthRate);
        return BigDouble.Floor(n);
    }
    private BigDouble GetPurchaseAmount(BigDouble currentPrice, BigDouble growthRate)
    {
        if (currentMultiplier == BuyMultiplier.x1) return 1;
        if (currentMultiplier == BuyMultiplier.x10) return 10;
        if (currentMultiplier == BuyMultiplier.x100) return 100;
        if (currentMultiplier == BuyMultiplier.Max) return CalculateMaxAffordableAmount(currentPrice, growthRate, CurrencyManager.Instance.totalMoney);
        return 0;
    }
    #endregion

    #region Buy Logic
    private void TryBuyUpgrade(ref BigDouble price, BigDouble growthRate, TextMeshProUGUI priceDisplay  , 
                               System.Action<BigDouble> applyUpgrade, System.Action<BigDouble> savePrice)
    {
        BigDouble amount = GetPurchaseAmount(price, growthRate);
        BigDouble totalCost = CalculateCostForAmount(price, growthRate,amount) + flatTax;
        priceDisplay.text = GameManager.FormatNumber(totalCost);

        if (CurrencyManager.Instance.totalMoney < totalCost) return;

        CurrencyManager.Instance.totalMoney -= totalCost;
        CurrencyManager.Instance.AddCurrency(0f);

        applyUpgrade(amount);

        price *= BigDouble.Pow(growthRate, amount);
        savePrice(price);
        PlayersSavedData.instance.didWeUseTheShopPrior = true;
        DisplayAllStatsOnRight();
        UpdateAllShopUI();
        AudioPlayer.Instance.SpendMoney();

    }
    #endregion

    #region Upgrades 
    public void UpdateAllShopUI()
    {
        UpdateAttackPrice();
        UpdateHpPrice();
        UpdateCritAmpPrice();
        UpdateSpeedPrice();
    }
    public void SetMultiplier(int multiplierIndex)
    {
        currentMultiplier = (BuyMultiplier)multiplierIndex;
        UpdateAllShopUI();
    }
    public void UpdateAttackPrice()
    {

        BigDouble growthRate = attackGrowthRate;
        BigDouble amount = GetPurchaseAmount(attackPrice, growthRate);
        BigDouble totalCost = CalculateCostForAmount(attackPrice, growthRate, amount) + flatTax;

        attackPriceDisplay.text = GameManager.FormatNumber(totalCost);
    }
    public void AttackIncreaseShop()
    {


        TryBuyUpgrade(ref attackPrice,
                      attackGrowthRate,
                      attackPriceDisplay, 
                      amount => PlayerStats.Instance.playerDamage *=(float)BigDouble.Pow(attackGrowthRate, amount).ToDouble(),
                      saved => PlayersSavedData.instance.savedAttackPrice = saved);
        

    }


    public void HealthIncreaseShop()
    {


        TryBuyUpgrade(ref hpPrice,
                      hpGrowthRate,
                      hpPriceDisplay,
                      amount => { PlayerStats.Instance.maxHealth *= (float)BigDouble.Pow(hpGrowthRate, amount).ToDouble();
                                  PlayerStats.Instance.currentHealth = PlayerStats.Instance.maxHealth;
                                  HealthBar.healthBarInstance.UpdateHealth();
                      },
                      saved => PlayersSavedData.instance.savedHpPrice = saved);

 
       

    }
    public void UpdateHpPrice()
    {

        BigDouble growthRate = hpGrowthRate; 
        BigDouble amount = GetPurchaseAmount(hpPrice, growthRate);

        BigDouble totalCost = CalculateCostForAmount(hpPrice, growthRate, amount) + flatTax;
        hpPriceDisplay.text = GameManager.FormatNumber(totalCost);
    }
    public void CritDamageIncreaseShop()
    {



        TryBuyUpgrade(ref critDamagePrice,
                      critGrowthRate,
                      critDamagePriceDisplay,
                      amount => PlayerStats.Instance.critDamageAmp *= (float)BigDouble.Pow(critGrowthRate, amount).ToDouble(),
                      saved => PlayersSavedData.instance.savedCritPrice = saved);
      

    }
    public void UpdateCritAmpPrice()
    {
     
        BigDouble growthRate = critGrowthRate; 
        BigDouble amount = GetPurchaseAmount(critDamagePrice, growthRate);

        BigDouble totalCost = CalculateCostForAmount(critDamagePrice, growthRate, amount) + flatTax;
        critDamagePriceDisplay.text = GameManager.FormatNumber(totalCost);
    }

    public void SpeedIncreaseShop()
    {

        TryBuyUpgrade(ref speedOfPlayer, 
                      speedGrowthRate,
                      speedOfPlayerDisplay,
                      amount => SpeedManager.instance.BuySpeed((int)amount.ToDouble()),
                      saved => PlayersSavedData.instance.savedSpeedPrice = saved);
        

    }
    public void UpdateSpeedPrice()
    {
      
        BigDouble growthRate = speedGrowthRate; 
        BigDouble amount = GetPurchaseAmount(speedOfPlayer, growthRate);


        BigDouble totalCost = CalculateCostForAmount(speedOfPlayer, growthRate, amount) + flatTax;
        speedOfPlayerDisplay.text = GameManager.FormatNumber(totalCost);
    }
    #endregion

    #region Stats Display
    public void DisplayAllStatsOnRight()
    {
        displayAllStatsOnTheRight.text =
                "ATK <pos=55%>" + GameManager.FormatNumber(PlayerStats.Instance.playerDamage) + "\n" +
                "HP <pos=55%>" + GameManager.FormatNumber(PlayerStats.Instance.maxHealth) + "\n" +
                "CRIT <pos=55%>" + GameManager.FormatNumber(PlayerStats.Instance.critDamageAmp) + "\n" +
                "SPD <pos=55%>" + GameManager.FormatNumber(PlayerStats.Instance.baseSpeed);
    }

    #endregion
}
