using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPower : MonoBehaviour
{
    [SerializeField] int maxPower = 4;

    int currentPower = 0;

    public int CurrentPower { get { return currentPower; } }

    void Start()
    {
        currentPower = maxPower;
    }
}
