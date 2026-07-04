using UnityEngine;

// 모든 SO 데이터 풀 — 첫 접근 시 Resources에서 1회 로드
public static class DataTable
{
    public static readonly Ingredient[]   Ingredients;
    public static readonly RecipeData[]   Recipes;
    public static readonly CustomerData[] Customers;

    static DataTable()
    {
        Ingredients = Resources.LoadAll<Ingredient>("Ingredients Data");
        Recipes     = Resources.LoadAll<RecipeData>("Recipe Data");
        Customers   = Resources.LoadAll<CustomerData>("Customer Data");
    }
}
