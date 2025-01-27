using UnityEngine;
using UnityEngine.UI;

public class BonusLevelManager : MonoBehaviour
{
    public LevelManager levelManager; // Pastikan ini ada

    public void CompleteBonusLevel()
    {
        levelManager.CompleteLevel(3); // Misal level 3 untuk bonus
    }
}
