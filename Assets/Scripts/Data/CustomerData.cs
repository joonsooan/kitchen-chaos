using UnityEngine;

[CreateAssetMenu(fileName = "CustomerData", menuName = "KitchenChaos/CustomerData")]
public class CustomerData : ScriptableObject
{
    public string customerName;
    public RecipeData requiredRecipe;
    public float toleranceSeconds;
}
