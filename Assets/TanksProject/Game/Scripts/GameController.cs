using System;
using System.Collections.Generic;

using UnityEngine;

using TanksProject.Game.Entity.MinesController;
using TanksProject.Game.Entity.PopulationController;
using TanksProject.Game.Data;
using TanksProject.Game.UI;

using data;

namespace TanksProject.Game
{
    public class GameController : MonoBehaviour
    {
        #region EXPOSED_FIELDS
        [SerializeField] private Common.Grid.Grid grid = null;
        [SerializeField] private MinesManager minesManager = null;

        [Header("UI Controllers")]
        [SerializeField] private StartConfigurationScreen startConfigurationScreen = null;
        [SerializeField] private SimulationScreen simulationScreen = null;
        #endregion

        #region PRIVATE_FIELDS
        private Dictionary<TEAM, PopulationManager> populationManagers = new Dictionary<TEAM, PopulationManager>();
        #endregion

        #region UNITY_CALLS
        private void Start()
        {
            Init();
        }

        private void FixedUpdate()
        {
            GameData.Inst.UpdateTime(Time.fixedDeltaTime);
        }
        #endregion

        #region PRIVATE_METHODS
        public void Init()
        {
            minesManager.Init(grid);

            for (int i = 0; i < Enum.GetValues(typeof(TEAM)).Length; i++)
            {
                populationManagers.Add((TEAM)i, GameData.Inst.PM((TEAM)i));
                populationManagers[(TEAM)i].Init(GetInitialPositions((TEAM)i), grid, minesManager.GetNearestMine);
            }

            grid.Init();
            Camera.main.orthographicSize = grid.Width / 2 + 5;
            Camera.main.transform.position = new Vector3(grid.Width / 2, 10, grid.Height / 2 + 5);

            startConfigurationScreen.Init(StartSimulation, StartLoadedSimulation);
            simulationScreen.Init(PauseSimulation, StopSimulation);
        }
        #endregion

        #region PRIVATE_METHODS
        private Vector2Int[] GetInitialPositions(TEAM team)
        {
            Vector2Int[] gridPos = new Vector2Int[GameData.Inst.PopulationCount];

            for (int i = 0; i < GameData.Inst.PopulationCount; i++)
            {
                int x = team == TEAM.RED ? i : grid.Width - i - 1;
                int y = team == TEAM.RED ? 0 : grid.Height - 1;
                gridPos[i] = new Vector2Int(x, y);
            }

            return gridPos;
        }

        private void StartSimulation()
        {
            for (int i = 0; i < Enum.GetValues(typeof(TEAM)).Length; i++)
            {
                populationManagers[(TEAM)i].StartSimulation();
            }

            minesManager.CreateMines();
        }

        private void StartLoadedSimulation(SimData simData)
        {
            for (int i = 0; i < Enum.GetValues(typeof(TEAM)).Length; i++)
            {
                populationManagers[(TEAM)i].StartLoadedSimulation(simData);
            }

            minesManager.CreateMines();
        }

        private void StopSimulation()
        {
            for (int i = 0; i < Enum.GetValues(typeof(TEAM)).Length; i++)
            {
                populationManagers[(TEAM)i].StopSimulation();
            }

            minesManager.DestroyMines();
        }

        private void PauseSimulation()
        {
            for (int i = 0; i < Enum.GetValues(typeof(TEAM)).Length; i++)
            {
                populationManagers[(TEAM)i].PauseSimulation();
            }
        }
        #endregion
    }
}