using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Mutation
{
  Giant,
  Blitz,
  SuperTongue,
  Bomberman
}

public class Frog : MonoBehaviour
{
  public Color color;
  public int playerIndex = 1;
  public float maxHealth = 100f;
  public float turnSpeed;
  public float jumpForce;
  public float tongueDrag;
  public float tongueEjectForce;
  public float tongueReturnForce;
  public HashSet<Mutation> mutations = new HashSet<Mutation>();
  public float minPitch;
  public float maxPitch;
  public AudioClip[] audioQuak;
  public AudioClip[] audioSlurp;
  public AudioClip[] audioHit;
  public AudioClip[] audioDeath;
  public AudioClip[] audioPickup;
  public AudioClip[] audioJump;
  public Tongue tongue;
  public Bomb bomb;

  public float Health
  {
    get { return health; }
  }

  public Vector2 MouthPosition
  {
    get { return transform.position; }
  }

  public Rigidbody2D Body
  {
    get { return body; }
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
    get { return !isDying && !IsStunned; }
  }

  public bool IsOutsidePond
  {
    get { return isOutsidePond; }
  }

  public bool CanJump
  {
    get
    {
      return !isDying
        && !IsStunned
        && body.velocity.magnitude < .1f
        && activeTongue == null;
    }
  }

  public bool CanUseAbilities
  {
    get { return !isDying && activeTongue == null; }
  }

  public bool IsAIControlled
  {
    get { return Controls.TimeSinceInput(playerIndex) > 5f; }
  }

  Pond pond;
  Rigidbody2D body;
  AudioSource audioSourceHigh;
  AudioSource audioSourceMedium;
  AudioSource audioSourceLow;
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

  [SerializeField]
  bool isDying = false;

  public void Mutate(Mutation mutation)
  {
    mutations.Add(mutation);
    UpdateMutations();
    Utils.PlayRandomClip(audioSourceHigh, audioPickup, minPitch, maxPitch);
  }

  public void Heal()
  {
    health = maxHealth;
  }

  void Awake()
  {
    pond = FindObjectOfType<Pond>();
    body = GetComponent<Rigidbody2D>();
    var audioSources = GetComponents<AudioSource>();
    audioSourceHigh = audioSources[0];
    audioSourceMedium = audioSources[1];
    audioSourceLow = audioSources[2];
    collider = GetComponent<CapsuleCollider2D>();
    spriteRenderer = GetComponent<SpriteRenderer>();
  }

  void Start()
  {
    Respawn();

    // Update all AIs with a slight offset
    InvokeRepeating("UpdateAI", .1f * (float)playerIndex, .4f);
    InvokeRepeating("Poop", .1f * (float)playerIndex, .5f);
    Invoke("Quak", 0f);
  }

  void FixedUpdate()
  {
    if (isDying)
      return;

    // Update rotation
    if (CanRotate && targetDirection != null)
      Utils.RotateTowards(body, targetDirection.Value, turnSpeed, Time.deltaTime);
  }

  void Update()
  {
    if (isDying)
      return;

    // Recover
    if (isStunned)
      isStunned = body.velocity.magnitude > 1f
        || Mathf.Abs(body.angularVelocity) > 120f;

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
      health += Time.deltaTime * 2f;

    health = Mathf.Min(maxHealth, health);

    // Die
    if (health <= .0f && !isDying)
    {
      isDying = true;
      Utils.PlayRandomClip(audioSourceHigh, audioDeath, minPitch, maxPitch);
      StartCoroutine("Die");
    }
  }

  void LateUpdate()
  {
    if (isDying)
      return;

    UpdateDrag();

    // Show "hops" by increasing sprite size
    float scale = Mathf.Min(2.56f + body.velocity.magnitude / 20f, 3.5f);
    spriteRenderer.size = new Vector2(scale, scale);

    // Desaturate frog to show hitpoints
    spriteRenderer.material.SetVector("_HSLAAdjust",
      new Vector4(0f, 1.5f * (health / maxHealth) - 1f, -.1f, 0f));
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
      Utils.PlayRandomClip(audioSourceLow, audioJump, minPitch, maxPitch);
    }
  }

  void ShootTongue()
  {
    if (CanUseAbilities)
    {
      float delay = mutations.Contains(Mutation.SuperTongue) ? 0.4f : 0f;
      activeTongue = Instantiate(tongue);
      activeTongue.Eject(this, body.mass, tongueDrag,
        tongueEjectForce, tongueReturnForce, delay);
      Utils.PlayRandomClip(audioSourceHigh, audioSlurp, minPitch, maxPitch);
    }
  }

  void Respawn()
  {
    health = maxHealth;
    isOutsidePond = false;
    isDying = false;

    body.velocity = new Vector2();
    body.angularVelocity = 0f;
    body.rotation = Random.Range(0f, 360f);
    body.position = pond.GetRandomPoint();

    mutations.Clear();
    UpdateMutations();
  }

  void Quak()
  {
    Utils.PlayRandomClip(audioSourceMedium, audioQuak, minPitch, maxPitch);
    Invoke("Quak", Random.Range(3f, 5f));
  }

  void Poop()
  {
    if (isDying)
      return;

    if (mutations.Contains(Mutation.Bomberman))
    {
      var giant = mutations.Contains(Mutation.Giant);
      var numBombs = giant ? 2 : Random.Range(2, 6);
      var scale = giant ? .5f : .2f;
      var offset = body.GetRelativeVector(Vector2.down)
        * collider.size.y * transform.localScale.y * 1.2f;

      // Poop bombs
      for (int i = 0; i < numBombs; i++)
      {
        var bombInstance = Instantiate(bomb);
        bombInstance.transform.position = body.position + offset
          + Random.insideUnitCircle * .1f;
        bombInstance.scale = scale;
      }
    }
  }

  IEnumerator Die()
  {
    body.Sleep();
    collider.enabled = false;

    Color c = spriteRenderer.material.color;

    for (float f = 0f; f <= 1.0f; f += 0.01f)
    {
      // Grow
      float scale = 2.56f + (f * 5.12f);
      spriteRenderer.size = new Vector2(scale, scale);

      // Turn to white and fade
      var v = spriteRenderer.material.GetVector("_HSLAAdjust");
      v.z = f / 2f;
      v.w = -f;
      spriteRenderer.material.SetVector("_HSLAAdjust", v);

      yield return new WaitForSeconds(.02f);
    }

    // Restore and respawn
    c.a = 1.0f;
    spriteRenderer.material.color = c;
    body.WakeUp();
    collider.enabled = true;
    Respawn();
  }

  void OnExplosion(float damage)
  {
    health -= damage;
    isStunned = true;
    Utils.PlayRandomClip(audioSourceHigh, audioHit);
  }

  void OnCollisionEnter2D(Collision2D collision)
  {
    if (isDying)
      return;

    // Become stunned on collision
    isStunned = true;

    // Lose a bit of health when colliding with walls
    if (collision.gameObject.tag == "Wall")
      health -= 10f;
    if (collision.gameObject.tag == "Tongue")
      health -= 5f;
    if (collision.gameObject.tag == "Frog"
      && mutations.Contains(Mutation.Bomberman))
      collision.gameObject.GetComponent<Frog>().Mutate(Mutation.Bomberman);

    Utils.PlayRandomClip(audioSourceHigh, audioHit);
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
