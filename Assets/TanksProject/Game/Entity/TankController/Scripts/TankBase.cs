using UnityEngine;

using TanksProject.Game.Entity.MineController;
using System;
using TanksProject.Game.Data;

namespace TanksProject.Game.Entity.TankController
{
    public enum STATE { DIE, SURVIVE, REPRODUCE }

    public class TankBase : MonoBehaviour
    {
        #region PROTECTED_FIELDS
        protected Genome genome;
        protected NeuralNetwork brain;
        protected Mine nearMine;
        protected Tank nearEnemyTank;
        protected Tank nearTeamTank;
        protected Common.Grid.Grid grid;
        protected Vector2Int currentTile = Vector2Int.zero;
        protected Vector2Int fromTile = Vector2Int.zero;
        protected float turnDuration = 1f;
        protected STATE state = STATE.SURVIVE;
        protected int generationsAlive = 0;
        #endregion

        #region PROPERTIES
        public Vector2Int Tile { get => currentTile; }
        public STATE State { get => state; }
        public int GenerationsAlive { get => generationsAlive; set => generationsAlive = value; }
        public Genome Genome { get => genome; }
        public NeuralNetwork Brain { get => brain; }
        public Mine NearMine { get => nearMine; }
        public Tank NearEnemyTank { get => nearEnemyTank; }
        public Tank NearTeamTank { get => nearTeamTank; }
        #endregion

        #region ACTIONS
        private Action<GameObject> onTakeMine = null;
        #endregion

        #region PUBLIC_METHODS
        public void Init(Common.Grid.Grid grid, Vector2Int currentTile, float turnDuration, Action<GameObject> onTakeMine)
        {
            this.grid = grid;
            this.currentTile = currentTile;
            fromTile = currentTile;
            this.turnDuration = turnDuration;
            this.onTakeMine = onTakeMine;
            generationsAlive = 0;
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
            OnReset();
        }

        public void SetNearestMine(Mine mine)
        {
            nearMine = mine;
        }

        public void SetNearestEnemyTank(Tank tank)
        {
            nearEnemyTank = tank;
        }

        public void SetNearestTeamTank(Tank tank)
        {
            nearTeamTank = tank;
        }

        public bool IsDead()
        {
            return state == STATE.DIE;
        }

        public void SetState(STATE state)
        {
            this.state = state;
        }

        public void Think()
        {
            if (IsDead() && !GameData.Inst.Learning)
            {
                return;
            }

            OnThink();
        }

        public void TakeMine()
        {
            OnTakeMine(nearMine.gameObject);
        }

        public virtual void OnReset()
        {
            state = STATE.SURVIVE;
            nearMine = null;
        }
        #endregion

        #region PROTECTED_METHODS
        protected Vector2Int GetDistToObject(Vector2Int targetTile)
        {
            return new Vector2Int(targetTile.x - currentTile.x, targetTile.y - currentTile.y);
        }

        protected Vector2Int GetAbsDistToObject(Vector2Int targetTile)
        {
            return new Vector2Int(Mathf.Abs(targetTile.x - currentTile.x), Mathf.Abs(targetTile.y - currentTile.y));
        }

        protected Vector2Int GetLastAbsDistToObject(Vector2Int targetTile, Vector2Int fromPos)
        {
            return new Vector2Int(Mathf.Abs(targetTile.x - fromPos.x), Mathf.Abs(targetTile.y - fromPos.y));
        }

        protected void SetMovement(Vector2Int movement)
        {
            if (movement != Vector2Int.zero)
            {
                fromTile = currentTile;
            }

            currentTile += movement;

            Move();
        }

        protected void RunAway()
        {
            currentTile = fromTile;

            Move();
        }

        protected virtual void OnThink()
        {
            generationsAlive++;
        }

        protected virtual void OnTakeMine(GameObject mine)
        {
            onTakeMine.Invoke(mine);
            nearMine = null;
        }
        #endregion

        #region PRIVATE_METHODS
        private void Move()
        {
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
        #endregion
    }
}
