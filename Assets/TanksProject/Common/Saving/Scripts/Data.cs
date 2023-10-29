using TanksProject.Game.Data;

namespace data
{
    [System.Serializable]
    public class ConfigurationData
    {
        public   int population_count           ;
        public   float generation_duration      ;
        public   int elites_count               ;
        public float mutation_chance            ;
        public float mutation_rate              ;
        public float hidden_layers_count        ;
        public float neurons_per_hidden_layers  ;
        public float bias                       ;
        public float sigmoid                    ;
    }
    [System.Serializable]
    public class SimData
    {
        public float bestFitness;
        public float avgFitness;
        public float worstFitness;


        public int generation_count;
        public int genomes_count;
        public GenomeData[] genomes;

        public ConfigurationData config;
    }

    [System.Serializable]
    public class NeuronData
    {
        public float[] weights;
        public int weights_count;
        public float bias;
        public float p;
    }
    [System.Serializable]
    public class NeuronLayerData
    {
        public NeuronData[] neurons;
        public int neuron_count;
        public float[] outputs;
        public int outputs_count;
        public int total_weight;
        public int input_count;
        public float bias;
        public float p;
    }
    [System.Serializable]
    public class NeuralNetworkData
    {
        public NeuronLayerData[] neurons_layers;
        public int neuron_layer_count;
        public int total_weights_count;
        public int input_count;
    }
    [System.Serializable]
    public class GenomeData
    {
        public float[] genomes;
        public int genome_count;
        public float fitness;
    }
    public static class Helper
    {
        public static ConfigurationData Cast_pm_configuration()
        {
            ConfigurationData config = new();
            config.population_count = GameData.Inst.PopulationCount;
            config.generation_duration = GameData.Inst.TurnsPerGeneration;
            config.elites_count = GameData.Inst.EliteCount;
            config.mutation_chance = GameData.Inst.MutationChance;
            config.mutation_rate = GameData.Inst.MutationRate;
            config.hidden_layers_count = GameData.Inst.HiddenLayers;
            config.neurons_per_hidden_layers = GameData.Inst.NeuronsCountPerHL;
            return config;
        }

        public static Genome Cast_gemomeData_genome(GenomeData data)
        {
            Genome genome = new Genome(data.genomes);
            genome.genome = new float[data.genome_count];
            for (int i = 0; i < data.genome_count; i++)
            {
                genome.genome[i] = data.genomes[i];
            }
            genome.fitness = data.fitness;
            return genome;
        }
    }
    

}