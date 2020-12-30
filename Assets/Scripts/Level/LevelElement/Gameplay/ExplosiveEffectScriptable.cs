using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ExplosiveEffectScriptable", menuName = "Effects/ExplosiveEffectScriptable", order = 1)]
public class ExplosiveEffectScriptable : ScriptableObject
{
    public PostProcessManager.ShockwaveSettings shockwaveSettings;
    public PostProcessManager.ScreenFlashSettings screenFlashSettings;
    public PostProcessManager.ShakeSettings shakeSettings;
    public Explosive.Settings explosiveSettings;
}
