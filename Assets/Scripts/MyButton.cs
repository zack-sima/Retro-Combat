using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MyButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler {
    public Text dimText, dimText2;
    public Image dimImage;
    public void OnPointerClick(PointerEventData eventData) {
    }
    public void OnPointerDown(PointerEventData eventData) {
        if (GetComponent<Button>().interactable) {
            if (dimText)
                dimText.color = new Color(dimText.color.r, dimText.color.g, dimText.color.b, 0.75f);
            if (dimText2)
                dimText2.color = new Color(dimText2.color.r, dimText2.color.g, dimText2.color.b, 0.75f);
            if (dimImage)
                dimImage.color = new Color(dimImage.color.r, dimImage.color.g, dimImage.color.b, 0.75f);
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        
    }
    public void OnPointerExit(PointerEventData eventData) {
        
    }

    public void OnPointerUp(PointerEventData eventData) {
        if (GetComponent<Button>().interactable) {
            if (dimText)
                dimText.color = new Color(dimText.color.r, dimText.color.g, dimText.color.b, 1.2f + 0.0f);
            if (dimText2)
                dimText2.color = new Color(dimText2.color.r, dimText2.color.g, dimText2.color.b, 1.2f + 0.0f);
            if (dimImage)
                dimImage.color = new Color(dimImage.color.r, dimImage.color.g, dimImage.color.b, 1.2f + 0.0f);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!GetComponent<Button>().interactable) {
            if (dimText)
                dimText.color = new Color(dimText.color.r, dimText.color.g, dimText.color.b, 0.75f);
            if (dimText2)
                dimText2.color = new Color(dimText2.color.r, dimText2.color.g, dimText2.color.b, 0.75f);
            if (dimImage)
                dimImage.color = new Color(dimImage.color.r, dimImage.color.g, dimImage.color.b, 0.75f);
        } else if (dimText.color == new Color(dimText.color.r, dimText.color.g, dimText.color.b, 0.75f)) {
            if (dimText)
                dimText.color = new Color(dimText.color.r, dimText.color.g, dimText.color.b, 1.2f + 0.0f);
            if (dimText2)
                dimText2.color = new Color(dimText2.color.r, dimText2.color.g, dimText2.color.b, 1.2f + 0.0f);
            if (dimImage)
                dimImage.color = new Color(dimImage.color.r, dimImage.color.g, dimImage.color.b, 1.2f + 0.0f);
        }
    }
}
