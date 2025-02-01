using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using Firebase.Firestore;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ProfileManager : MonoBehaviour
{
    public Image profileImage; // UI Image di halaman profile
    public Image homeProfileImage; // UI Image di homepage
    public Transform imageGrid; // Parent dari pilihan gambar
    public Sprite[] profileSprites; // Array gambar profil
    private FirebaseAuth auth;
    private FirebaseFirestore db;
    private string userId;
    private string selectedImageName;
    public GameObject profileButtonPrefab; // Prefab tombol gambar

    private void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;

        FirebaseUser user = auth.CurrentUser;
        if (user != null)
        {
            userId = user.UserId;
            LoadProfileImage(); // Load gambar yang dipilih sebelumnya
        }
        else
        {
            Debug.LogError("User belum login!");
        }

        LoadProfileChoices();
    }

    private void LoadProfileChoices()
    {
        foreach (Sprite sprite in profileSprites)
        {
            GameObject newButton = Instantiate(profileButtonPrefab, imageGrid);
            Image img = newButton.GetComponent<Image>();
            img.sprite = sprite;

            newButton.GetComponent<Button>().onClick.AddListener(() => SelectProfileImage(sprite.name, sprite));
        }
    }

    private void SelectProfileImage(string imageName, Sprite sprite)
    {
        selectedImageName = imageName;
        profileImage.sprite = sprite; // Ganti gambar di halaman profil
        homeProfileImage.sprite = sprite; // Ganti gambar di homepage
        SaveProfileImage();
    }

    private async void SaveProfileImage()
    {
        if (string.IsNullOrEmpty(userId)) return;

        DocumentReference docRef = db.Collection("users").Document(userId);
        Dictionary<string, object> updates = new Dictionary<string, object>
        {
            { "profileImage", selectedImageName }
        };

        await docRef.SetAsync(updates, SetOptions.MergeAll);
        Debug.Log("Foto profil berhasil disimpan: " + selectedImageName);
    }

    private async void LoadProfileImage()
    {
        if (string.IsNullOrEmpty(userId)) return;

        DocumentReference docRef = db.Collection("users").Document(userId);
        DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

        if (snapshot.Exists && snapshot.ContainsField("profileImage"))
        {
            selectedImageName = snapshot.GetValue<string>("profileImage");
            Sprite loadedSprite = System.Array.Find(profileSprites, s => s.name == selectedImageName);
            if (loadedSprite != null)
            {
                profileImage.sprite = loadedSprite;
                homeProfileImage.sprite = loadedSprite; // Set juga untuk homepage
            }
        }
        else
        {
            Debug.LogWarning("Foto profil belum dipilih.");
        }
    }
}
