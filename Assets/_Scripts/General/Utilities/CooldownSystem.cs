using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CooldownSystem : MonoBehaviour
{
    private readonly List<CooldownData> _cooldownList = new List<CooldownData>();
    private void Update() => ProcessCooldown();
    private void ProcessCooldown()
    {
        for (int i = _cooldownList.Count - 1; i >= 0; i--)
        {
            if (_cooldownList[i].isDone) _cooldownList.RemoveAt(i);
        }
    }

    public bool IsOnCooldown(CooldownData data)
    {
        return _cooldownList.Contains(data);
    }

    public CooldownData PutOnCooldown(IHasCooldown cooldown)
    {
        CooldownData data = new CooldownData(cooldown);
        _cooldownList.Add(data);
        return data;
    }
}

public class CooldownData
{
    public CooldownData(IHasCooldown cooldown)
    {
        finishedTime = Time.time + cooldown.cooldownDuration;
    }
    public float finishedTime { get; }
    public float remainingTime => Mathf.Max(finishedTime - Time.time, 0);
    public bool isDone => Time.time > finishedTime;
}