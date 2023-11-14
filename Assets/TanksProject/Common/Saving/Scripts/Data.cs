namespace TanksProject.Common.Saving
{
    [System.Serializable]
    public class ConfigurationData
    {
        public int population_count;
        public int turnsPerGeneration;
        public float turnDuration;
        public float mutation_chance;
        public float mutation_rate;
        public float hidden_layers_count;
        public float neurons_per_hidden_layers;
        public float bias;
        public float sigmoid;
    }

    [System.Serializable]
    public class TeamData
    {
        public int generation_count;
        public GenomeData[] genomes;
        public int timesDead = 0;
    }

    [System.Serializable]
    public class SimData
    {
        public float maxAvgFitness;
        public TeamData[] teamsData;
        public ConfigurationData config;
    }

    [System.Serializable]
    public class GenomeData
    {
        public float[] genomes;
        public float fitness;
    }
}