# Sushi Shakedown

A student sushi-bar time-management game: prepare food and layered drinks, serve impatient customers, and earn enough coins to pay a daily debt over seven days.

## Open and play

1. Install **Unity 6000.6.1f1 (Unity 6.6)** through Unity Hub. Use the version in `ProjectSettings/ProjectVersion.txt`.
2. Add this repository folder as a project in Hub. Let Unity restore the packages in `Packages/manifest.json` and import the assets.
3. Open `Assets/Scenes/Guru/MainMenu.unity` and press Play. Choose **Play**, then dismiss the **Day 1** screen to start the customer wave.

No external runtime service, API key, or generated artwork is required. Packages include URP 17.6, UGUI/TextMeshPro 2.6, Input System 1.20, Addressables 2.11 and Unity Test Framework 1.8. Python 3 and Bash are needed only for the test script. The project still uses legacy `Input` mouse polling; retain the checked-in **Both** input handling setting. Install the macOS build module only if running the optional macOS player build check.

| Scene | Role |
| --- | --- |
| `Assets/Scenes/Guru/MainMenu.unity` | Canonical entry point and settings |
| `Assets/Scenes/Guru/Customers.unity` | Customer movement, orders, waves, debt and overlay UI |
| `Assets/Scenes/KitchenScene.unity` | Additively loaded preparation and drag-and-drop stations |

The menu loads Customers and KitchenScene together. Opening KitchenScene alone is useful for editing, but is not the complete gameplay flow. Upgrades use an overlay within Customers; there is no standalone Upgrades scene.

## Controls and implemented features

- **Left mouse:** pick up and drag ingredients, dispensers, plates and cups; release over a preparation station or bin. Deliver matching dishes to an order bubble.
- **Right mouse on a cutting board:** advance cutting. Grills cook automatically and can burn food if left too long.
- **Plates:** one rice, fish and condiment ingredient. Their assembly order does not affect order matching.
- **Cups:** up to three drink layers. Layer order must match the customer's order.
- **Customer patience:** correct partial deliveries restore patience; wrong deliveries and timeouts cost coins. Completed orders earn a patience-based reward.
- **Progression:** timed waves, seven daily debt payments, failure/final screens, and coin-funded station, cutting/grill speed and patience upgrades.
- **Settings:** Easy/Hard difficulty, Waves/FreePlay mode, pause, and restart. FreePlay has continuous customer spawning.

The game does not implement staff morale, equipment durability, saved-run persistence, multiplayer, multithreaded simulation or adaptive AI. Difficulty is a fixed multiplier selected in settings. Customer variants and some animation/UI assets remain incomplete.

## Run tests

Close this project's editor before running batch commands. The same EditMode and PlayMode tests are available through **Window → General → Test Runner**.

```bash
./scripts/test-unity.sh
```

For another Unity installation or a CI agent with a valid Unity license:

```bash
UNITY_EDITOR="/path/to/Unity" \
TEST_RESULTS_DIR="/tmp/sushi-results" \
./scripts/test-unity.sh
```

The script runs both suites serially, writes XML and editor logs to `Logs/Tests` by default, and fails if a suite fails or discovers no tests. Allow Unity to finish package restore first; licensing and package availability are prerequisites on a clean agent. No `-quit` flag is used for test runs because Unity Test Framework exits after completion.

Set `UNITY_EDITOR` to the editor executable when running the following commands directly. Equivalent single-suite command:

```bash
"$UNITY_EDITOR" -batchmode -nographics -projectPath "$PWD" \
  -runTests -testPlatform PlayMode \
  -testResults /tmp/sushi-playmode.xml \
  -logFile /tmp/sushi-playmode.log
```

Optional development player build to verify runtime assemblies and the three configured scenes (requires macOS module):

```bash
"$UNITY_EDITOR" -batchmode -nographics -quit -projectPath "$PWD" \
  -executeMethod VerifyBuild.Perform -logFile /tmp/sushi-build.log
```

The build is written to the ignored `Builds/SushiShakedown.app`. This is a compile/build check, not a release certification.

## Architecture and hardening report

The project retains its original MonoBehaviour architecture. `CustomerData` is a shared ScriptableObject for coins, daily statistics, difficulty and upgrade levels. `DayManager` and `WaveManager` drive progression; `NPCSpawner`, `OrderAreaGroup` and `CustomerController` own customer lifecycles. `FoodManager` generates orders from ingredient assets; `OrderBubble` validates and consumes deliveries. Containers and draggable objects implement kitchen interactions through `GameManager` and `EventManager`. `OverLayManager` coordinates the day, pause, upgrades and results screens.

Runtime, editor build helpers and tests have separate assembly definitions. Serialized script GUIDs and scene/prefab references are preserved. `OrderRules` supplies a small deterministic matching seam; `FoodManager` accepts a range-selection delegate for reproducible generation tests. Existing coroutines and gameplay rules remain in place.

Intentional bug corrections:

- Customer outcomes resolve once. Failure cancels patience and pending deliveries; simultaneous deliveries cannot reward twice or finish early.
- Switching modes, disabling a wave manager, or reloading cancels spawning/countdown work. Restart restores time and input state.
- Existing difficulty and patience upgrades apply to newly spawned customers. Grill speed events report speed, not the number of grills.
- Settings copies preserve run statistics. New-run resets and daily happy-customer resets are explicit.
- Coin animations are cosmetic; score UI follows authoritative data and cannot add delayed rewards after a penalty.
- Trash costs three coins only when a disposable item is actually released into the bin; crossing the bin or returning a dispenser is free.
- Initial day UI is set after overlay initialization, avoiding a `Start` ordering race.

Missing core configuration produces actionable errors or safely rejects a spawn/recipe. Verbose legacy logs are compiled out by default; add the `SUSHI_VERBOSE_LOGS` scripting define to inspect them. Category logging can be enabled in `Assets/Resources/Guru/ScriptableObjects/LogSettings.asset`; its checked-in defaults are off. Warnings and errors remain visible.

See [script audit and regression map](docs/code-audit.md) and [verification and profiling evidence](docs/verification.md). The latter distinguishes batch measurements from interactive rendering performance.

## Roadmap and known limits

The first phase stabilizes code, adds repeatable tests and makes contributor instructions accurate ([issue #2](https://github.com/Collaboration95/Sushi-Shakedown/issues/2)). Remaining legacy limitations include mutable public data, scene-local fallback lookups, string-based event names, and direct prefab instantiation. This pass does not replace those systems with a new framework.

The **later visual/UI phase is separate**: missing animation coverage, interface layout, current screenshots and a generated-asset pipeline need their own acceptance criteria and review. No new illustration, animation library or provider selection is included here. Existing README screenshots are omitted because they have not been revalidated against the current Unity 6 build.
