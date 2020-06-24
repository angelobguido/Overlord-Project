using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TreasureUI : MonoBehaviour
{
    protected TextMeshProUGUI treasureText;
    // Start is called before the first frame update

    [SerializeField] 
    protected Sprite treasureSprite;
    protected int treasureAmount;

    private void Awake()
    {
        treasureText = GetComponent<TextMeshProUGUI>();
        treasureAmount = 0;
    }

    protected void OnEnable()
    {
        TreasureController.collectTreasureEvent += IncrementTreasure;
    }

    public void IncrementTreasure(int amount)
    {
        treasureAmount += amount;
        treasureText.text = "x "+treasureAmount.ToString();
    }

}
