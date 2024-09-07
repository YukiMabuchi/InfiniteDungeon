using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyXp : MonoBehaviour
{
    [SerializeField] int maxXp = 4;
    [SerializeField] int xpRamp = 2;
    public int MaxXp { get { return maxXp; } }

    public void IncreaseXp(int ramp)
    {
        maxXp = maxXp * (ramp * xpRamp);
    }
}
