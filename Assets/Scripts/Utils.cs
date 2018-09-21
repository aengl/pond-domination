using UnityEngine;

public static class Utils
{
  public static bool VectorHasLength(Vector2 v)
  {
    return v.x > float.Epsilon || v.y > float.Epsilon;
  }

  public static float GetRotationForVector(Vector2 v)
  {
    Vector2 n = v.normalized;
    return Mathf.Atan2(n.y, n.x) * Mathf.Rad2Deg - 90.0f;
  }

  public static void RotateTowards(
    Rigidbody2D body, float targetRotation, float speed, float timeDelta)
  {
    float rotation = body.rotation;

    // Figure out which way to rotate
    float rotationDelta = targetRotation - rotation;
    if (rotationDelta > 180f)
      rotationDelta -= 360f;

    // Update rotation
    if (Mathf.Abs(rotationDelta) > float.Epsilon)
    {
      float newRotation = rotation + speed * rotationDelta * timeDelta;
      body.MoveRotation(newRotation);
    }

    if (body.rotation > 360f) body.rotation -= 360f;
    if (body.rotation < -360f) body.rotation += 360f;
  }

  public static void RotateTowards(
    Rigidbody2D body, Vector2 target, float speed, float timeDelta)
  {
    Utils.RotateTowards(body, Utils.GetRotationForVector(target), speed, timeDelta);
  }

  public static void MoveForward(Rigidbody2D body, float speed, float timeDelta)
  {
    body.AddRelativeForce(Vector2.up * speed * timeDelta, ForceMode2D.Force);
  }
}
