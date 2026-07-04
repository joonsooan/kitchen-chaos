public class IngredientInstance
{
    public Ingredient Data { get; }
    public CookingMethod CurrentState { get; private set; }

    public IngredientInstance(Ingredient data, CookingMethod currentState = CookingMethod.None)
    {
        Data = data;
        CurrentState = currentState;
    }

    public void ApplyCookingMethod(CookingMethod method)
    {
        CurrentState = method;
    }
}
