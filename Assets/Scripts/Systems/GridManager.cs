using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
   
   public static GridManager Instance;
   public Tilemap telikeliTile; 

   private void Awake()
    {
        if(Instance == null) Instance = this;
    }

    public bool KonumTehlikeliMi(Vector2 worldPosition)
    {
        Vector3Int gridPosition  = telikeliTile.WorldToCell(worldPosition);
        if(telikeliTile.HasTile(gridPosition))
        {
            return true; // oyuncunun bastığı zeminin ince buz olduğunu algılasın...
        }
        return false;
    }


   
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
