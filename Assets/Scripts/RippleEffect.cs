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
    var emission = particleSystem.emission;
    emission.enabled = !frog.IsOutsidePond;
  }
}
