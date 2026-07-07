using UnityEngine;
using UnityEngine.UI;

public class AnswerShuffler : MonoBehaviour
{
    private void Awake()
    {
        Shuffle();
    }

    private void OnEnable()
    {
        Shuffle();
    }

    [ContextMenu("Shuffle Answers")]
    public void Shuffle()
    {
        int childCount = transform.childCount;
        if (childCount <= 1) return;

        // Simpan referensi ke semua child Transform
        Transform[] children = new Transform[childCount];
        for (int i = 0; i < childCount; i++)
        {
            children[i] = transform.GetChild(i);
        }

        // Lakukan pengacakan dengan algoritma Fisher-Yates
        for (int i = 0; i < childCount; i++)
        {
            Transform temp = children[i];
            int randomIndex = Random.Range(i, childCount);
            children[i] = children[randomIndex];
            children[randomIndex] = temp;
        }

        // Set Sibling Index secara berurutan berdasarkan array yang telah diacak
        for (int i = 0; i < childCount; i++)
        {
            children[i].SetSiblingIndex(i);
        }

        // Paksa Grid Layout Group / Horizontal Layout Group untuk memperbarui posisinya seketika
        LayoutGroup layoutGroup = GetComponent<LayoutGroup>();
        if (layoutGroup != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
        }
    }
}
