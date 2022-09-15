using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NetCheckout;

[CustomEditor(typeof(Settings))]
public class SettingsEditor : Editor
{
    private bool debugIsEnabled;
    private bool debugToggleIsOn;

    private bool playMakerIsEnabled;
    private bool playMakerToggleIsOn;

    private readonly string debugTooltip = "Use test keys in the built application.";
    private readonly string playMakerTooltip = "Use NetCheckout's custom Actions if PlayMaker is installed.";

    private const string DEBUG_SYMBOL = "NETCHECKOUT_DEBUG";
    private const string RELEASE_SYMBOL = "NETCHECKOUT_RELEASE";

    private const string PLAYMAKER_SYMBOL = "NETCHECKOUT_PLAYMAKER";

    private BuildTargetGroup targetGroup;

    private void Awake()
    {
        targetGroup = GetCurrentBuildTargetGroup();
        string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

        if (!defines.Contains(DEBUG_SYMBOL) && !defines.Contains(RELEASE_SYMBOL))
            SetDefineSymbol(DEBUG_SYMBOL);

        debugIsEnabled = defines.Contains(DEBUG_SYMBOL);
        playMakerIsEnabled = defines.Contains(PLAYMAKER_SYMBOL);
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        debugToggleIsOn = EditorGUILayout.Toggle(new GUIContent("Build With Test Keys", debugTooltip), debugIsEnabled);

        playMakerToggleIsOn = EditorGUILayout.Toggle(new GUIContent("Enable PlayMaker Actions", playMakerTooltip), playMakerIsEnabled);

        if (debugToggleIsOn && !debugIsEnabled)
        {
            SwitchToDebugMode();
        }

        if (!debugToggleIsOn && debugIsEnabled)
        {
            SwitchToReleaseMode();
        }

        if (playMakerToggleIsOn && !playMakerIsEnabled)
        {
            SetPlayMakerDefineSymbol();
        }

        if (!playMakerToggleIsOn && playMakerIsEnabled)
        {
            RemovePlayMakerDefineSymbol();
        }
    }

    private BuildTargetGroup GetCurrentBuildTargetGroup()
    {
        var target = EditorUserBuildSettings.activeBuildTarget;
        return BuildPipeline.GetBuildTargetGroup(target);
    }

    private void SetDefineSymbol(string symbol)
    {
        string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

        if (!defines.Contains(symbol))
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defines + ";" + symbol);
    }

    private void RemoveDefineSymbol(string symbol)
    {
        string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

        if (defines.Contains(symbol))
        {
            defines = defines.Replace(symbol, "");
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defines);
        }
    }

    private void ReplaceDefineSymbol(string fromSymbol, string toSymbol)
    {
        string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

        if (defines.Contains(fromSymbol))
        {
            defines = defines.Replace(fromSymbol, toSymbol);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defines);
        }
    }

    private void SwitchToDebugMode()
    {
        ReplaceDefineSymbol(RELEASE_SYMBOL, DEBUG_SYMBOL);

        debugIsEnabled = true;

        LogDebugStatus();
    }

    private void SwitchToReleaseMode()
    {
        ReplaceDefineSymbol(DEBUG_SYMBOL, RELEASE_SYMBOL);

        debugIsEnabled = false;

        LogDebugStatus();
    }

    private void SetPlayMakerDefineSymbol()
    {
        SetDefineSymbol(PLAYMAKER_SYMBOL);

        playMakerIsEnabled = true;

        LogPlayMakerStatus();
    }

    private void RemovePlayMakerDefineSymbol()
    {
        RemoveDefineSymbol(PLAYMAKER_SYMBOL);

        playMakerIsEnabled = false;

        LogPlayMakerStatus();
    }

    private void LogDebugStatus()
    {
        string state = debugIsEnabled ? "ENABLE" : "DISABLE";
        string message = string.Format("NET CHECKOUT will build with test keys {0}D on the {1} platform. {2}", state, targetGroup.ToString(),
            string.Format("If you are building for other platforms, you must {0} this option on those platforms as well.", state.ToLower()));

        Debug.LogWarning(message);
    }

    private void LogPlayMakerStatus()
    {
        string state = playMakerIsEnabled ? "ENABLE" : "DISABLE";
        string message = string.Format("NET CHECKOUT's PlayMaker Actions are {0}D on the {1} platform. {2}", state, targetGroup.ToString(),
            string.Format("If you are building for other platforms, you must {0} PlayMaker Actions on those platforms as well.", state.ToLower()));

        Debug.LogWarning(message);
    }
}