using UnityEngine;

public class RippleEffect : MonoBehaviour
{
  Frog frog;
  new ParticleSystem particleSystem;

  void Awake()
  {
    frog = GetComponentInParent<Frog>();
    particleSystem = GetComponent<ParticleSystem>();

  }

  void Update()
  {
    var enabled = !frog.IsOutsidePond;

    var emission = particleSystem.emission;
    emission.rateOverTime = frog.Body.velocity.magnitude > .5f ? 20 : 1;

    if (enabled && particleSystem.isPaused)
      particleSystem.Play();
    if (!enabled && particleSystem.isPlaying)
      particleSystem.Pause();
  }
}
