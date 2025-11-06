using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GalleryViewer : MonoBehaviour
{
    [Header("UI References")]
    public RawImage photoDisplay;
    public Button nextButton;
    public Button prevButton;
    public TextMeshProUGUI photoCounterText;

    private PhotoGallery gallery;
    private int currentPhotoIndex = 0;

    void Start()
    {
        // Finds the PhotoGallery instance
        gallery = PhotoGallery.Instance;

        nextButton.onClick.AddListener(NextPhoto);
        prevButton.onClick.AddListener(PreviousPhoto);

        // Hides the gallery at the start
        gameObject.SetActive(false);
    }

    // This is the main function the Player will call
    public void ToggleGallery(bool show)
    {
        gameObject.SetActive(show);

        // If we are opening the gallery, show the first photo
        if (show)
        {
            if (gallery != null && gallery.takenPhotos.Count > 0)
            {
                currentPhotoIndex = 0;
                ShowCurrentPhoto();
            }
            else
            {
                // Handles case where there are no photos
                photoDisplay.texture = null;
                if (photoCounterText != null)
                    photoCounterText.text = "0 / 0";

                nextButton.interactable = false;
                prevButton.interactable = false;
            }
        }
    }

    private void ShowCurrentPhoto()
    {
        // Set the Raw Image's texture to our photo
        photoDisplay.texture = gallery.takenPhotos[currentPhotoIndex];

        // Update the buttons to see if they should be clickable
        prevButton.interactable = (currentPhotoIndex > 0);
        nextButton.interactable = (currentPhotoIndex < gallery.takenPhotos.Count - 1);

        // Update the counter text
        if (photoCounterText != null)
        {
            photoCounterText.text = (currentPhotoIndex + 1) + " / " + gallery.takenPhotos.Count;
        }
    }

    // Called by the NextButton
    public void NextPhoto()
    {
        if (currentPhotoIndex < gallery.takenPhotos.Count - 1)
        {
            currentPhotoIndex++;
            ShowCurrentPhoto();
        }
    }

    // Called by the PreviousButton
    public void PreviousPhoto()
    {
        if (currentPhotoIndex > 0)
        {
            currentPhotoIndex--;
            ShowCurrentPhoto();
        }
    }
}
