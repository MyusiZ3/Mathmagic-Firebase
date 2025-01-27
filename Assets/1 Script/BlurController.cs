using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class BlurController : MonoBehaviour
{
    public Material blurMaterial; // Assign the material with BlurShader here
    public float blurAmount = 1.0f; // Blur intensity, default is 1
    public float alphaValue = 1.0f; // Transparency value, default is fully opaque

    private Image image;

    void Start()
    {
        image = GetComponent<Image>();
        if (blurMaterial != null)
        {
            image.material = blurMaterial;
        }
    }

    void Update()
    {
        if (image.material != null)
        {
            image.material.SetFloat("_BlurSize", blurAmount);
            image.material.SetFloat("_Alpha", alphaValue);
        }
    }

    public void SetBlur(float amount)
    {
        blurAmount = amount;
    }

    public void SetAlpha(float alpha)
    {
        alphaValue = alpha;
    }
}
