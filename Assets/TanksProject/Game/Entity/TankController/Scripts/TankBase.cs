using UnityEngine;

namespace TanksProject.Game.Entity.TankController
{
    public class TankBase : MonoBehaviour
    {
        #region EXPOSED_FIELDS
        [SerializeField] protected float Speed = 10.0f;
        [SerializeField] protected float RotSpeed = 15.0f;
        [SerializeField] protected bool dead = false;
        #endregion

        #region PROTECTED_FIELDS
        protected Genome genome;
        protected NeuralNetwork brain;
        protected GameObject nearMine;
        protected GameObject nearTank;
        protected float[] inputs;
        #endregion

        #region PUBLIC_METHODS
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

        public void SetNearestTank(GameObject tank)
        {
            nearTank = tank;
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
            return (transform.position - obj.transform.position).sqrMagnitude <= 2.0f;
        }

        public void Think(float dt)
        {
            if (dead)
            {
                return;
            }

            OnThink(dt);

            if (IsCloseToMine(nearMine))
            {
                OnTakeMine(nearMine);
            }
        }
        #endregion

        #region PROTECTED_METHODS
        protected Vector3 GetDirToObject(GameObject obj)
        {
            return (obj.transform.position - transform.position).normalized;
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

        protected virtual void OnThink(float dt)
        {

        }

        protected virtual void OnTakeMine(GameObject mine)
        {
        }

        protected virtual void OnReset()
        {
        }
        #endregion
    }
}
