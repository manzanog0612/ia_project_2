using System.Collections.Generic;

using UnityEngine;

using TanksProject.Game.Data;
using TanksProject.Game.Entity.MineController;

namespace TanksProject.Game.Entity.MinesController
{
    public class MinesManager : MonoBehaviour
    {
        #region EXPOSED_FIELDS
        [SerializeField] private GameObject MinePrefab = null;
        #endregion

        #region PRIVATE_FIELDS
        private List<Mine> mines = new List<Mine>();

        private Common.Grid.Grid grid = null;
        #endregion

        #region PUBLIC_METHODS
        public void Init(Common.Grid.Grid grid)
        {
            this.grid = grid;
        }

        public void CreateMines()
        {
            DestroyMines();

            for (int i = 0; i < GameData.Inst.MinesCount; i++)
            {
                Vector2Int tile = GetRandomTile();
                Vector3 pos = grid.GetTilePos(tile);
                GameObject go = Instantiate(MinePrefab, pos, Quaternion.identity, transform);

                Mine mine = go.GetComponent<Mine>();
                mine.SetTile(tile);
                mines.Add(mine);
            }
        }

        public void DestroyMines()
        {
            foreach (Mine go in mines)
            { 
                Destroy(go.gameObject); 
            }

            mines.Clear();
        }

        public Mine GetNearestMine(Vector3 pos)
        {
            Mine nearest = mines[0];
            float distance = (pos - nearest.transform.position).sqrMagnitude;

            foreach (Mine go in mines)
            {
                float newDist = (go.transform.position - pos).sqrMagnitude;
                if (newDist < distance)
                {
                    nearest = go;
                    distance = newDist;
                }
            }

            return nearest;
        }

        #endregion

        #region PRIVATE_METHODS
        private Vector2Int GetRandomTile()
        {
            Vector2Int tile = new Vector2Int(Random.Range(1, grid.Width - 1), Random.Range(1, grid.Height - 1));

            while (mines.Find(m => m.Tile == tile) != null)
            {
                tile = new Vector2Int(Random.Range(1, grid.Width - 1), Random.Range(1, grid.Height - 1));
            }
            
            return tile;
        }
        #endregion
    }
}