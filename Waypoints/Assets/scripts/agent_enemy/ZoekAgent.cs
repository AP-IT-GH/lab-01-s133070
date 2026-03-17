using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class ZoekAgent : Agent
{
    public Transform Target;
    public override void OnEpisodeBegin()
    {
        //voor als de agent van de map valt kleiner als 0
        if (this.transform.localPosition.y < 0)
        {
            this.transform.localPosition = new Vector3(0,0.5f,0);
            this.transform.localRotation = Quaternion.identity; //dit is een standard vector voor rotation zoals 0,0,0 of 1,1,1
        }
        //je moet geen transform doen omdat het vanboven wordt aangeroepen 'public transform'
        Target.localPosition = new Vector3(Random.value * 8 - 4, 0.5f, Random.value * 8 - 4);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Target en Agent posities
        sensor.AddObservation(Target.localPosition);
        sensor.AddObservation(this.transform.localPosition);

    }

    public float speedMultiplier = 0.1f;
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Acties, size = 2
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actionBuffers.ContinuousActions[0];
        controlSignal.z = actionBuffers.ContinuousActions[1];
        transform.Translate(controlSignal * speedMultiplier);

        // Beloningen
        float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.localPosition);

        // target bereikt
        if (distanceToTarget < 1.42f)
        {
            SetReward(1.0f);
            EndEpisode();
        }

         // Van het platform gevallen?
        else if (this.transform.localPosition.y < 0)
        {
            EndEpisode();
        }
        }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Vertical");
        continuousActionsOut[1] = Input.GetAxis("Horizontal");
    }

}
