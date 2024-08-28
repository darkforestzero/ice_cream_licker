using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class GameModel
{
    public class Settings
    {
        // diificulty
        public int kMaxNumberOfSimultaneousDrips_Easy = 2;
        public int kMaxNumberOfSimultaneousDrips_Medium = 3;
        public int kMaxNumberOfSimultaneousDrips_Hard = 5;

        // drips
        public float dripZoneActivationInterval = 3.0f;
        public float dripInterval = 5.0f;

        // icecream
        public int icecreamHPFull = 100;
        public int lickBrainDamage = 10;
        public int lickIcecreamDamage = 10;

        // brain / clean
        public float brainHPFull = 100;
        public float brainHPRechargePerSecond = 10.0f;
        public int cleanHPFull = 100;
    }

    private class DripZone
    {
        public float secondsToDrip = 0;
        public bool isActive = false;

        public DripZone()
        {
            secondsToDrip = 0;
            isActive = false;
        }
    }

    private Settings settings = new Settings();
    private float dripZoneActivationTime;
    private float brainHP;
    private int cleanHP;
    private int icecreamHP;

    private List<DripZone> dripZones = new List<DripZone>();


    public GameModel(Settings settings, int numDripZones)
    {
        Reset(settings, numDripZones);
    }

    public void Reset(Settings settings, int numDripZones)
    {
        Debug.Assert(settings != null);
        this.settings = settings;

        brainHP = settings.brainHPFull;
        cleanHP = settings.cleanHPFull;
        icecreamHP = settings.icecreamHPFull;
        dripZoneActivationTime = settings.dripZoneActivationInterval;

        dripZones = new List<DripZone>();
        for (int i = 0; i < numDripZones; i++)
        {
            dripZones.Add(new DripZone());
        }

        // pick kMaxNumberOfSimultaneousDrips_Medium random drip zones and set them to active
        // TODO: difficulty variable
        for (int i = 0; i < settings.kMaxNumberOfSimultaneousDrips_Medium; i++)
        {
            int idx = UnityEngine.Random.Range(0, dripZones.Count);
            ResetDripInterval(idx);
            dripZones[idx].isActive = true;
        }
    }

    public bool IsDripZoneActive(int idx)
    {
        Debug.Assert(0 <= idx && idx < dripZones.Count);
        return dripZones[idx].isActive;
    }
    // function that returns 0 - 1 value of how close the drip is to dripping
    public float GetDripTimeNormalized(int idx)
    {
        Debug.Assert(0 <= idx && idx < dripZones.Count);
        return 1 - (dripZones[idx].secondsToDrip / settings.dripInterval);
    }
    public float GetSecondsToDrip(int idx)
    {
        Debug.Assert(0 <= idx && idx < dripZones.Count);
        return dripZones[idx].secondsToDrip;
    }

    public float GetBrainHP()
    {
        return brainHP;
    }
    public float GetBrainHPFull()
    {
        return settings.brainHPFull;
    }
    // fire event on drip, including idx
    public event Action<int> DripEvent;
    // fire game over event
    public event Action GameOverEvent;
    private void FireDripEvent(int idx)
    {
        DripEvent?.Invoke(idx);
    }

    private void ActivateDripZones(float deltaTime)
    {
        dripZoneActivationTime -= deltaTime;
        if (dripZoneActivationTime <= 0)
        {
            dripZoneActivationTime = settings.dripZoneActivationInterval;
            // count number of active drips
            int numActiveDrips = 0;
            for (int i = 0; i < dripZones.Count; i++)
            {
                if (dripZones[i].isActive)
                {
                    numActiveDrips++;
                }
            }

            // TODO: difficulty variable
            int numToActivate = settings.kMaxNumberOfSimultaneousDrips_Medium - numActiveDrips;
            if (numToActivate > 0)
            {
                for (int i = 0; i < numToActivate; i++)
                {
                    int idx = UnityEngine.Random.Range(0, dripZones.Count);
                    ResetDripInterval(idx);
                    dripZones[idx].isActive = true;
                }
            }
        }
    }
    private void ResetDripInterval(int idx)
    {
        Debug.Log("Drip " + idx + " reset");
        Debug.Assert(0 <= idx && idx < dripZones.Count);
        dripZones[idx].secondsToDrip = settings.dripInterval;
        dripZones[idx].isActive = false;
    }

    public void Update(float deltaTime)
    {
        brainHP = Math.Min(brainHP + settings.brainHPRechargePerSecond * deltaTime, settings.brainHPFull);
        // Debug.Log("Brain HP: " + brainHP);

        ActivateDripZones(deltaTime);

        for (int i = 0; i < dripZones.Count; i++)
        {
            DripZone dripZone = dripZones[i];
            if (dripZone.isActive)
            {
                dripZone.secondsToDrip -= deltaTime;
                Debug.Log("Drip " + i + " seconds to drip: " + dripZone.secondsToDrip);
                if (dripZone.secondsToDrip <= 0)
                {
                    Debug.Log("Drip " + i + " dripped");
                    FireDripEvent(i);
                    ResetDripInterval(i);
                }
            }
        }
    }
    public void LickDripless()
    {
        brainHP = Math.Max(brainHP - settings.lickBrainDamage, 0);
        icecreamHP -= settings.lickIcecreamDamage;
        if (icecreamHP <= 0)
        {
            Debug.Log("Game Over: Ice Cream melted");
            GameOverEvent?.Invoke();
        }
    }
    public void Lick(int idx)
    {
        LickDripless();
        ResetDripInterval(idx);
    }
}