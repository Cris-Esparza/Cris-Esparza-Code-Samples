using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class LevelSelect : MonoBehaviour
{
    [SerializeField] GameObject levelSelectMenu;

    [SerializeField] GameObject mainMenu;

    [SerializeField] private Transform buttonParent;
    [SerializeField] private Sprite[] medalIcons;

    [SerializeField] private RectTransform scrollRect;
    [SerializeField] private RectTransform content;

    [SerializeField] private GameObject lockMessage;

    private float targetY;
    private Vector3 startingPosition;
    private Vector3 tempPosition;

    private GameObject previouslySelected;

    [SerializeField] private TextMeshProUGUI levelLockMessage;

    bool flipflop;
    float t;
    int currentlySelected;
    private void Start()
    {
        flipflop = false;

        if (gameObject.name == "LevelSelect")
            LevelSelectInit();
    }

    private void OnEnable()
    {
        if (gameObject.name == "LevelSelect")
            LevelSelectInit();
    }

    /// <summary>
    /// Loads a level by taking in a string, level name
    /// </summary>
    /// <param name="name"></param>
    public void LoadLevel(string name)
    {
        
        if(MonoBehaviourSingletonPersistent<DynamicInstantiation>.Instance.levelOrder.Contains(name))
        {
            int location = MonoBehaviourSingletonPersistent<DynamicInstantiation>.Instance.levelOrder.IndexOf(name);

            Debug.Log(PlayerPrefs.GetInt("FurthestLevel"));
            Debug.Log(location - 1);

            //Debug.Log(location+1);
            if (LevelUnlocked(location + 1))
            {
                if(PlayerPrefs.GetInt("FurthestComic") == 25)
                {
                    SceneManager.LoadScene(name);
                }
                else
                {
                    if (PlayerPrefs.GetInt("FurthestLevel") >= location-1)
                    {
                        SceneManager.LoadScene(name);
                    }
                    else
                    {
                        lockMessage.SetActive(true);
                        levelLockMessage.text = "Complete the level first!";
                    }
                }
            }
            else
                DisplayLockMessage(location+1);
        }
        else
        {
            Debug.Log(name + " was not found");
        }
    }

    public void LoadComic(int read)
    {
        if(read <= PlayerPrefs.GetInt("FurthestComic"))
        {
            MonoBehaviourSingletonPersistent<DynamicInstantiation>.Instance.comicRead = read;
            MonoBehaviourSingletonPersistent<DynamicInstantiation>.Instance.isComicRead = true;
            SceneManager.LoadScene("Comic Scene 0");
        }
    }

    void LevelSelectInit()
    {
        for(int i = 0; i < buttonParent.childCount; i++)
        {
            Transform button = buttonParent.GetChild(i);
            button.GetChild(1).GetComponent<Image>().sprite = medalIcons[PlayerPrefs.GetInt("Medal" + (i+1),0)];
        }
    }

    public static bool LevelUnlocked(int i)
    {
        if (i == 1)
            return true;

        bool ret = true;

        // count gold medals
        int c = 0;
        for (int j = 1; j <= 25; j++)
            c += PlayerPrefs.GetInt("Medal" + j, 0) > 2 ? 1 : 0;
        // Previous level req
        ret &= PlayerPrefs.GetInt("Medal" + (i - 1)) > 0;
        // Medium level req
        if (i > 10)
            ret &= c >= 5;
        // Hard level req
        if (i > 20)
            ret &= c >= 10;
        return ret;
    }

    public void DisplayLockMessage(int i)
    {
        lockMessage.SetActive(true);

        // count gold medals
        int c = 0;
        for (int j = 1; j <= 25; j++)
            c += PlayerPrefs.GetInt("Medal" + j, 0) > 2 ? 1 : 0;

        string msg = "";


        if (i > 20 && c < 10)
            msg = "You need " + (10 - c) + " more gold medals to continue";
        else if (i > 10 && c < 5)
            msg = "You need " + (5 - c) + " more gold medals to continue";
        else if (PlayerPrefs.GetInt("Medal" + (i - 1)) == 0)
            msg = "You need a bronze medal to continue";

        levelLockMessage.text = msg;
    }

    public void HideLockMessage()
    {
        lockMessage.SetActive(false);
    }

    void ScrollWithSelection(RectTransform _scrollRect, RectTransform _content)
    {
        GameObject selected = EventSystem.current.currentSelectedGameObject;
        if (selected == null || selected == previouslySelected) return;
        if (selected.transform.parent != _content.transform) return;
        RectTransform selectedRectTransform = selected.GetComponent<RectTransform>();


        float scrollViewMinY = _content.anchoredPosition.y;
        float scrollViewMaxY = _content.anchoredPosition.y + _scrollRect.rect.height;


        float selectedPositionY = Mathf.Abs(selectedRectTransform.anchoredPosition.y) + (selectedRectTransform.rect.height / 2);

        // If selection below scroll view
        if (selectedPositionY > scrollViewMaxY)
        {
            float newY = selectedPositionY - _scrollRect.rect.height;
            _content.anchoredPosition = new Vector2(_content.anchoredPosition.x, newY);
        }


        // If selection above scroll view
        else if (Mathf.Abs(selectedRectTransform.anchoredPosition.y) < scrollViewMinY)
        {
            _content.anchoredPosition =
                new Vector2(_content.anchoredPosition.x, Mathf.Abs(selectedRectTransform.anchoredPosition.y)
                - (selectedRectTransform.rect.height / 2));
        }
    }
}
