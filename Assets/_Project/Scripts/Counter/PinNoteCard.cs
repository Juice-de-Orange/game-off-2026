using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameOff.Counter
{
    /// <summary>P33: one paper in the pinboard close-up (PinBoardCloseUp): title, date, text, and wanted photos on the wanted list.</summary>
    public class PinNoteCard : MonoBehaviour
    {
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text date;
        [SerializeField] private TMP_Text body;
        [Tooltip("Row of photo frames, hidden on notes without photos.")]
        [SerializeField] private GameObject photoRow;
        [SerializeField] private RawImage[] photos = new RawImage[0];
        [SerializeField] private TMP_Text[] photoNames = new TMP_Text[0];
        [Tooltip("\"NO PHOTO\" in each frame without one.")]
        [SerializeField] private TMP_Text[] noPhoto = new TMP_Text[0];

        public void Show(PinNote note)
        {
            gameObject.SetActive(note != null);
            if (note == null) return;
            title.text = note.Title;
            date.text = note.Date;
            body.text = note.Body;
            bool anyPhotos = note.Photos.Count > 0 && photos.Length > 0;
            if (photoRow != null) photoRow.SetActive(anyPhotos);
            for (int i = 0; i < photos.Length; i++)
            {
                bool used = i < note.Photos.Count;
                photos[i].transform.parent.gameObject.SetActive(used);
                if (!used) continue;
                var (name, photo) = note.Photos[i];
                photos[i].texture = photo;
                photos[i].enabled = photo != null; // a photo of a traveler from an earlier day may be gone with that day
                if (i < photoNames.Length) photoNames[i].text = name;
                if (i < noPhoto.Length) noPhoto[i].gameObject.SetActive(photo == null);
            }
        }
    }
}
