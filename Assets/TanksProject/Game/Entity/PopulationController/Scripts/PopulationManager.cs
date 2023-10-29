using System;
using System.Collections.Generic;

using UnityEngine;

using TanksProject.Game.Entity.TankController;
using TanksProject.Game.Data;

using data;

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

        private List<Tank> tanks = new List<Tank>();
        private List<Genome> population = new List<Genome>();
        private List<NeuralNetwork> brains = new List<NeuralNetwork>();

        private bool isRunning = false;

        private Grid grid = null;
        private Vector2Int[] tankStartTiles = null;
        #endregion

        #region ACTIONS
        private Func<Vector3, GameObject> onGetNearestMine = null;
        #endregion

        #region PROPERTIES
        public int Generation { get; private set; }
        public float BestFitness { get; private set; }
        public float AvgFitness { get; private set; }
        public float WorstFitness { get; private set; }
        public TEAM Team { get => team; }
        #endregion

        #region UNITY_CALLS
        private void FixedUpdate()
        {
            if (!isRunning)
            { 
                return; 
            }

            float dt = Time.fixedDeltaTime;

            if (GameData.Inst.GenerationFinished)
            {
                Epoch();
            }
            else if (GameData.Inst.TurnFinished)
            {
                for (int i = 0; i < tanks.Count; i++)
                {
                    Tank t = tanks[i];

                    GameObject mine = onGetNearestMine.Invoke(t.transform.position);
                    t.SetNearestMine(mine);

                    GameObject nearestTank = GetNearestTank(t.gameObject);
                    t.SetNearestTank(nearestTank);

                    t.Think(dt);
                }
            }
        }
        #endregion

        #region PUBLIC_METHODS
        public void Init(Vector2Int[] tankStartTiles, Grid grid, Func<Vector3, GameObject> onGetNearestMine)
        {
            this.tankStartTiles = tankStartTiles;
            this.grid = grid;

            this.onGetNearestMine = onGetNearestMine;
        }

        public void StartLoadedSimulation(SimData sim)
        {
            genAlg = new GeneticAlgorithm(sim.config.elites_count, sim.config.mutation_chance, sim.config.mutation_rate);

            LoadInitialPopulation(sim);

            isRunning = true;
        }

        public void StartSimulation()
        {
            genAlg = new GeneticAlgorithm(GameData.Inst.EliteCount, GameData.Inst.MutationChance, GameData.Inst.MutationRate);

            GenerateInitialPopulation();

            isRunning = true;
        }

        public void PauseSimulation()
        {
            isRunning = !isRunning;
        }

        public void StopSimulation()
        {
            isRunning = false;

            Generation = 0;

            DestroyTanks();
        }

        public TeamData GetCurrentTeamData()
        {
            TeamData teamData = new TeamData();
            teamData.generation_count = Generation;
            teamData.genomes = new GenomeData[GameData.Inst.PopulationCount];

            for (int i = 0; i < GameData.Inst.PopulationCount; i++)
            {
                teamData.genomes[i] = new GenomeData();
                teamData.genomes[i].genomes = brains[i].GetWeights();
                teamData.genomes[i].fitness = population[i].fitness;
            }

            return teamData;
        }        
        #endregion

        #region PRIVATE_METHODS
        private float GetBestFitness()
        {
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
            float fitness = 0;
            foreach (Genome g in population)
            {
                fitness += g.fitness;
            }

            return fitness / population.Count;
        }

        private float GetWorstFitness()
        {
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

        private void GenerateInitialPopulation()
        {
            Generation = 0;

            DestroyTanks();

            for (int i = 0; i < GameData.Inst.PopulationCount; i++)
            {
                NeuralNetwork brain = CreateBrain();

                Genome genome = new Genome(brain.GetTotalWeightsCount());

                brain.SetWeights(genome.genome);
                brains.Add(brain);

                population.Add(genome);
                tanks.Add(CreateTank(genome, brain, tankStartTiles[i]));
            }
        }

        private void LoadInitialPopulation(SimData sim)
        {
            TeamData team = sim.teamsData[(int)this.team];
            Generation = team.generation_count;

            DestroyTanks();

            if (team.genomes.Length < GameData.Inst.PopulationCount)
            {
                Debug.LogError("load genomes count has not same amount of population count");
                return;
            }

            for (int i = 0; i < GameData.Inst.PopulationCount; i++)
            {
                NeuralNetwork brain = CreateBrain();

                Genome genome = Helper.Cast_gemomeData_genome(team.genomes[i]);

                brain.SetWeights(genome.genome);
                brains.Add(brain);

                population.Add(genome);
                tanks.Add(CreateTank(genome, brain, tankStartTiles[i]));
            }
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

        // Evolve!!!
        private void Epoch()
        {
            // Increment generation counter
            Generation++;

            // Calculate best, average and worst fitness
            BestFitness = GetBestFitness();
            AvgFitness = GetAvgFitness();
            WorstFitness = GetWorstFitness();

            // Evolve each genome and create a new array of genomes
            Genome[] newGenomes = genAlg.Epoch(population.ToArray());

            // Clear current population
            population.Clear();

            // Add new population
            population.AddRange(newGenomes);

            // Set the new genomes as each NeuralNetwork weights
            for (int i = 0; i < GameData.Inst.PopulationCount; i++)
            {
                NeuralNetwork brain = brains[i];

                brain.SetWeights(newGenomes[i].genome);

                tanks[i].SetBrain(newGenomes[i], brain);
                tanks[i].SetCurrentTile(tankStartTiles[i]);
                tanks[i].transform.rotation = Quaternion.identity;
            }

            if (Generation > 1 && 
                GameData.Inst.FitnessTillNewTest.Length > GameData.Inst.TestIndex && 
                AvgFitness > GameData.Inst.FitnessTillNewTest[GameData.Inst.TestIndex])
            {
                GameData.Inst.TestIndex++;
            }
        }
        #endregion

        #region UTILS
        private Tank CreateTank(Genome genome, NeuralNetwork brain, Vector2Int gridPos)
        {
            Vector3 position = grid.GetTilePos(gridPos);
            GameObject go = Instantiate(TankPrefab, position, Quaternion.identity, transform);
            Tank t = go.GetComponent<Tank>();
            t.Init(grid, gridPos, GameData.Inst.TurnDuration);
            t.SetBrain(genome, brain);
            return t;
        }

        private void DestroyTanks()
        {
            foreach (Tank go in tanks)
            { 
                Destroy(go.gameObject); 
            }

            tanks.Clear();
            population.Clear();
            brains.Clear();
        }

        private GameObject GetNearestTank(GameObject actualTank) 
        {
            GameObject nearestTank = null;
            float closerDistance = 100000;
            for (int j = 0; j < tanks.Count; j++)
            {
                if (tanks[j].gameObject == actualTank)
                {
                    continue;
                }

                float distance = Vector3.Distance(tanks[j].transform.position, actualTank.transform.position);

                if (distance < closerDistance)
                {
                    closerDistance = distance;
                    nearestTank = tanks[j].gameObject;
                }
            }

            return nearestTank;
        }
        #endregion

    }
}