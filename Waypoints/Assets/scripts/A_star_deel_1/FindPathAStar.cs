using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PathMarker {

    public MapLocation location;
    public float G, H, F;
    public GameObject marker;
    public PathMarker parent;

    public PathMarker(MapLocation l, float g, float h, float f, GameObject m, PathMarker p) {

        location = l;
        G = g;
        H = h;
        F = f;
        marker = m;
        parent = p;
    }

    public override bool Equals(object obj) {

        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            return false;
        else
            return location.Equals(((PathMarker)obj).location);
    }

   
}

public class FindPathAStar : MonoBehaviour {

    public Maze maze;
    public Material closedMaterial;
    public Material openMaterial;
    public GameObject start;
    public GameObject end;
    public GameObject pathP;




    PathMarker startNode;
    PathMarker goalNode;
    PathMarker lastPos;
    bool done = false;
    bool hasStarted = false;

    List<PathMarker> open = new List<PathMarker>();
    List<PathMarker> closed = new List<PathMarker>();

    void RemoveAllMarkers() {

        GameObject[] markers = GameObject.FindGameObjectsWithTag("marker");

        foreach (GameObject m in markers) Destroy(m);

        GameObject goal = GameObject.FindGameObjectWithTag("Goal");
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Destroy(goal);
        Destroy(player);
    }

    void BeginSearch() {

        done = false;
        RemoveAllMarkers();

        List<MapLocation> locations = new List<MapLocation>();

        for (int z = 1; z < maze.depth - 1; ++z) {
            for (int x = 1; x < maze.width - 1; ++x) {

                if (maze.map[x, z] != 1) {
                    locations.Add(new MapLocation(x, z));
                }
            }
        }
        //locations.Shuffle();

        Vector3 startLocation = new Vector3(1, 0.5f, 1);
        startNode = new PathMarker(new MapLocation(1, 1),
            0.0f, 0.0f, 0.0f, Instantiate(start, startLocation, Quaternion.identity), null);

        Vector3 endLocation = new Vector3(Random.Range(5, 8), 0.5f, Random.Range(5, 8));
        goalNode = new PathMarker(new MapLocation((int)endLocation.x, (int)endLocation.z),
            0.0f, 0.0f, 0.0f, Instantiate(end, endLocation, Quaternion.identity), null);

        open.Clear();
        closed.Clear();

        open.Add(startNode);
        lastPos = startNode;
    }

    void Search(PathMarker thisNode) {

        if (thisNode.Equals(goalNode)) {

              done = true;
            
            return;
        }

        foreach (MapLocation dir in maze.directions) {

            MapLocation neighbour = dir + thisNode.location;

            if (neighbour.x < 1 || neighbour.x > maze.width || neighbour.z < 1 || neighbour.z > maze.depth) continue;

            if (maze.map[neighbour.x, neighbour.z] == 1) continue;
            if (IsClosed(neighbour)) continue;

            float g = Vector2.Distance(thisNode.location.ToVector(), neighbour.ToVector()) + thisNode.G;
            float h = Vector2.Distance(neighbour.ToVector(), goalNode.location.ToVector());
            float f = g + h;

            GameObject pathBlock = Instantiate(pathP, new Vector3(neighbour.x * maze.scale, 0.0f, neighbour.z * maze.scale), Quaternion.identity);

            if (!UpdateMarker(neighbour, g, h, f, thisNode)) {

                open.Add(new PathMarker(neighbour, g, h, f, pathBlock, thisNode));
            }
        }
        open = open.OrderBy(p => p.F).ToList<PathMarker>();
        PathMarker pm = (PathMarker)open.ElementAt(0);
        closed.Add(pm);

        open.RemoveAt(0);
        //pm.marker.GetComponent<Renderer>().material = closedMaterial;

        lastPos = pm;
    }

    bool UpdateMarker(MapLocation pos, float g, float h, float f, PathMarker prt) {

        foreach (PathMarker p in open) {

            if (p.location.Equals(pos)) {

                p.G = g;
                p.H = h;
                p.F = f;
                p.parent = prt;
                return true;
            }
        }
        return false;
    }

    bool IsClosed(MapLocation marker) {

        foreach (PathMarker p in closed) {

            if (p.location.Equals(marker)) return true;
        }
        return false;
    }

    void Start() {

    }

    void Update() {

        if (Input.GetKeyDown(KeyCode.P)) {

            BeginSearch();
            hasStarted = true;   

            StopAllCoroutines(); //STOP VORIGE ZOEKTOCHT
            StartCoroutine(Searching()); //START AUTO A*
        }
        
// WEG HALEN VAN ZELF ITEREREN
        //if (hasStarted)
            //if (Input.GetKeyDown(KeyCode.C)) Search(lastPos);
    }

    // The coroutine function
    bool searchingHasFinished = false;
    IEnumerator Searching()
    {
        Debug.Log("searching started!");

        while (!done)
        {
            //Perform some task
            Debug.Log("Coroutine is running...");
            Search(lastPos);
            //Wait for the next frame
            yield return true;
        }

        searchingHasFinished = true;

        ReconstructPath(); //BOUWEN VAN PAD
        StartCoroutine(FollowPath());
        yield return null;

        Debug.Log("Coroutine finished!");
    }


    bool PathHasConstructed = false;
    List<PathMarker> path = new List<PathMarker>();

    // BOUWT HET PAD VAN START NAAR GOAL
    void ReconstructPath()
    {
        //Oude paden verwijderen zodat je bij drukken van 'P' een nieuwe lijst begint.
        path.Clear();
        
        path.Add(closed[closed.Count-1]);
        var p = closed[closed.Count-1].parent;
        while(p!= startNode)
        {
            path.Insert(0, p);
            p = p.parent;
        }
        path.Insert(0,startNode);
        PathHasConstructed = true;


    }

    //NIEUWE IEnumerator ZELF TOEGEVOEGD
    // WE GEBRUIKEN IEnumerator, omdat je deze dan kan gebruiken in StartCoroutines
    // 
    IEnumerator FollowPath()
{
    // Vind player via tag
    GameObject player = GameObject.FindGameObjectWithTag("Player");

    // Loop door alle nodes in het pad die we gemaakt hebben in de ReconstructPath
    // start bij 1 want 0 = startPos
    for (int i = 1; i < path.Count; i++)
    {
        MapLocation loc = path[i].location;

        // Zet positie van player naar volgende node
        //je doet * maze.scale zodat het wordt veranderd naar unity wereld, maar scale staat in Maze.CS op 1 dus eigenlijk is het niet nodig in deze scenario
        player.transform.position = new Vector3(loc.x * maze.scale, 0.5f, loc.z * maze.scale);

        // Wacht 1 seconde voor volgende stap
        yield return new WaitForSeconds(1f);
    }
}
   
}
