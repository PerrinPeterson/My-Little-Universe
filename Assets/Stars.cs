using UnityEngine;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System;
using UnityEngine.UI;
using TMPro;


//Seeded building of stars is ready, takes a very long time to render every time, but should only have to run once.
//The next step is to create a visual representation of the stars in a small scale so I can see how the stars move around as I change things.

[ExecuteAlways, ImageEffectAllowedInSceneView]
public class Stars : MonoBehaviour
{
    public int height = 10; // The number of stars in the y direction
    public int width = 10; // The number of stars in the x direction
    public int depth = 10; // The number of stars in the z direction
    public Vector3 starSectorSize = new Vector3(1, 1, 1); // The size of the sector of stars, in AU. in laymen terms, 1 AU will be one sector away, either up down left right forward or back.
    public float AUSize = 10000; // The size of an AU we'll be using, this is importnat for the scale of the universe.
    public Vector3 deadZoneSize = new Vector3(0.1f, 0.1f, 0.1f); // The size of the dead zone, in AU, where no stars will be placed inside a sector (to stop stars from being placed too close to each other)
    public int seed = 141211234; // The seed for the random number generator
    public Gradient starColor; // The color of the stars
    public Star[,,] stars; // The array of stars
    public int baseStarRadius = 150; //The radius of a size 1 Star, in unity units.
    public int radiusIncrease = 25; //The amount to increase the radius of the Star by for each size increase, in unity units. So a size 2 Star would be baseStarRadius + radiusIncrease, a size 3 Star would be baseStarRadius + radiusIncrease * 2, etc.

    public int[] weightList; // The list of weights for the stars

    public bool rerollRandomStar = false; // Whether or not to reroll the random Star
    public Star randomStar; // A random Star from the array
    public int RandomStarSize = 0; // The size of the random Star
    public Color RandomStarColor; // The color of the random Star
    bool reValidate = false;

    //Skybox
    public bool generateSkybox = false; // Whether or not to generate the skybox
    public Vector2Int resolution = new Vector2Int(512, 512); // The resolution of each side of the skybox, Higher will mean more collision checks to see if we can see a Star, but will take longer to generate the texture
    public Vector3Int currentLocation = new Vector3Int(10, 10, 10); // The current Star location in the array.
    public Texture2D[] textures; // The array of textures for the skybox
    public float glowMultiplier = 2.0f; // The multiplier for the glow of the stars, so we can see them better in the skybox
    public ComputeShader skyboxRenderer; // The skybox renderer shader, so we can do all the collision math on the gpu in parallel, and then just render the texture to the skybox.

    //Visualizer
    public bool refresh = false;
    public bool VisualizeStars = false; // Whether or not to visualize the stars
    public bool clearVisualStars = false; // Whether or not to clear the visual stars
    public GameObject starPref; // The Star prefab
    public float visualScale = 1.0f; // The scale of the stars, in Unity units
    public GameObject[] visualStars; // The array of visual stars

    //CubeVisualizing
    public bool cubeVisualize = false; // Whether or not to visualize the cube generation
    public GameObject gizmoOne; // The first gizmo
    public GameObject gizmoTwo; // The second gizmo
    public GameObject[] exitpointGizmos; // The exit point gizmo
    public Material exitPointMaterial; // The material for the exit point gizmo
    public GameObject visualStar;
    public Vector3Int starIndex = new Vector3Int(5, 5, 5); // The index of the Star point in the array

    //Collision Visualizing
    public bool collisionVisualize = false; // Whether or not to visualize the collision
    public TextMeshProUGUI t1Text;
    public TextMeshProUGUI t2Text;
    public TextMeshProUGUI distText;
    public GameObject collisionObject;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    void GenerateSkybox()
    {

        if (stars == null)
        {
            Debug.Log("Error: stars is null");
            return;
        }

        //We're going to generate a texture for the stars so it can be put in as a skybox
        //We'll generate it from the point 0, 0, 0, aka where the sun should be, and then we'll use the "bounds" of the solar system as a viewing plane. 

        //The texture will be a cube map, so we'll need to generate 6 textures, one for each side of the cube.

        Cubemap skybox = new Cubemap(resolution.x, TextureFormat.RGB24, false);


        textures = new Texture2D[6]; //Up down left right forward back
        textures[0] = generateTexture(new Vector3(-starSectorSize.x / 2, starSectorSize.y / 2, -starSectorSize.z / 2), new Vector3(starSectorSize.x / 2, starSectorSize.y / 2, starSectorSize.z / 2), resolution); //Up
        textures[1] = generateTexture(new Vector3(-starSectorSize.x / 2, -starSectorSize.y / 2, starSectorSize.z / 2), new Vector3(starSectorSize.x / 2, -starSectorSize.y / 2, -starSectorSize.z / 2), resolution); //Down
        textures[2] = generateTexture(new Vector3(starSectorSize.x / 2, starSectorSize.y / 2, starSectorSize.z / 2), new Vector3(starSectorSize.x / 2, -starSectorSize.y / 2, -starSectorSize.z / 2), resolution); //Right
        textures[3] = generateTexture(new Vector3(-starSectorSize.x / 2, starSectorSize.y / 2, -starSectorSize.z / 2), new Vector3(-starSectorSize.x / 2, -starSectorSize.y / 2, starSectorSize.z / 2), resolution); //Left
        textures[4] = generateTexture(new Vector3(-starSectorSize.x / 2, starSectorSize.y / 2, starSectorSize.z / 2), new Vector3(starSectorSize.x / 2, -starSectorSize.y / 2, starSectorSize.z / 2), resolution); //Front
        textures[5] = generateTexture(new Vector3(starSectorSize.x / 2, starSectorSize.y / 2, -starSectorSize.z / 2), new Vector3(-starSectorSize.x / 2, -starSectorSize.y / 2, -starSectorSize.z / 2), resolution); //Back


        //So I can disable some sides
        for (int i = 0; i < 6; i++)
        {
            if (textures[i] == null)
            {
                //Set to black
                textures[i] = new Texture2D(resolution.x, resolution.y);
                Color[] pixels = new Color[resolution.x * resolution.y];
                for (int j = 0; j < pixels.Length; j++)
                {
                    pixels[j] = Color.black;
                }
                textures[i].SetPixels(pixels);
                textures[i].Apply();
            }
        }

        //textures[2] = rotateTexture(textures[2], true); //Rotate the right texture 90 degrees clockwise, gross but I have to see.


        skybox.SetPixels(textures[0].GetPixels(), CubemapFace.PositiveY); //Set the pixels for the up side of the cube
        skybox.SetPixels(textures[1].GetPixels(), CubemapFace.NegativeY); //Set the pixels for the down side of the cube
        skybox.SetPixels(textures[2].GetPixels(), CubemapFace.PositiveX); //Set the pixels for the right side of the cube
        skybox.SetPixels(textures[3].GetPixels(), CubemapFace.NegativeX); //Set the pixels for the left side of the cube
        skybox.SetPixels(textures[4].GetPixels(), CubemapFace.PositiveZ); //Set the pixels for the front side of the cube
        skybox.SetPixels(textures[5].GetPixels(), CubemapFace.NegativeZ); //Set the pixels for the back side of the cube

        skybox.Apply(); //Apply the changes to the skybox
        Material skyboxMaterial = new Material(Shader.Find("Skybox/Cubemap"));
        skyboxMaterial.SetTexture("_Tex", skybox);

        ////Set the skybox in rendersettings
        RenderSettings.skybox = skyboxMaterial;

    }

    Texture2D rotateTexture(Texture2D originalTexture, bool clockwise)
    {
        Color32[] original = originalTexture.GetPixels32();
        Color32[] rotated = new Color32[original.Length];
        int w = originalTexture.width;
        int h = originalTexture.height;

        int iRotated, iOriginal;

        for (int j = 0; j < h; ++j)
        {
            for (int i = 0; i < w; ++i)
            {
                iRotated = (i + 1) * h - j - 1;
                iOriginal = clockwise ? original.Length - 1 - (j * w + i) : j * w + i;
                rotated[iRotated] = original[iOriginal];
            }
        }

        Texture2D rotatedTexture = new Texture2D(h, w);
        rotatedTexture.SetPixels32(rotated);
        rotatedTexture.Apply();
        return rotatedTexture;
    }

    //Takes in 2 directions, and a resolution, and fires out rays from the center of the Star system to the edges of the cube, and checks if there are any stars in the way.
    Texture2D generateTexture(Vector3 TopLeftWorldSpace, Vector3 BottomRightWorldSpace, Vector2Int resolution)
    {
        Texture2D tex = new Texture2D(resolution.x, resolution.y);
        RenderTexture result = new RenderTexture(resolution.x, resolution.y, 0);
        result.enableRandomWrite = true;
        result.Create();

        //Start with figuring out a stepsize for each pixel of the texture, so I know what to adjust the ray by
        Vector3 stepSize = new Vector3((BottomRightWorldSpace.x - TopLeftWorldSpace.x) / resolution.x, (BottomRightWorldSpace.y - TopLeftWorldSpace.y) / resolution.y, (BottomRightWorldSpace.z - TopLeftWorldSpace.z) / resolution.y); //The step size for each pixel of the texture, in world space
        //Remove the unused axis from the direction vectors, makes the math easier
        Vector3 test = new Vector3(BottomRightWorldSpace.x - TopLeftWorldSpace.x, BottomRightWorldSpace.y - TopLeftWorldSpace.y, BottomRightWorldSpace.z - TopLeftWorldSpace.z);
        //if the x axis zeros out, then we're only interested in the y and z axis
        Vector3 entryPoint = new Vector3(0, 0, 0); //The entry point of the ray, starts at the center of the solar system
        Vector3 starPosition = new Vector3(0, 0, 0); //The position of the Star, starts at the center of the solar system
        Vector3 direction = new Vector3(0, 0, 0); //The direction of the ray, starts at the center of the solar system


        //Shader stuff
        int kernel = skyboxRenderer.FindKernel("CSMain"); //The shader program
        ComputeBuffer starOffsetBuffer = new ComputeBuffer(stars.Length, sizeof(float) * 3); //The buffer for the star offsets, all vector3s
        ComputeBuffer starColorBuffer = new ComputeBuffer(stars.Length, sizeof(float) * 4); //The buffer for the star colors, all colors
        ComputeBuffer starSizeBuffer = new ComputeBuffer(stars.Length, sizeof(float)); //The buffer for the star sizes, all floats

        //temporary 1D arrays for the star data
        Vector3[] starOffsets = new Vector3[stars.Length]; //The array for the star offsets
        Color[] starColors = new Color[stars.Length]; //The array for the star colors
        float[] starSizes = new float[stars.Length]; //The array for the star sizes

        //Filling the Buffers with the star data
        //TODO: Fill the buffers
        for (int i = 0; i < width; i++)
            for (int j = 0; j < height; j++)
                for (int k = 0; k < depth; k++)
                {
                    //convert to 1D array
                    int index = i * height * depth + j * depth + k; //The index of the star in the array
                    starOffsets[index] = stars[i, j, k].offSet; //The position of the Star
                    starColors[index] = stars[i, j, k].color; //The color of the Star
                    starSizes[index] = stars[i, j, k].mass; //The size of the Star
                }

        //Set the buffers in the shader
        starOffsetBuffer.SetData(starOffsets); //Set the star offsets in the buffer
        starColorBuffer.SetData(starColors);
        starSizeBuffer.SetData(starSizes); //Set the star sizes in the buffer

        //Set the buffers in the shader
        skyboxRenderer.SetBuffer(kernel, "starOffsets", starOffsetBuffer); //Set the star offsets in the shader
        skyboxRenderer.SetBuffer(kernel, "starColors", starColorBuffer); //Set the star colors in the shader
        skyboxRenderer.SetBuffer(kernel, "starSizes", starSizeBuffer); //Set the star sizes in the shader

        //Set the data in the shader
        skyboxRenderer.SetInt("width", width); //The width of the star array
        skyboxRenderer.SetInt("height", height); //The height of the star array
        skyboxRenderer.SetInt("depth", depth); //The depth of the star array
        skyboxRenderer.SetFloat("baseRadius", baseStarRadius);
        skyboxRenderer.SetFloat("radiusIncrease", radiusIncrease);
        skyboxRenderer.SetFloat("AUSize", AUSize); //The size of an AU, so we can scale the stars to the right size
        skyboxRenderer.SetFloat("glowMultiplier", glowMultiplier); //The multiplier for the glow of the stars, so we can see them better in the skybox
        skyboxRenderer.SetInts("resolution", resolution.x, resolution.y);
        skyboxRenderer.SetVector("stepSize", new Vector4(stepSize.x, stepSize.y, stepSize.z, 0)); //The step size for each pixel of the texture, in world space
        skyboxRenderer.SetVector("topLeft", new Vector4(TopLeftWorldSpace.x, TopLeftWorldSpace.y, TopLeftWorldSpace.z, 0)); //The bottom right corner of the texture face, in world space
        skyboxRenderer.SetVector("test", new Vector4(test.x, test.y, test.z, 0)); //The test vector, to see which axis is zeroed out
        skyboxRenderer.SetVector("starSectorSize", new Vector4(starSectorSize.x, starSectorSize.y, starSectorSize.z, 0)); //The size of the sector of stars, in AU
        skyboxRenderer.SetInts("startingStarIndex", currentLocation.x, currentLocation.y, currentLocation.z); //The current location of the star in the array

        skyboxRenderer.SetTexture(kernel, "Result", result); //Set the texture to the render texture

        //Thread groups
        int threadGroupsX = Mathf.CeilToInt(resolution.x / 1024f); //The number of thread groups in the x direction
        int threadGroupsY = Mathf.CeilToInt(resolution.y / 1f); //The number of thread groups in the y direction
        int threadGroupsZ = 1; //The number of thread groups in the z direction

        //Dispatch the shader
        skyboxRenderer.Dispatch(kernel, threadGroupsX, threadGroupsY, threadGroupsZ); //Dispatch the shader

        RenderTexture.active = result; //Set the render texture as the active render texture
        tex.ReadPixels(new Rect(0, 0, resolution.x, resolution.y), 0, 0); //Read the pixels from the render texture
        tex.Apply(); //Apply the changes to the texture

        RenderTexture.active = null; //Set the active render texture to null

        //Release the buffers
        starOffsetBuffer.Release(); //Release the star offsets buffer
        starColorBuffer.Release(); //Release the star colors buffer
        starSizeBuffer.Release(); //Release the star sizes buffer
        //Release the render texture
        result.Release(); //Release the render texture

        return tex;

    }

    void Refresh()
    {
        // Create the array of stars
        stars = new Star[width, height, depth];

        int sizeOfWeights = 0;
        for (int i = 0; i < 100; i++)
        {
            sizeOfWeights += i;
        }

        if (weightList.Length != sizeOfWeights)
        {
            int[] weightedList = new int[sizeOfWeights];

            int index = 0;
            for (int i = 100; i > 0; i--)
            {
                for (int j = 100; j > i; j--)
                {
                    weightedList[index] = i;
                    index++;
                }
            }

            weightList = weightedList;
        }

        //I'm going to use a curve to determine the size of stars, with the smallest stars being the most common, and the largest stars being the least common.
        //y = (x^4)/1000000



        UnityEngine.Random.InitState(seed);
        // Create the stars
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                for (int k = 0; k < depth; k++)
                {
                    Star newStar = new Star();
                    int x = UnityEngine.Random.Range(1, 101);
                    newStar.mass = (int)(Mathf.Pow(x, 8) / 100000000000000) + 1;
                    stars[i, j, k] = newStar;
                }
            }
        }

        // Set the color of the stars
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                for (int k = 0; k < depth; k++)
                {
                    //The position of the gradient is the mass of the Star divided by 100, so it ranges from 0 to 1
                    stars[i, j, k].color = starColor.Evaluate((float)stars[i, j, k].mass / 100);
                }
            }
        }

        //Set the offset of the stars
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                for (int k = 0; k < depth; k++)
                {
                    stars[i, j, k].offSet = new Vector3(UnityEngine.Random.Range((-starSectorSize.x) / 2 + deadZoneSize.x, starSectorSize.x / 2 - deadZoneSize.x), UnityEngine.Random.Range(-starSectorSize.y / 2 + deadZoneSize.y, starSectorSize.y / 2 - deadZoneSize.y), UnityEngine.Random.Range(-starSectorSize.z / 2 + deadZoneSize.z, starSectorSize.z / 2 - deadZoneSize.z));
                }
            }
        }
   
    }

    // Update is called once per frame
    void Update()
    {

        if (refresh)
        {
            Refresh();
            refresh = false;
        }
        if (VisualizeStars && reValidate || VisualizeStars && visualStars == null || VisualizeStars && visualStars.Length == 0)
        {
            if (visualStars != null)
            {
                foreach (GameObject Star in visualStars)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(Star);
                    }
                    else
                    {
                        DestroyImmediate(Star);
                    }
                }
            }

            Visualize();
        }
        if (rerollRandomStar)
        {
            if (stars == null)
            {
                rerollRandomStar = false;
                return;
            }
            randomStar = stars[UnityEngine.Random.Range(0, width), UnityEngine.Random.Range(0, height), UnityEngine.Random.Range(0, depth)];
            RandomStarSize = randomStar.mass;
            RandomStarColor = randomStar.color;
            rerollRandomStar = false;
        }

        if (generateSkybox)
        {
            GenerateSkybox();
            generateSkybox = false;
        }

        if (clearVisualStars)
        {
            foreach (GameObject Star in visualStars)
            {
                if (Application.isPlaying)
                {
                    Destroy(Star);
                }
                else
                {
                    DestroyImmediate(Star);
                }
            }
            clearVisualStars = false;
        }


        
    
        if (collisionVisualize)
        {
            //Draw the line between the two gizmos, we'll use the same gizmos as the raycasting
            Debug.DrawLine(gizmoOne.transform.position, gizmoTwo.transform.position, Color.red);

            //Detect the sphere collisions
            //we'll do the smae math as in our checkForStarHit, but we'll do it all here so we can output some values.
            Vector3 p1 = gizmoOne.transform.position; //The entry point of the ray
            Vector3 p2 = gizmoTwo.transform.position; //The exit point of the ray

            Vector3 objectPos = collisionObject.transform.position; //The position of the object
            float radius = collisionObject.transform.localScale.x / 2; //The radius of the object, assuming its a sphere

            Vector3 rayDirNotNormalized = p1 - p2; //The direction of the ray
            Vector3 rayDir = rayDirNotNormalized.normalized; //The direction of the ray, normalized
            Vector3 objectDirection = (objectPos - p1); //The direction of the ray
            float a = Vector3.Dot(rayDir, rayDir);
            float b = 2 * Vector3.Dot(objectDirection, rayDir); //The distance between the entry point and the object
            float c = Vector3.Dot(objectDirection, objectDirection) - radius * radius;
            float discriminant = b * b - 4 * a * c; //The discriminant of the quadratic equation

            discriminant = Mathf.Sqrt(discriminant); //The square root of the discriminant
            float t1 = (-b - discriminant) / (2 * a); //The first solution of the quadratic equation    
            float t2 = (-b + discriminant) / (2 * a); //The second solution of the quadratic equation


            if (t1Text != null && t2Text != null && distText != null)
            {
                Vector3 intersectOne = new Vector3(p1.x + (rayDir.x * t1), p1.y + (rayDir.y * t1), p1.z + (rayDir.z * t1)); //The first intersection point
                Vector3 intersectTwo = new Vector3(p1.x + (rayDir.x * t2), p1.y + (rayDir.y * t2), p1.z + (rayDir.z * t2)); //The second intersection point

                t1Text.text = (p1 + (new Vector3(rayDir.x * t1, rayDir.y * t1, rayDir.z * t1))).ToString(); //Set the text of the t1 text object
                t2Text.text = (p1 + (new Vector3(rayDir.x * t2, rayDir.y * t2, rayDir.z * t2))).ToString(); //Set the text of the t2 text object
                distText.text = (Vector3.Distance(intersectOne, intersectTwo)).ToString(); //Set the text of the distance text object
            }
        }
    }

    private void OnValidate()
    {
        //Refresh();
    }

    void Visualize()
    {
        //This is so I can see a visual representation of the the stars, but in general this will be expensive for the larger universes.

        //Creates a bunch of coppies of the Star prefab in their correct position.
        visualStars = new GameObject[width * height * depth];
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                for (int k = 0; k < depth; k++)
                {
                    GameObject newStar = Instantiate(starPref, new Vector3(i * starSectorSize.x, j * starSectorSize.y, k * starSectorSize.z), Quaternion.identity);
                    float scale = ((float)stars[i, j, k].mass / 100);

                    newStar.transform.localScale = new Vector3(scale, scale, scale);

                    //Adjust the position of the Star to be in the correct position based on the offset, at this point we can think of the offset being a percentage away from the center of the sector
                    newStar.transform.position += new Vector3(stars[i, j, k].offSet.x, stars[i, j, k].offSet.y, stars[i, j, k].offSet.z);


                    //Create a copy of the material so that we can change the color of the Star
                    Material newMat = new Material(newStar.GetComponent<MeshRenderer>().sharedMaterial);
                    newMat.SetColor("_EmissionColor", stars[i, j, k].color);
                    newMat.SetColor("_Color", stars[i, j, k].color);
                    newStar.GetComponent<MeshRenderer>().material = newMat;

                    visualStars[i * height * depth + j * depth + k] = newStar;
                }
            }
        }
    }
}

//Pretty much a struct to hold the info of a Star
public class Star
{
    public int mass = 1; // The mass of the Star, ranging from 1 to 100 (1 being the smallest, aka brown dwarf, 100 being the largest, our sun would be around the 10 mark)
    public Color color; // The color of the Star
    public Vector3 offSet; // The offset of the Star from the center of the sector in AU. will be a random value between SizeOfSector/2-DeadZoneSize and -SizeOfSector/2+DeadZoneSize
}
