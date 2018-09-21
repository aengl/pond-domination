using UnityEngine;

public static class Utils
{
  public static bool VectorHasLength(Vector2 v)
  {
    return v.x > float.Epsilon || v.y > float.Epsilon;
  }

  public static float GetRotationForVector(Vector2 v)
  {
    return Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg - 90.0f;
  }

  public static void RotateTowards(Rigidbody2D body, float targetRotation, float turnSpeed)
  {
    float rotation = body.rotation;

    // Figure out which way to rotate
    float rotationDelta = targetRotation - rotation;
    if (rotationDelta > 180f)
      rotationDelta -= 360f;

    // Update rotation
    if (Mathf.Abs(rotationDelta) > float.Epsilon)
    {
      float newRotation = rotation + turnSpeed * rotationDelta * Time.fixedDeltaTime;
      body.MoveRotation(newRotation);
    }

    if (body.rotation > 360f) body.rotation -= 360f;
    if (body.rotation < -360f) body.rotation += 360f;
  }
}
