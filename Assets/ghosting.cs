using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ghosting : MonoBehaviour
{
    public float ghostScale = 1.0f;
    public float ghostInterval = 1.0f;
    public int maxGhosts = 10;
    public float ghostOpacity = 0.5f;
    public Color ghostColor = Color.green;
    private List<GameObject> ghosts = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //TODO: Happening many more times than once
        //Spawn a ghost every ghostInterval seconds
        if (Time.time % ghostInterval < Time.deltaTime)
        {
            SpawnGhost();
        }
    }

    void SpawnGhost()
    {
        GameObject ghost = Instantiate(gameObject, transform.position, transform.rotation);
        ghost.transform.localScale = new Vector3(ghostScale, ghostScale, ghostScale);
        //Ensure the ghost can't move
        ghost.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        ghost.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        if (ghost.GetComponent<gravityObject>() != null)
        {
            Destroy(ghost.GetComponent<gravityObject>());
        }
        //Kinda hacky way to disable the collider, technically should do it so that if its not a sphere, it still works
        ghost.GetComponent<SphereCollider>().enabled = false;
        ghost.GetComponent<MeshRenderer>().material.color = new Color(ghostColor.r, ghostColor.g, ghostColor.b, ghostOpacity);
        ghosts.Add(ghost);
        if (ghosts.Count > maxGhosts)
        {
            Destroy(ghosts[0]);
            ghosts.RemoveAt(0);
        }

    }
}
