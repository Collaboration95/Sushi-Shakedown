# Phase 1 code audit

Reviewed against issue #2 on 3 October 2026. Prioritize correctness and player impact, then cheap verification, then efficiency. The inventory below includes every project C# script, including legacy helpers; package sources are outside scope.

## Ranked findings and regression evidence

| Risk | Finding and fix | Verification |
| --- | --- | --- |
| High; money/customer outcome | Wrong delivery left patience running; duplicate callbacks could deduct/reward again. Make terminal outcomes idempotent and cancel timers. | `WrongDeliveryOrTimeoutResolvesOnlyOnce`, `TimeoutDuringDeliveryCancelsPendingRewardsAndCleansDishes` |
| High; order lifecycle | Concurrent animations could finish a customer while earlier deliveries remained; accepted dishes could reenter or survive a timeout. Track pending deliveries, consume once, clean up on disable. | `SimultaneousDeliveriesCompleteExactlyOnceAndCoinsCannotInflateScore`, timeout regression |
| High; progression | Independently started child wave coroutines survived cancellation; enabling/disabling or switching modes could leave spawners running. Use nested enumerators, exclusive modes, cancellation on disable. Clamp final wait to the wave deadline. | `StoppingWaveCancelsCountdownAndNestedSpawning`, `DisablingWaveManagerCancelsBothModes`, short and full-wave tests |
| High; scene transition | Persistent input state/camera/drag references and paused time survived restart; overlay Start could overwrite DayManager's initial screen. Reacquire scene camera, clear input state, restore global pause state, initialize overlay in Awake. | `MenuPlayStartsDayOneAndTimedWave`, `PauseRestartAndReloadRestoreTimeInputAndUniqueManagers` |
| High; missing configuration | Missing/disabled customer template could consume a counter slot. Validate before reservation; release on failed activation. | `InvalidCustomerTemplateDoesNotReserveSlot`, `DisabledCustomerTemplateReleasesReservedSlot` |
| High; disposal | Bin had no CustomerData reference, charged on entry rather than actual disposal, and could penalize a reusable dispenser. Wire prefab data and charge only after a disposable drop. | `TrashChargesOnlyOnceAfterDisposalAndNotForDispensers`, checked-in prefab data assertion |
| Medium; upgrades | Grill speed event reported grill area count; inactive stations missed upgrade changes. Report the right level and read current data on enable. | `GrillSpeedEventReportsSpeedRatherThanAreaCount`, `UpgradeEventsReportEachNewLevel`; station reproduction below |
| Medium; difficulty/patience | Newly spawned customers only listened to future difficulty/patience changes; empty animation lists threw; high movement speed could overshoot a counter. Initialize current settings, keep base animator when variants are absent, use MoveTowards. | `CustomerUsesCurrentDifficultyAndPatienceUpgrade`; animation/movement reproduction below |
| Medium; data lifecycle | Cloning/enabling CustomerData reset current statistics. Menu begins an explicit new run; daily stats reset explicitly, including happy customers. Invalid days/schedules produce precise exceptions. | `EnablingOrCloningDataPreservesRunState`, `NewRunResetsResourcesAndUpgrades`, debt tests, `SuccessfulDayPaysDebtAndResetsDailyHappyCount`, pause-copy test |
| Medium; UI accuracy | Late coin effects could inflate displayed score after a penalty. Coins are cosmetic; UI follows data. Pause toggles had a reversed Hard value and did not refresh slider visuals. | simultaneous-delivery/coin test, `PauseSettingsCopyPreservesMoneyAndShowsHardToggle` |
| Medium; asset/configuration | Food generation assumed nonempty arrays and valid prefab visuals; recipes allowed missing outputs/zero durations; cup Awake appended duplicate visual slots. Validate generation/recipes and recreate exactly three cup slots. | `MissingFoodConfigurationFailsWithoutPartialOrders`, `OrderGenerationIsDeterministicAndCapped`, recipe EditMode tests, `InvalidCuttingIngredientReturnsAndClearsHoverState` |
| Medium; scene hygiene | Customer camera contained orphan script GUID `5256fd50cf3238940af6937d0a4e8571`. Remove only that component/reference. | `CanonicalScenesContainNoMissingScripts`; no missing-script load warning after cleanup |
| Medium; teardown | Static instances were not cleared; UI unsubscribers assumed the event manager outlived them; persistent audio held dead scene loops. Clear owning instances, guard unsubscribe, attach loop sources to grills and prune dead keys when starting a loop. | scene reload smoke; teardown/audio reproduction below |
| Low; efficiency | Blocked input logged every frame, full counters logged every spawn retry, generation repeatedly queried components, plate validation allocated temporary lists, grills allocated progress payloads each frame, and NPC movement queried Camera.main every frame. Gate verbose logs, cache stable references/locals, use allocation-free matching and a value payload. | `RepresentativeCustomerWave`; rules tests; profiling note |
| Low; API/assembly | Obsolete sorted scene query and runtime NUnit/unused package imports. Use unsorted `FindObjectsByType`; isolate runtime, editor and tests in asmdefs. Upgrades is an overlay, not a nonexistent scene. | both suites; player build check; upgrade/navigation reproduction below |

## Focused Editor reproductions for guarded paths

Use a temporary scene/prefab copy; revert test configuration afterwards. These checks supplement the automated paths rather than claim every arbitrary inspector edit is supported.

1. **Station re-enable:** disable a grill/cutting station, purchase a speed upgrade, re-enable it and process a valid ingredient. It uses the current level. Drop an unsupported ingredient or clear the recipe list: no output is instantiated and no null dereference occurs. Invalid zero-time/null-output recipes are also rejected by EditMode tests.
2. **Customer visuals/movement:** clear the override-controller list on a copied customer; retain a valid base animator and required data/UI references. It uses that animator. Increase speed and let it walk to a counter: MoveTowards reaches the destination without oscillation. Remove a required reference: one actionable error disables the controller and a failed spawn releases its reservation.
3. **Optional/missing references:** remove LogSettings (category logging stays off with a warning), omit optional mood icons/coin effect objects (customer scoring still works), remove a food-spawner prefab (actionable error, no instantiation), and remove a slider on a copied toggle (no null exception during validation). Required core overlay/day references report a configuration error and disable that manager.
4. **Scene/audio teardown:** grill an ingredient so a loop plays, disable the grill or return to menu, then start a new run. The old loop stops with its station. Delete/destroy EventManager before a visual/progress component: unsubscribe teardown is safe. Destroy the owning singleton, then create a replacement: its static accessor points to the replacement.
5. **Spawn zone:** scale/offset a copied spawn-zone collider; MinY/MaxY use its world bounds, initialized in Awake before spawn calls.
6. **Navigation/UI:** invoke `LoadUpgrades` through a temporary inspector button: it warns that upgrades are an overlay rather than loading a missing scene. Normal upgrades remain accessible from the results screen. Return to menu while paused: the next run advances normally and starts at Day 1.

These are repeatable reproduction instructions; the automated smoke suites and build are the recorded executed evidence. Visual/audio perception and arbitrary malformed inspector variants still benefit from contributor checks in the interactive Editor.

## Complete script inventory

### Customers, spawning and progression

- `Assets/Scripts/Guru/Characters/Customer/CoinHandler.cs`
- `Assets/Scripts/Guru/Characters/Customer/CustomerController.cs`
- `Assets/Scripts/Guru/Characters/Customer/DayManager.cs`
- `Assets/Scripts/Guru/Characters/Customer/Hypno.cs`
- `Assets/Scripts/Guru/Characters/Customer/OrderBubble.cs`
- `Assets/Scripts/Guru/Characters/Customer/PatienceBar.cs`
- `Assets/Scripts/Guru/Characters/Customer/WaveManager.cs`
- `Assets/Scripts/Guru/Characters/NPCController.cs`
- `Assets/Scripts/Guru/Characters/NPCSpawner.cs`

### Food and deterministic order rules

- `Assets/Scripts/Guru/Food/Food.cs`
- `Assets/Scripts/Guru/Food/FoodDraggable.cs`
- `Assets/Scripts/Guru/Food/FoodManager.cs`
- `Assets/Scripts/Guru/Food/FoodSpawner.cs`
- `Assets/Scripts/Guru/Food/OrderRules.cs`

### Scenes and UI overlays

- `Assets/Scripts/Guru/GameScene/CustomerAudioManager.cs`
- `Assets/Scripts/Guru/GameScene/GameSceneManager.cs`
- `Assets/Scripts/Guru/GameScene/OrderArea.cs`
- `Assets/Scripts/Guru/GameScene/OrderAreaGroup.cs`
- `Assets/Scripts/Guru/GameScene/OverLayManager.cs`
- `Assets/Scripts/Guru/GameScene/SpawnZoneData.cs`
- `Assets/Scripts/Guru/MainMenuScene/GuruAudioManager.cs`
- `Assets/Scripts/Guru/MainMenuScene/MainMenuManager.cs`
- `Assets/Scripts/Guru/MainMenuScene/SettingsController.cs`

### Data and logging

- `Assets/Scripts/Guru/ScriptableObject/CustomerGameData.cs`
- `Assets/Scripts/Guru/ScriptableObject/LogSettings.cs`
- `Assets/Scripts/Guru/ScriptableObject/ScoreCounter.cs`
- `Assets/Scripts/Guru/ScriptableObject/SettingsClickTester.cs`
- `Assets/Scripts/Guru/customTooling/DebuggableMonoBehaviour.cs`
- `Assets/Scripts/ScriptableObjectScripts/AudioClipRefsSO.cs`
- `Assets/Scripts/ScriptableObjectScripts/CupIngredientSO.cs`
- `Assets/Scripts/ScriptableObjectScripts/CuttingRecipeSO.cs`
- `Assets/Scripts/ScriptableObjectScripts/DraggableObjectSO.cs`
- `Assets/Scripts/ScriptableObjectScripts/GameDataSO.cs`
- `Assets/Scripts/ScriptableObjectScripts/GrillingRecipeSO.cs`
- `Assets/Scripts/ScriptableObjectScripts/PlateIngredientSO.cs`

### Kitchen interactions and visuals

- `Assets/Scripts/Container/AssemblerContainer.cs`
- `Assets/Scripts/Container/BaseContainer.cs`
- `Assets/Scripts/Container/CuttingContainer.cs`
- `Assets/Scripts/Container/GrillContainer.cs`
- `Assets/Scripts/Container/IngredientDispenserContainer.cs`

### Event/input/audio and pattern helpers

- `Assets/Scripts/Guru/Patterns/Singleton.cs`
- `Assets/Scripts/Guru/Patterns/ToggleMode.cs`
- `Assets/Scripts/Guru/Patterns/ToggleSwitch.cs`
- `Assets/Scripts/Guru/Patterns/ToggleSwitchManager.cs`
- `Assets/Scripts/Manager/AudioManager.cs`
- `Assets/Scripts/Manager/EventManager.cs`
- `Assets/Scripts/Manager/GameManager.cs`
- `Assets/Scripts/Manager/RuntimeLog.cs`

### Other kitchen/UI/debug components

- `Assets/GameManagerClickerTester.cs`
- `Assets/ScoreParent.cs`
- `Assets/Scripts/BaseContainerVisual.cs`
- `Assets/Scripts/CupDraggable.cs`
- `Assets/Scripts/DraggableObject.cs`
- `Assets/Scripts/FoodSpawner.cs`
- `Assets/Scripts/IngredientDispenserDraggable.cs`
- `Assets/Scripts/IngredientDraggable.cs`
- `Assets/Scripts/PlateDraggable.cs`
- `Assets/Scripts/ProgressBarUI.cs`
- `Assets/Scripts/TrashBin.cs`
- `Assets/Scripts/TrashbinVisual.cs`
- `Assets/UpgradeManager.cs`

The legacy `FoodDraggable`/`Guru_FoodSpawner` path, empty toggle-group manager and click-testing helpers remain available; they are not the canonical serving pipeline. They were inventoried but not redesigned. The data-only ingredient/recipe assets retain their serialized formats. Broad ECS, pooling, event-framework, persistence and UI/art replacements are deferred.
