using System;
using UnityEngine;

public static class Controls
{
    readonly static float[] lastInput = { 0f, 0f, 0f, 0f };

    public static float? GetRotation(int playerIndex)
    {
        float x = Input.GetAxis(String.Format("Player{0} Horizontal", playerIndex));
        float y = Input.GetAxis(String.Format("Player{0} Vertical", playerIndex));
        bool rotate = Mathf.Abs(x) > float.Epsilon || Mathf.Abs(y) > float.Epsilon;
        if (rotate)
        {
            lastInput[playerIndex] = Time.fixedTime;
            return Mathf.Atan2(y, x) * Mathf.Rad2Deg - 90.0f;
        }
        return null;
    }

    public static bool Jump(int playerIndex)
    {
        bool jump = Input.GetAxis(String.Format("Player{0} Jump", playerIndex)) > float.Epsilon;
        if (jump)
            lastInput[playerIndex] = Time.fixedTime;
        return jump;
    }

    public static bool ShootTongue(int playerIndex)
    {
        bool shootTongue = Input.GetButton(String.Format("Player{0} Tongue", playerIndex));
        if (shootTongue)
            lastInput[playerIndex] = Time.fixedTime;
        return shootTongue;
    }

    public static float TimeSinceInput(int playerIndex)
    {
        return Time.fixedTime - lastInput[playerIndex];
    }
}
