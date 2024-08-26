using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class GameModel
{
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

    public const int kMaxNumberOfSimultaneousDrips_Easy = 2;
    public const int kMaxNumberOfSimultaneousDrips_Medium = 3;
    public const int kMaxNumberOfSimultaneousDrips_Hard = 5;

    public const float kDripZoneActivationInterval = 3.0f;

    public const float kDripInterval = 5.0f;
    public const float kBrainHPFull = 100;
    private const int kicecreamHPFull = 100;
    private const float kBrainHPRechargePerSecond = 10.0f;

    private float dripZoneActivationTime = kDripZoneActivationInterval;

    private float brainHP = kBrainHPFull;
    private int cleanHP = 100;
    private int icecreamHP = kicecreamHPFull;
    private List<DripZone> dripZones = new List<DripZone>();

    public bool IsDripZoneActive(int idx)
    {
        Debug.Assert(0 <= idx && idx < dripZones.Count);
        return dripZones[idx].isActive;
    }
    // function that returns 0 - 1 value of how close the drip is to dripping
    public float GetDripTimeNormalized(int idx)
    {
        Debug.Assert(0 <= idx && idx < dripZones.Count);
        return 1 - (dripZones[idx].secondsToDrip / kDripInterval);
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
    // fire event on drip, including idx
    public event Action<int> DripEvent;
    // fire game over event
    public event Action GameOverEvent;
    private void FireDripEvent(int idx)
    {
        DripEvent?.Invoke(idx);
    }

    public GameModel(int numDripZones)
    {
        Reset(numDripZones);
    }

    public void Reset(int numDripZones)
    {
        brainHP = kBrainHPFull;
        cleanHP = 100;
        icecreamHP = kicecreamHPFull;
        dripZoneActivationTime = kDripZoneActivationInterval;
        dripZones = new List<DripZone>();
        for (int i = 0; i < numDripZones; i++)
        {
            dripZones.Add(new DripZone());
        }

        // pick kMaxNumberOfSimultaneousDrips_Medium random drip zones and set them to active
        for (int i = 0; i < kMaxNumberOfSimultaneousDrips_Medium; i++)
        {
            int idx = UnityEngine.Random.Range(0, dripZones.Count);
            ResetDripInterval(idx);
            dripZones[idx].isActive = true;
        }
    }

    private void ActivateDripZones(float deltaTime)
    {
        dripZoneActivationTime -= deltaTime;
        if (dripZoneActivationTime <= 0)
        {
            dripZoneActivationTime = kDripZoneActivationInterval;
            // count number of active drips
            int numActiveDrips = 0;
            for (int i = 0; i < dripZones.Count; i++)
            {
                if (dripZones[i].isActive)
                {
                    numActiveDrips++;
                }
            }

            int numToActivate = kMaxNumberOfSimultaneousDrips_Medium - numActiveDrips;
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
        dripZones[idx].secondsToDrip = kDripInterval;
        dripZones[idx].isActive = false;
    }

    public void Update(float deltaTime)
    {
        brainHP = Math.Min(brainHP + kBrainHPRechargePerSecond * deltaTime, kBrainHPFull);
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
    public const int lickBrainHPCost = 10;
    public const int lickIcecreamHPCost = 10;
    public void LickDripless()
    {
        brainHP = Math.Max(brainHP - lickBrainHPCost, 0);
        icecreamHP -= lickIcecreamHPCost;
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