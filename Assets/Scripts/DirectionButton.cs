using UnityEngine;

/// <summary>
/// ButtonのEvent TriggerのPointer downとPointer upで制御
/// </summary>
public class DirectionButton : MonoBehaviour
{
    Player player;
    string direction;
    bool buttonDownFlag = false;

    void Awake()
    {
        player = FindObjectOfType<Player>();
    }

    void Update()
    {
        if (buttonDownFlag && player != null) player.Move(direction);
    }

    public void OnButtonDown(string _direction)
    {
        buttonDownFlag = true;
        direction = _direction;
    }

    public void OnButtonUp()
    {
        buttonDownFlag = false;
    }
}
