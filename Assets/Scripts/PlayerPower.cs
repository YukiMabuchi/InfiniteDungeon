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

    private void Update()
    {
        // テスト用
        if (Application.isEditor && Input.GetKeyDown(KeyCode.B))
        {
            maxPower = 1000;
            currentPower = maxPower;
        }
    }
}
