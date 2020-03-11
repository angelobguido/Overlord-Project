using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using EnemyGenerator;
using System.IO;
using System.Text;

public struct CombatRoomInfo
{
    public int roomId;
    public bool hasEnemies;
    public int nEnemies;
    public List<int> enemiesIndex;
    public int playerInitHealth;
    public int playerFinalHealth;
    public int timeToExit;
}

public class PlayerProfile : MonoBehaviour {

    public static PlayerProfile instance = null;

    private int roomID = 0;

    private const string PostDataURL = "http://damicore.icmc.usp.br/pag/data/upload.php?";
    private int attemptNumber = 1; //TODO: entender o por quê desse int

    [SerializeField]
    public string sessionUID;
    [SerializeField]
    private string profileString, heatMapString, enemyString;

    [SerializeField]
    private int mapCount = 0;
    [SerializeField]
    private int curMapId, curBatchId;

    [SerializeField]
    private List<Vector2Int> visitedRooms = new List<Vector2Int>();
    [SerializeField]
    private int mapVisitedCount = 0;
    [SerializeField]
    private int mapVisitedCountUnique = 0;
    [SerializeField]
    private int keysTaken = 0;
    [SerializeField]
    private int keysUsed = 0;
    [SerializeField]
    private List<int> formAnswers = new List<int>();
    [SerializeField]
    private int secondsToFinish = 0;
    System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
    [SerializeField]
    private int[,] heatMap;

    //Enemy Generator Data
    protected List<CombatRoomInfo> combatInfoList;
    protected int difficultyLevel;
    protected List<int> damageDoneByEnemy;
    protected int timesPlayerDied;
    protected bool hasFinished; //0 if player gave up, 1 if he completed the stage 
    protected CombatRoomInfo actualRoomInfo;

    private string result;

    void Awake()
    {
        //Singleton
        if (instance == null)
        {
            instance = this;
            combatInfoList = new List<CombatRoomInfo>();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    // Use this for initialization
    void Start () {
        // FIXME: utilizar uma ID única corretamente
        string dateTime = System.DateTime.Now.ToString();
        dateTime = dateTime.Replace("/", "-");
        sessionUID = Random.Range(0, 9999).ToString("00");
        sessionUID += "_";
        sessionUID += dateTime;

        attemptNumber = 0; //TODO: entender o por quê desse int
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnGameStart()
    {
        profileString = "";
        heatMapString = "";
        enemyString = "";
        mapCount = 0;
        visitedRooms = new List<Vector2Int>();
        mapVisitedCount = 0;
        mapVisitedCountUnique = 0;
        keysTaken = 0;
        keysUsed = 0;
        formAnswers = new List<int>();
        secondsToFinish = 0;
        stopWatch = new System.Diagnostics.Stopwatch();

        //Enemy Generator Data
        combatInfoList = new List<CombatRoomInfo>();
        difficultyLevel = -1;
        damageDoneByEnemy = new int[EnemyUtil.nBestEnemies].ToList();
        timesPlayerDied = 0;
        hasFinished = false; //0 if player gave up, 1 if he completed the stage 
    }

    //Events
    //From DoorBHV
    public void OnRoomFailEnter(Vector2Int offset)
    {
        //Log
        //Mais métricas - organiza em TAD
    }

    //From DoorBHV
    public void OnRoomEnter (int x, int y, bool hasEnemies, List<int> enemyList, int playerHealth)
    {
        //Log
        //Mais métricas - organiza em TAD
        heatMap[x / 2, y / 2]++;
        visitedRooms.Add(new Vector2Int(x, y));
        if (hasEnemies)
        {
            actualRoomInfo.roomId = 10 * x + y;
            actualRoomInfo.hasEnemies = hasEnemies;
            actualRoomInfo.playerInitHealth = playerHealth;
            actualRoomInfo.nEnemies = enemyList.Count;
            actualRoomInfo.enemiesIndex = enemyList;
            actualRoomInfo.timeToExit = System.Convert.ToInt32(stopWatch.ElapsedMilliseconds);
        }
        else
            actualRoomInfo.roomId = -1;
    }

    //From DoorBHV
    public void OnRoomFailExit(Vector2Int offset)
    {
        //Log
        //Mais métricas - organiza em TAD
    }

    //From DoorBHV
    public void OnRoomExit(Vector2Int offset, int playerHealth)
    {
        if(actualRoomInfo.roomId != -1)
        {
            actualRoomInfo.playerFinalHealth = playerHealth;
            actualRoomInfo.timeToExit = System.Convert.ToInt32(stopWatch.ElapsedMilliseconds) - actualRoomInfo.timeToExit;
            combatInfoList.Add(actualRoomInfo);
        }
        //Log
        //Mais métricas - organiza em TAD
    }

    //From DoorBHV
    public void OnKeyUsed(int id)
    {
        //Log
        keysUsed++;
        //Mais métricas - organiza em TAD
    }

    //From GameManager
    public void OnMapStart (int id, int batch, Room[,] rooms)
    {
        mapCount++;
        curMapId = id;
        curBatchId = batch;
        stopWatch.Start();
        heatMap = CreateHeatMap(rooms);
        combatInfoList = new List<CombatRoomInfo>();
        attemptNumber++;

        difficultyLevel = id;
        //Log
        //Mais métricas - organiza em TAD
    }

    //From inheritance
    private void OnApplicationQuit()
    {
        //Log
    }

    //From TriforceBHV
    public void OnMapComplete (bool victory)
    {
        stopWatch.Stop();
        secondsToFinish = stopWatch.Elapsed.Seconds;
        stopWatch.Reset();
        //Log
        //Mais métricas - organiza em TAD, agrega dados do nível
        //visitedRooms = visitedRooms.Distinct();
        mapVisitedCount = visitedRooms.Count;
        mapVisitedCountUnique = visitedRooms.Distinct().Count();

        hasFinished = victory;
        //Save to remote file
        SendProfileToServer();
        //Reset all values
        visitedRooms.Clear();
        formAnswers.Clear();
        keysTaken = 0;
        keysUsed = 0;
        profileString = "";
    }

    //From KeyBHV
    public void OnGetKey (int id)
    {
        //Log
        keysTaken++;
        //Mais métricas - organiza em TAD
    }

    //From FormBHV
    public void OnFormAnswered(int answer)
    {
        //Log
        formAnswers.Add(answer);
    }

    private void WrapProfileToString ()
    {
        profileString = "";
        profileString += "\nmapCount,"+mapVisitedCount + ",uniquemap," + mapVisitedCountUnique + ",keys," + keysTaken + ",locks," + keysUsed + ",time,"+ secondsToFinish;

    }

    private void WrapEnemyProfileToString()
    {
        enemyString = "";
        enemyString += "Difficulty," + difficultyLevel+"\n";
        enemyString += "Deaths," + timesPlayerDied + "\n";
        enemyString += "Victory?," + hasFinished + "\n";
        enemyString += "EnemyDamage,\n";
        for(int i = 0; i < EnemyUtil.nBestEnemies; ++i)
            enemyString += i+",";
        enemyString += "\n";
        for (int i = 0; i < EnemyUtil.nBestEnemies; ++i)
            enemyString += damageDoneByEnemy[i] + ",";
        enemyString += "\n";
        enemyString += "RoomID:,playerInitialHealth,PlayerFinalHealth,HealthLost,TimeToExit,hasEnemies,nEnemies,EnemiesIds,\n";
        foreach (CombatRoomInfo info in combatInfoList)
        {
            enemyString += info.roomId + ",";
            enemyString += info.playerInitHealth + ",";
            enemyString += info.playerFinalHealth + ",";
            enemyString += (info.playerFinalHealth-info.playerInitHealth) + ",";
            enemyString += info.timeToExit + ",";
            enemyString += info.hasEnemies + ",";
            enemyString += info.nEnemies + ",";
            foreach (int enemyId in info.enemiesIndex)
                enemyString += enemyId + ",";
            enemyString += "\n";
        }
        enemyString += "\n";
        enemyString += "\nForm,";
        if (formAnswers.Count > 0)
        {
            foreach (int answer in formAnswers)
            {
                enemyString += answer + ",";
            }
        }
        else
            enemyString += "-1,";
        enemyString += "\n";
    }

    private void WrapHeatMapToString()
    {
        heatMapString = "";
        for (int i = 0; i < Map.sizeX / 2; ++i)
        {
            for (int j = 0; j < Map.sizeY / 2; ++j)
            {
                heatMapString += heatMap[i, j].ToString()+",";
            }
            heatMapString += "\n";
        }
        //Debug.Log(heatMapString);
    }
    //File name: BatchId, MapId, SessionUID
    //Player profile: N Visited Rooms, N Unique Visited Rooms, N Keys Taken, N Keys Used, Form Answer 1, Form Answer 2,Form Answer 3
    private void SendProfileToServer ()
    {
        WrapProfileToString();
        WrapHeatMapToString();
        WrapEnemyProfileToString();
        StartCoroutine(PostData("Batch"+curBatchId.ToString() +"Map" + curMapId.ToString(), profileString, heatMapString, enemyString)); //TODO: verificar corretamente como serão salvos os arquivos

        string UploadFilePath = PlayerProfile.instance.sessionUID;


    }

    IEnumerator PostData(string name, string stringData, string heatMapData, string enemyData)
    {
        stringData = sessionUID + "," + stringData;
        byte[] data = System.Text.Encoding.UTF8.GetBytes(stringData);
        byte[] heatMapBinary = System.Text.Encoding.UTF8.GetBytes(heatMapData);
        byte[] enemyBinary = System.Text.Encoding.UTF8.GetBytes(enemyData);
        //This connects to a server side php script that will write the data
        //string post_url = postDataURL + "name=" + WWW.EscapeURL(name) + "&data=" + data ;
        string post_url = PostDataURL;
        Debug.Log("LogName:"+name);
        WWWForm form = new WWWForm();
        form.AddField("name", sessionUID);
        form.AddBinaryData("data", data, name + "_Attempt" + attemptNumber + ".csv", "text/csv");
        form.AddBinaryData("heatmap", heatMapBinary, "HM"+name + "_Attempt" + attemptNumber + ".csv", "text/csv");
        form.AddBinaryData("enemy", enemyBinary, "Enemy" + name + "_Attempt" + attemptNumber + ".csv", "text/csv");


        // Post the URL to the site and create a download object to get the result.
        WWW data_post = new WWW(post_url, form);
        yield return data_post; // Wait until the download is done

        if (data_post.error != null)
        {
            print("There was an error saving data: " + data_post.error);
        }
        else
        {
            Debug.Log("Upload complete!");
        }
    }

    public int[,] CreateHeatMap(Room[,] rooms)
    {
        int[,] heatMap = new int[Map.sizeX / 2, Map.sizeY / 2];
        for (int i = 0; i < Map.sizeX / 2; ++i)
        {
            //string aux = "";
            for (int j = 0; j < Map.sizeY / 2; ++j)
            {
                if (rooms[i * 2, j * 2] == null)
                {
                    heatMap[i, j] = -1;
                    //aux += "-1";
                }
                else
                {
                    heatMap[i, j] = 0;
                    //aux += "0";
                }
            }
            //Debug.Log(aux);
        }
        //Debug.Log("Finished Creating HeatMap");
        return heatMap;
    }

    public void OnEnemyDoesDamage(int index, int damage)
    {
        damageDoneByEnemy[index]+=damage;
    }

    public void OnDeath()
    {
        if (actualRoomInfo.roomId != -1)
        {
            actualRoomInfo.playerFinalHealth = 0;
            actualRoomInfo.timeToExit = System.Convert.ToInt32(stopWatch.ElapsedMilliseconds) - actualRoomInfo.timeToExit;
            combatInfoList.Add(actualRoomInfo);
        }
        timesPlayerDied++;
    }

    public void OnRetry()
    {
        OnMapComplete(false);
    }
}
