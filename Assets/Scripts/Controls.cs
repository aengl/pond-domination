using System;
using UnityEngine;

public static class Controls
{
  readonly static float[] lastInput = { 0f, 0f, 0f, 0f };

  public static float? GetRotation(int playerIndex)
  {
    Vector2 v = new Vector2(
        Input.GetAxis(String.Format("Player{0} Horizontal", playerIndex)),
        Input.GetAxis(String.Format("Player{0} Vertical", playerIndex))
    );
    bool rotate = Utils.VectorHasLength(v);
    if (rotate)
    {
      lastInput[playerIndex - 1] = Time.fixedTime;
      return Utils.GetRotationForVector(v);
    }
    return null;
  }

  public static bool Jump(int playerIndex)
  {
    bool jump = Input.GetAxis(String.Format("Player{0} Jump", playerIndex)) > float.Epsilon;
    if (jump)
      lastInput[playerIndex - 1] = Time.fixedTime;
    return jump;
  }

  public static bool ShootTongue(int playerIndex)
  {
    bool shootTongue = Input.GetButton(String.Format("Player{0} Tongue", playerIndex));
    if (shootTongue)
      lastInput[playerIndex - 1] = Time.fixedTime;
    return shootTongue;
  }

  public static float TimeSinceInput(int playerIndex)
  {
    return Time.fixedTime - lastInput[playerIndex - 1];
  }
}
