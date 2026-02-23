using System;
using UnityEngine;

public class FollowWaypoint : MonoBehaviour
{
    
    public float rotSpeed = 10.0f;
    public float speed = 10.0f;

    private float waitTracker = 5.0f;

    public GameObject[] waypoints;

    int currentWaypoint = 0;

    GameObject tracker;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tracker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Destroy(tracker.GetComponent<Collider>());
        //tracker.GetComponent<MeshRenderer>().enabled = false;
        tracker.transform.position = this.transform.position;
        tracker.transform.rotation = this.transform.rotation;
        
    }

    void Progress(){
        if(Vector3.Distance(tracker.transform.position, this.transform.position) > waitTracker)return;

        if(Vector3.Distance(tracker.transform.position, waypoints[currentWaypoint].transform.position) < 1){
            currentWaypoint++;
        }

        if(currentWaypoint >= waypoints.Length){
            currentWaypoint = 0;
        }

        tracker.transform.LookAt(waypoints[currentWaypoint].transform);
        tracker.transform.Translate(0, 0, (speed + 2) * Time.deltaTime);
    }

    // Update is called once per frame
    void Update()
    {
        Progress();
        
        Quaternion lookRotation = Quaternion.LookRotation(tracker.transform.position - this.transform.position);
        
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, lookRotation,
        Time.deltaTime * rotSpeed);

        this.transform.Translate(0, 0, speed * Time.deltaTime);

    }
}
