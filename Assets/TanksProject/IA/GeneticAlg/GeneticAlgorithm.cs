using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Genome
{
	public float[] genome;
	public float fitness = 0;

	public Genome(float[] genes)
	{
		genome = genes;
		fitness = 0;
	}

	public Genome(int genesCount)
	{
        genome = new float[genesCount];

        for (int j = 0; j < genesCount; j++)
            genome[j] = Random.Range(-1.0f, 1.0f);

        fitness = 0;
	}

    public Genome()
    {
        fitness = 0;
    }
}

public class GeneticAlgorithm 
{
	List<Genome> population = new List<Genome>();
	List<Genome> newPopulation = new List<Genome>();

	float totalFitness;

	float mutationChance = 0.0f;
	float mutationRate = 0.0f;

	public GeneticAlgorithm(float mutationChance, float mutationRate)
	{
		this.mutationChance = mutationChance;
		this.mutationRate = mutationRate;
	}

    public Genome[] GetRandomGenomes(int count, int genesCount)
    {
        Genome[] genomes = new Genome[count];

        for (int i = 0; i < count; i++)
        {
            genomes[i] = new Genome(genesCount);
        }

        return genomes;
    }


	public Genome[] Epoch(Genome[] reproducible, Genome[] elites)
	{
		totalFitness = 0;

		population.Clear();
		newPopulation.Clear();

		population.AddRange(reproducible);
		population.Sort(HandleComparison);
        

        foreach (Genome g in population)
		{
			totalFitness += g.fitness;
		}

        newPopulation.AddRange(elites);

		for (int i = 0; i < population.Count; i += 2)
		{
            Crossover(i < population.Count - 1 || reproducible.Length % 2 == 0);
        }

		return newPopulation.ToArray();
	}

    public Genome[] Epoch(Genome[] oldGenomes)
    {
        totalFitness = 0;

        population.Clear();
        newPopulation.Clear();

        population.AddRange(oldGenomes);
        population.Sort(HandleComparison);

        foreach (Genome g in population)
        {
            totalFitness += (g.fitness > 0 ? g.fitness : 0);
        }

        for (int i = population.Count - 1; i > population.Count - 5 &&  i > 0 ; i--)
		{
			newPopulation.Add(population[i]);
		}

        while (newPopulation.Count < population.Count)
        {
            Crossover(true);
        }

        return newPopulation.ToArray();
    }

    public Genome[] GetRandomCrossoverPopulation(Genome[] allGenomes)
	{
		Genome[] newPopulation = new Genome[allGenomes.Length];

        for (int i = 0; i < allGenomes.Length; i += 2)
		{
            Genome mom = RandomSelection(allGenomes);
            Genome dad = RandomSelection(allGenomes);

            Crossover(mom, dad, out Genome child1, out Genome child2);

			newPopulation[i] = child1;

			if (allGenomes.Length > i + 1)
			{ 
				newPopulation[i + 1] = child2; 
			}
        }

		return newPopulation;

    }

	void Crossover(bool bothChildren)
	{
		Genome mom = RouletteSelection();
		Genome dad = RouletteSelection();

		Genome child1;
		Genome child2;

		Crossover(mom, dad, out child1, out child2);

		newPopulation.Add(child1);

		if (bothChildren)
		{ 
			newPopulation.Add(child2); 
		}
	}

	void Crossover(Genome mom, Genome dad, out Genome child1, out Genome child2)
	{
		child1 = new Genome();
		child2 = new Genome();

		child1.genome = new float[mom.genome.Length];
		child2.genome = new float[mom.genome.Length];

		int pivot = Random.Range(0, mom.genome.Length);

		for (int i = 0; i < pivot; i++)
		{
			child1.genome[i] = mom.genome[i];

			if (ShouldMutate())
				child1.genome[i] += Random.Range(-mutationRate, mutationRate);

			child2.genome[i] = dad.genome[i];

			if (ShouldMutate())
				child2.genome[i] += Random.Range(-mutationRate, mutationRate);
		}

		for (int i = pivot; i < mom.genome.Length; i++)
		{
			child2.genome[i] = mom.genome[i];

			if (ShouldMutate())
				child2.genome[i] += Random.Range(-mutationRate, mutationRate);
			
			child1.genome[i] = dad.genome[i];

			if (ShouldMutate())
				child1.genome[i] += Random.Range(-mutationRate, mutationRate);
		}
	}

	bool ShouldMutate()
	{
		return Random.Range(0.0f, 1.0f) < mutationChance;
	}

	int HandleComparison(Genome x, Genome y)
	{
		return x.fitness > y.fitness ? 1 : x.fitness < y.fitness ? -1 : 0;
	}


	public Genome RouletteSelection()
	{
		float rnd = Random.Range(0, Mathf.Max(totalFitness, 0));

		float fitness = 0;

		for (int i = 0; i < population.Count; i++)
		{
			fitness += Mathf.Max(population[i].fitness, 0);
			if (fitness >= rnd)
				return population[i];
		}

		return population.Last();
	}

    public Genome RandomSelection(Genome[] genomesToChoose)
    {
        int rndIndex = Random.Range(0, genomesToChoose.Length);
        return genomesToChoose[rndIndex];
    }
}
