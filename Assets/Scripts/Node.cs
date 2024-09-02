using UnityEngine;

public class Node : MonoBehaviour
{
    public Vector2 position;
    public Vector2 parent;

    public Node(Vector2 _position, Vector2 _parent)
    {
        position = _position;
        parent = _parent;
    }
}
