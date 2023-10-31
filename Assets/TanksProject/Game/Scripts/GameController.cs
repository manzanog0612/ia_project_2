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

        private bool simulationOn = false;
        #endregion

        #region UNITY_CALLS
        private void Start()
        {
            Init();
        }

        private void FixedUpdate()
        {
            if (!simulationOn)
            {
                return;
            }

            GameData.Inst.UpdateTime(Time.fixedDeltaTime);

            if (GameData.Inst.GenerationFinished)
            {
                Epoch();
            }
            else if (GameData.Inst.TurnFinished)
            {
                ChangeTurn();
            }
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
            simulationScreen.Init(PauseSimulation, StopSimulation, SaveSimulation);
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

        private void SaveSimulation()
        {
            ConfigurationData config = new();
            config.population_count = GameData.Inst.PopulationCount;
            config.turnsPerGeneration = GameData.Inst.TurnsPerGeneration;
            config.turnDuration = GameData.Inst.TurnDuration;
            config.mutation_chance = GameData.Inst.MutationChance;
            config.mutation_rate = GameData.Inst.MutationRate;
            config.hidden_layers_count = GameData.Inst.HiddenLayers;
            config.neurons_per_hidden_layers = GameData.Inst.NeuronsCountPerHL;
            config.elites_count = GameData.Inst.EliteCount;
            config.bias = -GameData.Inst.Bias;
            config.sigmoid = GameData.Inst.P;
            SimData simData = new SimData();
            simData.config = config;
            simData.teamsData = new TeamData[populationManagers.Count];

            for (int i = 0; i < Enum.GetValues(typeof(TEAM)).Length; i++)
            {
                simData.teamsData[i] = populationManagers[(TEAM)i].GetCurrentTeamData();
            }

            Utilities.SaveLoadSystem.SaveConfig(simData);
        }

        private void StartSimulation()
        {
            for (int i = 0; i < Enum.GetValues(typeof(TEAM)).Length; i++)
            {
                populationManagers[(TEAM)i].StartSimulation();
            }

            minesManager.CreateMines();

            GameData.Inst.Reset();
            simulationOn = true;
        }

        private void StartLoadedSimulation(SimData simData)
        {
            for (int i = 0; i < Enum.GetValues(typeof(TEAM)).Length; i++)
            {
                populationManagers[(TEAM)i].StartLoadedSimulation(simData);
            }

            minesManager.CreateMines();

            simulationOn = true;
        }

        private void StopSimulation()
        {
            for (int i = 0; i < Enum.GetValues(typeof(TEAM)).Length; i++)
            {
                populationManagers[(TEAM)i].StopSimulation();
            }

            minesManager.DestroyMines();

            GameData.Inst.Reset();
            simulationOn = false;
        }

        private void PauseSimulation()
        {
            simulationOn = false;
        }

        private void Epoch()
        {
            for (int i = 0; i < Enum.GetValues(typeof(TEAM)).Length; i++)
            {
                populationManagers[(TEAM)i].Epoch();
            }

            minesManager.CreateMines();
        }

        private void ChangeTurn()
        {
            for (int i = 0; i < Enum.GetValues(typeof(TEAM)).Length; i++)
            {
                populationManagers[(TEAM)i].ChangeTurn();
            }
        }
        #endregion
    }
}