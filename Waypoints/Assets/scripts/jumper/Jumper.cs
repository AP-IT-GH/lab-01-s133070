using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections;


[RequireComponent(typeof(Rigidbody))]
public class Jumper : Agent
{
    private float springKracht = 4f;
    private float snelheid = 3f;
    private float afstandTeBewegen = 20f;
    private float minVertraging = 5.0f;
    private float maxVertraging = 9.0f;
    private float minVertragingMunt = 10.0f;
    private float maxVertragingMunt = 16.0f;
    private Rigidbody rb;
    private GameObject[] munten = new GameObject[0];
    private GameObject[] Obstakels1 = new GameObject[0];
    private GameObject[] Obstakels2 = new GameObject[0];
    private bool opGrond;
    private bool obstakelBeweegt;
    private bool muntBeweegt;

    [SerializeField] private Vector3 muntStartPositie = new Vector3(0f, 4.5f, -14f);
    [SerializeField] private Vector3 obstakel1StartPositie = new Vector3(0f, 0.5f, -14f);
    [SerializeField] private Vector3 obstakel2StartPositie = new Vector3(-14f, 0.5f, 0f);
    [SerializeField] private float maxAfstandVanStart = 2.0f;

    private Vector3 agentStartPositie;

    public override void OnEpisodeBegin()
    {
        Debug.Log("Herstart");
        //rigibody voor later op null te zetten
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        // Zet de agent terug op startpositie bij elke nieuwe episode (lokaal t.o.v. parent)
        transform.localPosition = new Vector3(0f, 0.5f, 0f);
        agentStartPositie = transform.localPosition;

        //reset van rigibody
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        obstakelBeweegt = false;
        muntBeweegt = false;

        if (Obstakels1.Length == 0)
        {
            Obstakels1 = GameObject.FindGameObjectsWithTag("Obstakel");
        }

        if (Obstakels2.Length == 0)
        {
            Obstakels2 = GameObject.FindGameObjectsWithTag("Obstakel2");
        }

        if (munten.Length == 0)
        {
            munten = GameObject.FindGameObjectsWithTag("Coin");
        }

        for (int i = 0; i < munten.Length; i++)
        {
            if (munten[i] != null)
            {
                munten[i].SetActive(true);
                munten[i].transform.localPosition = muntStartPositie;
            }
        }

        for (int i = 0; i < Obstakels1.Length; i++)
        {
            if (Obstakels1[i] != null)
            {
                Obstakels1[i].SetActive(true);
                Obstakels1[i].transform.localPosition = obstakel1StartPositie;
            }
        }

        for (int i = 0; i < Obstakels2.Length; i++)
        {
            if (Obstakels2[i] != null)
            {
                Obstakels2[i].SetActive(true);
                Obstakels2[i].transform.localPosition = obstakel2StartPositie;
            }
        }

        StopAllCoroutines();

        for (int i = 0; i < Obstakels1.Length; i++)
        {
            if (Obstakels1[i] != null)
            {
                StartCoroutine(MoveObjectWithRandomDelay(Obstakels1[i], true, Vector3.forward));
            }
        }

        for (int i = 0; i < Obstakels2.Length; i++)
        {
            if (Obstakels2[i] != null)
            {
                StartCoroutine(MoveObjectWithRandomDelay(Obstakels2[i], true, Vector3.right));
            }
        }

        for (int i = 0; i < munten.Length; i++)
        {
            if (munten[i] != null)
            {
                StartCoroutine(MoveObjectWithRandomDelay(munten[i], false, Vector3.forward));
            }
        }

    }


    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(opGrond ? 1f : 0f);
        sensor.AddObservation(rb.linearVelocity.y);
    }

    public override void OnActionReceived(ActionBuffers actieBuffers)
    {
        AddReward(0.0005f);

        float springInvoer = actieBuffers.ContinuousActions[0];
        //agent mag alleen als die op de grond is springen
        if (springInvoer > 0.5f && opGrond)
        {
            rb.AddForce(Vector3.up * springKracht, ForceMode.Impulse);
        }

        //alle objecten gebruikt, dan resetten nr oude positei
        if (AllObjectsEmpty())
        {
            EndEpisode();
        }

        //epsidoe herstarten als te ver vanaf begin gaat
        Vector3 currentPos = transform.localPosition;
        Vector2 currentXZ = new Vector2(currentPos.x, currentPos.z);
        Vector2 startXZ = new Vector2(agentStartPositie.x, agentStartPositie.z);
        if (Vector2.Distance(currentXZ, startXZ) > maxAfstandVanStart)
        {
            AddReward(-0.2f);
            EndEpisode();
        }

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continueActiesUit = actionsOut.ContinuousActions;
        continueActiesUit[0] = Input.GetKey(KeyCode.Space) ? 1f : 0f; 
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Obstakel") || collision.collider.CompareTag("Obstakel2"))
        {
            Debug.Log("obstakel graakt");
            AddReward(-1.0f);
            EndEpisode();
        }
        else if (collision.collider.CompareTag("Coin"))
        {
            collision.gameObject.SetActive(false);
            AddReward(1.5f);
        }
    }

    private void OnCollisionStay(Collision collision)
    {//hulp voor controle als de agent op de grond is of niet
        if (collision.gameObject.CompareTag("Plane"))
        {
            opGrond = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        //controle of die in de lucht is
        if (collision.gameObject.CompareTag("Plane"))
        {
            opGrond = false;
        }
    }
    private IEnumerator MoveObjectWithRandomDelay(GameObject objectToMove, bool isObstakel, Vector3 beweegRichting)
    {
        //telkens nieuwe wachttijd
        float wachttijd;
        if (isObstakel)
        {
            wachttijd = Random.Range(minVertraging, maxVertraging);
        }
        else
        {
            //de coins moeten minder snel voorkomenn
            wachttijd = Random.Range(minVertragingMunt, maxVertragingMunt);
        }
        //wachten totdat die wachtijd voorbij is
        yield return new WaitForSeconds(wachttijd);
        //wachttijd voorbij en dus terug op false dat nieuwe obstakel kan komen
        bool obstakelBezig = false;
        bool muntBezig = false;
        if (isObstakel)
        {
            //zorgen dat de obstakels niet tegelijk komen naar agent
            while (obstakelBeweegt)
            {
                yield return null;
            }

            obstakelBeweegt = true;
            obstakelBezig = true;
        }
        else
        {
            //zelfde als obstakels 1per1
            while (muntBeweegt)
            {
                yield return null;
            }

            muntBeweegt = true;
            muntBezig = true;
        }

        float stopTijd = Time.time + (afstandTeBewegen / snelheid);
        Vector3 richting = beweegRichting.normalized;

        while (objectToMove != null && objectToMove.activeInHierarchy && Time.time < stopTijd)
        {
            objectToMove.transform.localPosition += richting * snelheid * Time.deltaTime;

            yield return null;
        }

        if (objectToMove != null)
        {
            objectToMove.SetActive(false);
        }

        if (obstakelBezig)
        {
            obstakelBeweegt = false;
        }

        if (muntBezig)
        {
            muntBeweegt = false;
        }
    }

    private bool AllObjectsEmpty()
    {
        //bool die true of false zal geven als alles true is kan episode opnieuwe gestart worden
        return AllInactiveOrNull(Obstakels1) && AllInactiveOrNull(Obstakels2) && AllInactiveOrNull(munten);
    }

    private bool AllInactiveOrNull(GameObject[] obstakels)
    {
        // true enkel als alles null of inactief is
        for (int i = 0; i < obstakels.Length; i++)
        {
            if (obstakels[i] != null && obstakels[i].activeInHierarchy)
            {
                return false;
            }
        }

        return true;
    }

}
