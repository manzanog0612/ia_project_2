using UnityEngine;

public class Tank : TankBase
{
    float fitness = 0;
    protected override void OnReset()
    {
        fitness = 1;
    }

    protected override void OnThink(float dt)
    {
        Vector3 dirToGoodMine = GetDirToObject(goodMine);
        Vector3 dirToBadMine = GetDirToObject(badMine);
        //Vector3 dirCloserTank = GetDirToObject(nearTank);

        float distanceToGoodMine = GetDistToObject(goodMine);
        float distanceToBadMine = GetDistToObject(badMine);
        //float distanceToCloserTank = GetDistToObject(nearTank);

        inputs[0] = dirToGoodMine.x;
        inputs[1] = dirToGoodMine.z;
        //inputs[2] = distanceToGoodMine;
        inputs[2] = dirToBadMine.x;
        inputs[3] = dirToBadMine.z;
        inputs[4] = distanceToBadMine;
        inputs[5] = transform.forward.x;
        inputs[6] = transform.forward.z;
        //inputs[8] = dirCloserTank.x;
        //inputs[9] = dirCloserTank.z;
        //inputs[10] = distanceToCloserTank;

        float[] output = brain.Synapsis(inputs);

        switch (PopulationManager.Instance.testIndex)
        {
            case 0:
                RewardIfCloseToGoodMine(distanceToGoodMine);
                break;
            case 1:
                //if (distanceToBadMine < 3)
                //{
                //    fitness -= 1f;
                //}
                //else
                //{
                //    RewardIfCloseToGoodMine(distanceToGoodMine);
                //}

                break;
            default:
            //    if (IsCollidingWithTank())
            //    {
            //        fitness -= 0.3f;
            //    }
                break;
        }

        SetForces(output[0], output[1], dt);
    }

    protected override void OnTakeMine(GameObject mine)
    {
        switch (PopulationManager.Instance.testIndex)
        {
            case 0:
                if (IsGoodMine(mine))
                {
                    SetFitness((fitness + 100) * 2);
                }
                break;
            case 1:
                if (IsGoodMine(mine))
                {
                    SetFitness((fitness + 100) * 2);
                }
                else
                {
                   // SetFitness(fitness - fitness / 3);
                }
                break;
            default:
                break;
        } 
    }

    #region PRIVATE_METHODS
    private void RewardIfCloseToGoodMine(float distanceToGoodMine)
    {
        if (distanceToGoodMine < 3)
        {
            SetFitness(fitness + 5f);
        }
    }
    #endregion

    #region AUX
    private void SetFitness(float fitness)
    {
        this.fitness = fitness;
        genome.fitness = fitness;
    }
    #endregion
}
