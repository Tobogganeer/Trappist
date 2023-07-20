using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HP : MonoBehaviour
{
    [SerializeField]
    float _hp = 100f;
    public float HitPoints
    {
        get => _hp;
        set => _hp = Mathf.Clamp(value, 0f, MaxHitPoints);
    }

    [SerializeField]
    float _maxHP = 100f;
    public float MaxHitPoints
    {
        get => _maxHP;
        set
        {
            _maxHP = Mathf.Max(value, 0f);
            HitPoints = HitPoints; // Setter will clamp itself if it is over
        }
    }

    private void Awake()
    {
        _hp = MaxHitPoints;
    }


    public void Damage(float amount) => HitPoints -= amount;

    public void Heal(float amount) => HitPoints += amount;
    /// <summary>
    /// Sets MaxHitPoints and HitPoints to <paramref name="amount"/>.
    /// </summary>
    /// <param name="amount"></param>
    public void SetHP(float amount)
    {
        MaxHitPoints = amount;
        HitPoints = amount;
    }

    public static implicit operator float(HP hp) => hp.HitPoints;
}
