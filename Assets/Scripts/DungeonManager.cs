using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum DungeonType { Caverns, Rooms, Winding }

public class DungeonManager : MonoBehaviour
{
    public GameObject floorPrefab, wallPrefab, tilePrefab, exitPrefab;
    public GameObject[] randomItems, randomEnemies;
    [Range(50, 1000)] public int totalFloorCount;
    [Range(0, 100)] public int itemSpawnPercent;
    [Range(0, 100)] public int enemySpawnPercent;
    [Tooltip("DungeonTypeがWindingの時、低いほど部屋が作成される")]
    [Range(0, 100)] public int windingHallPercent;
    public DungeonType dungeonType;

    [HideInInspector] public float minX, maxX, minY, maxY;

    List<Vector3> floorList = new List<Vector3>();
    Vector3 doorPos;
    LayerMask floorMask;
    LayerMask wallMask;

    void Start()
    {
        floorMask = LayerMask.GetMask("Floor");
        wallMask = LayerMask.GetMask("Wall");

        switch (dungeonType)
        {
            case DungeonType.Caverns:
                RandomWalker();
                break;
            case DungeonType.Rooms:
                RoomWalker();
                break;
            case DungeonType.Winding:
                WindingWalker();
                break;
        }
    }
    void Update()
    {
        // テスト用
        if (Application.isEditor && Input.GetKeyDown(KeyCode.Backspace))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    /// <summary>
    /// ランダムマップジェネレーター
    /// </summary>
    void RandomWalker()
    {
        Vector3 curPos = Vector3.zero; // x: 0, y: 0, z: 0
        floorList.Add(curPos);

        while (floorList.Count < totalFloorCount)
        {
            curPos += RandomDirection();
            if (!InFloorList(curPos))
            {
                floorList.Add(curPos);
            }
        }

        StartCoroutine(DelayProgress());
    }

    void RoomWalker()
    {
        Vector3 curPos = Vector3.zero; // x: 0, y: 0, z: 0
        floorList.Add(curPos);

        while (floorList.Count < totalFloorCount)
        {
            // 道の作成
            curPos = TakeAHike(curPos);

            // 部屋の作成
            RandomRoom(curPos);
        }

        StartCoroutine(DelayProgress());
    }

    /// <summary>
    /// 部屋までの道に部屋を作る可能性を調節可能なマップジェネレーター
    /// </summary>
    void WindingWalker()
    {
        Vector3 curPos = Vector3.zero; // x: 0, y: 0, z: 0
        floorList.Add(curPos);

        while (floorList.Count < totalFloorCount)
        {
            // 道の作成
            curPos = TakeAHike(curPos);

            // 部屋の作成
            int roll = Random.Range(0, 100);
            if (roll > windingHallPercent)
            {
                RandomRoom(curPos);
            }
        }

        StartCoroutine(DelayProgress());
    }

    Vector3 TakeAHike(Vector3 myPos)
    {
        Vector3 walkDir = RandomDirection();
        int walkLength = Random.Range(9, 18); // 部屋までの道の長さ
        for (int i = 0; i < walkLength; i++)
        {
            if (!InFloorList(myPos + walkDir))
            {
                floorList.Add(myPos + walkDir);
            }
            myPos += walkDir;
        }

        return myPos;
    }

    void RandomRoom(Vector3 myPos)
    {
        int width = Random.Range(1, 5); // 半径
        int height = Random.Range(1, 5); // 半径
        for (int w = -width; w <= width; w++)
        {
            for (int h = -height; h <= height; h++)
            {
                Vector3 offset = new Vector3(w, h, 0);
                if (!InFloorList(myPos + offset))
                {
                    floorList.Add(myPos + offset);
                }
            }
        }
    }

    bool InFloorList(Vector3 myPos)
    {
        for (int i = 0; i < floorList.Count; i++)
        {
            if (Vector3.Equals(myPos, floorList[i]))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 上下左右のベクトルをランダムに生成する
    /// </summary>
    /// <returns>ベクトル</returns>
    Vector3 RandomDirection()
    {
        switch (Random.Range(1, 5))
        {
            case 1:
                return Vector3.up;
            case 2:
                return Vector3.right;
            case 3:
                return Vector3.down;
            case 4:
                return Vector3.left;
        }
        return Vector3.zero;
    }

    IEnumerator DelayProgress()
    {
        // floorListを元にタイルを生成
        for (int i = 0; i < floorList.Count; i++)
        {
            GameObject goTile = Instantiate(tilePrefab, floorList[i], Quaternion.identity);
            goTile.name = tilePrefab.name;
            goTile.transform.SetParent(transform);
        }

        // タイルの生成が終了するのを待つ
        while (FindObjectsOfType<TileSpawner>().Length > 0)
        {
            yield return null;
        }

        // Exitの作成
        Exitway();

        // アイテムの生成
        Vector2 hitSize = Vector2.one * .8f;
        for (int x = (int)minX - 2; x <= (int)maxX + 2; x++)
        {
            for (int y = (int)minY - 2; y <= (int)maxY + 2; y++)
            {
                Collider2D hitFloor = Physics2D.OverlapBox(new Vector2(x, y), hitSize, 0, floorMask);
                if (hitFloor)
                {
                    if (!Vector2.Equals(hitFloor.transform.position, doorPos))
                    {
                        Collider2D hitTop = Physics2D.OverlapBox(new Vector2(x, y + 1), hitSize, 0, wallMask);
                        Collider2D hitRight = Physics2D.OverlapBox(new Vector2(x + 1, y), hitSize, 0, wallMask);
                        Collider2D hitBottom = Physics2D.OverlapBox(new Vector2(x, y - 1), hitSize, 0, wallMask);
                        Collider2D hitLeft = Physics2D.OverlapBox(new Vector2(x - 1, y), hitSize, 0, wallMask);

                        RandomItems(hitFloor, hitTop, hitRight, hitBottom, hitLeft);
                        RandomEnemies(hitFloor, hitTop, hitRight, hitBottom, hitLeft);
                    }
                }
            }
        }

    }

    void RandomEnemies(Collider2D hitFloor, Collider2D hitTop, Collider2D hitRight, Collider2D hitBottom, Collider2D hitLeft)
    {
        if (!hitTop && !hitRight && !hitBottom && !hitLeft)
        {
            int roll = Random.Range(1, 101);
            if (roll <= enemySpawnPercent)
            {
                int enemyIndex = Random.Range(0, randomEnemies.Length);
                GameObject item = randomEnemies[enemyIndex];
                GameObject goEnemy = Instantiate(item, hitFloor.transform.position, Quaternion.identity);
                goEnemy.name = item.name;
                goEnemy.transform.SetParent(hitFloor.transform);
            }
        }
    }

    void RandomItems(Collider2D hitFloor, Collider2D hitTop, Collider2D hitRight, Collider2D hitBottom, Collider2D hitLeft)
    {
        if ((hitTop || hitRight || hitBottom || hitLeft) && !(hitTop && hitBottom) && !(hitRight && hitLeft))
        {
            int roll = Random.Range(1, 101);
            if (roll <= itemSpawnPercent)
            {
                int itemIndex = Random.Range(0, randomItems.Length);
                GameObject item = randomItems[itemIndex];
                GameObject goItem = Instantiate(item, hitFloor.transform.position, Quaternion.identity);
                goItem.name = item.name;
                goItem.transform.SetParent(hitFloor.transform);
            }
        }
    }

    void Exitway()
    {
        doorPos = floorList[floorList.Count - 1];
        GameObject goDoor = Instantiate(exitPrefab, doorPos, Quaternion.identity);
        goDoor.name = exitPrefab.name;
        goDoor.transform.SetParent(transform);
    }
}
