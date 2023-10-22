
using UnityEngine;
using UnityEngine.UI;

public class StartConfigurationScreen : MonoBehaviour
{
    public Text populationCountTxt;
    public Slider populationCountSlider;
    public Text minesCountTxt;
    public Slider minesCountSlider;
    public Text generationDurationTxt;
    public Slider generationDurationSlider;
    public Text eliteCountTxt;
    public Slider eliteCountSlider;
    public Text mutationChanceTxt;
    public Slider mutationChanceSlider;
    public Text mutationRateTxt;
    public Slider mutationRateSlider;
    public Text hiddenLayersCountTxt;
    public Slider hiddenLayersCountSlider;
    public Text neuronsPerHLCountTxt;
    public Slider neuronsPerHLSlider;
    public Text biasTxt;
    public Slider biasSlider;
    public Text sigmoidSlopeTxt;
    public Slider sigmoidSlopeSlider;
    public Button startButton;
    public Button loadButton;
    public Button saveButton;
    public Button loadSimButton;
    public GameObject simulationScreen;
    
    string populationText;
    string minesText;
    string generationDurationText;
    string elitesText;
    string mutationChanceText;
    string mutationRateText;
    string hiddenLayersCountText;
    string biasText;
    string sigmoidSlopeText;
    string neuronsPerHLCountText;


    void Start()
    {   
        populationCountSlider.onValueChanged.AddListener(OnPopulationCountChange);
        minesCountSlider.onValueChanged.AddListener(OnMinesCountChange);
        generationDurationSlider.onValueChanged.AddListener(OnGenerationDurationChange);
        eliteCountSlider.onValueChanged.AddListener(OnEliteCountChange);
        mutationChanceSlider.onValueChanged.AddListener(OnMutationChanceChange);
        mutationRateSlider.onValueChanged.AddListener(OnMutationRateChange);
        hiddenLayersCountSlider.onValueChanged.AddListener(OnHiddenLayersCountChange);
        neuronsPerHLSlider.onValueChanged.AddListener(OnNeuronsPerHLChange);
        biasSlider.onValueChanged.AddListener(OnBiasChange);
        sigmoidSlopeSlider.onValueChanged.AddListener(OnSigmoidSlopeChange);

        populationText = populationCountTxt.text;
        minesText = minesCountTxt.text;
        generationDurationText = generationDurationTxt.text;
        elitesText = eliteCountTxt.text;
        mutationChanceText = mutationChanceTxt.text;
        mutationRateText = mutationRateTxt.text;
        hiddenLayersCountText = hiddenLayersCountTxt.text;
        neuronsPerHLCountText = neuronsPerHLCountTxt.text;
        biasText = biasTxt.text;
        sigmoidSlopeText = sigmoidSlopeTxt.text;

        populationCountSlider.value = PopulationManager.Instance.PopulationCount;
        minesCountSlider.value = PopulationManager.Instance.MinesCount;
        generationDurationSlider.value = PopulationManager.Instance.GenerationDuration;
        eliteCountSlider.value = PopulationManager.Instance.EliteCount;
        mutationChanceSlider.value = PopulationManager.Instance.MutationChance * 100.0f;
        mutationRateSlider.value = PopulationManager.Instance.MutationRate * 100.0f;
        hiddenLayersCountSlider.value = PopulationManager.Instance.HiddenLayers;
        neuronsPerHLSlider.value = PopulationManager.Instance.NeuronsCountPerHL;
        biasSlider.value = -PopulationManager.Instance.Bias;
        sigmoidSlopeSlider.value = PopulationManager.Instance.P;

        startButton.onClick.AddListener(OnStartButtonClick);
        loadButton.onClick.AddListener(OnLoadData);
        saveButton.onClick.AddListener(OnSaveConfig);
        loadSimButton.onClick.AddListener(OnLoadSim);
    }

    void OnLoadData()
    {

        data.ConfigurationData config = Utilities.SaveLoadSystem.LoadConfigFile();
        if (config == null) return;
        populationCountSlider.onValueChanged.Invoke(config.population_count);
        minesCountSlider.onValueChanged.Invoke(config.mines_count);
        generationDurationSlider.onValueChanged.Invoke(config.generation_duration);
        eliteCountSlider.onValueChanged.Invoke(config.elites_count);
        mutationChanceSlider.onValueChanged.Invoke(config.mutation_chance * 100.0f);
        mutationRateSlider.onValueChanged.Invoke(config.mutation_rate * 100.0f);
        hiddenLayersCountSlider.onValueChanged.Invoke(config.hidden_layers_count);
        neuronsPerHLSlider.onValueChanged.Invoke(config.neurons_per_hidden_layers);
        biasSlider.onValueChanged.Invoke(-config.bias);
        sigmoidSlopeSlider.onValueChanged.Invoke(config.sigmoid);
    }

    void OnLoadSim()
    {
        data.SimData sim = Utilities.SaveLoadSystem.LoadSimFile();

        data.ConfigurationData config = sim.config; if (config == null) return;

        populationCountSlider.onValueChanged.Invoke(config.population_count);
        minesCountSlider.onValueChanged.Invoke(config.mines_count);
        generationDurationSlider.onValueChanged.Invoke(config.generation_duration);
        eliteCountSlider.onValueChanged.Invoke(config.elites_count);
        mutationChanceSlider.onValueChanged.Invoke(config.mutation_chance * 100.0f);
        mutationRateSlider.onValueChanged.Invoke(config.mutation_rate * 100.0f);
        hiddenLayersCountSlider.onValueChanged.Invoke(config.hidden_layers_count);
        neuronsPerHLSlider.onValueChanged.Invoke(config.neurons_per_hidden_layers);
        biasSlider.onValueChanged.Invoke(-config.bias);
        sigmoidSlopeSlider.onValueChanged.Invoke(config.sigmoid);

        PopulationManager.Instance.StartLoadsimulation(sim);

        this.gameObject.SetActive(false);
        simulationScreen.SetActive(true);

    }

    void OnSaveConfig()
    {
        //change to use population manager.
        data.ConfigurationData config = new();
        config.population_count = (int)populationCountSlider.value;
        config.mines_count = (int)minesCountSlider.value;
        config.generation_duration = generationDurationSlider.value;
        config.mutation_chance = mutationChanceSlider.value;
        config.mutation_rate = mutationRateSlider.value;
        config.hidden_layers_count = hiddenLayersCountSlider.value;
        config.neurons_per_hidden_layers = neuronsPerHLSlider.value;
        config.elites_count = (int)eliteCountSlider.value;
        config.bias = -biasSlider.value;
        config.sigmoid = sigmoidSlopeSlider.value;
        Utilities.SaveLoadSystem.SaveConfig(config);
    }
    void OnPopulationCountChange(float value)
    {
        PopulationManager.Instance.PopulationCount = (int)value;
        
        populationCountTxt.text = string.Format(populationText, PopulationManager.Instance.PopulationCount);
    }

    void OnMinesCountChange(float value)
    {
        PopulationManager.Instance.MinesCount = (int)value;        

        minesCountTxt.text = string.Format(minesText, PopulationManager.Instance.MinesCount);
    }

    void OnGenerationDurationChange(float value)
    {
        PopulationManager.Instance.GenerationDuration = (int)value;
        
        generationDurationTxt.text = string.Format(generationDurationText, PopulationManager.Instance.GenerationDuration);
    }

    void OnEliteCountChange(float value)
    {
        PopulationManager.Instance.EliteCount = (int)value;

        eliteCountTxt.text = string.Format(elitesText, PopulationManager.Instance.EliteCount);
    }

    void OnMutationChanceChange(float value)
    {
        PopulationManager.Instance.MutationChance = value / 100.0f;

        mutationChanceTxt.text = string.Format(mutationChanceText, (int)(PopulationManager.Instance.MutationChance * 100));
    }

    void OnMutationRateChange(float value)
    {
        PopulationManager.Instance.MutationRate = value / 100.0f;

        mutationRateTxt.text = string.Format(mutationRateText, (int)(PopulationManager.Instance.MutationRate * 100));
    }

    void OnHiddenLayersCountChange(float value)
    {
        PopulationManager.Instance.HiddenLayers = (int)value;
        

        hiddenLayersCountTxt.text = string.Format(hiddenLayersCountText, PopulationManager.Instance.HiddenLayers);
    }

    void OnNeuronsPerHLChange(float value)
    {
        PopulationManager.Instance.NeuronsCountPerHL = (int)value;

        neuronsPerHLCountTxt.text = string.Format(neuronsPerHLCountText, PopulationManager.Instance.NeuronsCountPerHL);
    }

    void OnBiasChange(float value)
    {
        PopulationManager.Instance.Bias = -value;

        biasTxt.text = string.Format(biasText, PopulationManager.Instance.Bias.ToString("0.00"));
    }

    void OnSigmoidSlopeChange(float value)
    {
        PopulationManager.Instance.P = value;

        sigmoidSlopeTxt.text = string.Format(sigmoidSlopeText, PopulationManager.Instance.P.ToString("0.00"));
    }


    void OnStartButtonClick()
    {
        PopulationManager.Instance.StartSimulation();
        this.gameObject.SetActive(false);
        simulationScreen.SetActive(true);
    }
    
}
