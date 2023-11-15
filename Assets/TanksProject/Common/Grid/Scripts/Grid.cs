using System.Collections.Generic;

using UnityEngine;

using TanksProject.Common.Grid.Entity.Tile;

namespace TanksProject.Common.Grid
{
    public class Grid : MonoBehaviour
    {
        #region EXPOSED_FIELDS
        [SerializeField] private int width = 9;
        [SerializeField] private int height = 9;
        [SerializeField] private float distanceBetweenNodes = 0.5f;
        [SerializeField] private GameObject prefabTile = null;
        [SerializeField] private Material mat1 = null;
        [SerializeField] private Material mat2 = null;
        #endregion

        #region PRIVATE_FIELDS
        private Tile[,] grid = null;
        #endregion

        #region PROPERTIES
        public int Width { get => width; }
        public int Height { get => height; }
        #endregion

        #region PUBLIC_METHODS
        public void Init()
        {
            grid = new Tile[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector3 pos = new Vector3(x + distanceBetweenNodes * x, 0, y + distanceBetweenNodes * y);
                    GameObject tileGO = Instantiate(prefabTile, pos, Quaternion.identity, transform);
                    tileGO.gameObject.name = "X:" + x + " Y:" + y;

                    Tile tile = new Tile();
                    tile.x = x;
                    tile.y = y;
                    tile.go = tileGO;

                    tileGO.GetComponentInChildren<MeshRenderer>().material = (x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0) ? mat1 : mat2;
                    
                    grid[x, y] = tile;
                }
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    grid[x, y].neighbours = FindNeighbours(x, y);
                }
            }
        }

        public Tile GetTile(int x, int y)
        {
            return grid[x, y];
        }

        public Tile GetTile(Vector2Int gridPosition)
        {
            return grid[gridPosition.x, gridPosition.y];
        }

        public Vector3 GetTilePos(Vector2Int gridPosition)
        {
            return GetTile(gridPosition).go.transform.position; ;
        }
        #endregion

        #region PRIVATE_METHODS
        private List<Tile> FindNeighbours(int posX, int posY)
        {
            List<Tile> tiles = new List<Tile>();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Tile t = grid[x, y];
                    if ((t.x == posX &&
                        (t.y == posY + 1 || t.y == posY - 1)) ||
                        (t.y == posY &&
                        (t.x == posX + 1 || t.x == posX - 1)))
                    {
                        tiles.Add(t);
                    }
                }
            }

            return tiles;
        }

        #endregion
    }
}