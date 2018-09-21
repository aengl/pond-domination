using UnityEngine;

public class Frog : MonoBehaviour
{
  public int playerIndex = 1;
  public float maxHealth = 100f;
  public float turnSpeed = 5f;
  public float jumpForce = 20f;
  public Tongue tongue;

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
    get { return !IsStunned && body.velocity.magnitude < .1f && activeTongue == null; }
  }

  public bool CanUseAbilities
  {
    get { return CanJump; }
  }

  public float Health
  {
    get { return health; }
  }

  Rigidbody2D body;
  SpriteRenderer spriteRenderer;
  float targetRotation = 0f;

  [SerializeField]
  Tongue activeTongue = null;

  [SerializeField]
  float health;

  [SerializeField]
  bool isStunned = false;

  [SerializeField]
  bool isOutsidePond = false;

  void Awake()
  {
    body = GetComponent<Rigidbody2D>();
    spriteRenderer = GetComponent<SpriteRenderer>();
  }

  void Start()
  {
    health = maxHealth;

    // Update all AIs with a slight offset
    InvokeRepeating("UpdateAI", .1f * (float)playerIndex, .4f);
  }

  void Update()
  {
    // Recover
    if (isStunned)
      isStunned = body.velocity.magnitude > 1f || Mathf.Abs(body.angularVelocity) > 120f;

    // Rotate
    float? rotation = Controls.GetRotation(playerIndex);
    if (rotation != null)
      targetRotation = rotation.Value;

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
      health = maxHealth;
      var position = Random.insideUnitCircle * 3f;
      body.velocity = new Vector2();
      body.angularVelocity = 0f;
      body.rotation = Random.Range(0f, 360f);
      body.position = new Vector3(position.x, position.y, 0f);
    }
  }

  void LateUpdate()
  {
    UpdateDrag();

    // Update rotation
    Utils.RotateTowards(body, targetRotation, turnSpeed);

    // Update scale
    float scale = 2.56f + body.velocity.magnitude / 20f;
    spriteRenderer.size = new Vector2(scale, scale);
  }

  void UpdateDrag()
  {
    float drag = 12f;
    float angularDrag = 2f;

    if (isStunned)
    {
      drag = 3f;
      angularDrag = .5f;
    }
    else if (isOutsidePond)
    {
      drag = 30f;
      angularDrag = 12f;
    }

    body.drag = drag;
    body.angularDrag = angularDrag;
  }

  void UpdateAI()
  {
    // AI takes over if the player hasn't been playing for a while
    if (Controls.TimeSinceInput(playerIndex) > 5f)
    {
      if (Random.value > .5f)
        Jump();
      else if (Random.value > .85f)
        ShootTongue();
      else if (activeTongue == null)
        targetRotation = Random.Range(0, 360f);
    }
  }

  void Jump()
  {
    if (CanJump)
      body.AddRelativeForce(Vector2.up * jumpForce * ForceMultiplier, ForceMode2D.Impulse);
  }

  void ShootTongue()
  {
    if (CanUseAbilities)
    {
      activeTongue = Instantiate(tongue);
      activeTongue.Eject(this);
    }
  }

  void OnCollisionEnter2D(Collision2D collision)
  {
    // Become stunned on collision
    isStunned = true;
  }

  void OnTriggerEnter2D(Collider2D collision)
  {
    isOutsidePond &= collision.name != "PondTrigger";
  }

  void OnTriggerExit2D(Collider2D collision)
  {
    isOutsidePond |= collision.name == "PondTrigger";
  }
}
