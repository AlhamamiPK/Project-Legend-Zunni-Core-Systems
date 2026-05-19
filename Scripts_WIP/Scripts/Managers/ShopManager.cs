using BreakInfinity;
using MoreMountains.Tools;
using TMPro;
using Unity.AppUI.UI;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
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
    private bool isShopOpen;
    [SerializeField] TextMeshProUGUI attackPriceDisplay;
    [SerializeField] TextMeshProUGUI hpPriceDisplay;
    [SerializeField] TextMeshProUGUI critDamagePriceDisplay;
    [SerializeField] TextMeshProUGUI speedOfPlayerDisplay;
    [SerializeField] TextMeshProUGUI displayAllStatsOnTheRight;
    [SerializeField] public GameObject shopPanel;
    //public static bool didWeUseTheShopPrior = false;

    private void Start()
    {
        if (PlayersSavedSata.instance.didWeUseTheShopPrior == false)
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
        else if (PlayersSavedSata.instance.didWeUseTheShopPrior == true)
        {
            attackPrice = PlayersSavedSata.instance.savedAttackPrice;
            hpPrice = PlayersSavedSata.instance.savedHpPrice;
            critDamagePrice = PlayersSavedSata.instance.savedCritPrice;
            speedOfPlayer = PlayersSavedSata.instance.savedSpeedPrice;
        }

        DisplayAllStatsOnRIght();
        IncreaseAllPrices();
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
    private void Update()
    {

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

    public void SetMultiplier(int multiplierIndex)
    {
        currentMultiplier = (BuyMultiplier)multiplierIndex;
        //Make this UpdateAllShopUI();
        //for now
        IncreaseAllPrices();
    }

    public BigDouble CalculateCostForAmount(BigDouble currentPrice, BigDouble growthRate, BigDouble amount)
    {
        if (currentPrice <= 0) return 0;
        if (amount == 1) return currentPrice;
        return currentPrice * (BigDouble.Pow(growthRate, amount) - 1) / (growthRate - 1);
        // return currentPrice * (System.Math.Pow(growthRate, amount) - 1) / (growthRate - 1);
    }
    public BigDouble CalculateMaxAffordableAmount(BigDouble currentPrice, BigDouble growthRate, BigDouble totalMoney)
    {
        //if (totalMoney < currentPrice) return 0;
        // Reverse geometric series using LOGS
        // double insideLog = (totalMoney * (growthRate - 1) / currentPrice) + 1;
        //double n = System.Math.Log(insideLog,growthRate);
        //return (int)System.Math.Floor(n);
        if (totalMoney < currentPrice) return 0;

        BigDouble insideLog = (totalMoney * (growthRate - 1) / currentPrice) + 1;

        // Use BigDouble.Log and BigDouble.Floor instead of System.Math
        BigDouble n = BigDouble.Log(insideLog, growthRate);
        return BigDouble.Floor(n);
    }
    private void IncreaseAllPrices()
    {
        // attackPriceDisplay.text = GameManager.FormatNumber(attackPrice);
        //hpPriceDisplay.text = GameManager.FormatNumber(hpPrice);
        //critDamagePriceDisplay.text = GameManager.FormatNumber(critDamagePrice);
        //speedOfPlayerDisplay.text = GameManager.FormatNumber(speedOfPlayer);

    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            shopPanel.SetActive(true);
        }//else shopPanel.SetActive(false);
    }
    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            shopPanel.SetActive(false);
            OnShopHoverExit();
        }
        //else shopPanel.SetActive(false);
    }
    public void UpdateAttackPrice()
    {
        // double growthRate = 1.07;//7% for now
        //double amount = 0;
        BigDouble growthRate = 1.07; // 7% for now
        BigDouble amount = 0;
        if (currentMultiplier == BuyMultiplier.x1)
        {
            amount = 1;
        }
        else if (currentMultiplier == BuyMultiplier.x10)
        {
            amount = 10;
        }
        else if (currentMultiplier == BuyMultiplier.x100)
        {
            amount = 100;
        }
        else if (currentMultiplier == BuyMultiplier.Max)
        {
            amount = CalculateMaxAffordableAmount(attackPrice, growthRate, CurrencyManager.Instance.totalMoney);
        }
        //double totalCost = CalculateCostForAmount(attackPrice, growthRate, (int)amount);
        //       attackPriceDisplay.text = GameManager.FormatNumber(totalCost);
        // Removed the (int) cast since CalculateCostForAmount now takes a BigDouble
        BigDouble totalCost = CalculateCostForAmount(attackPrice, growthRate, amount) + 5;
        attackPriceDisplay.text = GameManager.FormatNumber(totalCost);
    }
    public void AttackIncreaseShop()
    {
        //  double growthRate = 1.07;//7% for now
        // double amount = 0;
        BigDouble growthRate = 1.07;
        BigDouble amount = 0;
        if (currentMultiplier == BuyMultiplier.x1)
        {
            amount = 1;
        }
        else if (currentMultiplier == BuyMultiplier.x10)
        {
            amount = 10;
        }
        else if (currentMultiplier == BuyMultiplier.x100)
        {
            amount = 100;
        }
        else if (currentMultiplier == BuyMultiplier.Max)
        {
            amount = CalculateMaxAffordableAmount(attackPrice, growthRate, CurrencyManager.Instance.totalMoney);
        }
        BigDouble totalCost = CalculateCostForAmount(attackPrice, growthRate, amount) + 5;
        attackPriceDisplay.text = GameManager.FormatNumber(totalCost);

        if (CurrencyManager.Instance.totalMoney >= totalCost)
        {
            CurrencyManager.Instance.totalMoney -= totalCost;
            CurrencyManager.Instance.AddCurrencey(0f); // Updates UI

            //  PlayerStats.Instance.playerDamage *= (float)System.Math.Pow(1.07, amount);
            // attackPrice = attackPrice * System.Math.Pow(growthRate, amount);
            // 4. Update the math to use BigDouble.Pow. 
            // NOTE: We use .ToDouble() at the end to convert the result back to a float for your PlayerStats.
            PlayerStats.Instance.playerDamage *= (float)BigDouble.Pow(1.07, amount).ToDouble();
            attackPrice = attackPrice * BigDouble.Pow(growthRate, amount);
            PlayersSavedSata.instance.savedAttackPrice = attackPrice;
            PlayersSavedSata.instance.didWeUseTheShopPrior = true;
            DisplayAllStatsOnRIght();
            IncreaseAllPrices();
            UpdateAttackPrice();
            AudioPlayer.Instance.SpendMoney();
        }
        //if (CurrencyManager.Instance.totalMoney >= attackPrice)
        //{
        //    CurrencyManager.Instance.totalMoney -= attackPrice;
        //    CurrencyManager.Instance.AddCurrencey(0f);
        //    PlayerStats.Instance.playerDamage *= 1.07; //10%INcrease
        //    attackPrice = attackPrice * 1.07f; //7% Increase in price
        //    DisplayAllStatsOnRIght();
        //    IncreaseAllPrices();
        //    AudioPlayer.Instance.SpendMoney();
        //}
        //else Debug.Log("NOT ENOUGH MONEY FOR ATTACK BBITCHH");

    }


    public void HealthIncreaseShop()
    {
        BigDouble growthRate = 1.07;
        BigDouble amount = 0;
        if (currentMultiplier == BuyMultiplier.x1)
        {
            amount = 1;
        }
        else if (currentMultiplier == BuyMultiplier.x10)
        {
            amount = 10;
        }
        else if (currentMultiplier == BuyMultiplier.x100)
        {
            amount = 100;
        }
        else if (currentMultiplier == BuyMultiplier.Max)
        {
            amount = CalculateMaxAffordableAmount(hpPrice, growthRate, CurrencyManager.Instance.totalMoney);
        }
        BigDouble totalCost = CalculateCostForAmount(hpPrice, growthRate, amount) + 5;
        hpPriceDisplay.text = GameManager.FormatNumber(totalCost);

        if (CurrencyManager.Instance.totalMoney >= totalCost)
        {
            CurrencyManager.Instance.totalMoney -= totalCost;
            CurrencyManager.Instance.AddCurrencey(0f); // Updates UI

            //  PlayerStats.Instance.playerDamage *= (float)System.Math.Pow(1.07, amount);
            // attackPrice = attackPrice * System.Math.Pow(growthRate, amount);
            // 4. Update the math to use BigDouble.Pow. 
            // NOTE: We use .ToDouble() at the end to convert the result back to a float for your PlayerStats.
            PlayerStats.Instance.maxHealth *= (float)BigDouble.Pow(1.07, amount).ToDouble();
            hpPrice = hpPrice * BigDouble.Pow(growthRate, amount);
            PlayerStats.Instance.currentHealth = PlayerStats.Instance.maxHealth;
            HealthBar.healthBarInstance.UpdateHealth();

            PlayersSavedSata.instance.savedHpPrice = hpPrice;
            PlayersSavedSata.instance.didWeUseTheShopPrior = true;
            DisplayAllStatsOnRIght();
            IncreaseAllPrices();
            UpdateAttackPrice();
            UpdateHpPrice();
            AudioPlayer.Instance.SpendMoney();
        }
        //if (CurrencyManager.Instance.totalMoney >= hpPrice)
        //{
        //    CurrencyManager.Instance.totalMoney -= hpPrice;
        //    CurrencyManager.Instance.AddCurrencey(0f);
        //    PlayerStats.Instance.maxHealth *= 1.07; //7%INcrease
        //    PlayerStats.Instance.currentHealth = PlayerStats.Instance.maxHealth;
        //    HealthBar.healthBarInstance.UpdateHealth();
        //    hpPrice = hpPrice * 1.07f; //4% Increase in price
        //    DisplayAllStatsOnRIght();
        //    IncreaseAllPrices();
        //    AudioPlayer.Instance.SpendMoney();
        //}
        //else Debug.Log("NOT ENOUGH MONEY FOR Health BBITCHH");

    }
    public void UpdateHpPrice()
    {
        // double growthRate = 1.07;//7% for now
        //double amount = 0;
        BigDouble growthRate = 1.07; // 7% for now
        BigDouble amount = 0;
        if (currentMultiplier == BuyMultiplier.x1)
        {
            amount = 1;
        }
        else if (currentMultiplier == BuyMultiplier.x10)
        {
            amount = 10;
        }
        else if (currentMultiplier == BuyMultiplier.x100)
        {
            amount = 100;
        }
        else if (currentMultiplier == BuyMultiplier.Max)
        {
            amount = CalculateMaxAffordableAmount(hpPrice, growthRate, CurrencyManager.Instance.totalMoney);
        }
        //double totalCost = CalculateCostForAmount(attackPrice, growthRate, (int)amount);
        //       attackPriceDisplay.text = GameManager.FormatNumber(totalCost);
        // Removed the (int) cast since CalculateCostForAmount now takes a BigDouble
        BigDouble totalCost = CalculateCostForAmount(hpPrice, growthRate, amount);
        hpPriceDisplay.text = GameManager.FormatNumber(totalCost);
    }
    public void CritDamageIncreaseShop()
    {
        BigDouble growthRate = 1.20;
        BigDouble amount = 0;
        if (currentMultiplier == BuyMultiplier.x1)
        {
            amount = 1;
        }
        else if (currentMultiplier == BuyMultiplier.x10)
        {
            amount = 10;
        }
        else if (currentMultiplier == BuyMultiplier.x100)
        {
            amount = 100;
        }
        else if (currentMultiplier == BuyMultiplier.Max)
        {
            amount = CalculateMaxAffordableAmount(critDamagePrice, growthRate, CurrencyManager.Instance.totalMoney);
        }
        BigDouble totalCost = CalculateCostForAmount(critDamagePrice, growthRate, amount) + 5;
        critDamagePriceDisplay.text = GameManager.FormatNumber(totalCost);

        if (CurrencyManager.Instance.totalMoney >= totalCost)
        {
            CurrencyManager.Instance.totalMoney -= totalCost;
            CurrencyManager.Instance.AddCurrencey(0f); // Updates UI

            //  PlayerStats.Instance.playerDamage *= (float)System.Math.Pow(1.07, amount);
            // attackPrice = attackPrice * System.Math.Pow(growthRate, amount);
            // 4. Update the math to use BigDouble.Pow. 
            // NOTE: We use .ToDouble() at the end to convert the result back to a float for your PlayerStats.
            // PlayerStats.Instance.critDamageAmp += BigDouble(2.5 * amount).ToDouble();
            float increase = (float)(2.5 * amount).ToDouble();
            PlayerStats.Instance.critDamageAmp += increase;
            critDamagePrice = critDamagePrice * BigDouble.Pow(growthRate, amount);

            PlayersSavedSata.instance.savedCritPrice = critDamagePrice;
            PlayersSavedSata.instance.didWeUseTheShopPrior = true;
            DisplayAllStatsOnRIght();
            IncreaseAllPrices();
            UpdateAttackPrice();
            UpdateHpPrice();
            UpdateCritAmpPrice();
            AudioPlayer.Instance.SpendMoney();
        }
        //if (CurrencyManager.Instance.totalMoney >= critDamagePrice)
        //{
        //    CurrencyManager.Instance.totalMoney -= critDamagePrice;
        //    CurrencyManager.Instance.AddCurrencey(0f);
        //    PlayerStats.Instance.critDamageAmp += 2.5f;
        //    critDamagePrice = critDamagePrice * 1.07f; //4% Increase in price
        //    DisplayAllStatsOnRIght();
        //    IncreaseAllPrices();
        //    AudioPlayer.Instance.SpendMoney();
        //}
        //else Debug.Log("NOT ENOUGH MONEY FOR CritDamage BBITCHH");

    }
    public void UpdateCritAmpPrice()
    {
        // double growthRate = 1.07;//7% for now
        //double amount = 0;
        BigDouble growthRate = 1.07; // 7% for now
        BigDouble amount = 0;
        if (currentMultiplier == BuyMultiplier.x1)
        {
            amount = 1;
        }
        else if (currentMultiplier == BuyMultiplier.x10)
        {
            amount = 10;
        }
        else if (currentMultiplier == BuyMultiplier.x100)
        {
            amount = 100;
        }
        else if (currentMultiplier == BuyMultiplier.Max)
        {
            amount = CalculateMaxAffordableAmount(critDamagePrice, growthRate, CurrencyManager.Instance.totalMoney);
        }
        //double totalCost = CalculateCostForAmount(attackPrice, growthRate, (int)amount);
        //       attackPriceDisplay.text = GameManager.FormatNumber(totalCost);
        // Removed the (int) cast since CalculateCostForAmount now takes a BigDouble
        BigDouble totalCost = CalculateCostForAmount(critDamagePrice, growthRate, amount);
        critDamagePriceDisplay.text = GameManager.FormatNumber(totalCost);
    }

    public void SpeedIncreaseShop()
    {
        BigDouble growthRate = 1.04;//1%
        BigDouble amount = 0;
        if (currentMultiplier == BuyMultiplier.x1)
        {
            amount = 1;
        }
        else if (currentMultiplier == BuyMultiplier.x10)
        {
            amount = 10;
        }
        else if (currentMultiplier == BuyMultiplier.x100)
        {
            amount = 100;
        }
        else if (currentMultiplier == BuyMultiplier.Max)
        {
            amount = CalculateMaxAffordableAmount(speedOfPlayer, growthRate, CurrencyManager.Instance.totalMoney);
        }
        BigDouble totalCost = CalculateCostForAmount(speedOfPlayer, growthRate, amount) + 5;
        speedOfPlayerDisplay.text = GameManager.FormatNumber(totalCost);

        if (CurrencyManager.Instance.totalMoney >= totalCost)
        {
            CurrencyManager.Instance.totalMoney -= totalCost;
            CurrencyManager.Instance.AddCurrencey(0f); // Updates UI

            //  PlayerStats.Instance.playerDamage *= (float)System.Math.Pow(1.07, amount);
            // attackPrice = attackPrice * System.Math.Pow(growthRate, amount);
            // 4. Update the math to use BigDouble.Pow. 
            // NOTE: We use .ToDouble() at the end to convert the result back to a float for your PlayerStats.
            // PlayerStats.Instance.baseSpeed *= (float)BigDouble.Pow(1.01, amount).ToDouble();

            //PlayerStats.Instance.baseSpeed *= (float)BigDouble.Pow(1.01, amount).ToDouble();
            speedOfPlayer = speedOfPlayer * BigDouble.Pow(growthRate, amount);
            // ---THE CONNECTION-- -
            // 1. Convert the BigDouble amount to an int, and send it to SpeedManager
            int levelsBought = (int)amount.ToDouble();
            SpeedManager.instance.BuySpeed(levelsBought);

            // 2. Increase the price for the next purchase
            speedOfPlayer = speedOfPlayer * BigDouble.Pow(growthRate, amount);

            PlayersSavedSata.instance.savedSpeedPrice = speedOfPlayer;
            PlayersSavedSata.instance.didWeUseTheShopPrior = true;
            DisplayAllStatsOnRIght();
            IncreaseAllPrices();
            UpdateAttackPrice();
            UpdateHpPrice();
            UpdateSpeedPrice();
            AudioPlayer.Instance.SpendMoney();
        }
        //if (CurrencyManager.Instance.totalMoney >= speedOfPlayer)
        //{
        //    CurrencyManager.Instance.totalMoney -= speedOfPlayer;
        //    CurrencyManager.Instance.AddCurrencey(0f);
        //    PlayerStats.Instance.moveSpeed *= 1.01f; //1%INcrease
        //    speedOfPlayer = speedOfPlayer * 1.07f; //4% Increase in price
        //    DisplayAllStatsOnRIght();
        //    IncreaseAllPrices();
        //    AudioPlayer.Instance.SpendMoney();
        //}
        //else Debug.Log("NOT ENOUGH MONEY FOR Health BBITCHH");

    }
    public void UpdateSpeedPrice()
    {
        // double growthRate = 1.07;//7% for now
        //double amount = 0;
        BigDouble growthRate = 1.04; // 7% for now
        BigDouble amount = 0;
        if (currentMultiplier == BuyMultiplier.x1)
        {
            amount = 1;
        }
        else if (currentMultiplier == BuyMultiplier.x10)
        {
            amount = 10;
        }
        else if (currentMultiplier == BuyMultiplier.x100)
        {
            amount = 100;
        }
        else if (currentMultiplier == BuyMultiplier.Max)
        {
            amount = CalculateMaxAffordableAmount(speedOfPlayer, growthRate, CurrencyManager.Instance.totalMoney);
        }
        //double totalCost = CalculateCostForAmount(attackPrice, growthRate, (int)amount);
        //       attackPriceDisplay.text = GameManager.FormatNumber(totalCost);
        // Removed the (int) cast since CalculateCostForAmount now takes a BigDouble
        BigDouble totalCost = CalculateCostForAmount(speedOfPlayer, growthRate, amount);
        speedOfPlayerDisplay.text = GameManager.FormatNumber(totalCost);
    }

    public void DisplayAllStatsOnRIght()
    {
        // displayAllStatsOnTheRight.text = "ATk" + PlayerStats.Instance.playerDamage.ToString("F0") + "\n"
        //+ "HP" + PlayerStats.Instance.maxHealth.ToString("F0") + "\n"
        // + "Crit" + PlayerStats.Instance.critDamageAmp.ToString() + "\n"
        // + "Speed" + PlayerStats.Instance.moveSpeed.ToString("F0");
        displayAllStatsOnTheRight.text =
                "ATK <pos=55%>" + GameManager.FormatNumber(PlayerStats.Instance.playerDamage) + "\n" +
                "HP <pos=55%>" + GameManager.FormatNumber(PlayerStats.Instance.maxHealth) + "\n" +
                "CRIT <pos=55%>" + GameManager.FormatNumber(PlayerStats.Instance.critDamageAmp) + "\n" +
                "SPD <pos=55%>" + GameManager.FormatNumber(PlayerStats.Instance.baseSpeed);
    }
}
