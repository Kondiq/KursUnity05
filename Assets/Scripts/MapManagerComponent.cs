using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManagerComponent : MonoBehaviour
{
    public GameObject player;
    public GameObject floor;
    //public GameObject coinPrefab;
    //public int coinsAmount;
    private float mapWidth;
    private float mapHeight;
    private float tileWidth;
    private float tileHeight;
    public int tilesNumberWide=10;
    public int tilesNumberHigh=10;
    private Tile[,] tiles;
    //map boundaries
    private Tile floorTile;

    private LayerMask maskObstacles;


    private int defaultPriority = 1000;

    //priorities
    public int pPlayer = -100;
    public int pGrenade = -800;
    public int pNapalm = -600;
    public int pCover = 200;

    /*
     * List<Grenade> listGrenade;
     * List<Napalm> listNapalm;
     * 
     * events instead - lower priority when grenade thrown, increase priority after explosion
     */


    public struct Tile
    {
        public float x1, x2, z1, z2;
        public int x, y, priority;

        public Tile(float x1, float x2, float z1, float z2, int x, int y)
        {
            this.x1 = x1;
            this.x2 = x2;
            this.z1 = z1;
            this.z2 = z2;
            this.x = x;
            this.y = y;
            priority = 1000;
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        maskObstacles = LayerMask.GetMask("Obstacles");

        mapWidth = 10 * floor.transform.localScale.x;
        mapHeight = 10 * floor.transform.localScale.z;

        tileWidth = mapWidth / tilesNumberWide;
        tileHeight = mapHeight / tilesNumberHigh;
        
        //TODO check if right (1 pixel difference?)
        //map boundaries
        floorTile = new Tile(
            floor.transform.position.x - (mapWidth / 2),
            floor.transform.position.x + (mapWidth / 2),
            floor.transform.position.z - (mapWidth / 2),
            floor.transform.position.z + (mapWidth / 2),
            -1,-1
            );

        //tiles array
        tiles = new Tile[tilesNumberWide, tilesNumberHigh];

        for (int x = 0; x < tilesNumberWide; x++)
            for (int y = 0; y < tilesNumberHigh; y++)
                tiles[x, y] = new Tile(
                    floorTile.x1+x*tileWidth, floorTile.x1 + x*tileWidth + tileWidth,
                    floorTile.z1 + y * tileHeight, floorTile.z1 + y * tileHeight + tileHeight,
                    x,y);

        //SpawnCoins(coinsAmount);
    }

    public bool GetPlayerTile(out Tile tile)
    {
        return GetObjectTile(out tile, player);
    }

    //TODO refactor, dzielenie wspó³rzêdnych, unikn¹æ pêtli dla wiêkszej liczby tile'ów
    public bool GetObjectTile(out Tile tile, GameObject ob)
    {
        for (int x = 0; x < tilesNumberWide; x++)
            for (int y = 0; y < tilesNumberHigh; y++)
            {
                //Debug.Log(tiles[x, y].x1 + "-" + tiles[x, y].x2 + " x " + tiles[x, y].z1 + "-" + tiles[x, y].z2 + " object: " + ob.transform.position.x + "x" + ob.transform.position.z);
                if (
                    ob.transform.position.x > tiles[x, y].x1
                    && ob.transform.position.x < tiles[x, y].x2
                    && ob.transform.position.z > tiles[x, y].z1
                    && ob.transform.position.z < tiles[x, y].z2
                    )
                {
                    tile = tiles[x, y];
                    return true;
                }
            }


        tile = new Tile();
        return false;
    }

    public bool GetCoordinatesTile(out Tile tile, float tx, float ty)
    {
        for (int x = 0; x < tilesNumberWide; x++)
            for (int y = 0; y < tilesNumberHigh; y++)
            {
                //Debug.Log(tiles[x, y].x1 + "-" + tiles[x, y].x2 + " x " + tiles[x, y].z1 + "-" + tiles[x, y].z2 + " object: " + ob.transform.position.x + "x" + ob.transform.position.z);
                if (
                    tx > tiles[x, y].x1
                    && tx < tiles[x, y].x2
                    && ty > tiles[x, y].z1
                    && ty< tiles[x, y].z2
                    )
                {
                    tile = tiles[x, y];
                    return true;
                }
            }


        tile = new Tile();
        return false;
    }

    public Vector3 GetPlayerPosition()
    {
        return player.transform.position;
    }

    public void KillPlayer()
    {
        player.SetActive(false);
    }


    //public void SpawnCoins(int amount)
    //{
    //    Vector2 coinPosition;
    //    for(int i = 0; i < amount; i++)
    //    {
    //        coinPosition = AIPlayerFinder.GenerateWaypoint(floorTile,maskObstacles,60000);
    //        if(coinPosition!= default(Vector2))
    //            Instantiate(coinPrefab, new Vector3(coinPosition.x, 0.5f, coinPosition.y), coinPrefab.transform.rotation);
    //    }
        
    //}

    public float CalculateNeighboursPriority(Tile tile)
    {
        float totalPriority = 0;
        int tilesAmount = 0;
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) //if current node, skip
                    continue;

                if (tile.x+x >= 0 && tile.x+x < tilesNumberWide && tile.y+y >= 0 && tile.y+y < tilesNumberHigh) //if in grid bounds
                {
                    totalPriority += tiles[tile.x+x, tile.y+y].priority;
                    tilesAmount++;
                }
            }
        }
        if (tilesAmount < 8)
        {
            totalPriority = totalPriority / tilesAmount * 8;
        }
        return totalPriority;
    }

    public Tile FindTileBestPriority(Tile originTile)
    {
        Tile bestPriorityTile = tiles[0,0];
        for (int x = 0; x < tilesNumberWide; x++)
            for (int y = 0; y < tilesNumberHigh; y++)
            {                
                if (bestPriorityTile.priority < tiles[x, y].priority)
                    bestPriorityTile = tiles[x, y];
                else if (bestPriorityTile.x != tiles[x, y].x && bestPriorityTile.y != tiles[x, y].y)
                    if (bestPriorityTile.priority == tiles[x, y].priority)
                    {
                        if(CalculateNeighboursPriority(bestPriorityTile)< CalculateNeighboursPriority(tiles[x,y]))
                            bestPriorityTile = tiles[x, y];
                        else if(CalculateNeighboursPriority(bestPriorityTile) == CalculateNeighboursPriority(tiles[x, y])) //closer to originTile
                        {
                            if(Mathf.Abs(bestPriorityTile.x-originTile.x) + Mathf.Abs(bestPriorityTile.y - originTile.y)
                                < Mathf.Abs(tiles[x, y].x - originTile.x) + Mathf.Abs(tiles[x, y].y - originTile.y))
                                bestPriorityTile = tiles[x, y];
                        }
                    }

            }
        return bestPriorityTile;
    }

    //TODO
    public Tile FindTileBestPriorityInRange(Tile originTile, int radiusInTiles)
    {
        Tile bestPriorityTile = floorTile;//temp
        return bestPriorityTile;
    }

    public void ChangeTilePriority(float x, float y, int changeFactor)
    {
        Tile tileToChange;
        if(GetCoordinatesTile(out tileToChange, x, y))
            tileToChange.priority += changeFactor;
    }

    public void ResetTilesPriority()
    {
        for (int x = 0; x < tilesNumberWide; x++)
            for (int y = 0; y < tilesNumberHigh; y++)
                tiles[x, y].priority = defaultPriority;
    }

    //public void UpdateTilesPriority()
    //{
    //    ResetTilesPriority();
    //    Tile tilePlayer;
    //    if (GetPlayerTile(out tilePlayer))
    //        tilePlayer.priority += pPlayer;
    //}
}
