
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    //Hava değişkenleri
    [Header("Hava değişkenleri")]
    public float enyuksekTemperatur = -40f; 
    [Range (0,1)] public float ruzgarAralığı =0f; // 0 = sakin, 1= fırtına.

    public Vector2 ruzgarYonu = new Vector2 (-1, -0.5f).normalized;

    private void Awake()
    {
        if(Instance == null) Instance = this;
        else Destroy(gameObject);
    }
     
    private void Update()
    {
        ruzgarAralığı = Mathf.PerlinNoise(Time.time * 0.05f,0);
    }

    public float GetTemperatureAtPosition(Vector2 position)
    {
       return enyuksekTemperatur -(ruzgarAralığı *20f);// -20 kadar sıcaklığı duşurur..    
    }

}        
    



