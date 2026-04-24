using UnityEngine;

public class WorldItem : MonoBehaviour
{
    public SO_Item ItemData;
    public int Amount = 1;

    public void UpdateItemAmount(int amount)
    {
        Amount = amount;

        if (Amount <= 0)
        {
            Destroy(gameObject);
        }
    }
}
