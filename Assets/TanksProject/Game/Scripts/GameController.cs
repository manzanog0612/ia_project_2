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

        [Header("Default Generations")]
        [SerializeField] private TextAsset startGenerations = null;
        #endregion

        #region PRIVATE_FIELDS
        private Dictionary<TEAM, PopulationManager> populationManagers = new Dictionary<TEAM, PopulationManager>();
        private int teamsAmount = Enum.GetValues(typeof(TEAM)).Length;

        private bool simulationOn = false;
        private float maxAvg = 0;
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

            startConfigurationScreen.Init(StartSimulation, StartDefaultSimulation, StartLoadedSimulation);
            simulationScreen.Init(PauseSimulation, StopSimulation, SaveSimulation);
        }
        #endregion

        #region PRIVATE_METHODS
        #region SIMULATION_MANAGEMENT
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
            config.learning = GameData.Inst.Learning;
            config.minesOnCenter = GameData.Inst.MinesOnCenter;
            config.testIndex = GameData.Inst.TestIndex;
            config.minesMultiplier = GameData.Inst.MinesMultiplier;
            SimData simData = new SimData();
            simData.maxAvgFitness = maxAvg;
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

        private void StartDefaultSimulation()
        {
            StartLoadedSimulation(JsonUtility.FromJson<SimData>(startGenerations.text));
        }

        private void StartLoadedSimulation(SimData simData)
        {
            maxAvg = simData.maxAvgFitness;
            GameData.Inst.Learning = simData.config.learning;
            GameData.Inst.MinesOnCenter = simData.config.minesOnCenter;
            GameData.Inst.TestIndex = simData.config.testIndex;
            GameData.Inst.MinesMultiplier = simData.config.minesMultiplier;

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

        private void UpdateSimulationData()
        {
            float redTeamAvg = populationManagers[TEAM.RED].AvgFitness;
            float blueTeamAvg = populationManagers[TEAM.BLUE].AvgFitness;

            if (redTeamAvg > maxAvg || blueTeamAvg > maxAvg)
            {
                maxAvg = redTeamAvg > blueTeamAvg ? redTeamAvg : blueTeamAvg;
                SaveSimulation();
            }

            if (GameData.Inst.FitnessTillNewTest.Length > GameData.Inst.TestIndex &&
                Mathf.Max(redTeamAvg, blueTeamAvg) > GameData.Inst.FitnessTillNewTest[GameData.Inst.TestIndex])
            {
                GameData.Inst.TestIndex++;
            }
        }

        private void Epoch()
        {
            if (!GameData.Inst.Learning)
            {
                SetStateByMinesEaten();
            }

            List<TEAM> deadTeams = new List<TEAM>();

            minesManager.CreateMines();

            for (int i = 0; i < teamsAmount; i++)
            {
                TEAM team = (TEAM)i;
                populationManagers[team].Epoch();

                if (populationManagers[team].Dead)
                {
                    deadTeams.Add(team);
                }
            }

            UpdateSimulationData();

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
                    //StartDefaultSimulation();

                    Debug.Log("Both teams dead");
                }

                return;
            }

            //Debug.Log("neither teams dead");
        }

        private void ChangeTurn()
        {
            for (int i = 0; i < teamsAmount; i++)
            {
                populationManagers[(TEAM)i].ChangeTurn();
            }

            for (int i = 0; i < teamsAmount; i++)
            {
                populationManagers[(TEAM)i].OnTurnUpdated();
            }

            for (int i = 0; i < teamsAmount; i++)
            {
                populationManagers[(TEAM)i].HandleTanksOnSameTile();
            }

            HandleEnemiesInSameTile();

            for (int i = 0; i < teamsAmount; i++)
            {
                populationManagers[(TEAM)i].TrackFitness();
            }
        }
        #endregion

        #region TURN_DYNAMICS
        private void HandleEnemiesInSameTile()
        {
            for (int i = 0; i < populationManagers[TEAM.RED].Tanks.Count; i++)
            {
                Tank redTank = populationManagers[TEAM.RED].Tanks[i];
                Tank blueTank = populationManagers[TEAM.RED].Tanks[i].NearEnemyTank;

                if (redTank.Tile != blueTank.Tile)
                {
                    continue;
                }

                bool sameTileAsMine = minesManager.IsMineOnTile(redTank.Tile);
                bool redTankRanAway = redTank.ChooseWhetherToFlee();
                bool blueTankRanAway = blueTank.ChooseWhetherToFlee();

                if (!redTankRanAway && !blueTankRanAway) // both stayed
                {
                    TEAM dyingTank = (TEAM)UnityEngine.Random.Range(0, teamsAmount);

                    Tank loser = dyingTank == TEAM.RED ? redTank : blueTank;
                    Tank winner = dyingTank == TEAM.RED ? blueTank : redTank;

                    loser.SetState(STATE.DIE);

                    if (sameTileAsMine)
                    {
                        winner.TakeMine();
                    }
                }
                else if (redTankRanAway ^ blueTankRanAway) // one stayed, one fleed
                {
                    Tank tankWhomStayed = blueTankRanAway ? redTank : blueTank;
                    Tank tankWhomFleed = blueTankRanAway ? blueTank : redTank;

                    if (sameTileAsMine)
                    {
                        tankWhomStayed.TakeMine();
                    }
                    else
                    {
                        bool tankDies = UnityEngine.Random.Range(0, 100) < 75;

                        if (tankDies)
                        {
                            tankWhomFleed.SetState(STATE.DIE);
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
        #endregion

        #region AUX
        private TEAM GetContraryTeam(TEAM contraryTo)
        {
            return contraryTo == TEAM.BLUE ? TEAM.RED : TEAM.BLUE;
        }

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
        #endregion
        #endregion
    }
}