using System.Collections.Generic;
using System.Linq;
using TanksProject.Game.Data;
using TanksProject.Game.Entity.MineController;
using UnityEngine;

namespace TanksProject.Game.Entity.TankController
{
    public class Tank : TankBase
    {
        #region PRIVATE_METHODS
        private float fitness = 0;
        private int minesEaten = 0;
        private int penalties = 0;
        //private int turnsCloseToMine = 0;
        private int turnsNoMine = 0;
        private int lessTurnsTakenUntilMine = 0;
        private int turnGettingCloserToMine = 0;
        private int turnGettingFarFromMine = 0;
        //private bool lastTurnWasCloseToMine = false;
        private Mine lastNearMine = null;
        private Vector2Int lastMovement = Vector2Int.zero;
        //private Vector2Int lastLastMovement = Vector2Int.zero;
        private List<Vector2Int> movements = new List<Vector2Int>();
        #endregion

        #region PROPERTIES
        public int MinesEaten { get => minesEaten; }
        //public List<Vector2Int> Movements { get => movements; }
        #endregion

        #region OVERRIDE
        public override void OnReset()
        {
            base.OnReset();
            fitness = 0;
            minesEaten = 0;
            penalties = 0;
            turnsNoMine = 0;
            lessTurnsTakenUntilMine = GameData.Inst.TurnsPerGeneration;
            turnGettingCloserToMine = 0;
            turnGettingFarFromMine = 0;
            lastNearMine = null;
            lastMovement = Vector2Int.zero;
            //turnsCloseToMine = 0;
            //lastTurnWasCloseToMine = false;
            movements.Clear();
            //lastLastMovement = Vector2Int.zero;
        }

        protected override void OnThink()
        {
            Vector2Int tMine = Vector2Int.zero;
            if (nearMine != null)
            {
                tMine = nearMine.Tile;
            }

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
            Vector2Int lastTile = currentTile;
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

            //switch (GameData.Inst.TestIndex)
            //{
            //    //case 0:
            //    //case 1:
            //    //    RewardIfCloserToMine(lastTile);
            //    //    RewardIfMovementDifferent(movement);
            //    //    break;
            //    //case 2:
            //    //    RewardIfCloserToMine(lastTile);
            //    //    RewardIfMovementDifferent(movement);
            //    //    PunishIfMovingBackAndForth(movement);
            //    //    break;
            //    //case 3:
            //    //    RewardIfCloserToMine(lastTile);
            //    //    RewardIfMovementDifferent(movement);
            //    //    PunishIfMovingBackAndForth(movement);
            //    //    PunishIfFartherFromMine(lastTile);
            //    //    break;
            //    //case 4:
            //    //    RewardIfCloserToMine(lastTile);
            //    //    PunishIfMovingBackAndForth(movement);
            //    //    PunishIfFartherFromMine(lastTile);
            //    //    break;
            //    //case 5:
            //    //    PunishIfMovingBackAndForth(movement);
            //    //    PunishIfFartherFromMine(lastTile);
            //    //    break;
            //    //    //RewardIfCloserToMine();
            //    //    //PunishIfPasingByMineButNotEating();
            //    //    //    RewardIfCloseToMine();
            //    //    //    PunishIfNotMovingEnough();
            //    //    break;
            //    //case 2:
            //    //    RewardIfCloseToMine();
            //    //    PunishIfNotMovingEnough();
            //    //    PunishIfPasingByMineButNotEating();
            //    //    break;
            //    //case 3:
            //    //    RewardIfCloseToMine();
            //    //    PunishIfNotMovingEnough();
            //    //    PunishIfPasingByMineButNotEating();
            //    //    PunishIfCloseToMineManyTurnsButNotEating();
            //    //    break;
            //    default:
            //        RewardIfCloserToMine(movement);
            //        PunishIfFartherFromMine(lastTile);
            //        //PunishIfPasingByMineButNotEating();
            //        //PunishIfCloseToMineManyTurnsButNotEating();
            //        //PunishIfNotMovingEnough();
            //        //PunishIfPasingByMineButNotEating();
            //        //PunishIfCloseToMineManyTurnsButNotEating();
            //        break;
            //}

            //SetCloseToMine();

            if (nearMine != null)
            {
                if (currentTile == tMine)
                {
                    OnTakeMine(nearMine.gameObject);
                }
                else
                {
                    turnsNoMine++;
                }

                RewardIfCloserToMine(lastTile);
                PunishIfFartherFromMine(lastTile);
                TrackPenalties();
            }

            lastMovement = currentTile - lastTile;
            movements.Add(lastMovement);
            //lastLastMovement = lastMovement;
            lastNearMine = nearMine;
        }

        protected override void OnTakeMine(GameObject mine)
        {
            base.OnTakeMine(mine);

            minesEaten++;
            lessTurnsTakenUntilMine = turnsNoMine < lessTurnsTakenUntilMine ? turnsNoMine : lessTurnsTakenUntilMine;

            //switch (GameData.Inst.TestIndex)
            //{
            //    default:
            //        Debug.Log(gameObject.name + ": Rewarded by taking mine");
            //        SetFitness((fitness + 30) * 2);
            //        break;
            //}

            turnsNoMine = 0;
        }
        #endregion

        #region PUBLIC_METHODS
        public void OnTurnEnd()
        {
            switch (GameData.Inst.TestIndex)
            {
                case 0:
                    SetFitness(FitnessByDiverseMovement());
                    break;
                case 1:
                    SetFitness(FitnessByCachedMines() +
                               FitnessByDiverseMovement());
                    break;
                case 2:
                    SetFitness(FitnessByCachedMines() +
                               FitnessByTurnMovingTowardsMines());
                    break;
                case 3:
                    SetFitness(FitnessByCachedMines() +
                               FitnessByTurnMovingTowardsMines() -
                               FitnessByPenalties());
                    break;
                case 4:
                    SetFitness(FitnessByCachedMines() +
                               FitnessByTurnMovingTowardsMines() +
                               FitnessByLessTurnsTakenUntilMine() -
                               FitnessByPenalties());
                    break;
                case 5:
                    SetFitness(FitnessByCachedMines() -
                               FitnessByPenalties());
                    break;
                    //case 1:
                    //    SetFitness(FitnessByCachedMines() +
                    //               FitnessByGoingLeftRight());
                    //    break;
                    //case 2:
                    //    SetFitness(FitnessByCachedMines() +
                    //               FitnessByGoingLeftRight() +
                    //               FitnessByLessTurnsTakenUntilMine());
                    //    break;
                    //case 3:
                    //    SetFitness(FitnessByCachedMines() +
                    //               FitnessByGoingLeftRight() +
                    //               FitnessByLessTurnsTakenUntilMine() +
                    //               FitnessByTurnMovingTowardsMines());
                    //    break;
                    //case 4:
                    //    SetFitness(FitnessByCachedMines() +
                    //               FitnessByGoingLeftRight() +
                    //               FitnessByLessTurnsTakenUntilMine() +
                    //               FitnessByTurnMovingTowardsMines() -
                    //               FitnessByPenalties());
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region PRIVATE_METHODS
        private Vector2Int TraduceMovement(float movement)
        {
            if (movement > 0.8f)
            {
                return Vector2Int.up;
            }
            else if (movement > 0.6f)
            {
                return Vector2Int.right;
            }
            else if (movement > 0.4f)
            {
                return Vector2Int.down;
            }
            else if (movement > 0.2f)
            {
                return Vector2Int.left;
            }
            else
            {
                return Vector2Int.zero;
            }
            //if (movement > 0.6f)
            //{
            //    return Vector2Int.up;
            //}
            //else if (movement > 0.2f)
            //{
            //    return Vector2Int.right;
            //}
            //else if (movement > -0.2f)
            //{
            //    return Vector2Int.down;
            //}
            //else if (movement > -0.6f)
            //{
            //    return Vector2Int.left;
            //}
            //else
            //{
            //    return Vector2Int.zero;                
            //}
        }
        #endregion

        #region FITNESS_METHODS
        private int FitnessByCachedMines()
        {
            return minesEaten * 1000;
        }

        private int FitnessByDiverseMovement()
        {
            return movements.FindAll(m => m == Vector2Int.left || m == Vector2Int.right ||
                                          m == Vector2Int.up || m == Vector2Int.down).Distinct().ToList().Count * 100;
        }

        private int FitnessByLessTurnsTakenUntilMine()
        {
            return GameData.Inst.TurnsPerGeneration / (lessTurnsTakenUntilMine == 0 ? 1 : lessTurnsTakenUntilMine) * 10;
        }

        private int FitnessByTurnMovingTowardsMines()
        {
            return (turnGettingCloserToMine - turnGettingFarFromMine) * 10;
        }

        private int FitnessByPenalties()
        {
            return penalties * 300;
        }
        #endregion

        #region REWARD_METHODS
        //private void RewardIfMovementDifferent(Vector2Int movement)
        //{
        //    if (lastMovement != movement)
        //    {
        //        SetFitness(fitness + 2);
        //    }
        //}

        private void RewardIfCloserToMine(Vector2Int prevPos)
        {
            Vector2Int distToMine = GetAbsDistToObject(nearMine.Tile);
            Vector2Int lastDistToMine = GetLastAbsDistToObject(nearMine.Tile, prevPos);

            if (((distToMine.x < lastDistToMine.x && distToMine.y == lastDistToMine.y) ||
                (distToMine.y < lastDistToMine.y && distToMine.x == lastDistToMine.x)) && 
                lastNearMine == nearMine)
            {
                //Debug.Log("Rewarded by being closer to mine");
                turnGettingCloserToMine++;
                //SetFitness(fitness + 5);
            }
        }

        //private void SetCloseToMine()
        //{
        //    if (IsCloseToMine())
        //    {
        //        lastTurnWasCloseToMine = true;                
        //        turnsCloseToMine++;
        //    }
        //    else
        //    {
        //        //turnsCloseToMine = 0;
        //        lastTurnWasCloseToMine = false;
        //    }
        //
        //    lastNearMine = nearMine;
        //}
        #endregion

        #region PUNISH_METHODS
        //private void PunishIfMovingBackAndForth(Vector2Int movement)
        //{
        //    if (lastMovement != movement && movement == lastLastMovement)
        //    {
        //        SetFitness(fitness - 2);
        //    }
        //}

        private void PunishIfFartherFromMine(Vector2Int prevPos)
        {
            Vector2Int distToMine = GetAbsDistToObject(nearMine.Tile);
            Vector2Int lastDistToMine = GetLastAbsDistToObject(nearMine.Tile, prevPos);
        
            if (((distToMine.x > lastDistToMine.x && distToMine.y == lastDistToMine.y) ||
                (distToMine.y > lastDistToMine.y && distToMine.x == lastDistToMine.x) || 
                lastMovement == Vector2Int.zero) && lastNearMine == nearMine)
            {
                turnGettingFarFromMine++;
                //SetFitness(fitness - 3);
            }
        }
        //
        //private void PunishIfPasingByMineButNotEating()
        //{
        //    if (lastTurnWasCloseToMine && !IsCloseToMine() && lastCloserMine == nearMine)
        //    {
        //        SetFitness(fitness - 10);
        //        Debug.Log(gameObject.name + ": Punished by pasing by mine but not eating");
        //    }
        //}
        //
        //private void PunishIfCloseToMineManyTurnsButNotEating()
        //{
        //    if (turnsCloseToMine > 2)
        //    {
        //        SetFitness(fitness - 10);
        //        Debug.Log(gameObject.name + ": Punished by being close to mine but not eating");
        //    }
        //}

        private void TrackPenalties()
        {
            if (turnsNoMine > 50)
            {
                penalties++;
                turnsNoMine = 0;
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
            //if (fitness < 0)
            //{
            //    fitness = 0;
            //}

            this.fitness = fitness;
            genome.fitness = fitness;
        }
        #endregion
    }
}