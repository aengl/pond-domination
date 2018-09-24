﻿using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum Mutation
{
  Giant,
  Blitz,
  SuperTongue,
  Phantasm
}

public class Frog : MonoBehaviour
{
  public int playerIndex = 1;
  public float maxHealth = 100f;
  public float turnSpeed;
  public float jumpForce;
  public float tongueDrag;
  public float tongueEjectForce;
  public float tongueReturnForce;
  public Tongue tongue;
  public HashSet<Mutation> mutations = new HashSet<Mutation>();
  public float minPitch;
  public float maxPitch;
  public AudioClip[] audioQuak;
  public AudioClip[] audioSlurp;
  public AudioClip[] audioHit;
  public AudioClip[] audioDeath;
  public AudioClip[] audioPickup;
  public AudioClip[] audioJump;

  public int PlayerIndex
  {
    get { return playerIndex; }
  }

  public float Health
  {
    get { return health; }
  }

  public Vector2 MouthPosition
  {
    get { return transform.position; }
  }

  public float ForceMultiplier
  {
    get { return body.mass; }
  }

  public bool IsStunned
  {
    get { return isStunned; }
  }

  public bool CanRotate
  {
    get { return !IsStunned; }
  }

  public bool CanJump
  {
    get
    {
      return !IsStunned
        && body.velocity.magnitude < .1f
        && activeTongue == null;
    }
  }

  public bool CanUseAbilities
  {
    get { return !IsStunned && activeTongue == null; }
  }

  public bool IsAIControlled
  {
    get { return Controls.TimeSinceInput(playerIndex) > 5f; }
  }

  Pond pond;
  Rigidbody2D body;
  AudioSource audioSource;
  new CapsuleCollider2D collider;
  SpriteRenderer spriteRenderer;
  Vector2? targetDirection = null;

  [SerializeField]
  Tongue activeTongue = null;

  [SerializeField]
  float health;

  [SerializeField]
  bool isStunned = false;

  [SerializeField]
  bool isOutsidePond = false;

  public void Mutate(Mutation mutation)
  {
    mutations.Add(mutation);
    UpdateMutations();
    Utils.PlayRandomClip(audioSource, audioPickup, minPitch, maxPitch);
  }

  void Awake()
  {
    pond = FindObjectOfType<Pond>();
    body = GetComponent<Rigidbody2D>();
    audioSource = GetComponent<AudioSource>();
    collider = GetComponent<CapsuleCollider2D>();
    spriteRenderer = GetComponent<SpriteRenderer>();
  }

  void Start()
  {
    Respawn();

    // Update all AIs with a slight offset
    InvokeRepeating("UpdateAI", .1f * (float)playerIndex, .4f);
    Invoke("Quak", Random.Range(5f, 10f));
  }

  void FixedUpdate()
  {
    // Update rotation
    if (CanRotate && targetDirection != null)
      Utils.RotateTowards(body, targetDirection.Value, turnSpeed, Time.deltaTime);
  }

  void Update()
  {
    // Recover
    if (isStunned)
      isStunned = body.velocity.magnitude > 1f || Mathf.Abs(body.angularVelocity) > 120f;

    // Rotate
    Vector2? v = Controls.GetDirection(playerIndex);
    if (v != null || !IsAIControlled)
      // Don't ever set direction to null when AI controlled
      targetDirection = v;

    // Jump
    if (Controls.Jump(playerIndex))
      Jump();

    // Shoot tongue
    if (Controls.ShootTongue(playerIndex))
      ShootTongue();

    // Update health
    if (isOutsidePond)
      health -= Time.deltaTime * 10f;
    else
      health += Time.deltaTime;

    health = Mathf.Min(maxHealth, health);

    // Die
    if (health <= .0f)
    {
      Utils.PlayRandomClip(audioSource, audioDeath, minPitch, maxPitch);

      // Only respawn if it's the last frog for this player
      int frogCount = GameObject.FindGameObjectsWithTag("Frog")
        .Count(frog => frog.GetComponent<Frog>().PlayerIndex == playerIndex);
      if (frogCount == 1)
        Respawn();
      else
      {
        // Remove tongue
        if (activeTongue != null)
          Destroy(activeTongue);

        Destroy(gameObject);
      }
    }
  }

  void LateUpdate()
  {
    UpdateDrag();

    // Show "hops" by increasing sprite size
    float scale = Mathf.Min(2.56f + body.velocity.magnitude / 20f, 3.5f);
    spriteRenderer.size = new Vector2(scale, scale);

    // Desaturate frog to show hitpoints
    spriteRenderer.material.SetVector("_HSLAAdjust",
      new Vector4(0f, 1.5f * (health / maxHealth) - 1f, 0f, 0f));
  }

  void UpdateMutations()
  {
    // Scale
    if (mutations.Contains(Mutation.Giant))
      transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
    else
      transform.localScale = new Vector3(.2f, .2f, .2f);

    // Frog size determines pitch
    minPitch = .9f;
    maxPitch = 1.2f;
    if (mutations.Contains(Mutation.Giant))
    {
      minPitch = .6f;
      maxPitch = .9f;
    }

    // Mass
    body.mass = 1f;
    if (mutations.Contains(Mutation.Giant))
      body.mass *= 4f;

    // Turn speed
    turnSpeed = 5f;
    if (mutations.Contains(Mutation.Giant))
      turnSpeed /= 8f;
    if (mutations.Contains(Mutation.Blitz))
      turnSpeed *= 4f;

    // Jump force
    jumpForce = 20f;
    if (mutations.Contains(Mutation.Giant))
      jumpForce /= 10f;
    if (mutations.Contains(Mutation.Blitz))
      jumpForce *= 5f;

    // Tongue drag
    tongueDrag = 10f;
    if (mutations.Contains(Mutation.SuperTongue))
      tongueDrag *= 2f;

    // Tongue eject force
    tongueEjectForce = 40f;
    if (mutations.Contains(Mutation.SuperTongue))
      tongueEjectForce *= 4f;

    // Tongue return force
    tongueReturnForce = 40f;
    if (mutations.Contains(Mutation.Giant))
      tongueReturnForce /= 2f;
    if (mutations.Contains(Mutation.SuperTongue))
      tongueReturnForce *= 12f;

    // Create frog clone
    if (mutations.Contains(Mutation.Phantasm))
    {
      health = maxHealth;
      mutations.Remove(Mutation.Phantasm);
      var newFrog = Instantiate(this);
      newFrog.tag = "Frog";
      newFrog.mutations = mutations;
      newFrog.body.position = pond.GetRandomPoint();
    }
  }

  void UpdateDrag()
  {
    float drag = 12f;
    float angularDrag = 2f;

    if (isStunned)
    {
      drag = 3f;
      angularDrag = 1f;
    }
    else if (isOutsidePond)
    {
      drag = 30f;
      angularDrag = 12f;
    }

    // Fast frogs have higher drag and recover faster
    if (mutations.Contains(Mutation.Blitz))
    {
      drag *= 4f;
      angularDrag *= 2f;
    }

    body.drag = drag;
    body.angularDrag = angularDrag;
  }

  void UpdateAI()
  {
    // AI takes over if the player hasn't been playing for a while
    if (IsAIControlled)
    {
      if (Random.value > .5f)
        Jump();
      else if (Random.value > .85f)
        ShootTongue();
      else if (activeTongue == null)
        targetDirection = Random.insideUnitCircle;
    }
  }

  void Jump()
  {
    if (CanJump)
    {
      body.AddRelativeForce(Vector2.up * jumpForce * ForceMultiplier, ForceMode2D.Impulse);
      Utils.PlayRandomClip(audioSource, audioJump, minPitch, maxPitch, .25f);
    }
  }

  void ShootTongue()
  {
    if (CanUseAbilities)
    {
      activeTongue = Instantiate(tongue);
      activeTongue.Eject(this, body.mass, tongueDrag,
        tongueEjectForce, tongueReturnForce);
      Utils.PlayRandomClip(audioSource, audioSlurp, minPitch, maxPitch);
    }
  }

  void Respawn()
  {
    health = maxHealth;
    isOutsidePond = false;

    body.velocity = new Vector2();
    body.angularVelocity = 0f;
    body.rotation = Random.Range(0f, 360f);
    body.position = pond.GetRandomPoint();

    mutations.Clear();
    UpdateMutations();
  }

  void Quak()
  {
    Utils.PlayRandomClip(audioSource, audioQuak, minPitch, maxPitch, .5f);
    Invoke("Quak", Random.Range(3f, 5f));
  }

  void OnCollisionEnter2D(Collision2D collision)
  {
    // Become stunned on collision
    isStunned = true;
    Utils.PlayRandomClip(audioSource, audioHit);

    // Lose a bit of health
    health -= 5f;
  }

  void OnTriggerEnter2D(Collider2D collision)
  {
    isOutsidePond &= collision.tag != "Pond";
  }

  void OnTriggerExit2D(Collider2D collision)
  {
    isOutsidePond |= collision.tag == "Pond";
  }
}
