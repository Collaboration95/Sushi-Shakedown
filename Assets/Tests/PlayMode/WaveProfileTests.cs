using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using Unity.Profiling;
using System.Diagnostics;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class WaveProfileTests
{
    [UnityTest]
    public IEnumerator RepresentativeCustomerWave()
    {
        UnityEngine.Random.InitState(2026);
        yield return SceneManager.LoadSceneAsync("MainMenu");
        yield return null;
        GameSceneManager.instance.StartGame();
        yield return null;
        yield return null;
        var overlay = UnityEngine.Object.FindFirstObjectByType<OverLayManager>();
        overlay.customerData.ResetEverything();
        overlay.customerData.SetGameMode(GameMode.Waves);
        overlay.ClosePreDayUI();
        Time.timeScale = 10f;
        Time.captureDeltaTime = 1f / 60f;
        for (int i = 0; i < 120; i++) yield return null;
        int logCount = 0;
        Application.LogCallback countLogs = (message, trace, type) => { if (type == LogType.Log) logCount++; };
        Application.logMessageReceived += countLogs;
        var frame = new double[300];
        var cpu = new double[300];
        var process = Process.GetCurrentProcess();
        var cpuStart = process.TotalProcessorTime;
        var mainThread = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 1);
        var gc = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame", 1);
        long gcBytes = 0;
        var clock = Stopwatch.StartNew();
        for (int i = 0; i < frame.Length; i++)
        {
            clock.Restart();
            yield return null;
            frame[i] = clock.Elapsed.TotalMilliseconds;
            cpu[i] = mainThread.LastValue / 1000000.0;
            gcBytes += gc.LastValue;
        }
        double processCpu = (process.TotalProcessorTime - cpuStart).TotalMilliseconds;
        process.Dispose();
        mainThread.Dispose();
        gc.Dispose();
        Application.logMessageReceived -= countLogs;
        Array.Sort(frame);
        double total = 0, cpuTotal = 0;
        foreach (double t in frame) total += t;
        foreach (double t in cpu) cpuTotal += t;
        UnityEngine.Debug.Log($"WAVE_PROFILE frames=300 mean_ms={total/300:F3} p95_ms={frame[284]:F3} process_cpu_ms_per_frame={processCpu/300:F3} recorder_cpu_ms={cpuTotal/300:F3} managed_bytes={gcBytes} normal_logs={logCount} customers={UnityEngine.Object.FindObjectsByType<CustomerController>(FindObjectsSortMode.None).Length}");
        Time.timeScale = 1;
        Time.captureDeltaTime = 0;
        GameSceneManager.instance.BackToMainMenu();
        yield return null;

    }
}
