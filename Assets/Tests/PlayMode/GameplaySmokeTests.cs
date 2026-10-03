using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

public class GameplaySmokeTests
{
    private OverLayManager overlay;
    private CustomerData data;
    private NPCSpawner spawner;
    private WaveManager waves;
    private float oldCapture;

    [UnitySetUp] public IEnumerator LoadGame()
    {
        oldCapture = Time.captureDeltaTime;
        Time.captureDeltaTime = 1f / 60f;
        Time.timeScale = 1;
        Random.InitState(2026);
        yield return SceneManager.LoadSceneAsync("MainMenu"); yield return null;
        var settings = Object.FindFirstObjectByType<SettingsController>();
        settings.customerData.ResetEverything(); settings.customerData.SetGameMode(GameMode.Waves);
        settings.customerData.SetDifficulty(Difficulty.Easy);
        PersistentButton(Object.FindFirstObjectByType<MainMenuManager>().gameObject.scene.GetRootGameObjects(), "OnPlayButtonClicked").onClick.Invoke();
        yield return null; yield return null;
        overlay = Object.FindFirstObjectByType<OverLayManager>(); data = overlay.customerData;
        spawner = Object.FindFirstObjectByType<NPCSpawner>(); waves = Object.FindFirstObjectByType<WaveManager>();
    }
    [UnityTearDown] public IEnumerator Cleanup()
    {
        Time.timeScale = 1; Time.captureDeltaTime = oldCapture;
        if (GameSceneManager.instance != null) GameSceneManager.instance.BackToMainMenu();
        yield return null;
    }
    private IEnumerator Frames(int count) { for (int i = 0; i < count; i++) yield return null; }
    private static void SetField(object owner, string field, object value) => owner.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).SetValue(owner, value);
    private CustomerController Spawn()
    {
        if (overlay.PreDay_Screen.activeSelf) overlay.ClosePreDayUI();
        Assert.That(spawner.SpawnCustomer(), Is.True);
        return Object.FindObjectsByType<CustomerController>(FindObjectsSortMode.None).First(c => c.assignedOrderArea != null && !c.hasArrived && !c.isWalkingOffScreen);
    }
    private IEnumerator Arrive(CustomerController customer)
    {
        customer.transform.position = customer.targetPosition;
        yield return null;
        Assert.That(customer.hasArrived, Is.True); Assert.That(customer.orderBubble.GetOrders().Count, Is.GreaterThan(0));
    }

    [UnityTest] public IEnumerator MenuPlayStartsDayOneAndTimedWave()
    {
        Assert.That(SceneManager.GetSceneByName("Customers").isLoaded, Is.True);
        Assert.That(SceneManager.GetSceneByName("KitchenScene").isLoaded, Is.True);
        Assert.That(overlay.PreDay_Screen.activeSelf, Is.True, "Start order must not hide the day overlay");
        Assert.That(data.Day, Is.EqualTo(1)); Assert.That(waves.IsRunning, Is.False);
        overlay.ClosePreDayUI(); yield return Frames(330);
        Assert.That(data.WaveCount, Is.EqualTo(1)); Assert.That(waves.IsRunning, Is.True);
        Assert.That(Object.FindObjectsByType<CustomerController>(FindObjectsSortMode.None).Length, Is.GreaterThan(0));
    }
    [UnityTest] public IEnumerator EntireWaveCompletesAndTransitionsToFailure()
    {
        waves.hypno = null; // Fallback must also dismiss customers still walking to the counter.
        SetField(waves, "waveDurationSeconds", 2f); SetField(waves, "waveCountdownDuration", 0.1f);
        int completed = 0; waves.OnWavesCompleted += () => completed++;
        overlay.ClosePreDayUI(); yield return Frames(150);
        Assert.That(completed, Is.EqualTo(1)); Assert.That(waves.IsRunning, Is.False);
        Assert.That(overlay.Failure_Screen.activeSelf, Is.True);
        Assert.That(spawner.orderAreaGroup.AreAllOrderAreasFree(), Is.True);
    }
    [UnityTest] public IEnumerator StoppingWaveCancelsCountdownAndNestedSpawning()
    {
        SetField(waves, "waveCountdownDuration", 0.1f);
        waves.StartWaves(); waves.StopWaves(); yield return Frames(20);
        Assert.That(data.WaveCount, Is.Zero);
        waves.StartWaves(); yield return Frames(10);
        int wave = data.WaveCount; waves.StopWaves();
        foreach (var c in Object.FindObjectsByType<CustomerController>(FindObjectsSortMode.None)) c.ForceTimeout();
        yield return Frames(90);
        Assert.That(data.WaveCount, Is.EqualTo(wave)); Assert.That(spawner.orderAreaGroup.AreAllOrderAreasFree(), Is.True);
        Assert.That(waves.IsRunning, Is.False);
    }
    [UnityTest] public IEnumerator DisablingWaveManagerCancelsBothModes()
    {
        waves.StartEndlessCustomers(); Assert.That(waves.IsRunning, Is.True);
        waves.enabled = false; yield return Frames(150);
        Assert.That(waves.IsRunning, Is.False); Assert.That(spawner.orderAreaGroup.AreAllOrderAreasFree(), Is.True);
        waves.enabled = true; waves.StartWaves(); waves.StartWaves();
        yield return Frames(330); Assert.That(data.WaveCount, Is.EqualTo(1));
        waves.StartEndlessCustomers(); waves.StartWaves(); waves.StopWaves(); yield return Frames(150);
        Assert.That(waves.IsRunning, Is.False);
    }
    [UnityTest] public IEnumerator WrongDeliveryOrTimeoutResolvesOnlyOnce()
    {
        var c = Spawn(); yield return Arrive(c);
        int score = data.score; c.OnWrongDelivery("wrong"); c.ForceTimeout(); c.OnAllOrdersFulfilled();
        yield return Frames(120);
        Assert.That(data.score, Is.EqualTo(score - 5)); Assert.That(data.angryCustomersCount, Is.EqualTo(1));
        Assert.That(data.customersServed, Is.Zero); Assert.That(c.assignedOrderArea.IsFree(), Is.True);
    }
    [UnityTest] public IEnumerator CustomerUsesCurrentDifficultyAndPatienceUpgrade()
    {
        data.SetDifficulty(Difficulty.Hard); data.IncrementPatienceLevel();
        var c = Spawn(); yield return Arrive(c);
        Assert.That(typeof(CustomerController).GetField("difficultyMultiplier", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(c), Is.EqualTo(2));
        Assert.That(typeof(CustomerController).GetField("patienceLevelMultiplier", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(c), Is.EqualTo(2));
        Object.Destroy(c.gameObject); yield return null;
        Assert.That(spawner.orderAreaGroup.AreAllOrderAreasFree(), Is.True);
    }
    [UnityTest] public IEnumerator InvalidCustomerTemplateDoesNotReserveSlot()
    {
        var template = spawner.customerTemplate; spawner.customerTemplate = null;
        LogAssert.Expect(LogType.Error, "NPCSpawner requires a camera, spawn zone, order areas and a CustomerController template.");
        Assert.That(spawner.SpawnCustomer(), Is.False); Assert.That(spawner.orderAreaGroup.AreAllOrderAreasFree(), Is.True);
        spawner.customerTemplate = template; yield return null;
    }
    [UnityTest] public IEnumerator OrderGenerationIsDeterministicAndCapped()
    {
        var c = Spawn(); yield return Arrive(c);
        var bubble = c.orderBubble; bubble.StartOrder(20); Assert.That(bubble.GetOrders().Count, Is.EqualTo(3));
        var plate = bubble.FM.GetRandomFood((min, max) => min);
        Assert.That(plate.GetComponent<PlateDraggable>(), Is.Not.Null); Assert.That(plate.ingredientsDraggableObjectSOArray.Count, Is.EqualTo(3));
        int call = 0; var cup = bubble.FM.GetRandomFood((min, max) => call++ == 0 ? 1 : min);
        Assert.That(cup.GetComponent<CupDraggable>(), Is.Not.Null); Assert.That(cup.ingredientsDraggableObjectSOArray.Count, Is.EqualTo(1));
        Assert.That(cup.GetComponent<CupDraggable>().drinkSpriteArray.Count, Is.EqualTo(3));
        Object.Destroy(plate.gameObject); Object.Destroy(cup.gameObject); yield return null;
    }
    private void Deliver(OrderBubble bubble, Food food)
    {
        GameObject dish = food.GetComponent<CupDraggable>() != null ? Object.Instantiate(CupPrefab(bubble.FM)) : Object.Instantiate(PlatePrefab(bubble.FM));
        var field = dish.GetComponent<CupDraggable>() != null ? typeof(CupDraggable).GetField("currentIngredients", BindingFlags.NonPublic | BindingFlags.Instance) : typeof(PlateDraggable).GetField("currentIngredients", BindingFlags.NonPublic | BindingFlags.Instance);
        var target = (Component)dish.GetComponent<CupDraggable>() ?? dish.GetComponent<PlateDraggable>();
        field.SetValue(target, new System.Collections.Generic.List<DraggableObjectSO>(food.ingredientsDraggableObjectSOArray));
        dish.transform.position = bubble.transform.position;
        bubble.SendMessage("OnTriggerEnter2D", dish.GetComponent<Collider2D>());
        bubble.SendMessage("OnTriggerEnter2D", dish.GetComponent<Collider2D>()); // duplicate contact is ignored
    }
    private GameObject PlatePrefab(FoodManager fm) => (GameObject)typeof(FoodManager).GetField("platePrefab", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(fm);
    private GameObject CupPrefab(FoodManager fm) => (GameObject)typeof(FoodManager).GetField("cupPrefab", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(fm);
    [UnityTest] public IEnumerator SimultaneousDeliveriesCompleteExactlyOnceAndCoinsCannotInflateScore()
    {
        var c = Spawn(); yield return Arrive(c);
        c.orderBubble.StartOrder(3); c.OrderMultiplier = 3;
        foreach (var food in c.orderBubble.GetOrders().ToArray()) Deliver(c.orderBubble, food);
        yield return Frames(45);
        Assert.That(data.customersServed, Is.EqualTo(1)); Assert.That(data.score, Is.EqualTo(40));
        c.OnAllOrdersFulfilled(); data.DeductScore(5); yield return Frames(240);
        Assert.That(data.score, Is.EqualTo(35)); Assert.That(Object.FindFirstObjectByType<ScoreParent>().scoreText.text, Is.EqualTo("35"));
        Assert.That(data.customersServed, Is.EqualTo(1));
    }
    [UnityTest] public IEnumerator TimeoutDuringDeliveryCancelsPendingRewardsAndCleansDishes()
    {
        var c = Spawn(); yield return Arrive(c);
        c.orderBubble.deliveryAnimationDuration = 2;
        foreach (var food in c.orderBubble.GetOrders().ToArray()) Deliver(c.orderBubble, food);
        int score = data.score; c.ForceTimeout(); yield return Frames(150);
        Assert.That(data.score, Is.EqualTo(score - 1)); Assert.That(data.customersServed, Is.Zero);
        Assert.That(c.orderBubble.GetOrders(), Is.Empty);
    }
    [UnityTest] public IEnumerator PauseRestartAndReloadRestoreTimeInputAndUniqueManagers()
    {
        overlay.PauseButtonClick(); Assert.That(Time.timeScale, Is.Zero);
        overlay.FinalDayScreen_RestartButtonClick(); yield return null; yield return null;
        Assert.That(Time.timeScale, Is.EqualTo(1));
        PersistentButton(Object.FindFirstObjectByType<MainMenuManager>().gameObject.scene.GetRootGameObjects(), "OnPlayButtonClicked").onClick.Invoke(); yield return null; yield return null;
        overlay = Object.FindFirstObjectByType<OverLayManager>(); Assert.That(overlay.PreDay_Screen.activeSelf, Is.True);
        overlay.ClosePreDayUI();
        Assert.That(Object.FindObjectsByType<GameManager>(FindObjectsSortMode.None).Length, Is.EqualTo(1));
        Assert.That(Object.FindObjectsByType<EventManager>(FindObjectsSortMode.None).Length, Is.EqualTo(1));
        Assert.That(Object.FindObjectsByType<AudioManager>(FindObjectsSortMode.None).Length, Is.EqualTo(1));
        Assert.That(Object.FindObjectsByType<GameSceneManager>(FindObjectsSortMode.None).Length, Is.EqualTo(1));
        Assert.That(typeof(GameManager).GetField("uiIsBlockingInput", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(GameManager.Instance), Is.False);
        Assert.That(GameManager.Instance.currentlyDragging, Is.Null);
    }
    [UnityTest] public IEnumerator MissingFoodConfigurationFailsWithoutPartialOrders()
    {
        var c = Spawn(); yield return Arrive(c);
        SetField(c.orderBubble.FM, "drinkDraggableObjectSOArray", new DraggableObjectSO[0]);
        LogAssert.Expect(LogType.Error, "FoodManager requires ingredient arrays and plate/cup prefabs with Food, visuals and BoxCollider2D.");
        Assert.That(c.orderBubble.FM.GetRandomFood(), Is.Null); yield return null;
    }
    [UnityTest] public IEnumerator SuccessfulDayPaysDebtAndResetsDailyHappyCount()
    {
        SetField(waves, "waveDurationSeconds", 0.1f); waves.waveCountdownDuration = 0.1f;
        data.CustomerCoins = 100; data.HappyCustomerCount = 2;
        overlay.ClosePreDayUI(); yield return Frames(30);
        Assert.That(overlay.Info_Screen.activeSelf, Is.True); Assert.That(data.CustomerCoins, Is.EqualTo(100 - 1 - data.GetRansom(1)));
        // One en-route customer times out (-1), then the configured Day 1 debt is paid.
        overlay.Info_ContinueButtonClick(); yield return null;
        Assert.That(data.Day, Is.EqualTo(2)); Assert.That(data.HappyCustomerCount, Is.Zero);
        Assert.That(overlay.PreDay_Screen.activeSelf, Is.True);
    }
    [UnityTest] public IEnumerator UpgradePurchaseHonorsFundsAndMaximumLevel()
    {
        data.CustomerCoins = 19; overlay.Upgrade_GrillSpeed_ButtonClick();
        Assert.That(data.GrillSpeedCount, Is.EqualTo(1)); Assert.That(data.CustomerCoins, Is.EqualTo(19));
        data.CustomerCoins = 20; overlay.Upgrade_GrillSpeed_ButtonClick();
        Assert.That(data.GrillSpeedCount, Is.EqualTo(2)); Assert.That(data.CustomerCoins, Is.Zero);
        data.CustomerCoins = 100; overlay.Upgrade_GrillSpeed_ButtonClick(); overlay.Upgrade_GrillSpeed_ButtonClick();
        Assert.That(data.GrillSpeedCount, Is.EqualTo(3)); Assert.That(data.CustomerCoins, Is.EqualTo(70));
        yield return null;
    }
    [UnityTest] public IEnumerator TrashChargesOnlyOnceAfterDisposalAndNotForDispensers()
    {
        overlay.ClosePreDayUI();
        var bin = Object.FindFirstObjectByType<TrashBin>();
        var fm = Object.FindFirstObjectByType<FoodManager>();
        var plate = Object.Instantiate(PlatePrefab(fm)); plate.transform.position = new Vector3(100,100,0);
        var draggable = plate.GetComponent<DraggableObject>(); draggable.TryPickUpThis();
        int score = data.score;
        bin.SendMessage("OnTriggerEnter2D", plate.GetComponent<Collider2D>());
        Assert.That(data.score, Is.EqualTo(score));
        bin.SendMessage("OnTriggerExit2D", plate.GetComponent<Collider2D>()); draggable.HandleRelease();
        yield return null; Assert.That(data.score, Is.EqualTo(score));
        plate = Object.Instantiate(PlatePrefab(fm)); plate.transform.position = new Vector3(100,100,0);
        draggable = plate.GetComponent<DraggableObject>(); draggable.TryPickUpThis();
        bin.SendMessage("OnTriggerEnter2D", plate.GetComponent<Collider2D>());
        // Mark a valid drop as the physics trigger would, then release.
        typeof(DraggableObject).GetField("isColliding", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(draggable, true);
        draggable.HandleRelease(); yield return null; yield return null;
        Assert.That(data.score, Is.EqualTo(score - 3));
        var dispenser = Object.FindFirstObjectByType<IngredientDispenserDraggable>(); dispenser.TryPickUpThis();
        bin.SendMessage("OnTriggerEnter2D", dispenser.GetComponent<Collider2D>());
        typeof(DraggableObject).GetField("isColliding", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(dispenser, true);
        dispenser.HandleRelease(); yield return null;
        Assert.That(data.score, Is.EqualTo(score - 3)); Assert.That(dispenser, Is.Not.Null);
    }
    [UnityTest] public IEnumerator PauseSettingsCopyPreservesMoneyAndShowsHardToggle()
    {
        data.SetDifficulty(Difficulty.Hard); data.CustomerCoins = 23; data.score = 23;
        overlay.PauseButtonClick();
        var local = (CustomerData)typeof(OverLayManager).GetField("Pause_CustomerData_Local", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(overlay);
        var toggle = (ToggleSwitch)typeof(OverLayManager).GetField("DifficultyMode_Toggle", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(overlay);
        Assert.That(local.CustomerCoins, Is.EqualTo(23)); Assert.That(local.score, Is.EqualTo(23)); Assert.That(toggle.CurrentValue, Is.True);
        overlay.Accept(); Assert.That(Time.timeScale, Is.EqualTo(1)); yield return null;
    }

    [UnityTest] public IEnumerator DisabledCustomerTemplateReleasesReservedSlot()
    {
        var template = spawner.customerTemplate.GetComponent<CustomerController>(); template.enabled = false;
        Assert.That(spawner.SpawnCustomer(), Is.False); Assert.That(spawner.orderAreaGroup.AreAllOrderAreasFree(), Is.True);
        template.enabled = true; yield return null;
    }

    [UnityTest] public IEnumerator DefaultTwoMinuteWaveCompletesWithoutRuntimeExceptions()
    {
        overlay.ClosePreDayUI(); Time.timeScale = 10f;
        yield return Frames(800);
        Assert.That(waves.IsRunning, Is.False); Assert.That(overlay.Failure_Screen.activeSelf, Is.True);
        Assert.That(data.angryCustomersCount, Is.GreaterThan(0));
        Assert.That(spawner.orderAreaGroup.AreAllOrderAreasFree(), Is.True);
    }

    private static Button PersistentButton(GameObject[] roots, string method)
    {
        foreach (var root in roots)
            foreach (var button in root.GetComponentsInChildren<Button>(true))
                for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
                    if (button.onClick.GetPersistentMethodName(i) == method) return button;
        Assert.Fail("Missing button callback: " + method); return null;
    }
    [UnityTest] public IEnumerator FailureMenuButtonIsClickableAndReloads()
    {
        overlay.ShowFailureUI(1, 0, 30);
        var button = PersistentButton(new[] { overlay.Failure_Screen }, "Failure_RestartButtonClick");
        var canvas = button.GetComponentInParent<Canvas>().rootCanvas;
        var eventCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        var pointer = new PointerEventData(EventSystem.current)
        {
            position = RectTransformUtility.WorldToScreenPoint(eventCamera, button.transform.position),
            button = PointerEventData.InputButton.Left
        };
        yield return null; Canvas.ForceUpdateCanvases();
        pointer.position = RectTransformUtility.WorldToScreenPoint(eventCamera, button.transform.position);
        var hits = new System.Collections.Generic.List<RaycastResult>(); EventSystem.current.RaycastAll(pointer, hits);
        Assert.That(hits.Count, Is.GreaterThan(0));
        Assert.That(hits[0].gameObject.GetComponentInParent<Button>(), Is.EqualTo(button), "The visible menu button must receive the pointer hit: " + string.Join(", ", hits.Select(hit => hit.gameObject.name + " parent=" + hit.gameObject.transform.parent.name)));
        ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerClickHandler);
        yield return null;
        Assert.That(SceneManager.GetSceneByName("MainMenu").isLoaded, Is.True);
    }
    [UnityTest] public IEnumerator InvalidCuttingIngredientReturnsAndClearsHoverState()
    {
        overlay.ClosePreDayUI();
        var board = Object.FindFirstObjectByType<CuttingContainer>();
        var source = Object.FindFirstObjectByType<AssemblerContainer>();
        var dish = Object.Instantiate(PlatePrefab(Object.FindFirstObjectByType<FoodManager>()));
        var plate = dish.GetComponent<PlateDraggable>(); plate.SetParentContainer(source);
        Assert.That(plate.TryHandleIngredient(null), Is.False);
        board.SetHoveringDraggableObjectTracking(plate);
        yield return null;
        Assert.That(board.GetOwnedDraggable(), Is.Null); Assert.That(board.GetHoveringDraggableObjectTracking(), Is.Null);
        Assert.That(source.GetOwnedDraggable(), Is.EqualTo(plate));
        Object.Destroy(dish); yield return null;
    }

}
