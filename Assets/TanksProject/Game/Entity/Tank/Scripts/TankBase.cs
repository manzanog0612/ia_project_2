using UnityEngine;

public class TankBase : MonoBehaviour
{
    public float Speed = 10.0f;
    public float RotSpeed = 15.0f;
    public bool dead = false;

    protected Genome genome;
	protected NeuralNetwork brain;
    protected GameObject nearMine;
    protected GameObject goodMine;
    protected GameObject badMine;
    protected GameObject nearTank;
    protected GameObject nearObstacle;
    protected float[] inputs;

    public void SetBrain(Genome genome, NeuralNetwork brain)
    {
        this.genome = genome;
        this.brain = brain;
        inputs = new float[brain.InputsCount];
        OnReset();
    }

    public void SetNearestMine(GameObject mine)
    {
        nearMine = mine;
    }

    public void SetGoodNearestMine(GameObject mine)
    {
        goodMine = mine;
    }

    public void SetBadNearestMine(GameObject mine)
    {
        badMine = mine;
    }

    public void SetNearestTank(GameObject tank)
    {
        nearTank = tank;
    }

    public void SetNearestObstacle(GameObject obstacle)
    {
        nearObstacle = obstacle;
    }

    public bool IsDead()
    {
        return dead;
    }

    public void SetDead(bool dead)
    {
        this.dead = dead;
    }

    public bool IsCollidingWithObstacle(GameObject obj)
    {
        return (this.transform.position - obj.transform.position).sqrMagnitude <= 2.0f;
    }


    protected bool IsGoodMine(GameObject mine)
    {
        return goodMine == mine;
    }

    protected Vector3 GetDirToObject(GameObject obj)
    {
        return (obj.transform.position - this.transform.position).normalized;
    }

    protected float GetDistToObject(GameObject obj)
    {
        return Vector3.Distance(obj.transform.position, transform.position);
    }

    protected bool IsCloseToMine(GameObject mine)
    {
        return (this.transform.position - nearMine.transform.position).sqrMagnitude <= 2.0f;
    }

    protected void SetForces(float leftForce, float rightForce, float dt)
    {
        Vector3 pos = transform.position;
        float rotFactor = Mathf.Clamp((rightForce - leftForce), -1.0f, 1.0f);
        transform.rotation *= Quaternion.AngleAxis(rotFactor * RotSpeed * dt, Vector3.up);
        pos += transform.forward * Mathf.Abs(rightForce + leftForce) * 0.5f * Speed * dt;
        transform.position = pos;
    }

	public void Think(float dt) 
	{
        if (dead)
        {
            return;
        }

        OnThink(dt);

        if(IsCloseToMine(nearMine))
        {
            OnTakeMine(nearMine);
            PopulationManager.Instance.RelocateMine(nearMine);
        }
	}

    protected virtual void OnThink(float dt)
    {

    }

    protected virtual void OnTakeMine(GameObject mine)
    {
    }

    protected virtual void OnReset()
    {
        //SetDead(false);
    }
}
