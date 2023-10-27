using UnityEngine;

namespace TanksProject.Game
{
    public class GameController : MonoBehaviour
    {
        #region EXPOSED_FIELDS
        [SerializeField] private Common.Grid.Grid grid = null;
        #endregion

        #region UNITY_CALLS
        private void Start()
        {
            Init();
        }
        #endregion

        #region PRIVATE_METHODS
        public void Init()
        {
            grid.Init();
            Camera.main.orthographicSize = grid.Width / 2;
            Camera.main.transform.position = new Vector3(grid.Width / 2, 10, grid.Height / 2);
        }
        #endregion
    }
}