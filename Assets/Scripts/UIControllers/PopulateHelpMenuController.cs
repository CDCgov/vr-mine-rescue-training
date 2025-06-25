using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopulateHelpMenuController : MonoBehaviour
{
    [SerializeField]
    public HelpAsset[] HelpFields;
    [SerializeField]
    public  GridLayoutGroup GridLayoutReference;

    [SerializeField]
    public GameObject HelpItemPrefab;
    // Start is called before the first frame update
    void Start()
    {
        if(GridLayoutReference == null)
        {
            GridLayoutReference = GetComponentInChildren<GridLayoutGroup>();
        }

        for(int i = 0; i < HelpFields.Length; i++)
        {
            GameObject newItem = GameObject.Instantiate(HelpItemPrefab, GridLayoutReference.transform);
            HelpItemController helpItemController = newItem.GetComponent<HelpItemController>();
            helpItemController.SetHelpItem(HelpFields[i].HelpSprite, HelpFields[i].HelpString);
        }
    }

}
