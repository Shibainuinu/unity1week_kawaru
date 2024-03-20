using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorPlate : MonoBehaviour
{

    [SerializeField] private Image bgImage;
    [SerializeField] private Image colorImage;




    private void Awake()
    {
        bgImage.gameObject.SetActive(false);
    }

    public Color GetColor()
    {
        return colorImage.color;
    }

    public void Select(bool isSelect)
    {
        bgImage.gameObject.SetActive(isSelect);
    }

}
