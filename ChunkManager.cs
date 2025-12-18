using UnityEngine;
using System.Collections.Generic;

public class ChunkManager: MonoBehaviour
{
	public int chunkSize=32;
	public WorldGenerator worldGenerator;
	public int chunksOutOfScreenToComplete = 4;

	private Dictionary<Vector2Int, GameObject> allChunks;
	private int screenWidth, screenHeight;

	private HashSet<GameObject> previousActiveChunks;
    private HashSet<GameObject> currentActiveChunks;

    // Use this for initialization
    void Start()
	{
        screenWidth = Screen.width;
        screenHeight = Screen.height;

		allChunks = new Dictionary<Vector2Int, GameObject>();
		previousActiveChunks = new HashSet<GameObject>();
		currentActiveChunks = new HashSet<GameObject>();
    }

	// Update is called once per frame
	void Update()
	{
		Vector3 screenCenter = new Vector3((float)screenWidth/2,(float)screenHeight/2,0f );
        Vector3 centralPos = Camera.main.ScreenToWorldPoint(screenCenter);
        Vector3 screenCorner = new Vector3( screenWidth, screenHeight, 0f );
        Vector3 cornerPos = Camera.main.ScreenToWorldPoint(screenCorner);

        //Debug.Log("Screen: " + screenCorner.x + ";" + screenCorner.y);
        //Debug.Log("World: " + cornerPos.x + ";" + cornerPos.y);

		float distanceToX = cornerPos.x - centralPos.x;
        float distanceToY = cornerPos.y - centralPos.y;
		//Debug.Log(distanceToX+"; "+distanceToY);

		int x = (((int)cornerPos.x) / chunkSize)*chunkSize;
        int y = (((int)cornerPos.y) / (chunkSize / 2))* (chunkSize / 2);

		int outOfScreen = 0;
		bool goUp = false;
		bool wasVisibles = true;
        x += chunkSize / 2;
        y += chunkSize / 4;

        while (outOfScreen < chunksOutOfScreenToComplete)
		{
			if (!((x-distanceToX<centralPos.x+distanceToX && x + distanceToX > centralPos.x - distanceToX) && (y - distanceToY < centralPos.y + distanceToY && y + distanceToY > centralPos.y - distanceToY)))
			{
				//Debug.Log("Out of screen!"+x+";"+y);
				outOfScreen++;

				if (wasVisibles)
				{
					if (goUp == false)
						y -= chunkSize/2;
					else
						x -= chunkSize;

					goUp = !goUp;
					wasVisibles = false;
				}
			}
			else
			{
                //Debug.Log("On the screen!" + x + ";" + y);

                Vector2Int key = new Vector2Int(x, y);
				if (allChunks.ContainsKey(key))
				{
					if (!allChunks[key].activeSelf)
						allChunks[key].SetActive(true);
				}
				else
				{
					allChunks.Add(key, worldGenerator.GenerateChunk(key,new Vector2Int(chunkSize,chunkSize)));
				}

				currentActiveChunks.Add(allChunks[key]);
				previousActiveChunks.Remove(allChunks[key]);
				wasVisibles = true;
				outOfScreen = 0;
			}

			if (goUp)
			{
				x -= chunkSize / 2;
				y += chunkSize / 4;
			}
			else 
			{
                x += chunkSize / 2;
                y -= chunkSize / 4;
            }
        }

        foreach (GameObject obj in previousActiveChunks)
        {
            obj.SetActive(false);
        }

        previousActiveChunks = new HashSet<GameObject>(currentActiveChunks);
        currentActiveChunks.Clear();
    }
}