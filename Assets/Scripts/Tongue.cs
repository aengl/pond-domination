using UnityEngine;

public class Tongue : MonoBehaviour
{
  public float ejectForce;
  public float returnForce;
  public Frog frog;

  Rigidbody2D body;
  LineRenderer lineRenderer;

  [SerializeField]
  bool isReturning = false;

  [SerializeField]
  float returnDelay;

  public void Eject(Frog frog, float mass, float drag,
    float ejectForce, float returnForce, float returnDelay)
  {
    this.frog = frog;
    this.ejectForce = ejectForce * Mathf.Sqrt(mass);
    this.returnForce = returnForce * Mathf.Sqrt(mass) * 100f;
    this.returnDelay = returnDelay;

    body.mass = mass;
    body.drag = drag;

    transform.position = frog.MouthPosition;

    // Apply frog scale to tongue as well
    transform.localScale = frog.transform.localScale;
    lineRenderer.startWidth = frog.transform.localScale.x / 4f;
    lineRenderer.endWidth = lineRenderer.startWidth;
  }

  void Awake()
  {
    body = GetComponent<Rigidbody2D>();
    lineRenderer = GetComponent<LineRenderer>();
  }

  void Start()
  {
    // Ignore collisions with parent frog
    Physics2D.IgnoreCollision(frog.GetComponent<Collider2D>(), GetComponent<Collider2D>());

    // Eject from frog mouth
    Rigidbody2D frogBody = frog.GetComponent<Rigidbody2D>();
    Vector2 direction = frogBody.GetRelativeVector(Vector2.up);
    body.AddRelativeForce(direction * ejectForce * body.mass, ForceMode2D.Impulse);
  }

  void Update()
  {
    // Self-destruct if the parent frog is gone
    if (frog == null)
      Destroy(gameObject);
  }

  void FixedUpdate()
  {
    // Pull back tongue once it stops
    if (body.velocity.magnitude < .1f)
    {
      if (returnDelay <= 0f)
        isReturning = true;
      else
        returnDelay -= Time.fixedDeltaTime;
    }

    // Move back to frog mouth
    if (isReturning)
    {
      Vector2 back = frog.MouthPosition - body.position;
      body.AddForce(back * returnForce * body.mass * Time.fixedDeltaTime, ForceMode2D.Force);

      // Remove object once we're close to the frog mouth again
      if (back.magnitude < .1f)
        Destroy(gameObject);
    }
  }

  void LateUpdate()
  {
    lineRenderer.SetPosition(0, frog.MouthPosition);
    lineRenderer.SetPosition(1, transform.position);
  }
}
