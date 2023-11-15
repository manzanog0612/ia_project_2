using System.Collections.Generic;

using UnityEngine;

using TanksProject.Game.Data;
using TanksProject.Game.Entity.MineController;
using TanksProject.Game.Entity.PopulationController;
using System.Buffers;
using UnityEngine.Pool;
using static UnityEditor.PlayerSettings;
using System.Linq;

namespace TanksProject.Game.Entity.MinesController
{
    public class MinesManager : MonoBehaviour
    {
        #region EXPOSED_FIELDS
        [SerializeField] private GameObject MinePrefab = null;
        #endregion

        #region PRIVATE_FIELDS
        private List<Mine> activeMines = new List<Mine>();
        private ObjectPool<Mine> minesPool = null;

        private Common.Grid.Grid grid = null;
        #endregion

        #region PUBLIC_METHODS
        public void Init(Common.Grid.Grid grid)
        {
            this.grid = grid;

            minesPool = new ObjectPool<Mine>(CreateMine, OnGetMine, OnReleaseMine);
        }

        public void OnTakeMine(GameObject mine)
        {
            minesPool.Release(mine.GetComponent<Mine>());
            //activeMines.Remove(mine.GetComponent<Mine>());
            //Destroy(mine);
        }

        public void CreateMines()
        {
            DestroyMines();

            for (int i = 0; i < GameData.Inst.MinesCount; i++)
            {
                Vector2Int tile = GetRandomTile();
                Vector3 pos = grid.GetTilePos(tile);

                Mine mine = minesPool.Get();
                mine.gameObject.transform.position = pos;
                mine.gameObject.transform.rotation = Quaternion.identity;
                mine.SetTile(tile);
            }
        }

        public void DestroyMines()
        {
            for (int i = activeMines.Count - 1; i >= 0; i--)
            {
                minesPool.Release(activeMines[i]);
            }

           //foreach (Mine go in activeMines)
           //{ 
           //    Destroy(go.gameObject); 
           //}
           //
           //activeMines.Clear();
        }

        public Mine GetNearestMine(Vector3 pos)
        {
            if (activeMines.Count == 0)
            {
                return null;
            }

            Mine nearest = activeMines.First();
            float distance = (pos - nearest.transform.position).sqrMagnitude;

            foreach (Mine go in activeMines)
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

        public bool IsMineOnTile(Vector2Int tile)
        {
            for (int i = 0; i < activeMines.Count; i++)
            {
                if (activeMines[i].Tile == tile)
                {
                    return true;
                }
            }

            return false;
        }
        #endregion

        #region PRIVATE_METHODS
        private Vector2Int GetRandomTile()
        {
            Vector2Int tile = GetRandTile();

            while (activeMines.Find(m => m.Tile == tile) != null)
            {
                tile = GetRandTile();
            }

            Vector2Int GetRandTile()
            {
                if (GameData.Inst.MinesOnCenter)
                {
                    return new Vector2Int(Random.Range(GameData.Inst.PopulationCount, grid.Width - GameData.Inst.PopulationCount - 1),
                    Random.Range(1, grid.Height - 1));
                }
                else
                {
                    return new Vector2Int(Random.Range(1, grid.Width - 1), Random.Range(1, grid.Height - 1));
                }
            }

            return tile;
        }

        #region POOL_METHODS
        private Mine CreateMine()
        {
            GameObject go = Instantiate(MinePrefab, transform);
            return go.GetComponent<Mine>();
        }

        private void OnGetMine(Mine mine)
        {
            mine.gameObject.SetActive(true);
            activeMines.Add(mine);
        }

        private void OnReleaseMine(Mine mine)
        {
            mine.gameObject.SetActive(false);
            activeMines.Remove(mine);
        }
        #endregion
        #endregion
    }
}