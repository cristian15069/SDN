using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BodegaFinal : MonoBehaviour
{
    [Header("Configuración de Escena")]
    public string finalMenuSceneName = "FinalMenuScene";
    public float delayBeforeLoading = 4.0f;

    [Header("Referencias al Jugador")]
    public kaiAnimation playerScript;
    public Rigidbody2D playerRb;
    public Animator playerAnim;
    public GameObject playerUI;

    [Header("Animación de la Puerta")]
    public Animator doorAnimator;
    public string openTriggerName = "OpenDoor";

    [Header("UI de Interacción")]
    public GameObject pcIndicator;
    public GameObject mobileButton;

    [Header("Celebración")]
    public GameObject victoryPanel;
    public AudioSource audioSource;
    public AudioClip victorySound;
    public ParticleSystem confettiParticles;

    private bool isPlayerNear = false;
    private bool levelFinished = false;

    void Start()
    {
        if (mobileButton != null)
        {
            Button btn = mobileButton.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(ActivateFinalSequence);
            }
            mobileButton.SetActive(false);
        }

        if (pcIndicator != null) pcIndicator.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !levelFinished)
        {
            isPlayerNear = true;
            if (Application.isMobilePlatform)
            {
                if (mobileButton != null) mobileButton.SetActive(true);
            }
            else
            {
                if (pcIndicator != null) pcIndicator.SetActive(true);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            if (pcIndicator != null) pcIndicator.SetActive(false);
            if (mobileButton != null) mobileButton.SetActive(false);
        }
    }

    void Update()
    {
        if (isPlayerNear && !levelFinished && Input.GetKeyDown(KeyCode.V))
        {
            ActivateFinalSequence();
        }
    }

    public void ActivateFinalSequence()
    {
        if (levelFinished) return;
        levelFinished = true;

        if (pcIndicator != null) pcIndicator.SetActive(false);
        if (mobileButton != null) mobileButton.SetActive(false);

        if (playerScript != null) playerScript.enabled = false;

        if (playerRb != null)
        {
#if UNITY_6000_0_OR_NEWER
            playerRb.linearVelocity = Vector2.zero;
#else
            playerRb.velocity = Vector2.zero;
#endif
            playerRb.simulated = false;
        }

        if (playerAnim != null) playerAnim.SetFloat("speed", 0);
        if (playerUI != null) playerUI.SetActive(false);
        if (doorAnimator != null) doorAnimator.SetTrigger(openTriggerName);

        StartCoroutine(ShowVictorySequence());
    }

    System.Collections.IEnumerator ShowVictorySequence()
    {
        yield return new WaitForSeconds(0.5f);

        if (audioSource != null && victorySound != null)
            audioSource.PlayOneShot(victorySound);

 if (confettiParticles != null)
    {
        confettiParticles.Stop(); 
        confettiParticles.Clear(); 
        confettiParticles.Play(); 
    }

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            victoryPanel.transform.localScale = Vector3.zero;

            float timer = 0;
            while (timer < 0.5f)
            {
                timer += Time.deltaTime;
                victoryPanel.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, timer / 0.5f);
                yield return null;
            }
        }

        yield return new WaitForSeconds(delayBeforeLoading);
        SceneManager.LoadScene(finalMenuSceneName);
    }
}
