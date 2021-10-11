using UnityEngine;
using System.Collections.Generic;

public class Pathfinder : MonoBehaviour
{

	private bool debug = false;

	//public List<Vector3> grid;
	private List<Node> Grid;
	// for nested it would be 
	//public List<List<Vector3>> grid;
    private List<Vector3> path;
    public List<Vector3> neighbours;

	//sets the size between points
	public float scale;


	//number of points across and up
	public float height;
	public float width;

	//goal
    public Vector3 goal;
    public Vector3 scaledGoal;
    public Vector3 shiftedGoal;
	//start poinit
	public Vector3 startPoint;
    public Vector3 scaledStart;
    public Vector3 shiftedStart;

    


	//fake gizmos
	//public List<GameObject> fakeGizomos;


	// Use this for initialization
	void Start () 
	{
		
		// its to stop errors when not running. Trust me
		debug = true;

		//you have to do this apparently
		//grid = new List<Vector3>();
		Grid = new List<Node> ();
		path = new List<Vector3>();
		neighbours = new List<Vector3> ();

		//so for the nested grids you would need
		// grid = new List<List<Vector3>>
		//subgrid = new List<Vector3>

		//applying values
		scale = 2.0f;
		height = 33;
		width = 72;

        

		goal = new Vector3 (Random.Range(0, width), 0, Random.Range(0, height));
		startPoint = new Vector3 (6, 0, 7);

        //looking back this could all be done in the draw phase. saves on mathematics. Perhaps on a redo

        scaledStart = new Vector3(startPoint.x * scale, 0, startPoint.z * scale);
        scaledGoal = new Vector3(goal.x * scale, 0, goal.z * scale);

        //this makes it nice on the grid, as far as I can tell
        if ((goal.z % 2) != 0)
        {
            shiftedGoal = (scaledGoal + new Vector3((0.5f * scale), 0, 0));
        }
        if ((startPoint.z % 2) != 0)
        {
            shiftedStart = (scaledStart + new Vector3((0.5f * scale), 0, 0));
        }

        

      
		//create the grid
		CreateGrid();
        //CreateHexGrid();

        //one in ten chance of an obstacle 
        for (int i = 0; i < Grid.Count; i++)
        {
            if(Random.Range(0,5) == 2)
            {
                Grid[i].passable = false;

            }
        }

        //Grid[22].passable = false;
        //Grid[23].passable = false;
        //Grid[24].passable = false;
        //Grid[25].passable = false;
        //Grid[26].passable = false;

        //Grid[44].passable = false;
        //Grid[45].passable = false;
        //Grid[46].passable = false;
        //Grid[47].passable = false;
        //Grid[48].passable = false;
        //Grid[49].passable = false;

        //Grid[54].passable = false;
        //Grid[55].passable = false;
        
        //CalculateAStarPath();
        CalculateDJPath();
        

        //CalculateHexAStarPath();

	}

    void CreateHexGrid()
    {
        //empty any other members of the grid
        Grid.Clear();

        //draw grid
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {

                int d = i % 2;
                
                if (d != 0)
                {
                    Grid.Add(new Node(new Vector3((j * scale) + (0.5f * scale), 0, ((i * scale))), (int)((i * width) + j)));

                }
                else
                {
                    Grid.Add(new Node(new Vector3((j * scale), 0, (i * scale)), (int)((i * width) + j)));

                }

                /* see later notes on gizmos for the reason.
				//testing on replacing gizmos
				int num = (int)((i*width)+j) ;

				fakeGizomos.Add (GameObject.CreatePrimitive (PrimitiveType.Sphere));
				//this line makes all the new spheres children of the cube
				fakeGizomos [num].transform.parent = transform;

				fakeGizomos[num].transform.position = Grid [num].location;
				*/
            }
        }
    }

	void CreateGrid()
	{
		//empty any other members of the grid
		Grid.Clear ();

		//draw grid
		for (int i = 0; i < height; i++) 
		{
			for (int j = 0; j < width; j++) 
			{
                
                    Grid.Add(new Node(new Vector3 ((j* scale), 0, (i*scale)),(int)((i*width) + j)));
                
				/* see later notes on gizmos for the reason.
				//testing on replacing gizmos
				int num = (int)((i*width)+j) ;

				fakeGizomos.Add (GameObject.CreatePrimitive (PrimitiveType.Sphere));
				//this line makes all the new spheres children of the cube
				fakeGizomos [num].transform.parent = transform;

				fakeGizomos[num].transform.position = Grid [num].location;
				*/
			}
		}
	}

	// Update is called once per frame does nothing, basicaly 
	void Update () 
	{
		//ChangeColour ();
	}

	int GetArraylocationFromVector (Vector3 location)
	{
       
       

        float descaledx = location.x;
        descaledx = (descaledx / scale);

        float descaledz = location.z;
        descaledz = (descaledz / scale);
        descaledz = (descaledz *width);

        return (int)(descaledx + descaledz);
	}

    int GetHexGridArrayLocationFromVector(Vector3 location)
    {

        float descaledx = location.x;
        float descaledz = location.z;

        if (location.z % 2 != 0)
        {
            descaledx -= (0.5f * scale);
        }
        
            
             descaledx = (descaledx / scale);

             
             descaledz = (descaledz / scale);
             descaledz = (descaledz * width);

             return (int)(descaledx + descaledz);

        



       
    }

	int CalculateManhattanDist(Vector3 target, Vector3 start)
	{
		if(((target.x >= (width*scale)) || (target.x < 0)) || ((target.z >= (height* scale)) || (target.z < 0))) 
		{
			Debug.Log ("Target Out of Bounds");
			target = new Vector3 (0, 0, 0);
		}
		if(((start.x >= (width*scale)) || (start.x < 0)) || ((start.z >= (height* scale)) || (start.z < 0))) 
		{
			Debug.Log ("Start Out of Bounds");
			start = new Vector3 (0, 0, 0);
		}
        
       
		return (int)((Mathf.Abs(target.x - start.x)) + (Mathf.Abs(target.z - start.z)));

	}

    float CalculateHexManhattanDist(Vector3 target, Vector3 start)
    {

       return  (float)((Mathf.Abs(target.x - start.x) + Mathf.Abs(target.z - start.z) + Mathf.Abs((target.z - target.x) - (start.z - start.x))) / 2);
    }

	void CalculateAStarPath()
	{
		//test every node around current point, select one with least heuristic, add to path, again for all open nodes

		path.Clear ();

		int currentNodeInGrid;
	   


        //assuming the start point has not been scaled
		currentNodeInGrid = GetArraylocationFromVector (scaledStart);

        Grid[currentNodeInGrid].costFromStart = 0;

		//path.Add (Grid [currentNodeInGrid].location);

			//for the 3*3 you need x-1, (z-1, z, z+1) x, (z-1, z+1) x+1, (z-1, z, z+1)

			// gets the locations around the point (also the point,which is 5 in the list)

        //(path[c] != Grid[GetArraylocationFromVector(scaledGoal)].location)

        for (int c = 0; (Grid[currentNodeInGrid].location != Grid[GetArraylocationFromVector(scaledGoal)].location); c++)
        {

            neighbours.Clear();

             for (int i = -1; i < 2; i++)
                        {
                            for (int j = -1; j < 2; j++)
                            {
                                Vector3 currentPoint = Grid[currentNodeInGrid].location;
                                neighbours.Add((currentPoint += new Vector3((i * scale), 0, (j * scale))));
                            }
                        }

            
        
			            for (int i = 0; i < 9; i++) 
                        {
				            // makes sure the neighbours location is in the grid
			                if (((neighbours [i].x >= (width*scale)) || (neighbours [i].x < 0)) || ((neighbours [i].z >= (height*scale)) || (neighbours [i].z < 0))) 
                            {
				               // Debug.Log ("Neighbour out of Bounds");
				                neighbours [i] = new Vector3 (-1, 0, -1);
			                }


                            int currentGrid = GetArraylocationFromVector(neighbours[i]);


                            //assuming the location is in the grid, not the current location and passable it then gets the Heuristic and adds it to the coresponding node in the grid. stores the lowest heuristic and which of the neighbours it is

                            if ((neighbours[i] != new Vector3(-1, 0, -1)) && (Grid[currentGrid].passable == true) && (i != 4) && (Grid[currentGrid].open == true))
                            {
                                Grid[currentGrid].considered = true;


                                //this one gives the heuristic to the goal

                                Grid[currentGrid].heuristic = CalculateManhattanDist(scaledGoal, neighbours[i]);

                                //if the current node has no cost from start calculated or a shorter route has been found then

                                if ((Grid[currentGrid].costFromStart == 117) || (Grid[currentGrid].costFromStart > (Grid[currentNodeInGrid].costFromStart + CalculateManhattanDist(Grid[currentNodeInGrid].location, neighbours[i]))))
                                {
                                     //this one gets its distance from start by taking the distance from start of the current centre node and the manhatan distance to the current node
                                    Grid[currentGrid].costFromStart = (Grid[currentNodeInGrid].costFromStart + CalculateManhattanDist(Grid[currentNodeInGrid].location, neighbours[i]));
                                    
                                    //projected cost modified using these two
                                    Grid[currentGrid].projectedCost = (int)(Grid[currentGrid].heuristic + Grid[currentGrid].costFromStart);

                               
                                    //stores the location it came from as the shortest distance to it
                                    Grid[currentGrid].shortestPointFrom = Grid[currentNodeInGrid].location;
                                }


                                //by this point all the neighbours have a heuristic, costfrom start, &projected cost


                            }

				           
			            }

			            //sets current to the point with the lowest heuristic (it did)
                        Grid[currentNodeInGrid].open = false;

                        //currentNodeInGrid = GetArraylocationFromVector(neighbours[lowestProjectedCostLocation]);
                       
                        


                        int currentLowestOpenCost = 3000;
                        
                        for (int i = 0; i < Grid.Count; i++)
                        {
                            if (Grid[i].considered && Grid[i].open && (currentLowestOpenCost > Grid[i].projectedCost)&& (i != currentNodeInGrid))
                            {
                                currentLowestOpenCost = Grid[i].projectedCost;
                                currentNodeInGrid = i;
                                
                            }

                        }

        }

       BacktrackPath();

	}

    void CalculateDJPath()
    {
        path.Clear();
        int currentNodeInGrid;
        currentNodeInGrid = GetArraylocationFromVector(scaledStart);
        Grid[currentNodeInGrid].costFromStart = 0;

        for (int c = 0; c < (width * height); c++)
        {

            neighbours.Clear();

            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    Vector3 currentPoint = Grid[currentNodeInGrid].location;
                    neighbours.Add((currentPoint += new Vector3((i * scale), 0, (j * scale))));
                }
            }



            for (int i = 0; i < 9; i++)
            {
                // makes sure the neighbours location is in the grid
                if (((neighbours[i].x >= (width * scale)) || (neighbours[i].x < 0)) || ((neighbours[i].z >= (height * scale)) || (neighbours[i].z < 0)))
                {
                    // Debug.Log ("Neighbour out of Bounds");
                    neighbours[i] = new Vector3(-1, 0, -1);
                }


                int currentGrid = GetArraylocationFromVector(neighbours[i]);


                //assuming the location is in the grid, not the current location and passable it then gets the Heuristic and adds it to the coresponding node in the grid. stores the lowest heuristic and which of the neighbours it is

                if ((neighbours[i] != new Vector3(-1, 0, -1)) && (Grid[currentGrid].passable == true) && (i != 4) && (Grid[currentGrid].open == true))
                {
                    Grid[currentGrid].considered = true;


                    //this one gives the heuristic to the goal

                    Grid[currentGrid].heuristic = CalculateManhattanDist(scaledGoal, neighbours[i]);

                    //if the current node has no cost from start calculated or a shorter route has been found then

                    if ((Grid[currentGrid].costFromStart == 117) || (Grid[currentGrid].costFromStart > (Grid[currentNodeInGrid].costFromStart + CalculateManhattanDist(Grid[currentNodeInGrid].location, neighbours[i]))))
                    {
                        //this one gets its distance from start by taking the distance from start of the current centre node and the manhatan distance to the current node
                        Grid[currentGrid].costFromStart = (Grid[currentNodeInGrid].costFromStart + CalculateManhattanDist(Grid[currentNodeInGrid].location, neighbours[i]));

                        //projected cost modified using these two
                        Grid[currentGrid].projectedCost = (int)(Grid[currentGrid].heuristic + Grid[currentGrid].costFromStart);


                        //stores the location it came from as the shortest distance to it
                        Grid[currentGrid].shortestPointFrom = Grid[currentNodeInGrid].location;
                    }


                    //by this point all the neighbours have a heuristic, costfrom start, &projected cost


                }


            }

            //sets current to the point with the lowest heuristic (it did)
            Grid[currentNodeInGrid].open = false;

            //currentNodeInGrid = GetArraylocationFromVector(neighbours[lowestProjectedCostLocation]);




            int currentLowestOpenCost = 3000;

            for (int i = 0; i < Grid.Count; i++)
            {
                if (Grid[i].considered && Grid[i].open && (currentLowestOpenCost > Grid[i].projectedCost) && (i != currentNodeInGrid))
                {
                    currentLowestOpenCost = Grid[i].projectedCost;
                    currentNodeInGrid = i;

                }

            }

        }

        BacktrackPath();
    }

    void CalculateHexAStarPath()
    {
        //setup
        int currentNodeInGrid;
        currentNodeInGrid = GetArraylocationFromVector(shiftedStart);
        Grid[currentNodeInGrid].costFromStart = 0;

        //neighbours
        

        //(Grid[currentNodeInGrid].location != Grid[GetArraylocationFromVector(scaledGoal)].location)


        for (int c = 0; (Grid[currentNodeInGrid].location != Grid[GetArraylocationFromVector(shiftedGoal)].location); c++)
        {

            neighbours.Clear();

            neighbours.Add(Grid[currentNodeInGrid].location + new Vector3((1 * scale), 0, 0));
            neighbours.Add(Grid[currentNodeInGrid].location - new Vector3((1 * scale), 0, 0));

            neighbours.Add(Grid[currentNodeInGrid].location + new Vector3((0.5f * scale), 0, (1 * scale)));
            neighbours.Add(Grid[currentNodeInGrid].location - new Vector3((0.5f * scale), 0, (1 * scale)));

            neighbours.Add(Grid[currentNodeInGrid].location + new Vector3((0.5f * scale), 0, (-1 * scale)));
            neighbours.Add(Grid[currentNodeInGrid].location - new Vector3((0.5f * scale), 0, (-1 * scale)));

            for (int i = 0; i < neighbours.Count; i++)
            {
                // makes sure the neighbours location is in the grid
                if (((neighbours[i].x >= ((width * scale) + (0.5 * scale))) || (neighbours[i].x < 0)) || ((neighbours[i].z >= (height * scale)) || (neighbours[i].z < 0)))
                {
                    // Debug.Log ("Neighbour out of Bounds");
                    neighbours[i] = new Vector3(-1, 0, -1);
                }

                int currentGrid = GetHexGridArrayLocationFromVector(neighbours[i]); 


                //assuming the location is in the grid, not the current location and passable it then gets the Heuristic and adds it to the coresponding node in the grid. stores the lowest heuristic and which of the neighbours it is

                if ((neighbours[i] != new Vector3(-1, 0, -1)) && (Grid[currentGrid].passable == true)  && (Grid[currentGrid].open == true))
                {
                    Grid[currentGrid].considered = true;


                    //this one gives the heuristic to the goal

                    Grid[currentGrid].heuristic = CalculateHexManhattanDist(shiftedGoal, neighbours[i]);

                    //if the current node has no cost from start calculated or a shorter route has been found then

                    if ((Grid[currentGrid].costFromStart == 117) || (Grid[currentGrid].costFromStart > (Grid[currentNodeInGrid].costFromStart + CalculateHexManhattanDist(Grid[currentNodeInGrid].location, neighbours[i]))))
                    {
                        //this one gets its distance from start by taking the distance from start of the current centre node and the manhatan distance to the current node
                        Grid[currentGrid].costFromStart = (Grid[currentNodeInGrid].costFromStart + CalculateHexManhattanDist(Grid[currentNodeInGrid].location, neighbours[i]));

                        //projected cost modified using these two
                        Grid[currentGrid].projectedCost = (int)(Grid[currentGrid].heuristic + Grid[currentGrid].costFromStart);


                        //stores the location it came from as the shortest distance to it
                        Grid[currentGrid].shortestPointFrom = Grid[currentNodeInGrid].location;
                    }


                    //by this point all the neighbours have a heuristic, costfrom start, &projected cost


                }
            }


            //sets current to the point with the lowest heuristic (it did)
            Grid[currentNodeInGrid].open = false;

            //currentNodeInGrid = GetArraylocationFromVector(neighbours[lowestProjectedCostLocation]);




            int currentLowestOpenCost = 3000;

            for (int i = 0; i < Grid.Count; i++)
            {
                if (Grid[i].considered && Grid[i].open && (currentLowestOpenCost > Grid[i].projectedCost) && (i != currentNodeInGrid))
                {
                    currentLowestOpenCost = Grid[i].projectedCost;
                    currentNodeInGrid = i;

                }

            }

        }

       // BacktrackPath();
        
        path.Clear();
        path.Add(Grid[GetArraylocationFromVector(shiftedGoal)].location);
        for (int i = 0; path[i] != shiftedStart; i++)
            path.Add(Grid[GetArraylocationFromVector(path[i])].shortestPointFrom);
        

    }

    //once you reach the end this links each point till they hit the start
    void BacktrackPath()
    {
        path.Clear();

        path.Add (Grid[GetArraylocationFromVector(scaledGoal)].location);

        for (int i = 0; path[i] != scaledStart; i++ )

            path.Add(Grid[GetArraylocationFromVector(path[i])].shortestPointFrom);

    }

    //makes the fancy look happen
	void OnDrawGizmos () 
	{

		//with the set of spheres this is to be phased out; on second thoughts. This works nicer than the spheres at this point so. its coming back
		//primitives on the backburner

		// this if stops a host of null pointer object not found errors when the game is not running

	
		if (debug) 
		{



			// get the location in the array of the end point. also keeps the target inside of the nave mesh
			float targetpoint = GetArraylocationFromVector(scaledGoal);


            //this keeps the target in the grid
			if(((goal.x >= width) || (goal.x < 0)) || ((goal.y >= height) || (goal.y < 0))) 
			{
				Debug.Log ("Out of Bounds");
				goal = new Vector3 (0, 0, 0);
			}
				
			int targetstart = GetArraylocationFromVector (scaledStart);
            
			//draw all the spheres
			for (int i = 0; i < Grid.Count; i++) 
			{

				if (i == targetstart) {
					Gizmos.color = Color.clear;

				} 
				else if (i == targetpoint)
				{
					Gizmos.color = Color.red;

				}
				else if (Grid [i].passable == false) {
					Gizmos.color = Color.black;

				}
                else if (Grid[i].open == false)
                {
                    Gizmos.color = Color.green;
                }
				else if (Grid [i].considered == true) 
				{
                    Gizmos.color = Color.blue ;
				}
				else
				{
					Gizmos.color = new Color(0.3f,0.3f,0.3f);

				}

				Gizmos.DrawSphere (Grid [i].location, 0.5f);


			}

            Gizmos.color = Color.red;

			if (path.Count > 1) 
			{
				for (int i = 1; i<path.Count; i++) 
					{
					Gizmos.DrawLine (path [i - 1], path [i]);
					}

			}









		}



	}
    //functionaly empty
	void ChangeColour()
	{
		if (debug) 
		{

			/* this is on hold for the moment due to me getting ahead of myself
			// get the location in the array of the end point. also keeps the target inside of the nave mesh
			float targetpoint = ((goal.z * width) + (goal.x));
			if(((goal.z >= width) || (goal.z < 0)) || ((goal.x >= height) || (goal.x < 0))) 
			{
				Debug.Log ("Out of Bounds");
				goal = new Vector3 (0, 0, 0);
			}


			//draw all the spheres
			for (int i = 0; i < Grid.Count; i++) 
			{

				if (i == targetpoint) 
				{
					fakeGizomos [i].GetComponent<Renderer> ().material.color = Color.red;

				} else if(Grid[i].open == false) {

					fakeGizomos [i].GetComponent<Renderer> ().material.color = Color.black;
				} else {
					fakeGizomos [i].GetComponent<Renderer> ().material.color = Color.blue;
				}


			}
			*/
		}
	}

}

public class Node 
{
	public Vector3 location;
    public Vector3 shortestPointFrom;

	public bool passable;
	public bool open;
    public bool considered;

	public float heuristic;
	public int gridLocation;

    public float costFromStart;
    public int projectedCost;


	public Node()
	{
		location = Vector3.zero;
		passable = true;
		open = true;
        considered = false;
		heuristic = -69;
        costFromStart = 117;
        projectedCost = -404;
	}

	public Node(Vector3 place)
	{
		location = place;
		passable = true;
		open = true;
        considered = false;

		heuristic = -69;
        costFromStart = 117;
        projectedCost = -404;

	}

	public Node(Vector3 place, bool state)
	{
		location = place;
		open = state;
		passable = true;
        considered = false;

		heuristic = -69;
        costFromStart = 117;
        projectedCost = -404;

	}

	public Node(Vector3 place, int arrayLocation)
	{
		location = place;
		passable = true;
		open = true;
        considered = false;

		heuristic = -69;
		gridLocation = arrayLocation;
        costFromStart = 117;
        projectedCost = -404;

	}

	public Node(Vector3 place, bool state, int arrayLocation)
	{
		location = place;
		open = state;
		passable = true;
        considered = false;

		heuristic = -69;
		gridLocation = arrayLocation;
        costFromStart = 117;
        projectedCost = -404;

	}

}
