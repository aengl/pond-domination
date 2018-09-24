using UnityEngine;

public class Map : MonoBehaviour
{
  public Fly fly;

  BoxCollider2D boxCollider;

  public Vector2 GetRandomEdgePoint()
  {
    Bounds bounds = boxCollider.bounds;
    float x1 = bounds.min.x;
    float x2 = bounds.max.x;
    float y1 = bounds.min.y;
    float y2 = bounds.max.y;

    switch (Random.Range(0, 4))
    {
      case 0:
        return new Vector2(Random.Range(x1, x2), y1);
      case 1:
        return new Vector2(x1, Random.Range(y1, y2));
      case 2:
        return new Vector2(Random.Range(x1, x2), y2);
      case 3:
        return new Vector2(x2, Random.Range(y1, y2));
      default:
        throw new UnityException();
    }
  }

  void Awake()
  {
    boxCollider = GetComponent<BoxCollider2D>();
  }

  void Start()
  {
    InvokeRepeating("Spawn", 0f, 1f);
  }

  void Spawn()
  {
    // Spawn flies
    if (Random.value < .15f)
      SpawnFly();
  }

  void SpawnFly()
  {
    Fly fly = Instantiate(this.fly);
    fly.transform.position = GetRandomEdgePoint();
  }
}
