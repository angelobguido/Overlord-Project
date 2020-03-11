using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyUI : MonoBehaviour
{
    private List<Image> keyList;
    // Start is called before the first frame update

    [SerializeField] 
    protected Sprite keySprite;
    List<int> playerKeys;

    private void Awake()
    {
        playerKeys = new List<int>();
    }
    void Start()
    {
        //CreateHeartImage();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void CreateKeyImage()
    {
        int row = 0;
        int col = 0;
        int colMax = 10;


        keyList = new List<Image>();
        
        float rowColSize = keySprite.rect.size.x*1.1f;

        foreach(int key in Player.instance.GetComponent<Player>().keys)
        {
            if (!playerKeys.Contains(key))
                playerKeys.Add(key);
        }

        int currentKeys = playerKeys.Count;

        for (int i = 0; i < currentKeys; i++)
        {

            Vector2 keyAnchoredPosition = new Vector2(col * rowColSize, -row * rowColSize);

            GameObject keyGameObject = new GameObject("Key", typeof(Image));

            // Set as child of this transform
            keyGameObject.transform.parent = transform;
            keyGameObject.transform.localPosition = Vector3.zero;
            keyGameObject.transform.localScale = new Vector3(2, 2, 1);

            // Locate and Size heart
            keyGameObject.GetComponent<RectTransform>().anchoredPosition = keyAnchoredPosition;
            keyGameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(35, 35);

            // Set heart sprite
            Image keyImageUI = keyGameObject.GetComponent<Image>();
            keyImageUI.sprite = keySprite;
            keyImageUI.color = Util.colorId[playerKeys[i] - 1];


            keyList.Add(keyImageUI);

            col++;
            if (col >= colMax)
            {
                row++;
                col = 0;
            }
        }

    }
    public void ResetKeyGUI()
    {
        playerKeys.Clear();
    }
}
