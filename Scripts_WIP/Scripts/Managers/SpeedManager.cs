using UnityEngine;
using System;
using System.Collections.Generic;
using BreakInfinity;
public class SpeedManager : MonoBehaviour
{
    public static SpeedManager instance;
    [Serializable]
    public struct SpeedTier
    {
        public int maxLevel; // The Level where this tier ends;
        public float gainPerLevel; // How much speed is gained per level in this tier;

    }
    [Header(" Speed Balancing")]
    public float baseSpeed;
    public int currentSpeedLevel = 0; // Total number of times Speed has been bought;

    public List<SpeedTier> speedTiers;

    public float currentActualSpeed;

    private void Start()
    {
        RecalculateSpeed();
    }

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    //This has to be called whenever the Player buys speed( 1x,10x,100x,max);
    public void BuySpeed(int amountPurchased)
    {
        currentSpeedLevel += amountPurchased;
        RecalculateSpeed();
    }
    // This calculates the exact speed from scratch based on the total level;
    // This perfectly hadnles bulk buys crossing over multiple tiers;
    private void RecalculateSpeed()
    {
        float calculatedSpeed = baseSpeed;
        int levelsRemainingToCalculate = currentSpeedLevel;
        int previousTierMax = 0;

        foreach ( SpeedTier tier in speedTiers)
        {
            if (levelsRemainingToCalculate <= 0) break;

            // How many levels exist inside this specific tier?
            int levelsInThisTier = tier.maxLevel - previousTierMax;

            // How many levels are we actually applying to this tier?
            int levelsToApply = Mathf.Min(levelsRemainingToCalculate, levelsInThisTier);

            calculatedSpeed += (levelsToApply * tier.gainPerLevel);
            levelsRemainingToCalculate -= levelsToApply;
            previousTierMax = tier.maxLevel;
        }
        currentActualSpeed = calculatedSpeed;
        Debug.Log($"New Speed: {currentActualSpeed} at Level {currentSpeedLevel}");

        // Pass 'currentActualSpeed' to your PlayerStats or Movement Script here.
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.baseSpeed = currentActualSpeed;
        }
    }
}

