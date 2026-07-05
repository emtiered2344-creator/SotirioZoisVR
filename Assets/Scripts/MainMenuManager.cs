using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject SelectionMenu;
    [SerializeField] private GameObject TrainingMenu;

    void Start()
    {
        
        ShowMainMenu();
    }

    public void ClearMenus()
    {
        mainMenu.SetActive(false);
        SelectionMenu.SetActive(false);
        TrainingMenu.SetActive(false);
    }

    public void ShowMainMenu()
    {
        ClearMenus();
        mainMenu.SetActive(true);
    }

    public void ShowSelectionMenu()
    {
        ClearMenus();
        SelectionMenu.SetActive(true);
    }
    public void ShowTrainingMenu()
    {
        ClearMenus();
        TrainingMenu.SetActive(true);
    }
}
