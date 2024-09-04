using UnityEngine;

public class PlyerButton : MonoBehaviour
{
    public void PlayerAttack()
    {
        Player.instance.Attack();
    }
}
