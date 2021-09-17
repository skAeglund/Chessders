using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private AudioSource audioSource;

    [SerializeField] private AudioClip[] movingPieceSFX = new AudioClip[2];
    [SerializeField] private AudioClip[] capturePieceSFX = new AudioClip[2];
    [SerializeField] private AudioClip[] checkSFX = new AudioClip[2];
    [SerializeField] private AudioClip gameOverSFX;

    private int checkCounter = 0;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    public void PlayPieceMoveSFX(int currentPlayer)
    {
        audioSource.PlayOneShot(movingPieceSFX[currentPlayer]);
    }

    public void PlayPieceCapturedSFX(int currentPlayer)
    {
        audioSource.PlayOneShot(capturePieceSFX[currentPlayer]);
    }
    public void PlayCheckSFX()
    {
        audioSource.PlayOneShot(checkSFX[checkCounter % 2 == 0 ? 0 : 1]);
        checkCounter++;
    }
    public void PlayGameOverSFX()
    {
        audioSource.PlayOneShot(gameOverSFX);
    }
}
