using UnityEngine;
using System.Collections.Generic;

public class PhotoGallery : MonoBehaviour
{
    public static PhotoGallery Instance;

    // This is our list of saved photos.
    public List<Texture2D> takenPhotos = new List<Texture2D>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // keeps photos between scenes if we wanted to make more scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // A public function other scripts can call to add a photo.
    public void AddPhoto(Texture2D photo)
    {
        takenPhotos.Add(photo);
        Debug.Log("Photo added! Gallery now has: " + takenPhotos.Count + " photos.");
    }
}
