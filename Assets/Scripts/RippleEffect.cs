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
    if (enabled && particleSystem.isPaused)
      particleSystem.Play();
    if (!enabled && particleSystem.isPlaying)
    {
      particleSystem.Clear();
      particleSystem.Pause();
    }
  }
}
