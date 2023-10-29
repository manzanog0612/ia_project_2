using TanksProject.Game.Data;

namespace data
{
    [System.Serializable]
    public class ConfigurationData
    {
        public   int population_count           ;
        public float turnsPerGeneration         ;
        public float turnDuration               ;
        public   int elites_count               ;
        public float mutation_chance            ;
        public float mutation_rate              ;
        public float hidden_layers_count        ;
        public float neurons_per_hidden_layers  ;
        public float bias                       ;
        public float sigmoid                    ;
    }

    [System.Serializable]
    public class TeamData
    {
        public int generation_count;
        public GenomeData[] genomes;
    }

    [System.Serializable]
    public class SimData
    {
        public TeamData[] teamsData;

        public ConfigurationData config;
    }
    [System.Serializable]
    public class GenomeData
    {
        public float[] genomes;
        public float fitness;
    }
    public static class Helper
    {
        public static ConfigurationData Cast_pm_configuration()
        {
            ConfigurationData config = new();
            config.population_count = GameData.Inst.PopulationCount;
            config.turnsPerGeneration = GameData.Inst.TurnsPerGeneration;
            config.turnDuration = GameData.Inst.TurnDuration;
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
            genome.fitness = data.fitness;
            return genome;
        }
    }
    

}