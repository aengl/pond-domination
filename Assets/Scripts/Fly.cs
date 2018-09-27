using UnityEngine;
using System;

public class Fly : MonoBehaviour
{
  public Mutation mutation = Mutation.Giant;

  Pond pond;
  Rigidbody2D body;
  AudioSource audioSource;
  Vector2 target = new Vector2();
  Rigidbody2D stickingTo = null;
  public AudioClip[] clips;

  void Awake()
  {
    pond = FindObjectOfType<Pond>();
    body = GetComponent<Rigidbody2D>();
    audioSource = GetComponent<AudioSource>();
  }

  void Start()
  {
    // Pick random mutation
    Array mutations = Enum.GetValues(typeof(Mutation));
    int mutationIndex = UnityEngine.Random.Range(0, mutations.Length);
    mutation = (Mutation)mutations.GetValue(mutationIndex);

    // Update shader based on mutation
    GetComponent<SpriteRenderer>().material.SetVector("_HSLAAdjust",
      new Vector4((float)mutationIndex * .1f, .5f, -.2f, 0f));

    // Pick random waypoints
    InvokeRepeating("UpdateTarget", 0f, 1f);

    // Play fly sound
    audioSource.clip = Utils.GetRandomClip(clips);
    audioSource.pitch = UnityEngine.Random.Range(.8f, 1.8f);
    audioSource.Play();
  }

  void FixedUpdate()
  {
    if (stickingTo)
      body.position = stickingTo.position;
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
