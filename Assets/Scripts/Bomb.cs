using UnityEngine;

public class Bomb : MonoBehaviour
{
  public float timeToExplosion = 3f;
  public float areaOfEffect = 7.5f;
  public float damage = 200f;
  public float explosionForce = 500f;
  public float scale = .2f;

  [SerializeField]
  bool exploded = false;

  public void Explode()
  {
    if (exploded)
      return;

    // Prevent this method from being called again
    exploded = true;

    // Don't render sprite anymore
    GetComponent<SpriteRenderer>().enabled = false;

    // Start explosion particle effect
    var explosion = GetComponent<ParticleSystem>();
    explosion.Play();

    // Affect game objects in area of efffect
    Vector2 position = GetComponent<Rigidbody2D>().position;
    float scaledAOE = areaOfEffect * scale;
    Collider2D[] objectsInRange = Physics2D.OverlapCircleAll(position, scaledAOE);
    foreach (var c in objectsInRange)
    {
      // Knock stuff around
      Rigidbody2D body = c.GetComponent<Rigidbody2D>();
      if (body != null)
      {
        Vector2 distance = body.position - position;
        Vector2 force = distance.normalized;
        float distanceMultiplier = 1.0f - distance.magnitude / scaledAOE;
        body.AddForce(
          force * explosionForce * scale * distanceMultiplier,
          ForceMode2D.Impulse);

        // Notify objects that someone has set upon them the bomb
        c.SendMessage("OnExplosion", damage * scale * distanceMultiplier,
          SendMessageOptions.DontRequireReceiver);
      }
    }

    // Self-destruct after particle effect
    Destroy(gameObject, explosion.main.duration);
  }

  void OnExplosion()
  {
    Explode();
  }

  void Start()
  {
    transform.localScale = new Vector3(scale, scale, scale);
  }

  void Update()
  {
    timeToExplosion -= Time.deltaTime;
    if (timeToExplosion <= .0f)
      Explode();
  }
}
