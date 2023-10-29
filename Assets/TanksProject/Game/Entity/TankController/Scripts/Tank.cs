﻿using System.Collections.Generic;

using UnityEngine;

using TanksProject.Game.Data;

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

        protected override void OnThink(float dt)
        {
            Vector3 dirToMine = GetDirToObject(nearMine);
            Vector3 dirCloserTank = GetDirToObject(nearTank);

            float distanceToGoodMine = GetDistToObject(nearMine);

            List<float> inputs = new List<float>
            {
                dirToMine.x,
                dirToMine.z,
                transform.forward.x,
                transform.forward.z,
                dirCloserTank.x,
                dirCloserTank.z,
                (nearTank.transform.position - transform.position).x / 10.0f,
                (nearTank.transform.position - transform.position).z / 10.0f
            };

            float[] output = brain.Synapsis(inputs.ToArray());

            switch (GameData.Inst.TestIndex)
            {
                case 0:
                    RewardIfCloseToGoodMine(distanceToGoodMine);
                    break;
                case 1:
                   // RewardIfCloserToGoodMineThanBadMine(distanceToBadMine, distanceToGoodMine);
                    break;
                case 2:
                    //RewardIfCloserToGoodMineThanBadMine(distanceToBadMine, distanceToGoodMine);
                    //PunishIfCloseToBadMine(distanceToBadMine);
                    break;
                case 3:
                    //RewardIfCloserToGoodMineThanBadMine(distanceToBadMine, distanceToGoodMine);
                    //PunishIfCloseToBadMine(distanceToBadMine);
                    //PunishIfCollidingWithTanks();
                    break;
                default:
                    //RewardIfCloserToGoodMineThanBadMine(distanceToBadMine, distanceToGoodMine);
                    //PunishIfCloseToBadMine(distanceToBadMine);
                    //PunishIfCollidingWithTanks();
                    //PunishIfCollidingWithAnyObstacle();
                    break;
            }

            SetMovement(TraduceMovement(output[0]), dt);
        }

        protected override void OnTakeMine(GameObject mine)
        {
            //switch (GameData.Inst.TestIndex)
            //{
            //    case 0:
            //        if (IsGoodMine(mine))
            //        {
            //            SetFitness((fitness + 100) * 2);
            //        }
            //        break;
            //    default:
            //        if (IsGoodMine(mine))
            //        {
            //            SetFitness((fitness + 100) * 2);
            //        }
            //        else
            //        {
            //            SetFitness(fitness - 300);
            //        }
            //        break;
            //}
        }

        #region PRIVATE_METHODS
        private void RewardIfCloseToGoodMine(float distanceToGoodMine)
        {
            if (distanceToGoodMine < 3)
            {
                SetFitness(fitness + 5f);
            }
        }

        private void PunishIfCloseToBadMine(float distanceToBadMine)
        {
            if (distanceToBadMine < 3)
            {
                SetFitness(fitness - 5f);
            }
        }

        private void RewardIfCloserToGoodMineThanBadMine(float distanceToBadMine, float distanceToGoodMine)
        {
            if (distanceToBadMine > distanceToGoodMine)
            {
                RewardIfCloseToGoodMine(distanceToGoodMine);
            }
        }

        private void PunishIfCollidingWithTanks()
        {
            if (IsCollidingWithObstacle(nearTank))
            {
                SetFitness(fitness - 5f);
            }
        }

        private void PunishIfCollidingWithAnyObstacle()
        {
            //if (IsCollidingWithObstacle(nearObstacle))
            //{
            //    SetFitness(fitness - 5f);
            //}
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