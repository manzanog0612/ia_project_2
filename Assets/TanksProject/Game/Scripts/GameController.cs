using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using TanksProject.Game.Entity.MinesController;
using TanksProject.Game.Entity.PopulationController;
using TanksProject.Game.Entity.TankController;
using TanksProject.Game.Data;
using TanksProject.Game.UI;
using TanksProject.Common.Saving;

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
        private int teamsAmount = Enum.GetValues(typeof(TEAM)).Length;

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

            for (int i = 0; i < teamsAmount; i++)
            {
                populationManagers.Add((TEAM)i, GameData.Inst.PM((TEAM)i));
            }

            for (int i = 0; i < teamsAmount; i++)
            {
                TEAM currentTeam = (TEAM)i;
                TEAM contraryTeam = GetContraryTeam(currentTeam);
                populationManagers[currentTeam].Init(GetInitialPositions(currentTeam), grid, minesManager.GetNearestMine, populationManagers[contraryTeam].GetNearestTank, minesManager.OnTakeMine);
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
            Vector2Int[] gridPos = new Vector2Int[grid.Width];

            for (int i = 0; i < grid.Width; i++)
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
            config.bias = GameData.Inst.Bias;
            config.sigmoid = GameData.Inst.P;
            SimData simData = new SimData();
            simData.config = config;
            simData.teamsData = new TeamData[populationManagers.Count];

            for (int i = 0; i < teamsAmount; i++)
            {
                simData.teamsData[i] = populationManagers[(TEAM)i].GetCurrentTeamData();
            }

            SaveLoadSystem.SaveConfig(simData);
        }

        private void StartSimulation()
        {
            for (int i = 0; i < teamsAmount; i++)
            {
                populationManagers[(TEAM)i].StartSimulation();
            }

            minesManager.CreateMines();

            GameData.Inst.Reset();
            simulationOn = true;
        }

        private void StartLoadedSimulation(SimData simData)
        {
            for (int i = 0; i < teamsAmount; i++)
            {
                populationManagers[(TEAM)i].StartLoadedSimulation(simData);
            }

            minesManager.CreateMines();

            simulationOn = true;
        }

        private void StopSimulation()
        {
            for (int i = 0; i < teamsAmount; i++)
            {
                populationManagers[(TEAM)i].ResetSimulation();
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
            if ((populationManagers[TEAM.RED].Generation != 0 && 
                populationManagers[TEAM.RED].Generation % GameData.Inst.MinGenerationToStartHungerGames == 0) ||
                populationManagers[TEAM.BLUE].Generation != 0 &&
                populationManagers[TEAM.BLUE].Generation % GameData.Inst.MinGenerationToStartHungerGames == 0)
            {
                SaveSimulation();
            }

            if (populationManagers[TEAM.RED].Generation > GameData.Inst.MinGenerationToStartHungerGames || 
                populationManagers[TEAM.BLUE].Generation > GameData.Inst.MinGenerationToStartHungerGames)
            { 
                SetStateByMinesEaten(); 
            }

            List<TEAM> deadTeams = new List<TEAM>();

            for (int i = 0; i < teamsAmount; i++)
            {
                TEAM team = (TEAM)i;
                populationManagers[team].Epoch();
                
                if (populationManagers[team].Dead)
                {
                    deadTeams.Add(team);
                }
            }

            if (deadTeams.Count > 0)
            {
                if (deadTeams.Count == 1)
                {
                    TEAM deadTeam = deadTeams.First();
                    Genome[] contraryTeamRandomGenomes = populationManagers[GetContraryTeam(deadTeam)].GetRandomCrossoverPopulation();

                    populationManagers[deadTeam].StartNewSimulation(contraryTeamRandomGenomes);

                    Debug.Log(deadTeam.ToString() + " team dead");
                }
                else
                {
                    StopSimulation();
                    StartSimulation();

                    Debug.Log("Both teams dead");
                }

                return;
            }

            Debug.Log("neither teams dead");

            minesManager.CreateMines();
        }

        private void ChangeTurn()
        {
            for (int i = 0; i < teamsAmount; i++)
            {
                populationManagers[(TEAM)i].ChangeTurn();
            }

            //HandleEnemiesInSameTile();
        }

        #region TURN_DYNAMICS
        private void HandleEnemiesInSameTile()
        {
            for (int i = 0; i < populationManagers[TEAM.RED].Tanks.Count; i++)
            {
                for (int j = 0; j < populationManagers[TEAM.BLUE].Tanks.Count; j++)
                {
                    Tank redTank = populationManagers[TEAM.RED].Tanks[i];
                    Tank blueTank = populationManagers[TEAM.BLUE].Tanks[j];
        
                    if (redTank.Tile == blueTank.Tile /*&& algo mas falta aca como preguntar si esa casilla tiene comida */)
                    {
                        TEAM dyingTank = (TEAM)UnityEngine.Random.Range(0, teamsAmount);
        
                        if (dyingTank == TEAM.RED)
                        {
                            redTank.SetState(STATE.DIE);
                            blueTank.TakeMine();
                        }
                        else
                        {
                            blueTank.SetState(STATE.DIE);
                            redTank.TakeMine();
                        }
                    }
                }
            }
        }

        private void SetStateByMinesEaten()
        {
            for (int i = 0; i < teamsAmount; i++)
            {
                populationManagers[(TEAM)i].SetStateByMinesEaten();
            }
        }

        private TEAM GetContraryTeam(TEAM contraryTo)
        {
            return contraryTo == TEAM.BLUE ? TEAM.RED : TEAM.BLUE;
        }
        #endregion
        #endregion
    }
}