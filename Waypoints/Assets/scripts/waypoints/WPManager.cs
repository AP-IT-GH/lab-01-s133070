using UnityEngine;


//Maakt klasses zichtbaar in de Inspector en slaat data op.
[System.Serializable]
//dit zal de 'edge' of links tussen nodes bepalen.
public struct Link
{
    public enum direction { UNI, BI }
    public GameObject node1;
    public GameObject node2;
    
    //gaat bijhouden of het unidirectioneel of bidirectioneel is tussen de nodes(type link dus UNI of BI)
    public direction dir;
}

public class WPManager : MonoBehaviour
{

    public GameObject[] waypoints;
    public Link[] links;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
