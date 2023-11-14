using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using TanksProject.Game.Data;
using TanksProject.Game.Entity.MineController;

namespace TanksProject.Game.Entity.TankController
{
    public class Tank : TankBase
    {
        #region PRIVATE_METHODS
        private int minesEaten = 0;
        private int penalties = 0;
        private int turnsNoMine = 0;
        private int lessTurnsTakenUntilMine = 0;
        private int turnGettingCloserToMine = 0;
        private int turnGettingFarFromMine = 0;
        private Mine lastNearMine = null;
        private Vector2Int lastMovement = Vector2Int.zero;
        private List<Vector2Int> movements = new List<Vector2Int>();

        //Decisions
        private bool runAway = false;
        #endregion

        #region PROPERTIES
        public int MinesEaten { get => minesEaten; }
        #endregion

        #region OVERRIDE
        public override void OnReset()
        {
            base.OnReset();
            minesEaten = 0;
            penalties = 0;
            turnsNoMine = 0;
            lessTurnsTakenUntilMine = GameData.Inst.TurnsPerGeneration;
            turnGettingCloserToMine = 0;
            turnGettingFarFromMine = 0;
            lastNearMine = null;
            lastMovement = Vector2Int.zero;
            movements.Clear();

            runAway = false;
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
                absDistToMine.y,
                tEnemy.x,
                tEnemy.y,
                tTeam.x,
                tTeam.y,
                distEnemyTank.x,
                distEnemyTank.y,
                distTeamTank.x,
                distTeamTank.y,
                minesEaten,
                nearTeamTank.MinesEaten//18
            };

            float[] output = brain.Synapsis(inputs.ToArray());

            //Update tank
            Vector2Int movement = TraduceMovement(output[0]);
            Vector2Int lastTile = currentTile;
            runAway = output[1] > 0.5f;
            //bool thisTankEats = output[2] > 0;

            SetMovement(movement);

            lastMovement = currentTile - lastTile;
            movements.Add(lastMovement);
            lastNearMine = nearMine;
        }

        protected override void OnTakeMine(GameObject mine)
        {
            base.OnTakeMine(mine);

            minesEaten++;
            lessTurnsTakenUntilMine = turnsNoMine < lessTurnsTakenUntilMine ? turnsNoMine : lessTurnsTakenUntilMine;
            turnsNoMine = 0;
        }
        #endregion

        #region PUBLIC_METHODS
        public bool ChooseWhetherToFlee()
        {
            if (runAway)
            {
                RunAway();
            }

            return runAway;
        }

        public void TrackFitness()
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
                default:
                    SetFitness(FitnessByCachedMines() -
                               FitnessByPenalties());
                    break;
            }
        }

        public void OnTurnUpdated()
        {
            Vector2Int tMine = Vector2Int.zero;
            if (nearMine != null)
            {
                tMine = nearMine.Tile;
            }
            Vector2Int tEnemy = nearEnemyTank.Tile;
            Vector2Int tTeam = nearTeamTank.Tile;

            bool choiceToBeMade = false;

            if (currentTile == tEnemy && currentTile == tMine)
            {
                choiceToBeMade = true;
            }
            //else //if (currentTile == tTeam && currentTile == tMine && thisTankEats)
            //{
            //    //SetMovement(movement);
            //}

            if (choiceToBeMade)
            {
                return;
            }

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

                TrackIfCloserToMine(currentTile - lastMovement);
                TrackIfFartherFromMine(currentTile - lastMovement);
                TrackPenalties();
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
        private void TrackIfCloserToMine(Vector2Int prevPos)
        {
            Vector2Int distToMine = GetAbsDistToObject(nearMine.Tile);
            Vector2Int lastDistToMine = GetLastAbsDistToObject(nearMine.Tile, prevPos);

            if (((distToMine.x < lastDistToMine.x && distToMine.y == lastDistToMine.y) ||
                (distToMine.y < lastDistToMine.y && distToMine.x == lastDistToMine.x)) && 
                lastNearMine == nearMine)
            {
                turnGettingCloserToMine++;
            }
        }
        #endregion

        #region PUNISH_METHODS
        private void TrackIfFartherFromMine(Vector2Int prevPos)
        {
            Vector2Int distToMine = GetAbsDistToObject(nearMine.Tile);
            Vector2Int lastDistToMine = GetLastAbsDistToObject(nearMine.Tile, prevPos);
        
            if (((distToMine.x > lastDistToMine.x && distToMine.y == lastDistToMine.y) ||
                (distToMine.y > lastDistToMine.y && distToMine.x == lastDistToMine.x) || 
                lastMovement == Vector2Int.zero) && lastNearMine == nearMine)
            {
                turnGettingFarFromMine++;
            }
        }
        
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
        private void SetFitness(float fitness)
        {
            genome.fitness = fitness;
        }
        #endregion
    }
}