using UnityEngine;

public class Tongue : MonoBehaviour
{
    public float ejectForce = 50f;
    public float returnForce = 2000f;

    Rigidbody2D body;
    LineRenderer lineRenderer;

    [SerializeField]
    bool isReturning = false;

    [SerializeField]
    Frog frog;

    public void Eject(Frog parent)
    {
        frog = parent;
        transform.position = parent.MouthPosition;
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

    void FixedUpdate()
    {
        // Pull back tongue once it stops
        if (body.velocity.magnitude < .1f)
        {
            isReturning = true;
        }

        // Move back to frog mouth
        if (isReturning)
        {
            Vector2 back = frog.MouthPosition - body.position;
            body.AddForce(back * returnForce * body.mass * Time.fixedDeltaTime, ForceMode2D.Force);

            if (back.magnitude < 1.0f)
            {
                Destroy(gameObject);
                Destroy(this);
            }
        }
    }

    void LateUpdate()
    {
        lineRenderer.SetPosition(0, frog.MouthPosition);
        lineRenderer.SetPosition(1, transform.position);
    }
}
