using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneFader : MonoBehaviour
{
    [SerializeField] public float fadeTime = 1f;

    private Image fadeOutUIImage;

    public enum FadeDirection
    {
        In,
        Out
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        fadeOutUIImage = GetComponent<Image>();

        // Verifica che il component sia stato trovato
        if (fadeOutUIImage == null)
        {
            Debug.LogError("Image component non trovato su SceneFader!");
        }
    }

    public IEnumerator Fade(FadeDirection fadeDirection)
    {
        // Verifica che fadeOutUIImage sia assegnato
        if (fadeOutUIImage == null)
        {
            fadeOutUIImage = GetComponent<Image>();
            if (fadeOutUIImage == null)
            {
                Debug.LogError("fadeOutUIImage è ancora null in Fade()!");
                yield break; // Esce dal coroutine se l'immagine è ancora null
            }
        }

        float alpha = fadeDirection == FadeDirection.Out ? 1 : 0;
        float fadeEndValue = fadeDirection == FadeDirection.Out ? 0 : 1;

        if (fadeDirection == FadeDirection.Out)
        {
            while (alpha >= fadeEndValue)
            {
                SetColorImage(ref alpha, fadeDirection);
                yield return null;
            }

            fadeOutUIImage.enabled = false;
        }
        else
        {
            fadeOutUIImage.enabled = true;

            while (alpha <= fadeEndValue)
            {
                SetColorImage(ref alpha, fadeDirection);
                yield return null;
            }
        }
    }

    public IEnumerator FadeAndLoadScene(FadeDirection fadeDirection, string sceneToLoad)
    {
        // Verifica che fadeOutUIImage sia assegnato
        if (fadeOutUIImage == null)
        {
            fadeOutUIImage = GetComponent<Image>();
            if (fadeOutUIImage == null)
            {
                Debug.LogError("fadeOutUIImage è null in FadeAndLoadScene!");
                yield break;
            }
        }

        fadeOutUIImage.enabled = true;
        yield return Fade(fadeDirection);
        SceneManager.LoadScene(sceneToLoad);
    }

    void SetColorImage(ref float alpha, FadeDirection fadeDirection)
    {
        // Protezione contro null reference
        if (fadeOutUIImage == null)
        {
            Debug.LogError("fadeOutUIImage è null in SetColorImage!");
            return;
        }

        fadeOutUIImage.color = new Color(fadeOutUIImage.color.r, fadeOutUIImage.color.g, fadeOutUIImage.color.b, alpha);
        alpha += Time.deltaTime * (1 / fadeTime) * (fadeDirection == FadeDirection.Out ? -1 : 1);
    }
}
