using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Pool;

using TanksProject.Game.Entity.MineController;
using TanksProject.Game.Entity.TankController;
using TanksProject.Game.Data;
using TanksProject.Common.Saving;

using Grid = TanksProject.Common.Grid.Grid;

namespace TanksProject.Game.Entity.PopulationController
{
    public enum TEAM { RED, BLUE }

    public class PopulationManager : MonoBehaviour
    {
        #region EXPOSED_FiELDS
        [SerializeField] private TEAM team = default;                         
        [SerializeField] private GameObject TankPrefab;
        #endregion

        #region PRIVATE_FIELDS
        private GeneticAlgorithm genAlg;

        private List<Tank> activeTanks = new List<Tank>();
        private ObjectPool<Tank> tanksPool = null;

        private Grid grid = null;
        private Vector2Int[] tankStartTiles = null;
        private int timesDead = 0;
        private static int ids = 0;
        #endregion

        #region ACTIONS
        private Func<Vector3, Mine> onGetNearestMine = null;
        private Func<GameObject, Tank> onGetNearestEnemyTank = null;
        #endregion

        #region CONSTANT_FIELDS
        private float mutationMultiplier = 0.1f;
        #endregion

        #region PROPERTIES
        public int Generation { get; private set; }
        public float BestFitness { get; private set; }
        public float AvgFitness { get; private set; }
        public float WorstFitness { get; private set; }
        public TEAM Team { get => team; }
        public List<Tank> Tanks { get => activeTanks; }
        public bool Dead { get; private set; }
        #endregion

        #region ACITONS
        private Action<GameObject> onTakeMine = null;
        #endregion

        #region PUBLIC_METHODS
        public void Init(Vector2Int[] tankStartTiles, Grid grid, Func<Vector3, Mine> onGetNearestMine, Func<GameObject, Tank> onGetNearestEnemyTank,
                         Action<GameObject> onTakeMine)
        {
            this.tankStartTiles = tankStartTiles;
            this.grid = grid;

            this.onGetNearestMine = onGetNearestMine;
            this.onGetNearestEnemyTank = onGetNearestEnemyTank;

            this.onTakeMine = onTakeMine;

            Dead = false;

            tanksPool = new ObjectPool<Tank>(CreateTank, OnGetTank, OnReleaseTank);
        }

        public void StartLoadedSimulation(SimData sim)
        {
            genAlg = new GeneticAlgorithm(sim.config.mutation_chance, sim.config.mutation_rate);

            TeamData team = sim.teamsData[(int)this.team];
            Generation = team.generation_count;

            Genome[] genomes = new Genome[GameData.Inst.PopulationCount];

            int savedGenomesIndex = 0;
            for (int i = 0; i < GameData.Inst.PopulationCount; i++)
            {
                if (savedGenomesIndex == team.genomes.Length)
                {
                    savedGenomesIndex = 0;                            
                }

                GenomeData genomeData = team.genomes[savedGenomesIndex];
                genomes[i] = new Genome(genomeData.genomes);
                genomes[i].fitness = genomeData.fitness;

                savedGenomesIndex++;
            }

            CreatePopulationFromGenomes(genomes);
        }

        public void StartSimulation()
        {
            genAlg = new GeneticAlgorithm(GameData.Inst.MutationChance, GameData.Inst.MutationRate);

            ResetSimulation();

            for (int i = 0; i < GameData.Inst.PopulationCount; i++)
            {
                NeuralNetwork brain = CreateBrain();
                Genome genome = new Genome(brain.GetTotalWeightsCount());

                OnCreateTank(brain, genome, tankStartTiles[i]);
            }
        }

        public void StartNewSimulation(Genome[] genomesToUse)
        {
            genAlg = new GeneticAlgorithm(GameData.Inst.MutationChance + timesDead * mutationMultiplier, GameData.Inst.MutationRate + timesDead * mutationMultiplier);

            ResetSimulation();
            CreatePopulationFromGenomes(genomesToUse);

            Dead = false;
        }

        public void ResetSimulation()
        {
            Generation = 0;

            DestroyTanks();
        }

        public TeamData GetCurrentTeamData()
        {
            TeamData teamData = new TeamData();
            teamData.generation_count = Generation;
            teamData.genomes = new GenomeData[activeTanks.Count];

            for (int i = 0; i < activeTanks.Count; i++)
            {
                teamData.genomes[i] = new GenomeData();
                teamData.genomes[i].genomes = activeTanks[i].Brain.GetWeights();
                teamData.genomes[i].fitness = activeTanks[i].Genome.fitness;
            }

            return teamData;
        }

        public void OnTurnUpdated()
        {
            for (int i = 0; i < activeTanks.Count; i++)
            {
                activeTanks[i].OnTurnUpdated();
            }
        }

        public void HandleTanksOnSameTile()
        {
            for (int i = 0; i < activeTanks.Count; i++)
            {
                Tank tank1 = activeTanks[i];
                Tank tank2 = activeTanks[i].NearTeamTank;

                if (tank1.Tile != tank2.Tile || (tank1.NearMine != tank2.NearMine) || tank1.NearMine == null)
                {
                    continue;
                }

                if (tank1.NearMine.Tile != tank1.Tile)
                {
                    continue;
                }

                bool tank1Eats = tank1.ChooseWhetherToEat();
                bool tank2Eats = tank2.ChooseWhetherToEat();

                if (tank1Eats && tank2Eats)
                {
                    onTakeMine.Invoke(tank1.NearMine.gameObject);

                    tank1.TakeHalfFood();
                    tank2.TakeHalfFood();
                }
                else if (tank1Eats ^ tank2Eats)
                {
                    Tank tankWhomEats = tank1Eats ? tank1 : tank2;

                    tankWhomEats.TakeMine();
                }
            }
        }

        public void TrackFitness()
        {
            for (int i = 0; i < activeTanks.Count; i++)
            {
                activeTanks[i].TrackFitness();
            }
        }

        public void Epoch()
        {
            Generation++;

            Genome[] newGenomes = null;

            if (!GameData.Inst.Learning)
            {
                (int elites, List<Genome> reproducible) = GetTanksDataByState();

                UpdateTanksByState();

                if (elites <= 1)
                {
                    Dead = true;
                    return;
                }

                if (reproducible.Count >= 2)
                {
                    newGenomes = genAlg.Epoch(reproducible.ToArray());
                }
            }
            else
            {
                newGenomes = genAlg.Epoch(GetPopulation());
            }

            BestFitness = GetBestFitness();
            AvgFitness = GetAvgFitness();
            WorstFitness = GetWorstFitness();

            if (!GameData.Inst.Learning)
            {
                //Reset elites
                for (int i = 0; i < activeTanks.Count; i++)
                {
                    ResetTank(activeTanks[i], tankStartTiles[i]);
                }

                // Set the new genomes as each NeuralNetwork weights
                for (int i = 0; i < newGenomes.Length; i++)
                {
                    NeuralNetwork brain = CreateBrain();
                    brain.SetWeights(newGenomes[i].genome);
                    CreateTank(newGenomes[i], brain, tankStartTiles[i]);
                }
            }
            else
            {
                for (int i = 0; i < newGenomes.Length; i++)
                {
                    activeTanks[i].Brain.SetWeights(newGenomes[i].genome);
                    activeTanks[i].SetBrain(newGenomes[i], activeTanks[i].Brain);
                    ResetTank(activeTanks[i], tankStartTiles[i]);
                }
            }
        }

        public void ChangeTurn()
        {
            for (int i = 0; i < activeTanks.Count; i++)
            {
                Tank t = activeTanks[i];

                Mine mine = onGetNearestMine.Invoke(t.transform.position);
                t.SetNearestMine(mine);

                Tank nearestTeamTank = GetNearestTank(t.gameObject);
                t.SetNearestTeamTank(nearestTeamTank);

                Tank nearestEnemyTank = onGetNearestEnemyTank.Invoke(t.gameObject);
                t.SetNearestEnemyTank(nearestEnemyTank);

                t.Think();
            }
        }

        public Tank GetNearestTank(GameObject actualTank)
        {
            Tank nearestTank = null;
            float closerDistance = float.MaxValue;
            for (int j = 0; j < activeTanks.Count; j++)
            {
                if (activeTanks[j].gameObject == actualTank || activeTanks[j].IsDead())
                {
                    continue;
                }

                float distance = Vector3.Distance(activeTanks[j].transform.position, actualTank.transform.position);

                if (distance < closerDistance)
                {
                    closerDistance = distance;
                    nearestTank = activeTanks[j];
                }
            }

            return nearestTank;
        }

        public void SetStateByMinesEaten()
        {
            for (int i = 0; i < activeTanks.Count; i++)
            {
                Tank tank = activeTanks[i];

                if (tank.IsDead())
                {
                    continue;
                }

                if (tank.MinesEaten >= 2)
                {
                    tank.SetState(STATE.REPRODUCE);
                }
                else if (tank.MinesEaten == 1)
                {
                    tank.SetState(STATE.SURVIVE);
                }
                else
                {
                    tank.SetState(STATE.DIE);
                }
            }
        }

        public Genome[] GetRandomCrossoverPopulation()
        {
            return genAlg.GetRandomCrossoverPopulation(GetPopulation());
        }

        public Genome[] GetPopulation()
        {
            Genome[] genomes = new Genome[activeTanks.Count];

            for (int i = 0; i < activeTanks.Count; i++)
            {
                genomes[i] = activeTanks[i].Genome;
            }
            return genomes;
        }
        #endregion

        #region PRIVATE_METHODS
        private void UpdateTanksByState()
        {
            for (int i = 0; i < activeTanks.Count; i++)
            {
                Tank tank = activeTanks[i];
                if (tank.State == STATE.REPRODUCE || tank.State == STATE.SURVIVE)
                {
                    if (tank.TurnsAlive == 3)
                    {
                        tank.SetState(STATE.DIE);
                    }
                    else
                    {
                        tank.TurnsAlive++;
                    }
                }
            }

            for (int i = activeTanks.Count - 1; i >= 0; i--)
            {
                if (activeTanks[i].IsDead())
                {
                    DestroyTank(activeTanks[i]);
                }
            }
        }

        private (int, List<Genome>) GetTanksDataByState()
        {
            int elites = 0;
            List<Genome> reproducible = new List<Genome>();
            for (int i = 0; i < activeTanks.Count; i++)
            {
                Tank tank = activeTanks[i];
                if (tank.State == STATE.REPRODUCE || tank.State == STATE.SURVIVE)
                {
                    if (tank.State == STATE.REPRODUCE)
                    {
                        reproducible.Add(tank.Genome);
                    }

                    if (tank.TurnsAlive < 3)
                    {
                        elites++;
                    }
                }
            }

            return (elites, reproducible);
        }

        private void CreatePopulationFromGenomes(Genome[] genomes)
        {
            DestroyTanks();

            for (int i = 0; i < genomes.Length; i++)
            {
                NeuralNetwork brain = CreateBrain();

                Genome genome = new Genome(genomes[i].genome);
                genome.fitness = genomes[i].fitness;

                OnCreateTank(brain, genome, tankStartTiles[i]);
            }
        }

        private void OnCreateTank(NeuralNetwork brain, Genome genome, Vector2Int startTile)
        {
            brain.SetWeights(genome.genome);
            CreateTank(genome, brain, startTile);
        }

        private void ResetTank(Tank tank, Vector2Int startTile)
        {
            tank.SetCurrentTile(startTile);
            tank.transform.rotation = Quaternion.identity;
            tank.OnReset();
        }

        // Creates a new NeuralNetwork
        private NeuralNetwork CreateBrain()
        {
            NeuralNetwork brain = new NeuralNetwork();

            // Inputs
            brain.AddFirstNeuronLayer(GameData.Inst.InputsCount, GameData.Inst.Bias, GameData.Inst.P);

            for (int i = 0; i < GameData.Inst.HiddenLayers; i++)
            {
                // Add each hidden layer
                brain.AddNeuronLayer(GameData.Inst.NeuronsCountPerHL, GameData.Inst.Bias, GameData.Inst.P);
            }

            // Outputs
            brain.AddNeuronLayer(GameData.Inst.OutputsCount, GameData.Inst.Bias, GameData.Inst.P);

            return brain;
        }
        #endregion

        #region POOL_METHODS
        private Tank CreateTank()
        {
            GameObject go = Instantiate(TankPrefab, transform);
            Tank t = go.GetComponent<Tank>();
            t.gameObject.name = t.gameObject.name.Replace("(Clone)", ids++.ToString());
            return t;
        }

        private void OnGetTank(Tank tank)
        {
            tank.gameObject.SetActive(true);
            tank.OnReset();
            activeTanks.Add(tank);
        }

        private void OnReleaseTank(Tank tank)
        {
            tank.gameObject.SetActive(false);
            activeTanks.Remove(tank);
        }
        #endregion

        #region UTILS
        private float GetBestFitness()
        {
            Genome[] population = GetPopulation();
            float fitness = 0;
            foreach (Genome g in population)
            {
                if (fitness < g.fitness)
                {
                    fitness = g.fitness;
                }
            }

            return fitness;
        }

        private float GetAvgFitness()
        {
            Genome[] population = GetPopulation();
            float fitness = 0;
            foreach (Genome g in population)
            {
                fitness += g.fitness;
            }

            return fitness / population.Length;
        }

        private float GetWorstFitness()
        {
            Genome[] population = GetPopulation();
            float fitness = float.MaxValue;
            foreach (Genome g in population)
            {
                if (fitness > g.fitness)
                {
                    fitness = g.fitness;
                }
            }

            return fitness;
        }

        private Tank CreateTank(Genome genome, NeuralNetwork brain, Vector2Int gridPos)
        {
            Vector3 position = grid.GetTilePos(gridPos);
            
            Tank tank = tanksPool.Get();
            tank.transform.position = position;
            tank.transform.rotation = Quaternion.identity;
            tank.Init(grid, gridPos, GameData.Inst.TurnDuration, onTakeMine);
            tank.SetBrain(genome, brain);

            return tank;
        }

        private void DestroyTanks()
        {
            for (int i = activeTanks.Count - 1; i >= 0 ; i--)
            {
                tanksPool.Release(activeTanks[i]);
            }
        }

        private void DestroyTank(Tank go)
        {
            tanksPool.Release(go);
        }
        #endregion
    }
}