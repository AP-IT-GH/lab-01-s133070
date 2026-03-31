using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class Oblix : Agent
{
    public float speedMultiplier = 0.1f;
    public float rotationmultiplier = 5f;

    private bool HitStone;
    private int carryingStone = 0;
    private GameObject[] Stones = new GameObject[0];
    private GameObject[] CollectPointArray = new GameObject[0];

    public override void OnEpisodeBegin()
    {
        carryingStone = 0;
        HitStone = false;

        transform.localPosition = new Vector3(0, 0.5f, 0);
        transform.localRotation = Quaternion.identity;

//zonder reset start elke episode niet vanuit dezelfde fysische toestand
//waardoor de agent moeilijker correct leert.
//daarom restten van de rigibody
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        

        if (Stones.Length == 0)
        {
            Stones = GameObject.FindGameObjectsWithTag("Target");
        }

        if (CollectPointArray.Length == 0)
        {
            CollectPointArray = GameObject.FindGameObjectsWithTag("marker");
        }

        foreach (var stone in Stones)
        {
            stone.SetActive(true);
        }

        foreach (var collectionpoint in CollectPointArray)
        {
            collectionpoint.SetActive(true);
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(this.transform.localPosition);
        sensor.AddObservation(HitStone);
        
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        AddReward(-0.0002f);

        Vector3 controlSignal = Vector3.zero;
        controlSignal.z = actionBuffers.ContinuousActions[0];
        transform.Translate(controlSignal * speedMultiplier);

        transform.Rotate(0.0f, rotationmultiplier * actionBuffers.ContinuousActions[1], 0.0f);

        if (transform.localPosition.y < 0)
        {
            AddReward(-1.0f);
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Vertical");
        continuousActionsOut[1] = Input.GetAxis("Horizontal");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!HitStone && collision.gameObject.CompareTag("Target"))
        {
            HitStone = true;
            carryingStone += 1;
            AddReward(0.5f);
            collision.gameObject.SetActive(false);
        }
        else if (HitStone && collision.gameObject.CompareTag("marker"))
        {
            AddReward(2.0f);
            HitStone = false;
            carryingStone -= 1;
            collision.gameObject.SetActive(false);

            if (GameObject.FindGameObjectsWithTag("Target").Length == 0 && carryingStone == 0)
            {
                EndEpisode();
            }
        }
        else if (HitStone && collision.gameObject.CompareTag("Target"))
        {
            AddReward(-0.1f);
        }
    }
}