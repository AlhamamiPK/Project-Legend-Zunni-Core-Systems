using BreakInfinity;
using UnityEngine;
/// <summary>
/// PlayersSavedData: Persists shop prices, player stats, stage progression,
/// artifacts, and any other cross-session data.
/// </summary>
public class PlayerSavedData : MonoBehaviour
{
    #region Singleton
    public static PlayerSavedData instance {  get; private set; }

    private void Awake()
    {
        if (instance != null && instance != this) {

            Destroy(gameObject);
            return;
        }
        instance = this;
    }
    #endregion

    #region Shop Prices
    public bool didWeUseTheShopPrior = false;
    public BigDouble savedAttackPrice;
    public BigDouble savedHpPrice;
    public BigDouble savedCritPrice;
    public BigDouble savedSpeedPrice;
    #endregion

    #region Player Stats
    //Coming Soon
    #endregion

    #region Stage Progression
    //Coming Soon
    #endregion

    #region Artifacts
    //Coming Soon
    #endregion

}
