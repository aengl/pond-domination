using UnityEngine;

public class HealthBar : MonoBehaviour
{
    public Frog frog;

    RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void LateUpdate()
    {
        float healthBarWidth = frog.Health / frog.maxHealth * 100f;

        // Hide bar at full health
        //if (frog.maxHealth - frog.Health < float.Epsilon)
        //    healthBarWidth = 0f;

        // Update health bar
        rectTransform.sizeDelta = new Vector2(healthBarWidth, rectTransform.sizeDelta.y);

        // Billboard
        transform.LookAt(Camera.main.transform);
    }
}
