using BreakInfinity;
using MoreMountains.Tools;
using TMPro;
using Unity.AppUI.UI;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class PlayersSavedSata : MonoBehaviour
{
    #region Make Our Script Global
    public static PlayersSavedSata instance;

    private void Awake()
    {
        if (instance == null) instance = this;
    }
    #endregion
    public bool didWeUseTheShopPrior = false;
    public BigDouble savedAttackPrice;
    public BigDouble savedHpPrice;
    public BigDouble savedCritPrice;
    public BigDouble savedSpeedPrice;

}
