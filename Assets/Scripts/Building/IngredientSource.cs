using UnityEngine;

/// <summary>
/// Marks an ingredient basket and knows which ingredient it hands out. Identification
/// only for now - the pickup flow starts once carried-ingredient identity is designed
/// (consumers should read the Ingredient property).
/// </summary>
[DisallowMultipleComponent]
public class IngredientSource : MonoBehaviour, IInteractable
{
    [SerializeField] private Ingredient ingredient;

    public Ingredient Ingredient => ingredient;

    public void Interact(PlayerController player)
    {
        string ingredientName = ingredient != null ? ingredient.ingredientName : "UNASSIGNED";
        Debug.Log($"[IngredientSource] {name}: ingredient={ingredientName}, item={player.CurrentItemType}");
    }
}
