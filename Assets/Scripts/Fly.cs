using UnityEngine;

public class Fly : MonoBehaviour
{
  public Mutation mutation = Mutation.Giant;

  Pond pond;
  Rigidbody2D body;
  Vector2 target = new Vector2();
  Rigidbody2D stickingTo = null;

  void Awake()
  {
    pond = FindObjectOfType<Pond>();
    body = GetComponent<Rigidbody2D>();
  }

  void Start()
  {
    InvokeRepeating("UpdateTarget", 0f, 1f);
  }

  void FixedUpdate()
  {
    if (stickingTo)
    {
      body.position = stickingTo.position;
    }
    else
    {
      Vector2 toTarget = target - body.position;
      Utils.RotateTowards(body, toTarget, 1f, Time.fixedDeltaTime);
      Utils.MoveForward(body, 10f, Time.fixedDeltaTime);
      Debug.DrawLine(body.position, target, Color.white);
    }
  }

  void UpdateTarget()
  {
    target = pond.GetRandomPoint();
  }

  void OnTriggerEnter2D(Collider2D collision)
  {
    if (collision.tag == "Tongue")
    {
      // Stick to tongue
      stickingTo = collision.GetComponent<Rigidbody2D>();
    }
    else if (collision.tag == "Frog")
    {
      // Power-up frog
      Destroy(gameObject);
      collision.GetComponent<Frog>().Mutate(mutation);
    }
  }
}
