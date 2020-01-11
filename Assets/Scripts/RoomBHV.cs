using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnemyGenerator;

public class RoomBHV : MonoBehaviour {

	public int x;
	public int y;
	public int northDoor = -1; //-1 for non-existant
	public int southDoor = -1;
	public int eastDoor = -1;
	public int westDoor = -1;
	public int availableKeyID = 0;
	public bool isStart = false;
	public bool isEnd = false;

    public bool hasEnemies;
    //TODO change this for the variable SO enemies
    public bool[] hasTower;
    public int nSlimes;
    public int difficultyLevel;
    public List<int> enemiesIndex;
    private int enemiesDead;

    public DoorBHV doorNorth;
	public DoorBHV doorSouth;
	public DoorBHV doorEast;
	public DoorBHV doorWest;

	public KeyBHV keyPrefab;
    public TriforceBHV triPrefab;

    //TODO change this for the SO enemies prefabs
    public GameObject enemyPrefab, towerPrefab;


    public Collider2D colNorth;
	public Collider2D colSouth;
	public Collider2D colEast;
	public Collider2D colWest;

	public TileBHV tilePrefab;
    public BlockBHV blockPrefab;

    //TODO change for SO enemies
    private void Awake()
    {
        hasEnemies = true;
        enemiesIndex = new List<int>();
        enemiesDead = 0;
        /*nSlimes = Random.Range(0, 5);
        hasTower = new bool[4];
        for (int i = 0; i < 4; ++i)
        {
            if (Random.Range(0, 99) > 49)
            {
                hasTower[i] = true;
            }
            else
            {
                hasTower[i] = false;
            }
        }*/
    }

    // Use this for initialization
    void Start () {
		SetLayout ();
		if (availableKeyID > 0){ // existe uma chave
			// instancia chave
			KeyBHV key = Instantiate(keyPrefab, transform);
			key.keyID = availableKeyID;
			//Debug.Log ("KeyID: " + key.keyID);
			key.SetRoom (x, y);
		}
		if (isStart){
			//Algum efeito
			transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.green;
            hasEnemies = false;
        }
        else if (isEnd){
            TriforceBHV tri = Instantiate(triPrefab, transform);
            tri.SetRoom(x, y);
            //Algum efeito
            transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.red;
            hasEnemies = false;
        }
        else
        {
            SelectEnemies();
        }
    }

    // Update is called once per frame
    void Update () {
        
	}

	void SetLayout(){
		doorNorth.keyID = northDoor;
		doorSouth.keyID = southDoor;
		doorEast.keyID = eastDoor;
		doorWest.keyID = westDoor;
		float centerX = Room.sizeX / 2.0f - 0.5f;
		float centerY = Room.sizeY / 2.0f - 0.5f;
		const float delta = 0.0f; //para que os colisores das portas e das paredes não se sobreponham completamente
		//Posiciona as portas - são somados/subtraídos 1 para que as portas e colisores estejam periféricos à sala
		doorNorth.transform.localPosition = new Vector2 (0.0f, centerY + 1 - delta);
		doorSouth.transform.localPosition = new Vector2 (0.0f, -centerY -1 + delta);
		doorEast.transform.localPosition = new Vector2 (centerX + 1 - delta, 0.0f);
		doorWest.transform.localPosition = new Vector2 (-centerX -1 + delta, 0.0f);

		//Posiciona os colisores das paredes da sala
		colNorth.transform.localPosition = new Vector2 (0.0f, centerY + 1);
		colSouth.transform.localPosition = new Vector2 (0.0f, -centerY - 1);
		colEast.transform.localPosition = new Vector2 (centerX + 1, 0.0f);
		colWest.transform.localPosition = new Vector2 (-centerX -1, 0.0f);
		colNorth.GetComponent<BoxCollider2D> ().size = new Vector2(Room.sizeX + 2, 1);
		colSouth.GetComponent<BoxCollider2D> ().size = new Vector2(Room.sizeX + 2, 1);
		colEast.GetComponent<BoxCollider2D> ().size = new Vector2 (1, Room.sizeY + 2);
		colWest.GetComponent<BoxCollider2D> ().size = new Vector2 (1, Room.sizeY + 2);

		//Ajusta sprites das paredes
		colNorth.gameObject.GetComponent<SpriteRenderer>().size = new Vector2(Room.sizeX + 2, 1);
		colSouth.gameObject.GetComponent<SpriteRenderer>().size = new Vector2(Room.sizeX + 2, 1);
		colEast.gameObject.GetComponent<SpriteRenderer>().size = new Vector2 (1, Room.sizeY + 2);
		colWest.gameObject.GetComponent<SpriteRenderer>().size = new Vector2 (1, Room.sizeY + 2);

		//Posiciona os tiles
		Room thisRoom = GameManager.instance.GetMap().rooms[x, y]; //TODO fazer de forma similar para tirar construção de salas do GameManager
		for (int ix = 0; ix < Room.sizeX; ix++){
			for (int iy = 0; iy < Room.sizeY; iy++){
				int tileID = thisRoom.tiles [ix, iy];
                TileBHV tileObj;
                if (tileID == 1)
                    tileObj = Instantiate(blockPrefab);
                else
                    tileObj = Instantiate(tilePrefab);
				tileObj.transform.SetParent (transform);
				tileObj.transform.localPosition = new Vector2 (ix - centerX, Room.sizeY -1 - iy - centerY);
				tileObj.GetComponent<SpriteRenderer> (); //FIXME provisório para diferenciar sprites
				tileObj.id = tileID;
				tileObj.x = ix;
				tileObj.y = iy;
			}
		}
	}

    private void SelectEnemies()
    {
        difficultyLevel = GameManager.instance.GetMap().rooms[x, y].difficulty;
        float actualDifficulty = 0;
        int auxIndex;
        while (actualDifficulty < difficultyLevel)
        {
            auxIndex = Random.Range(0, EnemyUtil.nBestEnemies);
            enemiesIndex.Add(auxIndex);
            actualDifficulty += GameManager.instance.enemyLoader.bestEnemies[auxIndex].fitness;
        }
    }

    public void SpawnEnemies()
    {
        GameObject enemy;
        if (enemiesIndex.Count != 4)
        {
            for (int i = 0; i < enemiesIndex.Count; ++i)
            {
                enemy = GameManager.instance.enemyLoader.InstantiateEnemyWithIndex(enemiesIndex[i], new Vector3(transform.position.x, transform.position.y, 0f), transform.rotation);
                enemy.GetComponent<EnemyController>().SetRoom(this);
            }
        }
        else
        {
            enemy = GameManager.instance.enemyLoader.InstantiateEnemyWithIndex(enemiesIndex[0], new Vector3(transform.position.x + 6, transform.position.y + 5.5f, 0f), transform.rotation);
            enemy.GetComponent<EnemyController>().SetRoom(this);
            enemy = GameManager.instance.enemyLoader.InstantiateEnemyWithIndex(enemiesIndex[1], new Vector3(transform.position.x + 6, transform.position.y - 6, 0f), transform.rotation);
            enemy.GetComponent<EnemyController>().SetRoom(this);
            enemy = GameManager.instance.enemyLoader.InstantiateEnemyWithIndex(enemiesIndex[2], new Vector3(transform.position.x - 6, transform.position.y - 6, 0f), transform.rotation);
            enemy.GetComponent<EnemyController>().SetRoom(this);
            enemy = GameManager.instance.enemyLoader.InstantiateEnemyWithIndex(enemiesIndex[3], new Vector3(transform.position.x - 6, transform.position.y + 5.5f, 0f), transform.rotation);
            enemy.GetComponent<EnemyController>().SetRoom(this);
        }
    }

    public void CheckIfAllEnemiesDead()
    {
        enemiesDead++;
        if(enemiesDead == enemiesIndex.Count)
        {
            hasEnemies = false;
            doorEast.OpenDoorAfterKilling();
            doorWest.OpenDoorAfterKilling();
            doorNorth.OpenDoorAfterKilling();
            doorSouth.OpenDoorAfterKilling();
        }
    }
}
