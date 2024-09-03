using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class Exitway : MonoBehaviour
{
    void Reset()
    {
        GetComponent<Rigidbody2D>().isKinematic = true;
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        box.size = Vector2.one * .1f;
        box.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            DungeonManager.instance.UpdateFloorCount();
            DungeonManager.instance.GenerateDungeon();
        }
    }
}

