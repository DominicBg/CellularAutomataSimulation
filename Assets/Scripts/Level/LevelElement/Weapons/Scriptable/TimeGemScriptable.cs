using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TimeGemElement;

[CreateAssetMenu(fileName = "TimeGemScriptable", menuName = "Equipable/TimeGemScriptable", order = 1)]
public class TimeGemScriptable : EquipableBaseScriptable
{
    public BackgroundMaskJob.Settings backgroundMaskSettings;
    public BackGroundEffectJob.Settings backgroundEffectSettings;
    public SlowTimeBlurEffectGemJob.Settings enabledSettings;

    public PostProcessManager.ScreenFlashSettings onEnableFlash;
    public PostProcessManager.ScreenFlashSettings onDisableFlash;
    public PostProcessManager.ShockwaveSettings shockwave;
    public IllusionEffectSettings illusionSettings;


    public float dispersion;
    public float fadeoff;
}
