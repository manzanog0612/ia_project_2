using System.Collections.Generic;

using UnityEngine;

using TanksProject.Game.Data;

namespace TanksProject.Game.Entity.MinesController
{
    public class MinesManager : MonoBehaviour
    {
        #region EXPOSED_FIELDS
        [SerializeField] private GameObject MinePrefab = null;
        #endregion

        #region PRIVATE_FIELDS
        private List<GameObject> mines = new List<GameObject>();

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
                Vector3 position = GetRandomPos();
                GameObject go = Instantiate(MinePrefab, position, Quaternion.identity);
                mines.Add(go);
            }
        }

        public void DestroyMines()
        {
            foreach (GameObject go in mines)
            { 
                Destroy(go); 
            }

            mines.Clear();
        }

        public GameObject GetNearestMine(Vector3 pos)
        {
            GameObject nearest = mines[0];
            float distance = (pos - nearest.transform.position).sqrMagnitude;

            foreach (GameObject go in mines)
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
        private Vector3 GetRandomPos()
        {
            Vector3 pos = grid.GetTilePos(new Vector2Int(Random.Range(1, grid.Width - 1), Random.Range(1, grid.Height - 1)));

            while (mines.Find(m => m.transform.position == pos) != null)
            {
                pos = grid.GetTilePos(new Vector2Int(Random.Range(1, grid.Width - 1), Random.Range(1, grid.Height - 1)));
            }
            
            return pos;
        }
        #endregion
    }
}