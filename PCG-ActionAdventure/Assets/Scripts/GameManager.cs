using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	public GraphToMapConverter graphToMapConverter;
	public GraphGenerator graphGenerator;
	public MeshGenerator meshGenerator;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void PlacePlayer(){
		List<Vector3> roomCenters = graphToMapConverter.roomCenterCoords;
		int startNodeNumber  = System.Array.IndexOf (graphGenerator.nodeArray, graphGenerator.startNode);
		Debug.Log (startNodeNumber);
	}
}
