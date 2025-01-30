using UnityEngine;
using UnityEngine.UI;

public class BlurUIController : MonoBehaviour
{
    public GameObject blurPanel; // Assign BlurBackground dari Inspector

    void Start()
    {
        blurPanel.SetActive(false); // Mulai dengan blur mati
    }

    public void ShowBlur(bool show)
    {
        blurPanel.SetActive(show);
    }
}
