using CarterGames.Assets.AudioManager;
using UnityEngine;

public class PlaySound : MonoBehaviour
{
    public string soundName;
    public float intensity = 1f;

    public void Play()
    {
        AudioManager.instance.Play(soundName, intensity);
    }
}