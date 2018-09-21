using UnityEngine;

public class Pond : MonoBehaviour
{
  PolygonCollider2D polygonCollider;

  public Vector2 GetRandomPoint()
  {
    // Returns a random point inside the pond
    Vector2 point;
    do
    {
      point = Random.insideUnitCircle * 8f;
    } while (!polygonCollider.OverlapPoint(point));
    return point;
  }

  void Awake()
  {
    polygonCollider = GetComponent<PolygonCollider2D>();
  }
}
