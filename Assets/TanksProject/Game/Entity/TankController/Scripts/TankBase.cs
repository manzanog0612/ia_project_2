using TanksProject.Game.Entity.MineController;
using UnityEngine;

namespace TanksProject.Game.Entity.TankController
{
    public class TankBase : MonoBehaviour
    {
        #region EXPOSED_FIELDS
        [SerializeField] protected float Speed = 10.0f;
        [SerializeField] protected float RotSpeed = 15.0f;
        [SerializeField] protected bool dead = false;
        #endregion

        #region PROTECTED_FIELDS
        protected Genome genome;
        protected NeuralNetwork brain;
        protected Mine nearMine;
        protected GameObject nearTank;
        protected Common.Grid.Grid grid;
        protected float[] inputs;
        protected Vector2Int currentTile = Vector2Int.zero;
        protected float turnDuration = 1f;
        #endregion

        #region PUBLIC_METHODS
        public void Init(Common.Grid.Grid grid, Vector2Int currentTile, float turnDuration)
        {
            this.grid = grid;
            this.currentTile = currentTile;
            this.turnDuration = turnDuration;
        }

        public void SetCurrentTile(Vector2Int currentTile)
        {
            this.currentTile = currentTile;
            transform.position = grid.GetTilePos(currentTile);
        }

        public void SetBrain(Genome genome, NeuralNetwork brain)
        {
            this.genome = genome;
            this.brain = brain;
            inputs = new float[brain.InputsCount];
            OnReset();
        }

        public void SetNearestMine(Mine mine)
        {
            nearMine = mine;
        }

        public void SetNearestTank(GameObject tank)
        {
            nearTank = tank;
        }

        public bool IsDead()
        {
            return dead;
        }

        public void SetDead(bool dead)
        {
            this.dead = dead;
        }

        public bool IsCollidingWithObstacle(GameObject obj)
        {
            return (transform.position - obj.transform.position).sqrMagnitude <= 2.0f;
        }

        public void Think(float dt)
        {
            if (dead)
            {
                return;
            }

            OnThink(dt);
        }
        #endregion

        #region PROTECTED_METHODS
        protected Vector3 GetDirToObject(GameObject obj)
        {
            return (obj.transform.position - transform.position).normalized;
        }

        protected Vector2Int GetDistToObject(Vector2Int targetTile)
        {
            return new Vector2Int(targetTile.x - currentTile.x, targetTile.y - currentTile.y);
        }

        protected Vector2Int GetAbsDistToObject(Vector2Int targetTile)
        {
            return new Vector2Int(Mathf.Abs(targetTile.x - currentTile.x), Mathf.Abs(targetTile.y - currentTile.y));
        }

        protected void SetMovement(Vector2Int movement)
        {
            currentTile += movement;

            if (currentTile.x == grid.Width)
            {
                currentTile.x = 0;
            }
            else if (currentTile.x == -1)
            {
                currentTile.x = grid.Width - 1;
            }

            if (currentTile.y == grid.Height)
            {
                currentTile.y = grid.Height - 1;
            }
            else if (currentTile.y == -1)
            {
                currentTile.y = 0;
            }

            transform.position = grid.GetTilePos(currentTile);
        }

        protected virtual void OnThink(float dt)
        {

        }

        protected virtual void OnTakeMine(GameObject mine)
        {
        }

        protected virtual void OnReset()
        {
        }
        #endregion
    }
}
