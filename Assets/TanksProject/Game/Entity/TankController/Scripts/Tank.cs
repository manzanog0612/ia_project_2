using System.Collections.Generic;

using UnityEngine;

using TanksProject.Game.Data;
using TanksProject.Game.Entity.MineController;

namespace TanksProject.Game.Entity.TankController
{
    public class Tank : TankBase
    {
        float fitness = 0;

        protected override void OnReset()
        {
            base.OnReset();
            fitness = 1;
        }

        protected override void OnThink()
        {
            //Vector3 dirToMine = GetDirToObject(nearMine.gameObject);

            Vector2Int distToMine = GetDistToObject(nearMine.Tile);

            List<float> inputs = new List<float>
            {
                //dirToMine.x,
                //dirToMine.z,
                currentTile.x / (float)grid.Width,
                currentTile.y / (float)grid.Height,
                nearMine.Tile.x / (float)grid.Width,
                nearMine.Tile.y / (float)grid.Height,
                distToMine.x / 10f,
                distToMine.y / 10f,                
               //transform.forward.x,
               //transform.forward.z,
               //dirCloserTank.x,
               //dirCloserTank.z,
               //(nearTank.transform.position - transform.position).x / 10.0f,
               //(nearTank.transform.position - transform.position).z / 10.0f
            };

            float[] output = brain.Synapsis(inputs.ToArray());

            SetMovement(TraduceMovement(output[0]));

            RewardIfCloseToMine();

            if (currentTile == nearMine.Tile)
            {
                OnTakeMine(nearMine.gameObject);
            }
        }

        protected override void OnTakeMine(GameObject mine)
        {
            switch (GameData.Inst.TestIndex)
            {
                default:
                    SetFitness((fitness + 100) * 2);
                break;
            }
        }

        #region PRIVATE_METHODS
        private void RewardIfCloseToMine()
        {
            Vector2Int absDistToMine = GetAbsDistToObject(nearMine.Tile);
            if (absDistToMine.x < 3 && absDistToMine.y < 3)
            {
                SetFitness(fitness + 100);
            }
        }

        private Vector2Int TraduceMovement(float movement)
        {
            if (movement > 0.8)
            {
                return Vector2Int.up;
            }
            else if (movement > 0.6)
            {
                return Vector2Int.right;
            }
            else if (movement > 0.4)
            {
                return Vector2Int.down;
            }
            else if (movement > 0.2)
            {
                return Vector2Int.left;
            }
            else
            {
                return Vector2Int.zero;
            }
        }
        #endregion

        #region AUX
        private void SetFitness(float fitness)
        {
            if (fitness < 0)
            {
                fitness = 0;
            }

            this.fitness = fitness;
            genome.fitness = fitness;
        }
        #endregion
    }
}