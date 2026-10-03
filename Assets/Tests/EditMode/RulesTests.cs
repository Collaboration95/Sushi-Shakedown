using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

public class RulesTests
{
    private CustomerData data;
    private readonly List<Object> owned = new List<Object>();
    private T Make<T>() where T : ScriptableObject { var item = ScriptableObject.CreateInstance<T>(); owned.Add(item); return item; }
    [SetUp] public void Setup() { data = Make<CustomerData>(); data.ResetEverything(); }
    [TearDown] public void Cleanup() { foreach (var item in owned) Object.DestroyImmediate(item); owned.Clear(); }

    [Test] public void EnablingOrCloningDataPreservesRunState()
    {
        data.Day = 3; data.score = 17; data.CustomerCoins = 25; data.customersServed = 4; data.GrillSpeedCount = 3;
        var copy = Object.Instantiate(data); owned.Add(copy);
        Assert.That(copy.Day, Is.EqualTo(3)); Assert.That(copy.score, Is.EqualTo(17));
        Assert.That(copy.CustomerCoins, Is.EqualTo(25)); Assert.That(copy.customersServed, Is.EqualTo(4));
        Assert.That(copy.GrillSpeedCount, Is.EqualTo(3));
    }
    [Test] public void GrillSpeedEventReportsSpeedRatherThanAreaCount()
    {
        data.GrillAreaCount = 4; int notified = 0;
        data.OnGrillSpeed_Increased += value => notified = value;
        data.IncrementGrillSpeed(); Assert.That(notified, Is.EqualTo(2));
    }
    [Test] public void UpgradeEventsReportEachNewLevel()
    {
        int assembly = 0, grill = 0, patience = 0, cutting = 0;
        data.OnFAA_Increased += value => assembly = value; data.OnGrillArea_Increased += value => grill = value;
        data.OnPatienceLevel_Increased += value => patience = value; data.OnCuttingSpeed_Increased += value => cutting = value;
        data.IncrementFAA(); data.IncrementGrillArea(); data.IncrementPatienceLevel(); data.IncrementCuttingSpeed();
        Assert.That(new[] { assembly, grill, patience, cutting }, Is.EqualTo(new[] { 3, 3, 2, 2 }));
    }
    [TestCase(1, 10)] [TestCase(7, 7)] [TestCase(8, 10)]
    public void DebtScheduleWrapsAfterWeek(int day, int debt) => Assert.That(data.GetRansom(day), Is.EqualTo(debt));
    [TestCase(0)] [TestCase(-1)] public void InvalidDayIsRejected(int day) => Assert.Throws<ArgumentOutOfRangeException>(() => data.GetRansom(day));
    [Test] public void MissingDebtScheduleHasActionableFailure() { data.Ransom = Array.Empty<int>(); Assert.Throws<InvalidOperationException>(() => data.GetRansom(1)); }
    [Test] public void RewardsAndPenaltiesUpdateMoneyScoreAndCountsOnce()
    {
        int notified = 0; data.OnScoreChanged += value => notified = value;
        data.AddScore(10); data.AddScore(3); data.DeductScore(5);
        Assert.That(data.score, Is.EqualTo(8)); Assert.That(data.CustomerCoins, Is.EqualTo(18));
        Assert.That(data.HappyCustomerCount, Is.EqualTo(1)); Assert.That(data.normalCustomersCount, Is.EqualTo(1));
        Assert.That(data.angryCustomersCount, Is.EqualTo(1)); Assert.That(notified, Is.EqualTo(8));
    }
    [Test] public void DifficultyAndModeEventsOnlyFireOnTransitions()
    {
        int difficulty = 0, modes = 0;
        data.OnDifficultyChanged += _ => difficulty++; data.OnGameModeChanged += _ => modes++;
        data.SetDifficulty(Difficulty.Hard); data.SetDifficulty(Difficulty.Hard);
        data.SetGameMode(GameMode.Waves); data.SetGameMode(GameMode.Waves);
        Assert.That(difficulty, Is.EqualTo(1)); Assert.That(modes, Is.EqualTo(1));
    }
    [Test] public void NewRunResetsResourcesAndUpgrades()
    {
        data.Day = 5; data.AddScore(10); data.IncrementFAA(); data.IncrementGrillSpeed(); data.IncrementPatienceLevel(); data.IncrementCuttingSpeed();
        data.ResetEverything(); Assert.That(data.Day, Is.EqualTo(1)); Assert.That(data.score, Is.Zero);
        Assert.That(data.CustomerCoins, Is.EqualTo(10)); Assert.That(data.HappyCustomerCount, Is.Zero);
        Assert.That(new[] { data.FoodAssemblyAreaCount, data.GrillAreaCount, data.GrillSpeedCount, data.PatienceLevel, data.CuttingSpeed }, Is.EqualTo(new[] { 2, 2, 1, 1, 1 }));
    }
    [Test] public void PlateMatchingIgnoresOrderButCountsDuplicates()
    {
        var a = Make<DraggableObjectSO>(); var b = Make<DraggableObjectSO>();
        Assert.That(OrderRules.Matches(new[] { a, b, a }, new[] { a, a, b }, false), Is.True);
        Assert.That(OrderRules.Matches(new[] { a, b, b }, new[] { a, a, b }, false), Is.False);
    }
    [Test] public void DrinkLayersMustMatchInOrder()
    {
        var a = Make<DraggableObjectSO>(); var b = Make<DraggableObjectSO>();
        Assert.That(OrderRules.Matches(new[] { a, b }, new[] { a, b }, true), Is.True);
        Assert.That(OrderRules.Matches(new[] { b, a }, new[] { a, b }, true), Is.False);
    }
    [Test] public void MissingOrEmptyIngredientsNeverMatch()
    {
        Assert.That(OrderRules.Matches(null, null, false), Is.False);
        Assert.That(OrderRules.Matches(Array.Empty<DraggableObjectSO>(), Array.Empty<DraggableObjectSO>(), false), Is.False);
        Assert.That(OrderRules.Matches(new DraggableObjectSO[] { null }, new DraggableObjectSO[] { null }, true), Is.False);
        var a = Make<DraggableObjectSO>(); Assert.That(OrderRules.Matches(new[] { a }, new[] { a, a }, false), Is.False);
    }
    [Test] public void CheckedInRecipesHaveUsableInputsOutputsAndDurations()
    {
        var cutting = AssetDatabase.FindAssets("t:CuttingRecipeSO"); var grilling = AssetDatabase.FindAssets("t:GrillingRecipeSO");
        Assert.That(cutting.Length, Is.GreaterThan(0)); Assert.That(grilling.Length, Is.GreaterThan(0));
        foreach (var id in cutting)
        {
            var recipe = AssetDatabase.LoadAssetAtPath<CuttingRecipeSO>(AssetDatabase.GUIDToAssetPath(id));
            Assert.That(recipe.inputIngredient, Is.Not.Null, recipe.name); Assert.That(recipe.cuttingProgressMax, Is.GreaterThan(0), recipe.name);
            ValidateOutput(recipe.outputIngredient);
        }
        foreach (var id in grilling)
        {
            var recipe = AssetDatabase.LoadAssetAtPath<GrillingRecipeSO>(AssetDatabase.GUIDToAssetPath(id));
            Assert.That(recipe.inputIngredient, Is.Not.Null, recipe.name); Assert.That(recipe.grillingProgressMax, Is.GreaterThan(0), recipe.name);
            ValidateOutput(recipe.outputIngredient);
        }
    }
    private void ValidateOutput(DraggableObjectSO output)
    {
        Assert.That(output, Is.Not.Null); Assert.That(output.prefab, Is.Not.Null, output.name);
        Assert.That(output.prefab.GetComponent<DraggableObject>(), Is.Not.Null, output.name);
    }
    [Test] public void CookingRejectsMissingAndInvalidRecipeConfiguration()
    {
        var station = new GameObject("Recipe test"); station.SetActive(false); owned.Add(station);
        var grill = station.AddComponent<GrillContainer>(); var cutting = station.AddComponent<CuttingContainer>();
        var ingredient = Make<DraggableObjectSO>();
        var grillLookup = typeof(GrillContainer).GetMethod("GetRecipeWithInput", BindingFlags.NonPublic | BindingFlags.Instance);
        var cutLookup = typeof(CuttingContainer).GetMethod("GetCuttingRecipeSOWithInput", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.That(grillLookup.Invoke(grill, new object[] { ingredient }), Is.Null);
        Assert.That(cutLookup.Invoke(cutting, new object[] { ingredient }), Is.Null);
        var recipe = Make<GrillingRecipeSO>(); recipe.inputIngredient = ingredient; recipe.grillingProgressMax = 0;
        typeof(GrillContainer).GetField("grillingRecipeSOArray", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(grill, new[] { recipe });
        Assert.That(grillLookup.Invoke(grill, new object[] { ingredient }), Is.Null);
    }
    [Test] public void CanonicalScenesContainNoMissingScripts()
    {
        foreach (var scene in EditorBuildSettings.scenes)
        {
            if (!scene.enabled) continue;
            var opened = EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Additive);
            try
            {
                foreach (var root in opened.GetRootGameObjects())
                    foreach (var transform in root.GetComponentsInChildren<Transform>(true))
                        Assert.That(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject), Is.Zero, scene.path + ":" + transform.name);
            }
            finally { EditorSceneManager.CloseScene(opened, true); }
        }
        var bin = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Kitchen/Tools/trashbin.prefab");
        Assert.That(bin.GetComponent<TrashBin>().cd, Is.Not.Null, "Trash penalties need shared CustomerData");
    }

}
