/* * Script: ShopManager
 * Project: Legend Zunni
 * Description: Handles infinite-scaling shop mechanics using BigDouble (BreakInfinity).
 * Utilizes a DRY architecture for dynamic multiplier calculations (x1, x10, x100, Max) 
 * and cleanly isolates UI updates from backend mathematical logic.
 */
using BreakInfinity;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
public class ShopManager : MonoBehaviour
{
    public static bool isHoveringShop = false;
    [Header("Custom Cursors")]
    public Sprite normalCursor;
    public Sprite shopCursor;
    public Vector2 cursorHotspot = Vector2.zero;
    private BigDouble attackPrice;
    private BigDouble hpPrice;
    private BigDouble critDamagePrice;
    private BigDouble speedOfPlayer;
    [SerializeField] TextMeshProUGUI attackPriceDisplay;
    [SerializeField] TextMeshProUGUI hpPriceDisplay;
    [SerializeField] TextMeshProUGUI critDamagePriceDisplay;
    [SerializeField] TextMeshProUGUI speedOfPlayerDisplay;
    [SerializeField] TextMeshProUGUI displayAllStatsOnTheRight;
    [SerializeField] public GameObject shopPanel;
    [Header("Shop Balancing / Growth Rates")]
    private BigDouble attackGrowthRate = 1.07;
    private BigDouble hpGrowthRate = 1.07;
    private BigDouble critGrowthRate = 1.20;
    private BigDouble speedGrowthRate = 1.04;
    private BigDouble flatTax = 5;

    private void Start()
    {
        if (!PlayersSavedSata.instance.didWeUseTheShopPrior)
        {
            attackPrice = 3;
            hpPrice = 1;
            critDamagePrice = 10;
            speedOfPlayer = 9;
            PlayersSavedSata.instance.savedAttackPrice = attackPrice;
            PlayersSavedSata.instance.savedHpPrice = hpPrice;
            PlayersSavedSata.instance.savedCritPrice = critDamagePrice;
            PlayersSavedSata.instance.savedSpeedPrice = speedOfPlayer;
        }
        else 
        {
            attackPrice = PlayersSavedSata.instance.savedAttackPrice;
            hpPrice = PlayersSavedSata.instance.savedHpPrice;
            critDamagePrice = PlayersSavedSata.instance.savedCritPrice;
            speedOfPlayer = PlayersSavedSata.instance.savedSpeedPrice;
        }

        DisplayAllStatsOnRIght();
        attackPriceDisplay.text = GameManager.FormatNumber(attackPrice);
        hpPriceDisplay.text = GameManager.FormatNumber(hpPrice);
        critDamagePriceDisplay.text = GameManager.FormatNumber(critDamagePrice);
        speedOfPlayerDisplay.text = GameManager.FormatNumber(speedOfPlayer);
        SetupPanelHoverEvents();

    }

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
        //CustomMouse.Instance.mouseSkin.sprite = shopCursor;
    }

    // Triggered by the EventTrigger when mouse leaves shopPanel
    public void OnShopHoverExit()
    {
        isHoveringShop = false;
        CustomMouse.Instance.imageForMyMouse.sprite = normalCursor;
        // CustomMouse.Instance.mouseSkin.sprite = normalCursor;
    }
    //Later Make it look Good
    //This is called ENMUS  to use u need to use 0,1,2,3,4,etc 
    //Assagin it Using the Buttons//
    public enum BuyMultiplier { x1, x10, x100, Max }// No ; for some reason
    public BuyMultiplier currentMultiplier = BuyMultiplier.x1;

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
    public void UpdateAttackPrice()
    {

        BigDouble growthRate = attackGrowthRate; 
        BigDouble amount = GetPurchaseAmount(attackPrice, growthRate);
        BigDouble totalCost = CalculateCostForAmount(attackPrice,growthRate, amount)+ flatTax;
        
        attackPriceDisplay.text = GameManager.FormatNumber(totalCost);
    }
    public void AttackIncreaseShop()
    {
        
        BigDouble growthRate = attackGrowthRate;
        BigDouble amount = GetPurchaseAmount(attackPrice, growthRate);
        BigDouble totalCost = CalculateCostForAmount(attackPrice, growthRate, amount) + flatTax;
        attackPriceDisplay.text = GameManager.FormatNumber(totalCost);

        if (CurrencyManager.Instance.totalMoney >= totalCost)
        {
            CurrencyManager.Instance.totalMoney -= totalCost;
            CurrencyManager.Instance.AddCurrencey(0f); // Updates UI


            PlayerStats.Instance.playerDamage *= (float)BigDouble.Pow(growthRate, amount).ToDouble();
            attackPrice = attackPrice * BigDouble.Pow(growthRate, amount);
            PlayersSavedSata.instance.savedAttackPrice = attackPrice;
            PlayersSavedSata.instance.didWeUseTheShopPrior = true;
            DisplayAllStatsOnRIght();
            UpdateAttackPrice();
            AudioPlayer.Instance.SpendMoney();
        }
        

    }


    public void HealthIncreaseShop()
    {
        BigDouble growthRate = hpGrowthRate;
        BigDouble amount = GetPurchaseAmount(hpPrice, growthRate);
        BigDouble totalCost = CalculateCostForAmount(hpPrice, growthRate, amount) + flatTax;
        hpPriceDisplay.text = GameManager.FormatNumber(totalCost);

        if (CurrencyManager.Instance.totalMoney >= totalCost)
        {
            CurrencyManager.Instance.totalMoney -= totalCost;
            CurrencyManager.Instance.AddCurrencey(0f); // Updates UI

  
            PlayerStats.Instance.maxHealth *= (float)BigDouble.Pow(hpGrowthRate, amount).ToDouble();
            hpPrice = hpPrice * BigDouble.Pow(growthRate, amount);
            PlayerStats.Instance.currentHealth = PlayerStats.Instance.maxHealth;
            HealthBar.healthBarInstance.UpdateHealth();

            PlayersSavedSata.instance.savedHpPrice = hpPrice;
            PlayersSavedSata.instance.didWeUseTheShopPrior = true;
            DisplayAllStatsOnRIght();
            UpdateAttackPrice();
            UpdateHpPrice();
            AudioPlayer.Instance.SpendMoney();
        }
       

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
        BigDouble growthRate = critGrowthRate;
        BigDouble amount = GetPurchaseAmount(critDamagePrice, growthRate);
        BigDouble totalCost = CalculateCostForAmount(critDamagePrice, growthRate, amount) + flatTax;
        critDamagePriceDisplay.text = GameManager.FormatNumber(totalCost);

        if (CurrencyManager.Instance.totalMoney >= totalCost)
        {
            CurrencyManager.Instance.totalMoney -= totalCost;
            CurrencyManager.Instance.AddCurrencey(0f); // Updates UI

            
            float increase = (float)(2.5 * amount).ToDouble();
            PlayerStats.Instance.critDamageAmp += increase;
            critDamagePrice = critDamagePrice * BigDouble.Pow(growthRate, amount);

            PlayersSavedSata.instance.savedCritPrice = critDamagePrice;
            PlayersSavedSata.instance.didWeUseTheShopPrior = true;
            DisplayAllStatsOnRIght();
            UpdateAttackPrice();
            UpdateHpPrice();
            UpdateCritAmpPrice();
            AudioPlayer.Instance.SpendMoney();
        }
       

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
        BigDouble growthRate = speedGrowthRate;
        BigDouble amount = GetPurchaseAmount(speedOfPlayer, growthRate);
        BigDouble totalCost = CalculateCostForAmount(speedOfPlayer, growthRate, amount) + flatTax;
        speedOfPlayerDisplay.text = GameManager.FormatNumber(totalCost);

        if (CurrencyManager.Instance.totalMoney >= totalCost)
        {
            CurrencyManager.Instance.totalMoney -= totalCost;
            CurrencyManager.Instance.AddCurrencey(0f); // Updates UI

           

        
            // ---THE CONNECTION-- -
            // 1. Convert the BigDouble amount to an int, and send it to SpeedManager
            int levelsBought = (int)amount.ToDouble();
            SpeedManager.instance.BuySpeed(levelsBought);

            // 2. Increase the price for the next purchase
            speedOfPlayer = speedOfPlayer * BigDouble.Pow(growthRate, amount);

            PlayersSavedSata.instance.savedSpeedPrice = speedOfPlayer;
            PlayersSavedSata.instance.didWeUseTheShopPrior = true;
            DisplayAllStatsOnRIght();
            UpdateAttackPrice();
            UpdateHpPrice();
            UpdateSpeedPrice();
            AudioPlayer.Instance.SpendMoney();
        }
  

    }
    public void UpdateSpeedPrice()
    {
      
        BigDouble growthRate = speedGrowthRate; 
        BigDouble amount = GetPurchaseAmount(speedOfPlayer, growthRate);


        BigDouble totalCost = CalculateCostForAmount(speedOfPlayer, growthRate, amount) + flatTax;
        speedOfPlayerDisplay.text = GameManager.FormatNumber(totalCost);
    }

    public void DisplayAllStatsOnRIght()
    {
        displayAllStatsOnTheRight.text =
                "ATK <pos=55%>" + GameManager.FormatNumber(PlayerStats.Instance.playerDamage) + "\n" +
                "HP <pos=55%>" + GameManager.FormatNumber(PlayerStats.Instance.maxHealth) + "\n" +
                "CRIT <pos=55%>" + GameManager.FormatNumber(PlayerStats.Instance.critDamageAmp) + "\n" +
                "SPD <pos=55%>" + GameManager.FormatNumber(PlayerStats.Instance.baseSpeed);
    }

    
}
