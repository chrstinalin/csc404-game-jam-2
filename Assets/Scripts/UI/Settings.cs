using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class OptionsMenu : MonoBehaviour
{
    [Header("Navigation")]
    public Button backButton;
    
    private GameObject lastSelected;

    void Start()
    {
        // Select back button on start
        if (backButton != null)
        {
            EventSystem.current.SetSelectedGameObject(backButton.gameObject);
            lastSelected = backButton.gameObject;
        }
    }

    void Update()
    {
        PreventMouseDeselection();
        
        // Allow pressing Cancel button to go back
        if (Input.GetButtonDown("Cancel"))
        {
            backButton.onClick.Invoke();
        }
        
        // Allow pressing Submit on the selected back button
        if (Input.GetButtonDown("Submit"))
        {
            backButton.onClick.Invoke();
        }
    }

    void PreventMouseDeselection()
    {
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            EventSystem.current.SetSelectedGameObject(lastSelected);
        }
        else
        {
            lastSelected = EventSystem.current.currentSelectedGameObject;
        }
    }
}