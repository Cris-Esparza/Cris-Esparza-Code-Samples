using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PearlDropUI : MonoBehaviour
{
    private SpriteRenderer pearlSprite;

    [SerializeField]
    private GameObject UIParent;

    float pearl_alpha;

    // Start is called before the first frame update
    void Start()
    {
        pearlSprite = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        pearl_alpha = 1;
    }

    // Update is called once per frame
    void Update()
    {

        pearlSprite.color = new Color(1, .25f, .25f, pearl_alpha);
        pearl_alpha -= 0.01f;

        if (pearl_alpha <= 0)
        {
            gameObject.transform.position = UIParent.transform.position;
            gameObject.SetActive(false);
        }
    }
}
