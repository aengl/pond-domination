using UnityEngine;

public class BombermanEffect : MonoBehaviour
{
  Frog frog;
  new ParticleSystem particleSystem;

  void Awake()
  {
    frog = GetComponentInParent<Frog>();
    particleSystem = GetComponent<ParticleSystem>();
  }

  void Start()
  {
    var main = particleSystem.main;
    main.startColor = frog.color;
  }

  void Update()
  {
    var enabled = frog.mutations.Contains(Mutation.Bomberman);
    if (enabled && particleSystem.isPaused)
      particleSystem.Play();
    if (!enabled && particleSystem.isPlaying)
    {
      particleSystem.Clear();
      particleSystem.Pause();
    }

    var scale = frog.mutations.Contains(Mutation.Giant) ? 3f : 1f;
    transform.localScale = new Vector3(scale, scale, scale);
  }
}
