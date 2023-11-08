using System.Collections.Generic;

using UnityEngine;

using TanksProject.Game.Data;
using TanksProject.Game.Entity.MineController;

namespace TanksProject.Game.Entity.TankController
{
    public class Tank : TankBase
    {
        #region PRIVATE_METHODS
        private float fitness = 0;
        private int turnsNotMovingAndNoMine = 0;
        private int turnsCloseToMine = 0;
        private int minesEaten = 0;
        private bool lastTurnWasCloseToMine = false;
        private Mine lastCloserMine = null;
        private Vector2Int lastMovement = Vector2Int.zero;
        private Vector2Int lastLastMovement = Vector2Int.zero;
        private List<Vector2Int> movements = new List<Vector2Int>();
        #endregion

        #region PROPERTIES
        public int MinesEaten { get => minesEaten; }
        public List<Vector2Int> Movements { get => movements; }
        #endregion

        #region OVERRIDE
        protected override void OnReset()
        {
            base.OnReset();
            fitness = 0;
            minesEaten = 0;
            turnsNotMovingAndNoMine = 0;
            turnsCloseToMine = 0;
            lastTurnWasCloseToMine = false;
            movements.Clear();
            lastCloserMine = null;
            lastMovement = Vector2Int.zero;
            lastLastMovement = Vector2Int.zero;
        }

        protected override void OnThink()
        {
            Vector2Int tMine = nearMine.Tile;
            Vector2Int tEnemy = nearEnemyTank.Tile;
            Vector2Int tTeam = nearTeamTank.Tile;

            Vector2Int distToMine = GetDistToObject(tMine);
            Vector2Int absDistToMine = GetAbsDistToObject(tMine);
            Vector2Int distEnemyTank = GetDistToObject(tEnemy);
            Vector2Int distTeamTank = GetDistToObject(tTeam);

            List<float> inputs = new List<float>
            {
                currentTile.x,
                currentTile.y,
                tMine.x,
                tMine.y,
                distToMine.x,
                distToMine.y,
                absDistToMine.x,
                absDistToMine.y
                //turnsNotMovingAndNoMine,
                //tEnemy.x,
                //tEnemy.y,
                //tTeam.x,
                //tTeam.y,
                //distEnemyTank.x,
                //distEnemyTank.y,
                //distTeamTank.x,
                //distTeamTank.y,
                //minesEaten
            };

            float[] output = brain.Synapsis(inputs.ToArray());

            //Update tank
            Vector2Int movement = TraduceMovement(output[0]);
            //bool runAway = output[1] > 0;
            //bool thisTankEats = output[2] > 0;
            //bool tankRanAway = false;
            //
            //if (currentTile == tEnemy && currentTile == tMine && runAway)
            //{
            //    movement = currentTile - fromTile;
            //    tankRanAway = true;
            //    RunAway();
            //}
            //else //if (currentTile == tTeam && currentTile == tMine && thisTankEats)
            //{
                SetMovement(movement);
            //}

            //turnsNotMovingAndNoMine = movement == Vector2Int.zero ? turnsNotMovingAndNoMine + 1 : 0;

            //if (tankRanAway)
            //{
            //    return;
            //}

            switch (GameData.Inst.TestIndex)
            {
                case 0:
                case 1:
                    RewardIfCloserToMine();
                    RewardIfMovementDifferent(movement);
                    break;
                case 2:
                    RewardIfCloserToMine();
                    RewardIfMovementDifferent(movement);
                    PunishIfMovingBackAndForth(movement);
                    break;
                case 3:
                    RewardIfCloserToMine();
                    RewardIfMovementDifferent(movement);
                    PunishIfMovingBackAndForth(movement);
                    PunishIfFartherFromMine();
                    break;
                case 4:
                    RewardIfCloserToMine();
                    PunishIfMovingBackAndForth(movement);
                    PunishIfFartherFromMine();
                    break;
                case 5:
                    PunishIfMovingBackAndForth(movement);
                    PunishIfFartherFromMine();
                    break;
                //    //RewardIfCloserToMine();
                //    //PunishIfPasingByMineButNotEating();
                //    //    RewardIfCloseToMine();
                //    //    PunishIfNotMovingEnough();
                //    break;
                //case 2:
                //    RewardIfCloseToMine();
                //    PunishIfNotMovingEnough();
                //    PunishIfPasingByMineButNotEating();
                //    break;
                //case 3:
                //    RewardIfCloseToMine();
                //    PunishIfNotMovingEnough();
                //    PunishIfPasingByMineButNotEating();
                //    PunishIfCloseToMineManyTurnsButNotEating();
                //    break;
                default:
                    RewardIfCloserToMine();
                    PunishIfFartherFromMine();
                    //PunishIfPasingByMineButNotEating();
                    //PunishIfCloseToMineManyTurnsButNotEating();
                    //PunishIfNotMovingEnough();
                    //PunishIfPasingByMineButNotEating();
                    //PunishIfCloseToMineManyTurnsButNotEating();
                    break;
            }

            SetCloseToMine();

            if (currentTile == nearMine.Tile)
            {
                OnTakeMine(nearMine.gameObject);
            }

            movements.Add(lastMovement);
            lastLastMovement = lastMovement;
            lastMovement = movement;
        }

        protected override void OnTakeMine(GameObject mine)
        {
            base.OnTakeMine(mine);

            minesEaten++;

            switch (GameData.Inst.TestIndex)
            {
                default:
                    Debug.Log(gameObject.name + ": Rewarded by taking mine");
                    SetFitness((fitness + 30) * 2);
                    break;
            }
        }
        #endregion

        #region PRIVATE_METHODS
        private Vector2Int TraduceMovement(float movement)
        {
            if (movement > 0.6f)
            {
                return Vector2Int.up;
            }
            else if (movement > 0.2f)
            {
                return Vector2Int.right;
            }
            else if (movement > -0.2f)
            {
                return Vector2Int.down;
            }
            else if (movement > -0.6f)
            {
                return Vector2Int.left;
            }
            else
            {
                return Vector2Int.zero;                
            }
        }
        #endregion

        #region REWARD_METHODS
        private void RewardIfMovementDifferent(Vector2Int movement)
        {
            if (lastMovement != movement)
            {
                SetFitness(fitness + 2);
            }
        }

        private void RewardIfCloserToMine()
        {
            Vector2Int distToMine = GetAbsDistToObject(nearMine.Tile);
            Vector2Int lastDistToMine = GetLastAbsDistToObject(nearMine.Tile);

            if (((distToMine.x < lastDistToMine.x && distToMine.y == lastDistToMine.y) ||
                (distToMine.y < lastDistToMine.y && distToMine.x == lastDistToMine.x)) && 
                lastCloserMine == nearMine)
            {
                Debug.Log(gameObject.name + ": Rewarded by being closeR to mine");
                SetFitness(fitness + 5);
            }
        }

        private void SetCloseToMine()
        {
            if (IsCloseToMine())
            {
                lastTurnWasCloseToMine = true;
                lastCloserMine = nearMine;
                turnsCloseToMine++;
            }
            else
            {
                turnsCloseToMine = 0;
                lastTurnWasCloseToMine = false;
            }
        }
        #endregion

        #region PUNISH_METHODS
        private void PunishIfMovingBackAndForth(Vector2Int movement)
        {
            if (lastMovement != movement && movement == lastLastMovement)
            {
                SetFitness(fitness - 2);
            }
        }

        private void PunishIfFartherFromMine()
        {
            Vector2Int distToMine = GetAbsDistToObject(nearMine.Tile);
            Vector2Int lastDistToMine = GetLastAbsDistToObject(nearMine.Tile);

            if (((distToMine.x > lastDistToMine.x && distToMine.y == lastDistToMine.y) ||
                (distToMine.y > lastDistToMine.y && distToMine.x == lastDistToMine.x) || 
                lastMovement == Vector2Int.zero) && lastCloserMine == nearMine)
            {
                SetFitness(fitness - 10);
            }
        }

        private void PunishIfNotMovingEnough()
        {
            if (turnsNotMovingAndNoMine > GameData.Inst.TurnsPerGeneration / 10)
            {
                SetFitness(fitness - 9);
                Debug.Log(gameObject.name + ": Punished by not moving");
            }
        }

        private void PunishIfPasingByMineButNotEating()
        {
            if (lastTurnWasCloseToMine && !IsCloseToMine() && lastCloserMine == nearMine)
            {
                SetFitness(fitness - 10);
                Debug.Log(gameObject.name + ": Punished by pasing by mine but not eating");
            }
        }

        private void PunishIfCloseToMineManyTurnsButNotEating()
        {
            if (turnsCloseToMine > 2)
            {
                SetFitness(fitness - 10);
                Debug.Log(gameObject.name + ": Punished by being close to mine but not eating");
            }
        }
        #endregion

        #region AUX
        private bool IsCloseToMine()
        {
            Vector2Int absDistToMine = GetAbsDistToObject(nearMine.Tile);
            return absDistToMine.x < 3 && absDistToMine.y < 3;
        }

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