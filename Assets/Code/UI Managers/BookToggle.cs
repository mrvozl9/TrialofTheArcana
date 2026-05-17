using UnityEngine;

public class BookToggle : MonoBehaviour
{
    [Header("References")]
    public GameObject bookObject;
    public UIBook uiBook;

    [Header("Key")]
    public KeyCode openBookKey = KeyCode.Tab;

    private bool isBookOpen;

    void Start()
    {
        isBookOpen = false;
        bookObject.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(openBookKey))
            ToggleBook();
    }

    void ToggleBook()
    {
        isBookOpen = !isBookOpen;
        bookObject.SetActive(isBookOpen);

        if (isBookOpen)
            uiBook.ResetBook();
    }

    public void CloseBook()
    {
        isBookOpen = false;
        bookObject.SetActive(false);
    }
}
