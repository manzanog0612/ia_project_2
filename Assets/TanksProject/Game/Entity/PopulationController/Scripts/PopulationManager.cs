using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

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

        private List<Tank> tanks = new List<Tank>();
        private List<Genome> population = new List<Genome>();
        private List<NeuralNetwork> brains = new List<NeuralNetwork>();

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
        public List<Tank> Tanks { get => tanks; }
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
        }

        public void StartLoadedSimulation(SimData sim)
        {
            genAlg = new GeneticAlgorithm(sim.config.mutation_chance, sim.config.mutation_rate);

            TeamData team = sim.teamsData[(int)this.team];
            Generation = team.generation_count;

            Genome[] genomes = new Genome[GameData.Inst.PopulationCount];

            for (int i = 0; i < GameData.Inst.PopulationCount; i++)
            {
                GenomeData genomeData = team.genomes[i];
                genomes[i] = new Genome(genomeData.genomes);
                genomes[i].fitness = genomeData.fitness;
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
            teamData.genomes = new GenomeData[population.Count];

            for (int i = 0; i < population.Count; i++)
            {
                teamData.genomes[i] = new GenomeData();
                teamData.genomes[i].genomes = brains[i].GetWeights();
                teamData.genomes[i].fitness = population[i].fitness;
            }

            return teamData;
        }

        public void Epoch()
        {
            Generation++;

            Genome[] newGenomes = null;

            if (Generation > GameData.Inst.MinGenerationToStartHungerGames)
            {
                List<Genome> elites = new List<Genome>();
                List<Genome> reproducible = new List<Genome>();
                for (int i = 0; i < tanks.Count; i++)
                {
                    Tank tank = tanks[i];
                    if (tank.State == STATE.REPRODUCE || tank.State == STATE.SURVIVE)
                    {
                        if (tank.State == STATE.REPRODUCE)
                        {
                            reproducible.Add(population[i]);
                            brains.Add(CreateBrain());
                        }

                        if (tank.TurnsAlive == 3)
                        {
                            tank.SetState(STATE.DIE);
                        }
                        else
                        {
                            elites.Add(population[i]);
                        }
                    }
                }

                for (int i = tanks.Count - 1; i >= 0; i--)
                {
                    if (tanks[i].IsDead())
                    {
                        DestroyTank(tanks[i]);
                    }
                }

                if (elites.Count < 2)
                {
                    Dead = true;
                    return;
                }

                // Evolve each genome and create a new array of genomes
                newGenomes = genAlg.Epoch(reproducible.ToArray(), elites.ToArray());
            }
            else
            {
                for (int i = 0; i < tanks.Count; i++)
                {
                    tanks[i].Genome.fitness *= tanks[i].MinesEaten;

                    //if (tanks[i].Movements.Contains(Vector2Int.left) || tanks[i].Movements.Contains(Vector2Int.right))
                    //{
                    //    tanks[i].Genome.fitness += 10;
                    //
                    //    if (tanks[i].Movements.Contains(Vector2Int.left) && tanks[i].Movements.Contains(Vector2Int.right))
                    //    {
                    //        tanks[i].Genome.fitness += 10;
                    //        tanks[i].Genome.fitness *= 2;
                    //    }
                    //}
                }

                newGenomes = genAlg.Epoch(population.ToArray());
            }

            BestFitness = GetBestFitness();
            AvgFitness = GetAvgFitness();
            WorstFitness = GetWorstFitness();

            // Clear current population
            population.Clear();

            // Add new population
            population.AddRange(newGenomes);

            // Set the new genomes as each NeuralNetwork weights
            for (int i = 0; i < population.Count; i++)
            {
                NeuralNetwork brain = brains[i];

                brain.SetWeights(population[i].genome);

                if (tanks.Count <= i)
                {
                    tanks.Add(CreateTank(population[i], brain, tankStartTiles[i]));
                }
                else
                {
                    tanks[i].SetBrain(population[i], brain);
                    tanks[i].SetCurrentTile(tankStartTiles[i]);
                    tanks[i].transform.rotation = Quaternion.identity;
                }                
            }

            if (Generation > 1 &&
                GameData.Inst.FitnessTillNewTest.Length > GameData.Inst.TestIndex &&
                AvgFitness > GameData.Inst.FitnessTillNewTest[GameData.Inst.TestIndex])
            {
                GameData.Inst.TestIndex++;
            }
        }

        public void ChangeTurn()
        {
            for (int i = 0; i < tanks.Count; i++)
            {
                Tank t = tanks[i];

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
                    nearestTank = tanks[j];
                }
            }

            return nearestTank;
        }

        public void SetStateByMinesEaten()
        {
            for (int i = 0; i < tanks.Count; i++)
            {
                Tank tank = tanks[i];

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
            return genAlg.GetRandomCrossoverPopulation(population.ToArray());
        }
        #endregion

        #region PRIVATE_METHODS
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
            brains.Add(brain);

            population.Add(genome);
            tanks.Add(CreateTank(genome, brain, startTile));
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

        #region UTILS
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

        private Tank CreateTank(Genome genome, NeuralNetwork brain, Vector2Int gridPos)
        {
            Vector3 position = grid.GetTilePos(gridPos);
            GameObject go = Instantiate(TankPrefab, position, Quaternion.identity, transform);
            Tank t = go.GetComponent<Tank>();
            t.Init(grid, gridPos, GameData.Inst.TurnDuration, onTakeMine);
            t.SetBrain(genome, brain);
            t.gameObject.name = t.gameObject.name.Replace("(Clone)", (ids++).ToString());
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

        private void DestroyTank(Tank go)
        {
            tanks.Remove(go);
            population.Remove(go.Genome);
            brains.Remove(go.Brain);

            Destroy(go.gameObject);
        }
        #endregion
    }
}