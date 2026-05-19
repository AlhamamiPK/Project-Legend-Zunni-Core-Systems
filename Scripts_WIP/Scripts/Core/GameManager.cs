using System;
using System.Collections;
using System.Collections.Generic;
using BreakInfinity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [System.Serializable]
    public class EnemySpawnSetUp
    {
        public string name;
        public GameObject enemyPrefab;
    }
    int stage = 3;
    [Header("Spawning Setup")]
    [SerializeField] List<EnemySpawnSetUp> enemyList = new List<EnemySpawnSetUp>();
    [SerializeField] public Vector3 enemyPos;
  

    [System.Serializable]
    
    public class Stages
    {
        public string number;
        public StageData stageData;
    }
    [Header("Stages Stuff")]
    [SerializeField] List<Stages> stages = new List<Stages>();
    [SerializeField] public int currentStageIndex = 1;
    [SerializeField] public int enemiesKilledThisStage;
    [SerializeField] public int allEnemiesKilled;
    [SerializeField] public static bool isBossDead =false;

    [Header("Current Stage Instances Tracker")]
    private GameObject currentBackgroundInstance;
    private GameObject currentEnemySpawnerInstance;
    private GameObject currentShopInstance;
    private GameObject currentBossSpawnerInstance;
    [Header("The Scene Shop")]
    [Tooltip("Drag the MissoShop FROM YOUR SCENE HIERARCHY here! NOT the prefab!")]
    public GameObject sceneShopObject;
    [Header("Spawn Offsets (Relative to Player)")]
    [Tooltip("Offset from the player to spawn the parallax background")]
    [SerializeField] private Vector3 backgroundOffset = new Vector3(0f, 0f, 0f);

    [Tooltip("Offset from the player to spawn the Shop")]
    [SerializeField] private Vector3 shopOffset = new Vector3(15f, 0f, 0f);

    [Tooltip("Offset from the player to spawn the Boss Trigger")]
    [SerializeField] private Vector3 bossTriggerOffset = new Vector3(30f, 0f, 0f);

    [Header("Stage Text And Image")]
    [SerializeField] public TextMeshProUGUI stageCounter;
    private void Awake()
    {
        if(instance == null) { instance = this; }
        stageCounter.text = currentStageIndex.ToString();
        SpawnShop();
        LoadStage(currentStageIndex);
    }

    public void BossDefeated()
    {
        isBossDead = true;
        Debug.Log("YOYO U JUST FUCKED UP THE BOSS NEXT NOW WE GO TO THE NEXT STAGE");
       // isBossDead = true;
        NormalEnmiesSpanwer.bossIsSpawned = false;
        LoadStage(currentStageIndex + 1);
        BossSetActiveTrigger.playMusicOnce = true;
        
    }
    public void BossTimeOut()
    {
        isBossDead = true;
        Debug.Log("Time Ran OUT MUDA FUCK GO baCK 2 STAGESSSSS");
        NormalEnmiesSpanwer.bossIsSpawned = false;
       // isBossDead = true;
        int penaltyStage = Mathf.Max(0,currentStageIndex - 2);
        LoadStage(penaltyStage);
        BossSetActiveTrigger.playMusicOnce = true;
    }
    public void LoadStage(int index)
    {
        if(index < 0 || index >= stages.Count)
        {
            Debug.Log("YOYO THE STAGES DOESN EXIST FIX UR SHHHIT");
            return;
        }
        currentStageIndex = index;
        enemiesKilledThisStage = 0; // Reset The Kills For Each New Stage
        isBossDead = true;
        NormalEnmiesSpanwer.bossIsSpawned = false;
        StageData stageData = stages[currentStageIndex].stageData;
        stageCounter.text = currentStageIndex.ToString();

        // 1. CLEANUP THE OLD STAGE
        if (currentBackgroundInstance != null) Destroy(currentBackgroundInstance);
        //if (currentEnemySpawnerInstance != null) Destroy(currentEnemySpawnerInstance);
        if (currentShopInstance != null) Destroy(currentShopInstance);
        if (currentBossSpawnerInstance != null) Destroy(currentBossSpawnerInstance);

       // StartCoroutine(DeleteOldBossStuff());

        // Safely get Player Position
        Vector3 playerPos = Vector3.zero;
        if (PlayerStats.Instance != null)
        {
            playerPos = PlayerStats.Instance.transform.position;
        }

        // 2. SPAWN NEW STAGE ELEMENTS WITH OFFSETS
        if (stageData.parallaxBackGround != null)
        {
            // Spawn background relative to player + offset
            currentBackgroundInstance = Instantiate(stageData.parallaxBackGround, playerPos + backgroundOffset, Quaternion.identity);
        }

        if (stageData.enemySpawners != null)
        {
            // Keeping your original enemyPos logic here, or you could change it to playerPos + enemySpawnerOffset too!
            currentEnemySpawnerInstance = Instantiate(stageData.enemySpawners, enemyPos, Quaternion.identity);
        }
        // Note: We DO NOT spawn the Shop or Boss yet. Those come after the kills!
        //isBossDead = false;
    }
    public void SpawnSpecificEnemy(int index, Vector3 spawnPosition)
    {
        //Safety Check: Does this enemy index exist?
        if (index < 0 || index >= enemyList.Count)
        {
            Debug.LogWarning("Enemy index" + index + "Does not exist FIX UR SHIT ZUNNI");
            return;
        }

        GameObject prefabToSpawn = enemyList[index].enemyPrefab;
        GameObject newEnemyObj = Instantiate(prefabToSpawn, spawnPosition, Quaternion.Euler(0, 180, 0));
        Enemy spawnedEnemyScript = newEnemyObj.GetComponent<Enemy>();
        if (spawnedEnemyScript != null && PlayerStats.Instance != null)
        {
            spawnedEnemyScript.Initialize(stage);
        }
    }
    private IEnumerator DeleteOldBossStuff()
    {
        
        yield return new WaitForSeconds(1f);
        if (currentBossSpawnerInstance != null) Destroy(currentBossSpawnerInstance);


    }
    public void OnEnemyKilled()
    {
        enemiesKilledThisStage++;
        allEnemiesKilled++;

        StageData stageData = stages[currentStageIndex].stageData;

        if (enemiesKilledThisStage >= stageData.enemiesToKillBeforeBosses)
        {
            SpawnShop();
            SpawnBoss(stageData);
        }
    }
    public void SpawnShop()
    {
        StageData currentStageData = stages[currentStageIndex].stageData;

        // Safety check: Does this stage even have a shop?
        if (currentStageData.missoShop == null) return;

        // 1. CLEANUP: Destroy the old shop so we don't leave 50 shops behind in the level
        if (currentShopInstance != null)
        {
            Destroy(currentShopInstance);
        }

        // 2. GET PLAYER POS: Where is the player right now?
        Vector3 playerPos = Vector3.zero;
        if (PlayerStats.Instance != null)
        {
            playerPos = PlayerStats.Instance.transform.position;
        }

        // 3. SPAWN: Drop the shop exactly at the player's position + your custom X offset!
        currentShopInstance = Instantiate(currentStageData.missoShop, playerPos + shopOffset, Quaternion.identity);

        Debug.Log("Shop Spawned directly ahead of the player!");
    }
    //public void SpawnShop()
    //{
    //    // 1. Make sure we linked the shop in the inspector
    //    if (sceneShopObject == null)
    //    {
    //        Debug.LogError(" You forgot to drag the MissoShop from the hierarchy into the GameManager!");
    //        return;
    //    }

    //    // 2. Get Player Position
    //    Vector3 playerPos = Vector3.zero;
    //    if (PlayerStats.Instance != null)
    //    {
    //        playerPos = PlayerStats.Instance.playerTransform.position;
    //    }

    //    // 3. THE MAGIC TRICK: Calculate X based on the player, but KEEP the Shop's original Y position so it stays glued to the floor!
    //    float lockedGroundY = sceneShopObject.transform.position.y;
    //    Vector3 finalTeleportPos = new Vector3(playerPos.x + shopOffset.x, lockedGroundY, 0f);

    //    // 4. Move it and turn it on
    //    sceneShopObject.transform.position = finalTeleportPos;
    //    sceneShopObject.SetActive(true);

    //    Debug.Log(" Shop TELEPORTED cleanly to: " + finalTeleportPos);
    //}
    private void SpawnBoss(StageData data) // Renamed for clarity!
    {
        isBossDead = false;

        if (currentEnemySpawnerInstance != null)
        {
            currentEnemySpawnerInstance.SetActive(false);
        }

        // Safely get Player Position
        Vector3 playerPos = Vector3.zero;
        if (PlayerStats.Instance != null)
        {
            playerPos = PlayerStats.Instance.transform.position;
        }

        // ONLY Spawn Boss Trigger using the Inspector offset
        if (data.bossSpawner != null && currentBossSpawnerInstance == null)
        {
            currentBossSpawnerInstance = Instantiate(data.bossSpawner, playerPos + bossTriggerOffset, Quaternion.identity);
        }
    }

    //public static string FormatNumber(BigDouble value)
    //{
    //    if(value < 1000)
    //    {
    //        return value.ToString("F0");
    //    }

    //    // string[] suffixes = { "", "K", "M", "B", "T", "Aa","Ab","Ac" };
    //    string[] suffixes = {
    //    "", "K", "M", "B", "T", 
    //    "Aa", "Ab", "Ac", "Ad", "Ae", "Af", "Ag", "Ah", "Ai", "Aj", "Ak", "Al", "Am", "An", "Ao", "Ap", "Aq", "Ar", "As", "At", "Au", "Av", "Aw", "Ax", "Ay", "Az",
    //    "Ba", "Bb", "Bc", "Bd", "Be", "Bf", "Bg", "Bh", "Bi", "Bj", "Bk", "Bl", "Bm", "Bn", "Bo", "Bp", "Bq", "Br", "Bs", "Bt", "Bu", "Bv", "Bw", "Bx", "By", "Bz",
    //    "Ca", "Cb", "Cc", "Cd", "Ce", "Cf", "Cg", "Ch", "Ci", "Cj", "Ck", "Cl", "Cm", "Cn", "Co", "Cp", "Cq", "Cr", "Cs", "Ct", "Cu", "Cv", "Cw", "Cx", "Cy", "Cz",
    //    "Da", "Db", "Dc", "Dd", "De", "Df", "Dg", "Dh", "Di", "Dj", "Dk", "Dl", "Dm", "Dn", "Do", "Dp", "Dq", "Dr", "Ds", "Dt", "Du", "Dv", "Dw", "Dx", "Dy", "Dz",
    //    "Ea", "Eb", "Ec", "Ed", "Ee", "Ef", "Eg", "Eh", "Ei", "Ej", "Ek", "El", "Em", "En", "Eo", "Ep", "Eq", "Er", "Es", "Et", "Eu", "Ev", "Ew", "Ex", "Ey", "Ez",
    //    "Fa", "Fb", "Fc", "Fd", "Fe", "Ff", "Fg", "Fh", "Fi", "Fj", "Fk", "Fl", "Fm", "Fn", "Fo", "Fp", "Fq", "Fr", "Fs", "Ft", "Fu", "Fv", "Fw", "Fx", "Fy", "Fz",
    //    "Ga", "Gb", "Gc", "Gd", "Ge", "Gf", "Gg", "Gh", "Gi", "Gj", "Gk", "Gl", "Gm", "Gn", "Go", "Gp", "Gq", "Gr", "Gs", "Gt", "Gu", "Gv", "Gw", "Gx", "Gy", "Gz",
    //    "Ha", "Hb", "Hc", "Hd", "He", "Hf", "Hg", "Hh", "Hi", "Hj", "Hk", "Hl", "Hm", "Hn", "Ho", "Hp", "Hq", "Hr", "Hs", "Ht", "Hu", "Hv", "Hw", "Hx", "Hy", "Hz",
    //    "Ia", "Ib", "Ic", "Id", "Ie", "If", "Ig", "Ih", "Ii", "Ij", "Ik", "Il", "Im", "In", "Io", "Ip", "Iq", "Ir", "Is", "It", "Iu", "Iv", "Iw", "Ix", "Iy", "Iz",
    //    "Ja", "Jb", "Jc", "Jd", "Je", "Jf", "Jg", "Jh", "Ji", "Jj", "Jk", "Jl", "Jm", "Jn", "Jo", "Jp", "Jq", "Jr", "Js", "Jt", "Ju", "Jv", "Jw", "Jx", "Jy", "Jz",
    //    "Ka", "Kb", "Kc", "Kd", "Ke", "Kf", "Kg", "Kh", "Ki", "Kj", "Kk"
    //};
    //    int suffixIndex = 0;

    //    while (value >= 1000d && suffixIndex< suffixes.Length - 1)
    //    {
    //        value = value / 1000d;
    //        suffixIndex++;
    //    }
    //    return value.ToString("0.#")+suffixes[suffixIndex];
    //}

    public static string FormatNumber(BigDouble value)
    {
        // If it's under 1000, convert it to a normal double and print it
        if (value < 1000)
        {
            return value.ToDouble().ToString("F0"); // cite: BigDouble.cs
        }

        // 3. Keep your massive suffix list!
        string[] suffixes = {
      "", "K", "M", "B", "T", 
      "Aa", "Ab", "Ac", "Ad", "Ae", "Af", "Ag", "Ah", "Ai", "Aj", "Ak", "Al", "Am", "An", "Ao", "Ap", "Aq", "Ar", "As", "At", "Au", "Av", "Aw", "Ax", "Ay", "Az",
      "Ba", "Bb", "Bc", "Bd", "Be", "Bf", "Bg", "Bh", "Bi", "Bj", "Bk", "Bl", "Bm", "Bn", "Bo", "Bp", "Bq", "Br", "Bs", "Bt", "Bu", "Bv", "Bw", "Bx", "By", "Bz",
      "Ca", "Cb", "Cc", "Cd", "Ce", "Cf", "Cg", "Ch", "Ci", "Cj", "Ck", "Cl", "Cm", "Cn", "Co", "Cp", "Cq", "Cr", "Cs", "Ct", "Cu", "Cv", "Cw", "Cx", "Cy", "Cz",
      "Da", "Db", "Dc", "Dd", "De", "Df", "Dg", "Dh", "Di", "Dj", "Dk", "Dl", "Dm", "Dn", "Do", "Dp", "Dq", "Dr", "Ds", "Dt", "Du", "Dv", "Dw", "Dx", "Dy", "Dz",
      "Ea", "Eb", "Ec", "Ed", "Ee", "Ef", "Eg", "Eh", "Ei", "Ej", "Ek", "El", "Em", "En", "Eo", "Ep", "Eq", "Er", "Es", "Et", "Eu", "Ev", "Ew", "Ex", "Ey", "Ez",
      "Fa", "Fb", "Fc", "Fd", "Fe", "Ff", "Fg", "Fh", "Fi", "Fj", "Fk", "Fl", "Fm", "Fn", "Fo", "Fp", "Fq", "Fr", "Fs", "Ft", "Fu", "Fv", "Fw", "Fx", "Fy", "Fz",
      "Ga", "Gb", "Gc", "Gd", "Ge", "Gf", "Gg", "Gh", "Gi", "Gj", "Gk", "Gl", "Gm", "Gn", "Go", "Gp", "Gq", "Gr", "Gs", "Gt", "Gu", "Gv", "Gw", "Gx", "Gy", "Gz",
      "Ha", "Hb", "Hc", "Hd", "He", "Hf", "Hg", "Hh", "Hi", "Hj", "Hk", "Hl", "Hm", "Hn", "Ho", "Hp", "Hq", "Hr", "Hs", "Ht", "Hu", "Hv", "Hw", "Hx", "Hy", "Hz",
      "Ia", "Ib", "Ic", "Id", "Ie", "If", "Ig", "Ih", "Ii", "Ij", "Ik", "Il", "Im", "In", "Io", "Ip", "Iq", "Ir", "Is", "It", "Iu", "Iv", "Iw", "Ix", "Iy", "Iz",
      "Ja", "Jb", "Jc", "Jd", "Je", "Jf", "Jg", "Jh", "Ji", "Jj", "Jk", "Jl", "Jm", "Jn", "Jo", "Jp", "Jq", "Jr", "Js", "Jt", "Ju", "Jv", "Jw", "Jx", "Jy", "Jz",
      "Ka", "Kb", "Kc", "Kd", "Ke", "Kf", "Kg", "Kh", "Ki", "Kj", "Kk","Kl", "Km", "Kn", "Ko", "Kp", "Kq", "Kr", "Ks", "Kt", "Ku", "Kv", "Kw", "Kx", "Ky", "Kz",
    "La", "Lb", "Lc", "Ld", "Le", "Lf", "Lg", "Lh", "Li", "Lj", "Lk", "Ll", "Lm", "Ln", "Lo", "Lp", "Lq", "Lr", "Ls", "Lt", "Lu", "Lv", "Lw", "Lx", "Ly", "Lz",
    "Ma", "Mb", "Mc", "Md", "Me", "Mf", "Mg", "Mh", "Mi", "Mj", "Mk", "Ml", "Mm", "Mn", "Mo", "Mp", "Mq", "Mr", "Ms", "Mt", "Mu", "Mv", "Mw", "Mx", "My", "Mz",
    "Na", "Nb", "Nc", "Nd", "Ne", "Nf", "Ng", "Nh", "Ni", "Nj", "Nk", "Nl", "Nm", "Nn", "No", "Np", "Nq", "Nr", "Ns", "Nt", "Nu", "Nv", "Nw", "Nx", "Ny", "Nz",
    "Oa", "Ob", "Oc", "Od", "Oe", "Of", "Og", "Oh", "Oi", "Oj", "Ok", "Ol", "Om", "On", "Oo", "Op", "Oq", "Or", "Os", "Ot", "Ou", "Ov", "Ow", "Ox", "Oy", "Oz",
    "Pa", "Pb", "Pc", "Pd", "Pe", "Pf", "Pg", "Ph", "Pi", "Pj", "Pk", "Pl", "Pm", "Pn", "Po", "Pp", "Pq", "Pr", "Ps", "Pt", "Pu", "Pv", "Pw", "Px", "Py", "Pz",
    "Qa", "Qb", "Qc", "Qd", "Qe", "Qf", "Qg", "Qh", "Qi", "Qj", "Qk", "Ql", "Qm", "Qn", "Qo", "Qp", "Qq", "Qr", "Qs", "Qt", "Qu", "Qv", "Qw", "Qx", "Qy", "Qz",
    "Ra", "Rb", "Rc", "Rd", "Re", "Rf", "Rg", "Rh", "Ri", "Rj", "Rk", "Rl", "Rm", "Rn", "Ro", "Rp", "Rq", "Rr", "Rs", "Rt", "Ru", "Rv", "Rw", "Rx", "Ry", "Rz",
    "Sa", "Sb", "Sc", "Sd", "Se", "Sf", "Sg", "Sh", "Si", "Sj", "Sk", "Sl", "Sm", "Sn", "So", "Sp", "Sq", "Sr", "Ss", "St", "Su", "Sv", "Sw", "Sx", "Sy", "Sz",
    "Ta", "Tb", "Tc", "Td", "Te", "Tf", "Tg", "Th", "Ti", "Tj", "Tk", "Tl", "Tm", "Tn", "To", "Tp", "Tq", "Tr", "Ts", "Tt", "Tu", "Tv", "Tw", "Tx", "Ty", "Tz",
    "Ua", "Ub", "Uc", "Ud", "Ue", "Uf", "Ug", "Uh", "Ui", "Uj", "Uk", "Ul", "Um", "Un", "Uo", "Up", "Uq", "Ur", "Us", "Ut", "Uu", "Uv", "Uw", "Ux", "Uy", "Uz",
    "Va", "Vb", "Vc", "Vd", "Ve", "Vf", "Vg", "Vh", "Vi", "Vj", "Vk", "Vl", "Vm", "Vn", "Vo", "Vp", "Vq", "Vr", "Vs", "Vt", "Vu", "Vv", "Vw", "Vx", "Vy", "Vz",
    "Wa", "Wb", "Wc", "Wd", "We", "Wf", "Wg", "Wh", "Wi", "Wj", "Wk", "Wl", "Wm", "Wn", "Wo", "Wp", "Wq", "Wr", "Ws", "Wt", "Wu", "Wv", "Ww", "Wx", "Wy", "Wz",
    "Xa","Xb", "Xc", "Xd", "Xe", "Xf", "Xg", "Xh", "Xi", "Xj", "Xk", "Xl", "Xm", "Xn", "Xo", "Xp", "Xq", "Xr", "Xs", "Xt", "Xu", "Xv", "Xw", "Xx", "Xy", "Xz",
    "Ya", "Yb", "Yc", "Yd", "Ye", "Yf", "Yg", "Yh", "Yi", "Yj", "Yk", "Yl", "Ym", "Yn", "Yo", "Yp", "Yq", "Yr", "Ys", "Yt", "Yu", "Yv", "Yw", "Yx", "Yy", "Yz",
    "Za", "Zb", "Zc", "Zd", "Ze", "Zf", "Zg", "Zh", "Zi", "Zj", "Zk", "Zl", "Zm", "Zn", "Zo", "Zp", "Zq", "Zr", "Zs", "Zt", "Zu", "Zv", "Zw", "Zx", "Zy", "Zz"
    };

        // 4. Because BigDouble tracks Exponent (zeros), math is instant!
        // 1,000 has an exponent of 3. (3 / 3 = 1 -> "K")
        int suffixIndex = (int)(value.Exponent / 3); // cite: BigDouble.cs

        // Safety check
        if (suffixIndex >= suffixes.Length)
        {
            suffixIndex = suffixes.Length - 1;
        }

        // 5. Shift the number down to match the suffix
        // If value is 1,500,000, we divide by 10^6 to get 1.5
        BigDouble displayValue = value / BigDouble.Pow10(suffixIndex * 3); // cite: BigDouble.cs

        // 6. Convert the small decimal back to a normal double to print it
        return displayValue.ToDouble().ToString("0.#") + suffixes[suffixIndex]; // cite: BigDouble.cs
    }
}


