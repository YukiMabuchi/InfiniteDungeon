using UnityEngine;

public class PlyerButton : MonoBehaviour
{
    public void PlayerAttack()
    {
        PlayerPower playerPower = Player.instance.GetComponent<PlayerPower>();
        playerPower.AttackEnemy();
    }
}
