using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class ZoekGreenZone : Agent
{
    public Transform Target;
    public Transform Green_Zone;
    private bool HitEnemy = false;
    public override void OnEpisodeBegin()
    {

        HitEnemy = false;
        Target.GetComponent<SphereCollider>().enabled = true;
        Target.GetComponent<MeshRenderer>().enabled = true;

        //voor als de agent van de map valt kleiner als 0
        if (this.transform.localPosition.y < 0)
        {
            this.transform.localPosition = new Vector3(0,0.5f,0);
            this.transform.localRotation = Quaternion.identity; //dit is een standard vector voor rotation zoals 0,0,0 of 1,1,1
        }
        //je moet geen transform doen omdat het vanboven wordt aangeroepen 'public transform'
        //*8-4 dit is voor het bereik van random.value 0-1 naar -4 +4 te veranderen, zo word het target rond de oorsprong 0,0,0 geplaatst
        Target.localPosition = new Vector3(Random.value * 8 - 4, 0.5f, Random.value * 8 - 4);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //Agent posities
        //geeft de locatie van de agent door aan het neurale netwerk, zo kan het leren door de positie te weten
        sensor.AddObservation(Target.localPosition - this.transform.localPosition);
        sensor.AddObservation(Green_Zone.localPosition - this.transform.localPosition);//toevoegen van observable green_zone
        sensor.AddObservation(HitEnemy ? 1f : 0f);
    }

    public float speedMultiplier = 0.1f;
    public float rotationmultiplier = 5;
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        //per stap een afstraffing
        AddReward(-0.00001f);

        // Acties, size = 2
        Vector3 controlSignal = Vector3.zero;
        controlSignal.z = actionBuffers.ContinuousActions[0];//voor en achteruit bewegen
        transform.Translate(controlSignal * speedMultiplier);

        transform.Rotate(0.0f, rotationmultiplier* actionBuffers.ContinuousActions[1], 0.0f);

        // Beloningen
        float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.localPosition);
        //float distanceToGreenZone = Vector3.Distance(this.transform.localPosition, Green_Zone.localPosition);

        // target bereikt
        if (!HitEnemy && distanceToTarget < 1.42f)
        {
            HitEnemy = true;
            AddReward(0.8f);
            Target.GetComponent<MeshRenderer>().enabled = false; 
            Target.GetComponent<SphereCollider>().enabled = false;           
        }

        

        if (HitEnemy && this.transform.localPosition.z - Green_Zone.localPosition.z < 0.9f)
            {
                AddReward(0.2f);
                EndEpisode();
                
            }

         // Van het platform gevallen?
        else if (this.transform.localPosition.y < 0)
        {
            AddReward(-0.5f);
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
