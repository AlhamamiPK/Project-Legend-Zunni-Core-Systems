using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
public class SpawningUI : MonoBehaviour
{   
    public static SpawningUI instance;
    
    [SerializeField] private GameObject levelUPUIPrefab;
    [SerializeField] private Transform mainCanvusTransform;
    [SerializeField] private Vector3 offset;
    private void Awake()
    {
        if (instance == null) { instance = this; }
    }
    public void SpawnLevelUpUI()
    {

          Instantiate(levelUPUIPrefab,mainCanvusTransform);
    }
}
