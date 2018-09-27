using UnityEngine;

public class Bomb : MonoBehaviour
{
  public float timeToExplosion = 3f;
  public float areaOfEffect = 1.5f;
  public float damage = 25f;
  public float explosionForce = 100f;

  [SerializeField]
  bool exploded = false;

  public void Explode()
  {
    if (exploded)
      return;

    // Don't render sprite anymore
    GetComponent<SpriteRenderer>().enabled = false;

    // Start explosion particle effect
    var explosion = GetComponent<ParticleSystem>();
    explosion.Play();

    // Self-destruct after particle effect
    Destroy(gameObject, explosion.main.duration);

    // Affect game objects in area of efffect
    Vector2 position = GetComponent<Rigidbody2D>().position;
    Collider2D[] objectsInRange = Physics2D.OverlapCircleAll(position, areaOfEffect);
    Debug.Log(objectsInRange.Length);
    foreach (var c in objectsInRange)
    {
      // Knock stuff around
      Rigidbody2D body = c.GetComponent<Rigidbody2D>();
      if (body != null)
      {
        Vector2 distance = body.position - position;
        Vector2 force = distance.normalized;
        float distanceMultiplier = 1.0f - distance.magnitude / areaOfEffect;
        body.AddForce(
          force * explosionForce * distanceMultiplier,
          ForceMode2D.Impulse);

        // Notify objects that someone has set upon them the bomb
        c.SendMessage("OnExplosion", damage * distanceMultiplier,
          SendMessageOptions.RequireReceiver);
      }
    }

    // Prevent this method from being called again
    exploded = true;
  }

  void Update()
  {
    timeToExplosion -= Time.deltaTime;
    if (timeToExplosion <= .0f)
      Explode();
  }
}
