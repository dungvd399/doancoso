using Solo.MOST_IN_ONE;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HapticController : SingletonMonoBehaviour<HapticController>
{
    private const float HapticCooldown = 0.1f;
    private float lastHapticTime = 0f;
    public void PlayHaptic()
    {
        if (PlayerprefSave.Haptic == 0)
        {
            return; // Haptics are disabled in settings
        }
        if (Time.time - lastHapticTime < HapticCooldown)
        {
            return; // Prevent haptic spam
        }
        lastHapticTime = Time.time;
        MOST_HapticFeedback.Generate(MOST_HapticFeedback.HapticTypes.Selection);
    }
}
