using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class CenteredGridLayout : MonoBehaviour
{
    [Tooltip("Jumlah kolom maksimal.")]
    public int columns = 3;

    [Tooltip("Ukuran masing-masing cell/tombol.")]
    public Vector2 cellSize = new Vector2(100f, 100f);

    [Tooltip("Jarak antar cell secara horizontal dan vertikal.")]
    public Vector2 spacing = new Vector2(10f, 10f);

    private void Update()
    {
        ArrangeChildren();
    }

    private void OnTransformChildrenChanged()
    {
        ArrangeChildren();
    }

    /// <summary>
    /// Mengatur posisi RectTransform anak-anak agar tersusun rapi di tengah secara baris-per-baris.
    /// </summary>
    public void ArrangeChildren()
    {
        // Kumpulkan semua child yang aktif
        List<RectTransform> activeChildren = new List<RectTransform>();
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.gameObject.activeSelf)
            {
                RectTransform rt = child.GetComponent<RectTransform>();
                if (rt != null)
                {
                    activeChildren.Add(rt);
                }
            }
        }

        int childCount = activeChildren.Count;
        if (childCount == 0) return;

        // Hitung total baris yang dibutuhkan
        int rows = Mathf.CeilToInt((float)childCount / columns);

        // Dapatkan RectTransform parent
        RectTransform parentRect = GetComponent<RectTransform>();
        if (parentRect == null) return;

        // Hitung tinggi total seluruh layout untuk centering vertikal
        float totalHeight = rows * cellSize.y + (rows - 1) * spacing.y;

        int currentChildIndex = 0;

        for (int r = 0; r < rows; r++)
        {
            // Tentukan jumlah element di baris ini
            int elementsInRow = Mathf.Min(columns, childCount - currentChildIndex);
            if (elementsInRow <= 0) break;

            // Hitung lebar total untuk baris spesifik ini
            float rowWidth = elementsInRow * cellSize.x + (elementsInRow - 1) * spacing.x;

            // Tentukan koordinat X mulai (horizontal center)
            float startX = -rowWidth / 2f + cellSize.x / 2f;

            // Tentukan koordinat Y (vertical center)
            float yPos = (totalHeight / 2f) - (r * (cellSize.y + spacing.y)) - (cellSize.y / 2f);

            for (int c = 0; c < elementsInRow; c++)
            {
                if (currentChildIndex >= activeChildren.Count) break;

                RectTransform childRect = activeChildren[currentChildIndex];

                // Set jangkar ke tengah agar localPosition konsisten
                childRect.anchorMin = new Vector2(0.5f, 0.5f);
                childRect.anchorMax = new Vector2(0.5f, 0.5f);
                childRect.pivot = new Vector2(0.5f, 0.5f);
                childRect.sizeDelta = cellSize;

                float xPos = startX + c * (cellSize.x + spacing.x);
                childRect.localPosition = new Vector3(xPos, yPos, 0f);

                currentChildIndex++;
            }
        }
    }
}
