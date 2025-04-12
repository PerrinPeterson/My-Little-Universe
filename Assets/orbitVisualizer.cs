using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


//Run in the editor to visualize the orbit of a gravity object
[ExecuteInEditMode]
public class orbitVisualizer : MonoBehaviour
{
    public int numSteps = 1000;
    public float timeStep = 0.1f;
    public bool usePhysicsTimeStep;

    public bool relativeToBody;
    public gravityObject centralBody;
    public float width = 100;
    public bool useThickLines;

    void Start()
    {
        if (Application.isPlaying)
        {
            HideOrbits();
        }
    }

    void Update()
    {

        if (!Application.isPlaying)
        {
            DrawOrbits();
        }
    }

    void DrawOrbits()
    {
        gravityObject[] bodies = FindObjectsByType<gravityObject>(FindObjectsSortMode.None);
        var virtualBodies = new VirtualBody[bodies.Length];
        var drawPoints = new Vector3[bodies.Length][];
        int referenceFrameIndex = 0;
        Vector3 referenceBodyInitialPosition = Vector3.zero;

        // Initialize virtual bodies (don't want to move the actual bodies)
        for (int i = 0; i < virtualBodies.Length; i++)
        {
            virtualBodies[i] = new VirtualBody(bodies[i]);
            drawPoints[i] = new Vector3[numSteps];

            if (bodies[i] == centralBody && relativeToBody)
            {
                referenceFrameIndex = i;
                referenceBodyInitialPosition = virtualBodies[i].position;
            }
        }

        // Simulate
        for (int step = 0; step < numSteps; step++)
        {
            Vector3 referenceBodyPosition = (relativeToBody) ? virtualBodies[referenceFrameIndex].position : Vector3.zero;
            // Update velocities
            for (int i = 0; i < virtualBodies.Length; i++)
            {
                virtualBodies[i].velocity += CalculateAcceleration(i, virtualBodies) * timeStep;
            }
            // Update positions
            for (int i = 0; i < virtualBodies.Length; i++)
            {
                Vector3 newPos = virtualBodies[i].position + virtualBodies[i].velocity * timeStep;
                virtualBodies[i].position = newPos;
                if (relativeToBody)
                {
                    var referenceFrameOffset = referenceBodyPosition - referenceBodyInitialPosition;
                    newPos -= referenceFrameOffset;
                }
                if (relativeToBody && i == referenceFrameIndex)
                {
                    newPos = referenceBodyInitialPosition;
                }

                drawPoints[i][step] = newPos;
            }
        }

        // Draw paths
        for (int bodyIndex = 0; bodyIndex < virtualBodies.Length; bodyIndex++)
        {
            var pathColour = bodies[bodyIndex].gameObject.GetComponentInChildren<MeshRenderer>().sharedMaterial.color; //

            if (useThickLines)
            {
                LineRenderer lineRenderer = bodies[bodyIndex].gameObject.GetComponentInChildren<LineRenderer>();
                lineRenderer.enabled = true;
                lineRenderer.positionCount = drawPoints[bodyIndex].Length;
                lineRenderer.SetPositions(drawPoints[bodyIndex]);
                lineRenderer.startColor = pathColour;
                lineRenderer.endColor = pathColour;
                lineRenderer.widthMultiplier = width;
            }
            else
            {
                for (int i = 0; i < drawPoints[bodyIndex].Length - 1; i++)
                {
                    Debug.DrawLine(drawPoints[bodyIndex][i], drawPoints[bodyIndex][i + 1], pathColour);
                }

                // Hide renderer
                LineRenderer lineRenderer = bodies[bodyIndex].gameObject.GetComponentInChildren<LineRenderer>();
                if (lineRenderer)
                {
                    lineRenderer.enabled = false;
                }
            }

        }
    }

    //TODO: Either re write this, or change my gravity system to be less bonkers
    Vector3 CalculateAcceleration(int i, VirtualBody[] virtualBodies)
    {
        Vector3 acceleration = Vector3.zero;
        for (int j = 0; j < virtualBodies.Length; j++)
        {
            if (i == j)
            {
                continue; //ignore self
            }
            //Vector3 forceDir = (virtualBodies[j].position - virtualBodies[i].position).normalized;
            //float sqrDst = (virtualBodies[j].position - virtualBodies[i].position).sqrMagnitude;
            //acceleration += forceDir * 0.00674f * virtualBodies[j].mass / sqrDst;

            Vector3 direction = virtualBodies[j].position - virtualBodies[i].position;
            direction.Normalize();
            float distance = (virtualBodies[j].position - virtualBodies[i].position).magnitude;
            float force = GameObject.Find("Universe").GetComponent<UniversalConstants>().GravitationalConstant * ((virtualBodies[j].mass * virtualBodies[i].mass) / (distance * distance));
            float forceMultiplier = 1; //Trying to get my solar system to work properly, not sure if I need this actually
            force *= forceMultiplier;
            acceleration += direction * force / virtualBodies[i].mass;
        }
        return acceleration;
    }

    void HideOrbits()
    {
        gravityObject[] bodies = FindObjectsByType<gravityObject>(FindObjectsSortMode.None);

        // Draw paths
        for (int bodyIndex = 0; bodyIndex < bodies.Length; bodyIndex++)
        {
            var lineRenderer = bodies[bodyIndex].gameObject.GetComponentInChildren<LineRenderer>();
            lineRenderer.positionCount = 0;
        }
    }

    void OnValidate()
    {
        if (usePhysicsTimeStep)
        {
            timeStep = 0.01f;
        }
    }

    class VirtualBody
    {
        public Vector3 position;
        public Vector3 velocity;
        public float mass;

        public VirtualBody(gravityObject body)
        {
            position = body.transform.position;
            velocity = body.StartingVelocity;
            mass = body.Mass;
        }
    }
}
