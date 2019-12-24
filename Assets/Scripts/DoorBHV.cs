using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorBHV : MonoBehaviour
{

    //public GameManager gm;
    public int keyID;
    public bool isOpen;
    public Sprite lockedSprite;
    public Sprite openedSprite;
    public Transform teleportTransform;
    //	public int moveX;
    //	public int moveY;
    [SerializeField]
    private DoorBHV destination;
    private RoomBHV parentRoom;
    [SerializeField]
    private AudioClip unlockSnd;

    private AudioSource audioSrc;

    private void Awake()
    {
        parentRoom = transform.parent.GetComponent<RoomBHV>();
        audioSrc = GetComponent<AudioSource>();
    }

    // Use this for initialization
    void Start()
    {
        if (keyID < 0)
        {
            Destroy(gameObject);
        }
        else if (keyID > 0)
        {
            //Render the locked door sprite with the color relative to its ID
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            sr.sprite = lockedSprite;
            sr.color = Util.colorId[keyID - 1];
            //text.text = keyID.ToString ();
        }
        //gm = GameManager.instance;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            if (keyID == 0 || isOpen)
            {
                audioSrc.PlayOneShot(audioSrc.clip, 0.8f);
                MovePlayerToNextRoom();
                GameManager.instance.UpdateRoomGUI(destination.parentRoom.x, destination.parentRoom.y);
            }
            else if (Player.instance.keys.Contains(keyID))
            {
                audioSrc.PlayOneShot(unlockSnd, 0.7f);
                Player.instance.keys.Remove(keyID);
                Player.instance.usedKeys.Add(keyID);
                GameManager.instance.UpdateKeyGUI();
                GameManager.instance.UpdateRoomGUI(destination.parentRoom.x, destination.parentRoom.y);
                OpenDoor();
                destination.OpenDoor();
                isOpen = true;
                destination.isOpen = true;
                OnKeyUsed(keyID);
                MovePlayerToNextRoom();
            } else
            {
                OnRoomFailExit();
                OnRoomFailEnter();
            }
            /*if(parent.isEnd)
            {
                Debug.Log("The end");
                GameManager.state = GameManager.LevelPlayState.Won;
                //TODO change this to when the sierpinsk-force is taken
                gm.LevelComplete();
                return;
            }*/
        }
    }

    private void MovePlayerToNextRoom ()
    {
        //Enemy spawning logic here TODO make it better and work with the variable enemies SOs
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        //Debug.Log("Lenght>>>" + enemies.Length);
        if (enemies.Length == 0)
        {
            parentRoom.hasEnemies = false;
        }
        else
        {
            foreach (GameObject enemy in enemies)
            {
                Destroy(enemy);
            }
        }
        //The normal room transition
        Player.instance.transform.position = destination.teleportTransform.position;
        RoomBHV parent = destination.parentRoom;
        Player.instance.AdjustCamera(parent.x, parent.y);


        GameManager.instance.enemyLoader.InstantiateEnemyWithIndex(0, new Vector3(destination.transform.parent.position.x + 6, destination.transform.parent.position.y + 5.5f, 0f), destination.transform.parent.rotation);
        GameManager.instance.enemyLoader.InstantiateEnemyWithIndex(1, new Vector3(destination.transform.parent.position.x + 6, destination.transform.parent.position.y - 6, 0f), destination.transform.parent.rotation);
        GameManager.instance.enemyLoader.InstantiateEnemyWithIndex(2, new Vector3(destination.transform.parent.position.x - 6, destination.transform.parent.position.y - 6, 0f), destination.transform.parent.rotation);
        GameManager.instance.enemyLoader.InstantiateEnemyWithIndex(3, new Vector3(destination.transform.parent.position.x - 6, destination.transform.parent.position.y + 5.5f, 0f), destination.transform.parent.rotation);
        //Spawn the enemies TODO make it spawn the variable enemies SOs
        /*if (destination.transform.parent.gameObject.GetComponent<RoomBHV>().hasEnemies)
        {
            for (int i = 0; i < parent.nSlimes; ++i)
                Instantiate(enemyPrefab, new Vector3(destination.transform.parent.position.x + 0.1f * i, destination.transform.parent.position.y + 0.1f * i, 0f), destination.transform.parent.rotation);
            if (parent.hasTower[0])
                Instantiate(towerPrefab, new Vector3(destination.transform.parent.position.x + 6, destination.transform.parent.position.y + 5.5f, 0f), destination.transform.parent.rotation);
            if (parent.hasTower[1])
                Instantiate(towerPrefab, new Vector3(destination.transform.parent.position.x + 6, destination.transform.parent.position.y - 6, 0f), destination.transform.parent.rotation);
            if (parent.hasTower[2])
                Instantiate(towerPrefab, new Vector3(destination.transform.parent.position.x - 6, destination.transform.parent.position.y - 6, 0f), destination.transform.parent.rotation);
            if (parent.hasTower[3])
                Instantiate(towerPrefab, new Vector3(destination.transform.parent.position.x - 6, destination.transform.parent.position.y + 5.5f, 0f), destination.transform.parent.rotation);
        }*/
        OnRoomExit();
        OnRoomEnter();
    }

    public void SetDestination(DoorBHV other)
    {
        destination = other;
    }

    //Methods to Player Profile
    private void OnRoomFailEnter()
    {
        PlayerProfile.instance.OnRoomFailEnter(new Vector2Int(destination.parentRoom.x, destination.parentRoom.y));
    }

    private void OnRoomEnter()
    {
        PlayerProfile.instance.OnRoomEnter(destination.parentRoom.x, destination.parentRoom.y);
    }

    private void OnRoomFailExit()
    {
        PlayerProfile.instance.OnRoomFailExit(new Vector2Int(parentRoom.x, parentRoom.y));
    }

    private void OnRoomExit()
    {
        PlayerProfile.instance.OnRoomExit(new Vector2Int(parentRoom.x, parentRoom.y));
    }

    private void OnKeyUsed(int id)
    {
        PlayerProfile.instance.OnKeyUsed(id);
    }

    public void OpenDoor()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.sprite = openedSprite;
    }
}