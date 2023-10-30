using UnityEngine;

namespace TanksProject.Game.Entity.MineController
{
    public class Mine : MonoBehaviour
    {
        #region PRIVATE_FIELDS
        private Vector2Int tile;
        #endregion

        #region PROPERTIES
        public Vector2Int Tile { get => tile; }
        #endregion

        #region PUBLIC_METHODS
        public void SetTile(Vector2Int tile)
        {
            this.tile = tile;
        }
        #endregion
    }
}