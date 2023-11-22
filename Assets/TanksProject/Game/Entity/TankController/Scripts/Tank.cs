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
        private int goodCalls = 0;
        private int badCalls = 0;
        private bool halfFood = false;

        //Decisions
        private bool runAway = false;
        private Vector2Int movementInCaseOfLettingOtherEat = Vector2Int.zero;
        private bool thisTankEats = false;
        #endregion

        #region PROPERTIES
        public int MinesEaten { get => minesEaten; }
        public int GoodCalls { get => goodCalls; set => goodCalls = value; }
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
            goodCalls = 0;
            badCalls = 0;
            halfFood = false;

            runAway = false;
            movementInCaseOfLettingOtherEat = Vector2Int.zero;
        }

        protected override void OnThink()
        {
            Vector2Int tMine = GetMineTile(nearMine);
            Vector2Int tEnemy = GetTankTile(nearEnemyTank);
            Vector2Int tTeam = GetTankTile(nearTeamTank);

            int minesEatenByCloserTeammate = 0;
            if (nearTeamTank != null)
            {
                minesEatenByCloserTeammate = nearTeamTank.MinesEaten;
            }

            Vector2Int distToMine = GetDistToObject(tMine);
            Vector2Int absDistToMine = GetAbsDistToObject(tMine);
            Vector2Int distEnemyTank = GetDistToObject(tEnemy);
            Vector2Int distTeamTank = GetDistToObject(tTeam);

            Vector2Int absDistToEnemyTank = GetAbsDistToObject(tEnemy);
            Vector2Int absDistToTeamTank = GetAbsDistToObject(tTeam);
            bool nextToEnemy = nearEnemyTank != null && absDistToEnemyTank.x <= 1 && absDistToEnemyTank.y <= 1;
            bool nextToTeam = nearTeamTank != null && absDistToTeamTank.x <= 1 && absDistToTeamTank.y <= 1;
            int nextTo = nextToEnemy ? -1 : (nextToTeam ? 1 : 0);

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
                minesEatenByCloserTeammate,
                nextTo,
                lastMovement.x,
                lastMovement.y//21
            };

            float[] output = brain.Synapsis(inputs.ToArray());

            //Update tank
            Vector2Int movement = TraduceMovement(output[0]);
            Vector2Int lastTile = currentTile;
            runAway = output[1] > 0.5f;
            thisTankEats = output[2] > 0.5f;
            movementInCaseOfLettingOtherEat = TraduceMovementToAdjacent(output[3]);

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
        public void TakeHalfFood()
        {
            if (halfFood)
            {
                minesEaten++;
                lessTurnsTakenUntilMine = turnsNoMine < lessTurnsTakenUntilMine ? turnsNoMine : lessTurnsTakenUntilMine;
                turnsNoMine = 0;

                halfFood = false;
            }
            else
            {
                halfFood = true;
            }

            nearMine = null;
        }

        public bool ChooseWhetherToFlee()
        {
            if (runAway)
            {
                if (IsValid(nearMine) && currentTile == nearMine.Tile && minesEaten == 0)
                {
                    badCalls++;
                }

                RunAway();
            }

            if (minesEaten >= 2)
            {
                if (runAway)
                {
                    goodCalls++;
                }
                else
                {
                    badCalls++;
                }               
            }

            return runAway;
        }

        public bool ChooseWhetherToEat()
        {
            if (thisTankEats)
            {
                if (minesEaten > nearTeamTank.minesEaten)
                {
                    badCalls++;
                }
                else
                {
                    goodCalls++;
                }
            }
            else
            {
                if (minesEaten < nearTeamTank.minesEaten)
                {
                    badCalls++;
                }
                else if (!(minesEaten == 0 && nearTeamTank.minesEaten == 0))
                {
                    goodCalls++;
                }

                SetMovement(movementInCaseOfLettingOtherEat);
            }

            return thisTankEats;
        }

        public void OnTurnUpdated()
        {
            Vector2Int tMine = GetMineTile(nearMine);
            Vector2Int tEnemy = GetTankTile(nearEnemyTank);
            Vector2Int tTeam = GetTankTile(nearTeamTank);

            bool mineIsValid = IsValid(nearMine);

            if ((currentTile == tEnemy) || 
                (currentTile == tTeam && currentTile == tMine && mineIsValid))
            {
                return; //decision to be made
            }

            if (mineIsValid)
            {
                TrackIfCloserToMine(currentTile - lastMovement);
                TrackIfFartherFromMine(currentTile - lastMovement);
                TrackPenalties();

                if (currentTile == tMine)
                {
                    OnTakeMine(nearMine.gameObject);
                }
                else
                {
                    turnsNoMine++;
                }                
            }
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
                    SetFitness(FitnessByCachedMines());
                    break;
                case 3:
                    SetFitness(FitnessByCachedMines() +
                               FitnessByTurnMovingTowardsMines());
                    break;
                case 4:
                    SetFitness(FitnessByCachedMines() +
                               FitnessByTurnMovingTowardsMines() +
                               FitnessByPenalties());
                    break;
                case 5:
                    SetFitness(FitnessByCachedMines() +
                               FitnessByTurnMovingTowardsMines() +
                               FitnessByLessTurnsTakenUntilMine() +
                               FitnessByPenalties());
                    break;
                case 6:
                    SetFitness(FitnessByCachedMines() +
                               FitnessByTurnMovingTowardsMines() +
                               FitnessByLessTurnsTakenUntilMine() +
                               FitnessByGoodAndBadCalls() +
                               FitnessByPenalties());
                    break;
                default:
                    SetFitness(FitnessByCachedMines() +
                               FitnessByLessTurnsTakenUntilMine() +
                               FitnessByGoodAndBadCalls());
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
        }

        private Vector2Int TraduceMovementToAdjacent(float movement)
        {
            if (movement > 0.75f)
            {
                return Vector2Int.up;
            }
            else if (movement > 0.5f)
            {
                return Vector2Int.right;
            }
            else if (movement > 0.25f)
            {
                return Vector2Int.down;
            }
            else
            {
                return Vector2Int.left;
            }
        }
        #endregion

        #region FITNESS_METHODS
        private int FitnessByCachedMines()
        {
            return minesEaten * 1500;
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
            return (turnGettingCloserToMine - turnGettingFarFromMine) * 5;
        }

        private int FitnessByPenalties()
        {
            return -penalties * 150;
        }

        private int FitnessByGoodAndBadCalls()
        {
            return (goodCalls - badCalls) * 250;
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
                (distToMine.y > lastDistToMine.y && distToMine.x == lastDistToMine.x)) && 
                lastNearMine == nearMine)
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

        private Vector2Int GetTankTile(Tank tank)
        {
            if (IsValid(tank))
            {
                return tank.Tile;
            }

            return Vector2Int.zero;
        }

        private Vector2Int GetMineTile(Mine mine)
        {
            if (IsValid(mine))
            {
                return mine.Tile;
            }

            return Vector2Int.zero;
        }

        private bool IsValid(MonoBehaviour obj)
        {
            return obj != null && obj.gameObject.activeInHierarchy;
        }
        #endregion
    }
}