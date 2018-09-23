using UnityEngine;

public static class Utils
{
  public static bool VectorHasLength(Vector2 v)
  {
    return Mathf.Abs(v.x) > float.Epsilon || Mathf.Abs(v.y) > float.Epsilon;
  }

  public static void RotateTowards(Rigidbody2D body,
    Vector2 direction, float speed, float timeDelta)
  {
    Vector2 up = new Vector2(body.transform.up.x, body.transform.up.y);
    Debug.DrawLine(body.position, body.position + up, Color.yellow);
    Debug.DrawLine(body.position, body.position + direction, Color.red);
    float angle = Vector2.SignedAngle(up, direction);
    if (Mathf.Abs(angle) > float.Epsilon)
    {
      float newRotation = body.rotation + speed * angle * timeDelta;
      body.MoveRotation(newRotation);
    }
  }

  public static void MoveForward(Rigidbody2D body, float speed, float timeDelta)
  {
    body.AddRelativeForce(Vector2.up * speed * timeDelta, ForceMode2D.Force);
  }

  public static AudioClip GetRandomClip(AudioClip[] clips)
  {
    int i = Random.Range(0, clips.Length);
    return clips[i];
  }

  public static void PlayRandomClip(AudioSource audioSource, AudioClip[] clips,
    float minPitch = .5f, float maxPitch = 1.5f, float volume = 1.0f)
  {
    audioSource.volume = volume;
    audioSource.pitch = Random.Range(minPitch, maxPitch);
    audioSource.PlayOneShot(GetRandomClip(clips));
  }
}
