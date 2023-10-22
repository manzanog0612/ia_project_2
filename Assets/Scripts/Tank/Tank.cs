using UnityEngine;

public class Tank : TankBase
{
    float fitness = 0;
    protected override void OnReset()
    {
        base.OnReset();
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
        //(obstacle.transform.position - birdBehaviour.transform.position).x / 10.0f;
        inputs[0] = dirToGoodMine.x;
        inputs[1] = dirToGoodMine.z;
        inputs[2] = (goodMine.transform.position - transform.position).x / 10.0f;
        inputs[3] = (goodMine.transform.position - transform.position).z / 10.0f;
        inputs[4] = dirToBadMine.x;
        inputs[5] = dirToBadMine.z;
        inputs[6] = (badMine.transform.position - transform.position).x / 10.0f;
        inputs[7] = (badMine.transform.position - transform.position).z / 10.0f;
        inputs[8] = transform.forward.x;
        inputs[9] = transform.forward.z;
        //inputs[8] = dirCloserTank.x;
        //inputs[9] = dirCloserTank.z;
        //inputs[10] = distanceToCloserTank;

        float[] output = brain.Synapsis(inputs);

        switch (PopulationManager.Instance.testIndex)
        {
            case 0:
                RewardIfCloseToGoodMine(distanceToGoodMine);
                PunishIfCloseToBadMine(distanceToBadMine);
                break;
            case 1:
                break;
            //case 1:
                //if (distanceToBadMine < 3)
                //{
                //    fitness -= 1f;
                //}
                //else
                //{
                //    RewardIfCloseToGoodMine(distanceToGoodMine);
                //}

                //break;
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
            case 1:
                if (IsGoodMine(mine))
                {
                    SetFitness((fitness + 100) * 2);
                }
                else
                {
                    //SetDead(true);
                    SetFitness(fitness - 300);
                }
                break;
            //case 1:
            //    if (IsGoodMine(mine))
            //    {
            //        SetFitness((fitness + 100) * 2);
            //    }
            //    else
            //    {
            //       // SetFitness(fitness - fitness / 3);
            //    }
            //    break;
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

    private void PunishIfCloseToBadMine(float distanceToBadMine)
    {
        if (distanceToBadMine < 3)
        {
            SetFitness(fitness - 5f);
        }
    }
    #endregion

    #region AUX
    private void SetFitness(float fitness)
    {
        if (fitness < 0)
        {
            fitness = 0;
            //SetDead(true);
        }

        this.fitness = fitness;
        genome.fitness = fitness;
    }
    #endregion
}
