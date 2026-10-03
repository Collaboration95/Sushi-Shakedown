# Unity 6.6 hardening verification

Executed locally on 3 October 2026 with Unity **6000.6.1f1**, macOS/Apple Silicon. This report records observed results; reproduction instructions in the audit are not presented as executed manual tests.

## Automated and build evidence

| Check | Result |
| --- | --- |
| `./scripts/test-unity.sh` — EditMode | **19 / 19 passed** |
| `./scripts/test-unity.sh` — PlayMode | **21 / 21 passed** |
| `VerifyBuild.Perform` — macOS development player | **Succeeded; 3 scenes; 0 build errors** |
| Headless Editor scene flow | Menu Play → Customers + KitchenScene → Day 1 → wave start → customer arrival/orders |
| Default 120-second wave | Completed at accelerated simulation time; failure overlay, no running spawner, all reservations free |
| Success/debt flow | Configured Day 1 payment, results overlay, Day 2, daily happy-customer reset |
| Serving/failure/reload | Concurrent servings resolve once; timeout cancels rewards; delayed coins cannot inflate score; pause/restart/reload restores time/input and unique managers |
| Scene configuration | All canonical scenes have no missing script components; trash prefab has its required data reference |
| README | Rendered locally with Pandoc and visually checked for headings, scene table and commands |

Results XML and logs are generated in ignored `Logs/Tests/EditMode.xml`, `PlayMode.xml` and accompanying `.log` files. The build uses the editor-only helper, with runtime/test assemblies separate. Tests that intentionally remove configuration declare their expected errors; unexpected runtime error/exception logs fail the PlayMode runner.

The built player's Main Menu and Day 1 screen were also checked visually, then the day screen was closed to start the wave. The default wave reached the failure overlay without runtime exceptions. A native-player menu reload was not confirmed; the automated suite verifies the visible failure button through the EventSystem raycast and its serialized click callback. This is supplementary player evidence; the headless suites provide the repeated Editor lifecycle and full-wave coverage. Interactive Editor audio/visual perception remains a useful contributor check.

## Before/after customer-wave profiling

Baseline: commit `75e28ea`, copied to a separate temporary project with only the runtime/test assembly setup, removal of unused assembly imports, and the identical profiling harness. Gameplay logic and verbose logging were otherwise unchanged. This avoids changing the user's checkout while measuring the old code.

`WaveProfileTests.RepresentativeCustomerWave` uses seed 2026, the canonical scene flow, a fixed 1/60-second capture step, 10× simulation time, 120 warm-up frames and **300 sampled frames**. Three customer objects remain active at the sample end in both runs. Frame intervals use Stopwatch between test resumes; process CPU is `Process.TotalProcessorTime` delta / 300. This is a headless Editor comparison including editor/test overhead, not a rendered-player FPS benchmark.

| Metric | Baseline | Hardened |
| --- | ---: | ---: |
| Mean measured frame interval | 0.255 ms | 0.201 ms |
| 95th-percentile interval | 0.372 ms | 0.250 ms |
| Process CPU per sampled frame | 0.924 ms | 0.670 ms |
| Normal gameplay log messages in sample | 82 | 0 |
| Active customer objects at sample end | 3 | 3 |

In this single local sample, mean frame interval decreased about **21%**, p95 about **33%**, and process CPU about **27%**. Treat these as directional evidence, with OS/editor noise and no rendering or player-input workload. No meaningful headless CPU/frame regression was observed. Representative interactive cooking/serving performance still needs a rendered Profiler capture before making a shipped-performance claim.

The Unity `Main Thread` and `GC Allocated In Frame` recorders returned **zero in both batch runs**, despite baseline logging allocations. Those counters are unavailable in this configuration; the zero values are **not** evidence of zero GC allocation. Structural GC improvements are independently identifiable: grill progress updates now use a readonly struct instead of allocating a class per frame; plate matching no longer copies ingredient lists per candidate; each patience countdown reuses one wait instruction. Existing instantiation, UI status strings, click raycast arrays and on-delivery ingredient copies remain. No numerical GC reduction is claimed.

Reproduce the directional comparison by using the same harness on an isolated baseline and the new branch, same editor/license/packages and machine conditions. For rendered profiling, open MainMenu, start a wave, prepare/serve food with the Profiler CPU and Memory modules recording, and compare the same actions on both branches.

## Scope and limitations

No generated art, animation library, UI redesign, asset-provider selection or broad gameplay architecture replacement is included. Missing-animation coverage and UI layout improvements belong to the separate visual phase described in the README. Existing Unity/package messages about fonts, import upgrades or licensing-token refresh may appear in editor logs; tests and the player build still succeeded. Generated performance-test resource JSON, build outputs, caches and incidental URP/player-settings reserialization are excluded from the PR.
