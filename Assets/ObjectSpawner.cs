using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ObjectSpawner : MonoBehaviour
{
    public enum ObjectType{
        CandyItem,
        Enemy
    }

    public Tilemap tilemap;
    public GameObject[] objectPrefabs;
    public float CandyProbability = 0.5f;
    public float EnemyProbability = 0.1f;
    public int maxObjects = 6;

    public float spawnDelay = 0.5f;

    private List<Vector3> validSpawnPositions = new List<Vector3>();
    private List<GameObject> spawnedObjects = new List<GameObject>();
    private bool isSpawning = false;



    // Start is called before the first frame update
    void Start()
    {
        GatherValidSpawnPositions();
        StartCoroutine(SpawnObjectsIfNeeded());


    }

    // Update is called once per frame
    void Update()
    {
        if(!tilemap.gameObject.activeInHierarchy){
            LevelChange();
        }
        if(!isSpawning && ActiveObjectCount() < maxObjects){
            StartCoroutine(SpawnObjectsIfNeeded());    
        }
    }

    private void LevelChange(){
        tilemap = GameObject.Find("Ground").GetComponent<Tilemap>();
        GatherValidSpawnPositions();
        DestroyAllSpawnedObjects();
        
    }

    private int ActiveObjectCount(){
            spawnedObjects.RemoveAll(go => go == null);
            return spawnedObjects.Count;
    }

    private IEnumerator SpawnObjectsIfNeeded(){
        isSpawning =true;
        while(ActiveObjectCount() < maxObjects){
            SpawnObject();
            yield return new WaitForSeconds(spawnDelay);
        }
        isSpawning = false;
    }

    private bool positionHasObject(Vector3 position){
        return spawnedObjects.Any(obj => obj && Vector3.Distance(obj.transform.position, position) < 1.0f);
    }

    private ObjectType RandomObjectType(){
            float randomChoice = Random.value;

            if(randomChoice <= CandyProbability){
                return ObjectType.CandyItem;
            }
            else{
                return ObjectType.Enemy;
            }
    }

    private void SpawnObject(){
        if(validSpawnPositions.Count == 0){
            return; 
        }
        Vector3 spawnPosition = Vector3.zero;
        bool validPositionFound = false;

        while(!validPositionFound && validSpawnPositions.Count > 0){
            int randomIndex = Random.Range(0, validSpawnPositions.Count);
            Vector3 potentionalPosition = validSpawnPositions[randomIndex];
            Vector3 leftPosition = potentionalPosition + Vector3.left;
            Vector3 rightPosition = potentionalPosition + Vector3.right;

            if( !positionHasObject(leftPosition) && !positionHasObject(rightPosition)){
                spawnPosition = potentionalPosition;
                validPositionFound = true;
            }
            validSpawnPositions.RemoveAt(randomIndex);
        }

        if(validPositionFound){
            ObjectType objectType = RandomObjectType();
            GameObject prefab = objectPrefabs[(int)objectType];
            GameObject gameObject = Instantiate(objectPrefabs[(int)objectType], spawnPosition, Quaternion.identity);
            spawnedObjects.Add(gameObject);

            if(objectType == ObjectType.CandyItem){ 
                StartCoroutine(DestroyObject(gameObject));
            }
        }

    }

    private IEnumerator DestroyObject(GameObject gameObject){
        yield return new WaitForSeconds(6.0f);
        if(gameObject){
            spawnedObjects.Remove(gameObject);
            validSpawnPositions.Add(gameObject.transform.position);
            Destroy(gameObject);

        }
    }

    private void DestroyAllSpawnedObjects(){
        spawnedObjects.ForEach(obj => Destroy(obj));
        spawnedObjects.Clear();
    }

    private void GatherValidSpawnPositions()
    {
        validSpawnPositions.Clear();
        BoundsInt bounds = tilemap.cellBounds;
        TileBase[] allTiles = tilemap.GetTilesBlock(bounds);
        Vector3 start = tilemap.CellToWorld(new Vector3Int(bounds.xMin, bounds.yMin, 0));

        for(int x = 0; x < bounds.size.x; x++)
        {
            for(int y = 0; y < bounds.size.y; y++)
            {
                TileBase title = allTiles[x + y * bounds.size.x];
                if(title != null)
                {
                    Vector3 place = start + new Vector3(x + 0.5f, y + 1.5f, 0);
                    validSpawnPositions.Add(place);
                }
            }
        }
    }
}
