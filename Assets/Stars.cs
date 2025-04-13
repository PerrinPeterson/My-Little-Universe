using UnityEngine;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System;


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
        textures[0] = generateTexture(new Vector3(-starSectorSize.x * AUSize / 2, starSectorSize.y * AUSize / 2, starSectorSize.z * AUSize / 2), new Vector3(starSectorSize.x * AUSize / 2, starSectorSize.y * AUSize / 2, -starSectorSize.z  * AUSize / 2), resolution); //Up
        textures[1] = generateTexture(new Vector3(starSectorSize.x * AUSize / 2, -starSectorSize.y * AUSize / 2, starSectorSize.z * AUSize / 2), new Vector3(-starSectorSize.x * AUSize / 2, -starSectorSize.y * AUSize / 2, -starSectorSize.z * AUSize / 2), resolution); //Down
        textures[2] = generateTexture(new Vector3(starSectorSize.x * AUSize / 2, starSectorSize.y * AUSize / 2, starSectorSize.z * AUSize / 2), new Vector3(starSectorSize.x * AUSize / 2, -starSectorSize.y * AUSize / 2, -starSectorSize.z * AUSize / 2), resolution); //Left
        textures[3] = generateTexture(new Vector3(-starSectorSize.x * AUSize / 2, -starSectorSize.y * AUSize / 2, starSectorSize.z * AUSize / 2), new Vector3(-starSectorSize.x * AUSize / 2, starSectorSize.y * AUSize / 2, -starSectorSize.z * AUSize / 2), resolution); //Right
        textures[4] = generateTexture(new Vector3(starSectorSize.x * AUSize / 2, starSectorSize.y * AUSize / 2, starSectorSize.z * AUSize / 2), new Vector3(-starSectorSize.x * AUSize / 2, -starSectorSize.y * AUSize / 2, starSectorSize.z * AUSize / 2), resolution); //Front
        textures[5] = generateTexture(new Vector3(starSectorSize.x * AUSize / 2, -starSectorSize.y * AUSize / 2, -starSectorSize.z * AUSize / 2), new Vector3(-starSectorSize.x * AUSize / 2, starSectorSize.y * AUSize / 2, -starSectorSize.z * AUSize / 2), resolution); //Back

        skybox.SetPixels(textures[0].GetPixels(), CubemapFace.PositiveY); //Set the pixels for the up side of the cube
        skybox.SetPixels(textures[1].GetPixels(), CubemapFace.NegativeY); //Set the pixels for the down side of the cube
        skybox.SetPixels(textures[2].GetPixels(), CubemapFace.NegativeX); //Set the pixels for the left side of the cube
        skybox.SetPixels(textures[3].GetPixels(), CubemapFace.PositiveX); //Set the pixels for the right side of the cube
        skybox.SetPixels(textures[4].GetPixels(), CubemapFace.PositiveZ); //Set the pixels for the front side of the cube
        skybox.SetPixels(textures[5].GetPixels(), CubemapFace.NegativeZ); //Set the pixels for the back side of the cube

        ////Now we need to apply the texture to the skybox
        //Debug.Log("Applying skybox texture");
        skybox.Apply(); //Apply the changes to the skybox
        ////Set the skybox material to the new skybox
        Material skyboxMaterial = new Material(Shader.Find("Skybox/Cubemap"));
        skyboxMaterial.SetTexture("_Tex", skybox); //Set the texture of the skybox material to the new skybox

        ////Set the skybox in rendersettings
        RenderSettings.skybox = skyboxMaterial; //Set the skybox in the render settings

    }

    //Takes in 2 directions, and a resolution, and fires out rays from the center of the Star system to the edges of the cube, and checks if there are any stars in the way.
    Texture2D generateTexture(Vector3 TopLeftWorldSpace, Vector3 BottomRightWorldSpace, Vector2Int resolution)
    {
        Texture2D tex = new Texture2D(resolution.x, resolution.y);


        //Start with figuring out a stepsize for each pixel of the texture, so I know what to adjust the ray by
        Vector3 stepSize = new Vector3((BottomRightWorldSpace.x - TopLeftWorldSpace.x) / resolution.x, (BottomRightWorldSpace.y - TopLeftWorldSpace.y) / resolution.y, (BottomRightWorldSpace.z - TopLeftWorldSpace.z) / resolution.y); //The step size for each pixel of the texture, in world space
        //Remove the unused axis from the direction vectors, makes the math easier
        Vector3 test = new Vector3(TopLeftWorldSpace.x - BottomRightWorldSpace.x, TopLeftWorldSpace.y - BottomRightWorldSpace.y, TopLeftWorldSpace.z - BottomRightWorldSpace.z);
        //if the x axis zeros out, then we're only interested in the y and z axis
        Vector3 entryPoint = new Vector3(0, 0, 0); //The entry point of the ray, starts at the center of the solar system
        Vector3 starPosition = new Vector3(0, 0, 0); //The position of the Star, starts at the center of the solar system
        Vector3 direction = new Vector3(0, 0, 0); //The direction of the ray, starts at the center of the solar system
        //int starSize = 15; //The radius of the Star

        //Left or Right
        if (test.x == 0)
        {
            for (int i = 0; i < resolution.x; i++)
            {
                for (int j = 0; j < resolution.y; j++)
                {
                    Vector3 exitPoint = BottomRightWorldSpace - new Vector3(0, stepSize.y * i, stepSize.z * j); //The exit point of the ray from the starting sector.
                    direction = exitPoint;


                    entryPoint = setEntryPoint(exitPoint, currentLocation); //Set the new entry point, for the next cube. We know we're going in an 
                    Vector3Int index = GetNextSectorIndex(currentLocation, exitPoint); //Get the next sector index, so we can check if the ray hits a star in the next sector
                    direction.Normalize(); //Normalize the direction vector, so we can use it for the raycasting
                    Color backupColor = Color.black;
                    while (index != new Vector3(-1, -1, -1)) //Once we get a vector3 Zero'd out, we've hit the edge of the universe.
                    {
                        //Calculate the new exit point, based on the new entry point and the direction
                        exitPoint = CalculateExitPoint(entryPoint, direction); //The exit point of the ray
                        //Check if the ray hits a star
                        Color inheritedColor = Color.black;
                        if (CheckForStarHit(entryPoint, exitPoint, stars[(int)index.x, (int)index.y, (int)index.z], out inheritedColor)) //If the ray hits a star
                        {
                            //If the ray hits a star, we need to set the color of the pixel to the color of the star
                            tex.SetPixel(i, j, stars[(int)index.x, (int)index.y, (int)index.z].color); //Set the color of the pixel to the color of the star
                            Debug.Log("Hit a star, Left/Right");
                            break; //Break out of the loop, we've hit a star
                        }
                        else
                        {
                            backupColor += inheritedColor; //Might have to clamp this, not sure.

                            //If we don't hit a star, we move on to the next sector
                            entryPoint = setEntryPoint(exitPoint, index); //Set the new entry point, for the next cube. We know we're going in an
                            index = GetNextSectorIndex(index, exitPoint); //Get the next sector index, so we can check if the ray hits a star in the next sector.
                        }
                    }

                    //Exiting the while loop means no hits, so set the pixel to black
                    if (index == new Vector3(-1, -1, -1)) //If we hit the edge of the universe
                    {
                        tex.SetPixel(i, j, Color.black); //Set the color of the pixel to black
                    }
                }
            }
        }
        else if (test.y == 0)
        {
            //Up or Down
            for (int i = 0; i < resolution.x; i++)
            {
                for (int j = 0; j < resolution.y; j++)
                {
                    Vector3 exitPoint = BottomRightWorldSpace - new Vector3(stepSize.x * i, 0, stepSize.z * j); //The exit point of the ray from the starting sector.
                    direction = exitPoint; //Because we assume the start is 0, 0, 0, the direction is just the exitPoint
                    entryPoint = setEntryPoint(exitPoint, currentLocation); //Set the new entry point, for the next cube. We know we're going in an 
                    Vector3Int index = GetNextSectorIndex(currentLocation, exitPoint); //Get the next sector index, so we can check if the ray hits a star in the next sector
                    direction.Normalize(); //Normalize the direction vector, so we can use it for the raycasting
                    Color backupColor = Color.black;
                    while (index != new Vector3(-1, -1, -1)) //Once we get a vector3 Zero'd out, we've hit the edge of the universe.
                    {
                        //Calculate the new exit point, based on the new entry point and the direction
                        exitPoint = CalculateExitPoint(entryPoint, direction); //The exit point of the ray
                        Color inheritedColor = Color.black;
                        //Check if the ray hits a star
                        if (CheckForStarHit(entryPoint, exitPoint, stars[(int)index.x, (int)index.y, (int)index.z], out inheritedColor)) //If the ray hits a star
                        {
                            //If the ray hits a star, we need to set the color of the pixel to the color of the star
                            tex.SetPixel(i, j, stars[(int)index.x, (int)index.y, (int)index.z].color); //Set the color of the pixel to the color of the star
                            Debug.Log("Hit a star, Up/Down");
                            break; //Break out of the loop, we've hit a star
                        }
                        else
                        {
                            //If we don't hit a star, we move on to the next sector
                            backupColor += inheritedColor; //Might have to clamp this, not sure.
                            entryPoint = setEntryPoint(exitPoint, index); //Set the new entry point, for the next cube. We know we're going in an
                            index = GetNextSectorIndex(index, exitPoint); //Get the next sector index, so we can check if the ray hits a star in the next sector.
                        }
                    }
                    //Exiting the while loop means no hits, so set the pixel to black
                    if (index == new Vector3(-1, -1, -1)) //If we hit the edge of the universe
                    {
                        tex.SetPixel(i, j, Color.black); //Set the color of the pixel to black
                    }
                }
            }
        }
        else
        {
            //Front or Back
            for (int i = 0; i < resolution.x; i++)
            {
                for (int j = 0; j < resolution.y; j++)
                {
                    Vector3 exitPoint = BottomRightWorldSpace - new Vector3(stepSize.x * i, stepSize.y * j, 0); //The exit point of the ray from the starting sector.
                    direction = exitPoint;
                    entryPoint = setEntryPoint(exitPoint, currentLocation); //Set the new entry point, for the next cube. We know we're going in an 
                    Vector3Int index = GetNextSectorIndex(currentLocation, exitPoint); //Get the next sector index, so we can check if the ray hits a star in the next sector
                    direction.Normalize(); //Normalize the direction vector, so we can use it for the raycasting
                    Color backupColor = Color.black;
                    while (index != new Vector3(-1, -1, -1)) //Once we get a vector3 Zero'd out, we've hit the edge of the universe.
                    {
                        //Calculate the new exit point, based on the new entry point and the direction
                        exitPoint = CalculateExitPoint(entryPoint, direction); //The exit point of the ray
                        Color inheritedColor = Color.black;
                        //Check if the ray hits a star
                        if (CheckForStarHit(entryPoint, exitPoint, stars[(int)index.x, (int)index.y, (int)index.z], out inheritedColor)) //If the ray hits a star
                        {
                            //If the ray hits a star, we need to set the color of the pixel to the color of the star
                            tex.SetPixel(i, j, stars[(int)index.x, (int)index.y, (int)index.z].color); //Set the color of the pixel to the color of the star
                            Debug.Log("Hit a star, Front/Back");
                            break; //Break out of the loop, we've hit a star
                        }
                        else
                        {
                            //If we don't hit a star, we move on to the next sector
                            backupColor += inheritedColor; //Might have to clamp this, not sure.
                            entryPoint = setEntryPoint(exitPoint, index); //Set the new entry point, for the next cube. We know we're going in an
                            index = GetNextSectorIndex(index, exitPoint); //Get the next sector index, so we can check if the ray hits a star in the next sector.
                        }
                    }
                    //Exiting the while loop means no hits, so set the pixel to black
                    if (index == new Vector3(-1, -1, -1)) //If we hit the edge of the universe
                    {
                        tex.SetPixel(i, j, Color.black); //Set the color of the pixel to black
                    }
                }
            }
        }

        return tex;
    }

    Vector3Int GetNextSectorIndex(Vector3Int currentStarIndex, Vector3 exitPoint)
    {
        //Whichever of the 3 axis is equal to the starSectorSize / 2, in either positive or negative direction, is the axis we need to move in.
        double test = starSectorSize.x * AUSize / 2 + 0.001f;
        if (exitPoint.x >= starSectorSize.x * AUSize / 2 - 0.001f)
        {
            currentStarIndex.x += 1;
        }
        if (exitPoint.x <= -starSectorSize.x * AUSize / 2 + 0.001f)
        {
            currentStarIndex.x -= 1;
        }
        if (exitPoint.y >= starSectorSize.y * AUSize / 2 - 0.001f)
        {
            currentStarIndex.y += 1;
        }
        if (exitPoint.y <= -starSectorSize.y * AUSize / 2 + 0.001f)
        {
            currentStarIndex.y -= 1;
        }
        if (exitPoint.z >= starSectorSize.z * AUSize / 2 - 0.001f)
        {
            currentStarIndex.z += 1;
        }
        if (exitPoint.z <= -starSectorSize.z * AUSize / 2 + 0.001f)
        {
            currentStarIndex.z -= 1;
        }

        //Check to make sure the next sector is within bounds
        if (currentStarIndex.x < 0 || currentStarIndex.x >= width || currentStarIndex.y < 0 || currentStarIndex.y >= height || currentStarIndex.z < 0 || currentStarIndex.z >= depth)
        {
            //If the next sector is out of bounds, return an invalid value
            return new Vector3Int(-1, -1, -1);
        }

        return currentStarIndex;
    }


    //Puts the entry point where the exit point is, and inverts it so its entering the next cube.
    Vector3 setEntryPoint(Vector3 oldExitPoint, Vector3 currentStarIndex)
    {
        
        if (oldExitPoint.x >= starSectorSize.x * AUSize / 2 - 0.0001f || oldExitPoint.x <= -starSectorSize.x * AUSize / 2 + 0.0001f)
        {
            oldExitPoint.x = -oldExitPoint.x;
        }
        if (oldExitPoint.y >= starSectorSize.y * AUSize / 2 - 0.0001f || oldExitPoint.y <= -starSectorSize.y * AUSize / 2 + 0.0001f)
        {
            oldExitPoint.y = -oldExitPoint.y;
        }
        if (oldExitPoint.z >= starSectorSize.z * AUSize / 2 - 0.0001f || oldExitPoint.z <= -starSectorSize.z * AUSize / 2 + 0.0001f)
        {
            oldExitPoint.z = -oldExitPoint.z;
        }

        return oldExitPoint;

    }

    //Calculates the P2 we need for intersection detection.
    Vector4 CalculateExitPoint(Vector3 entryPoint, Vector3 direction)
    {
        const float EPSILON = 0.01f;
        //We can create the other 5 planes of the cube, and then we can use the entry point and the direction to find the exit point of the ray.
        //We can also calculate the max distance a ray could travel before hitting a plane, which is the distance from one corner of the cube to the oposite corner.

        //The max distance the ray can travel before hitting a plane, we can use some simple trig to find this
        //d = sqrt(x^2 + y^2 + z^2) + 10; //The max distance the ray can travel before hitting a plane, with a little extra for floating point errors
        float maxDistance = MathF.Sqrt(MathF.Pow(starSectorSize.x * AUSize, 2) + MathF.Pow(starSectorSize.y * AUSize, 2) + MathF.Pow(starSectorSize.z * AUSize, 2));

        //points on each plane, for the plane definitions
        float hx = starSectorSize.x * AUSize / 2; //Half the size of the sector in the x direction
        float hy = starSectorSize.y * AUSize / 2; //Half the size of the sector in the y direction
        float hz = starSectorSize.z * AUSize / 2; //Half the size of the sector in the z direction

        Plane[] planes = new Plane[6]; //The planes of the cube, we'll use this to construct the planes
        planes[0] = new Plane(Vector3.forward, new Vector3(0, 0, hz)); //The front plane of the cube
        planes[1] = new Plane(Vector3.back, new Vector3(0, 0, -hz)); //The back plane of the cube
        planes[2] = new Plane(Vector3.right, new Vector3(hx, 0, 0)); //The right plane of the cube
        planes[3] = new Plane(Vector3.left, new Vector3(-hx, 0, 0)); //The left plane of the cube
        planes[4] = new Plane(Vector3.up, new Vector3(0, hy, 0)); //The top plane of the cube
        planes[5] = new Plane(Vector3.down, new Vector3(0, -hy, 0)); //The bottom plane of the cube


        //The entry point should be converted to local space before hitting this function, meaning that either the x, y, or z axis should be 0
        //we'll normalize the direction vector to make it easier
        direction.Normalize();

        Vector3 exitPoint = new Vector3(0, 0, 0); //The exit point of the ray, with the face index, so If theres no Star hit, we can generate the next cube.
        Vector3 p1 = entryPoint; //The entry point of the ray, TODO: Make sure that we set up the exit point correctly, any point that hits the min/max of the cube needs to to have that dimension inverted, so it can be used for the next cube.

        

        Vector3 p2 = new Vector3(entryPoint.x + direction.x * maxDistance, entryPoint.y + direction.y * maxDistance, entryPoint.z + direction.z * maxDistance); //The end point of the ray, not the exit point
        float minDistance = maxDistance;
        //Now we'll do basic collision detection to see which plane the ray will hit
        for (int i = 0; i < planes.Length; i++)
        {
            Plane plane = planes[i]; //The plane we're checking against

            if (plane.Raycast(new Ray(p1, direction), out float enter)) //If the ray hits the plane
            {
                if (enter > 0 && enter <= minDistance) //If the ray hits the plane within the max distance
                {
                    Vector3 hitPoint = p1 + direction * enter; //The point where the ray hits the plane
                    hitPoint.x = Mathf.Clamp(hitPoint.x, -hx, hx);
                    hitPoint.y = Mathf.Clamp(hitPoint.y, -hy, hy);
                    hitPoint.z = Mathf.Clamp(hitPoint.z, -hz, hz);
                    //Getting a strange issue where the ray isn't hitting within the bounds of the cube, not sure why.
                    if (Mathf.Abs(hitPoint.x) < hx + EPSILON && Mathf.Abs(hitPoint.y) < hy + EPSILON && Mathf.Abs(hitPoint.z) < hz + EPSILON) //If the hit point is within the bounds of the cube
                    {
                        exitPoint = new Vector3(hitPoint.x, hitPoint.y, hitPoint.z); //The exit point of the ray, with the face index
                        minDistance = enter; //The minimum distance to the plane
                    }
                }
            }

        }


        return exitPoint;
    }

    bool CheckForStarHit(Vector3 entryPoint, Vector3 exitPoint, Star star, out Color color)
    {
        //check if a ray between the two points hits the Star
        Vector3 starPosition = new Vector3(star.offSet.x * AUSize, star.offSet.y * AUSize, star.offSet.z * AUSize); //The position of the Star
        float starSize = (baseStarRadius + (radiusIncrease * star.mass)) * 0.5f; //The radius of the Star, discovered recently that the scale is actually the diameter, so we need to divide by 2

        Vector3 rayDirNotNormalized = entryPoint - exitPoint; //The direction of the ray
        Vector3 starDirection = (starPosition - entryPoint); //The direction of the ray

        float a = Vector3.Dot(rayDirNotNormalized, rayDirNotNormalized);
        float b = 2 * Vector3.Dot(starDirection, rayDirNotNormalized); //The distance between the entry point and the Star
        float c = Vector3.Dot(starDirection, starDirection) - starSize * starSize;

        float discriminant = b * b - 4 * a * c; //The discriminant of the quadratic equation

        if (discriminant >= 0)
        {
            //Possible hit
            discriminant = Mathf.Sqrt(discriminant); //The square root of the discriminant
            float t1 = (-b - discriminant) / (2 * a); //The first solution of the quadratic equation
            float t2 = (-b + discriminant) / (2 * a); //The second solution of the quadratic equation

            if ((t1 >= 0 && t1 <= 1) || (t2 >= 0 && t2 <= 1)) //If both solutions are positive, then the ray hits the Star
            {
                color = Color.black; //Return black, we'll grab the star we hit outside of this function
                return true; //Show the visual Star
            }

            color = Color.black;
            return false;
        }
        else
        {
            //Calculate the closest distance between the ray and the star
            float t = Vector3.Dot(starDirection, rayDirNotNormalized);

            //if the closest distance is less than 4x the radius of the star, we can inherit some light from the star
            if (t < 0)
            {
                t = -t;
            }

            if (t < starSize * 4)
            {
                //We'll inherit between 0 and 50% of the light from the star, depending on how close we are to it
                //We'll use a curve, so the light falls off quickly at the edge of the extended radius
                t = t - starSize; //Ignore the first radius
                t = t / (starSize * 3); //Divide by the extended radius, t is now the percentage of light we can inherit
                t = Mathf.Clamp01(t); //Clamp t between 0 and 1

                float light = (Mathf.Pow(-t, 2) + 1) / 2; //Should limit the light at 0.5 at 0, to 0 at 1
                color = new Color(star.color.r * light, star.color.g * light, star.color.b * light); //The color of the star, multiplied by the light
            }
            else
            {
                color = Color.black; //The color of the star
            }
            return false;
        }
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


        //Cube Visualizing
        if (cubeVisualize)
        {
            Vector3Int visualizedIndex = starIndex; //Set the visualized index to the current index
            //Draw a line between the two points
            Debug.DrawLine(gizmoOne.transform.position, gizmoTwo.transform.position, Color.red);

            Vector3 rayDirection = gizmoTwo.transform.position - gizmoOne.transform.position; //The direction of the ray

            Vector3 entryPoint = gizmoOne.transform.position; //The entry point of the ray
            Vector3 exitpoint = CalculateExitPoint(entryPoint, rayDirection); //The exit point of the ray

            if (exitpointGizmos == null || exitpointGizmos.Length == 0) //If the exit point gizmo doesn't exist, create it
            {
                exitpointGizmos = new GameObject[100]; //Set the size of the array to 100
            }

            for (int j = 0; j < exitpointGizmos.Length; j++)
            {
                if (exitpointGizmos[j] == null) //If the gizmo doesn't exist, create it
                {
                    break; //Break out of the loop, we've hit the end of the array
                }
                if (exitpointGizmos[j].activeSelf)
                {
                    exitpointGizmos[j].SetActive(false); //Deactivate the gizmo
                }
            }


            //If the exit point gizmo doesn't exist, create it
            if (exitpointGizmos[0] == null)
            {
                GameObject exitpointGizmo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                exitpointGizmo.transform.localScale = new Vector3(10f, 10f, 10f);
                exitpointGizmo.GetComponent<MeshRenderer>().material = exitPointMaterial;
                exitpointGizmos[0] = exitpointGizmo; //Set the first gizmo to the new gizmo
            }
            exitpointGizmos[0].transform.position = new Vector3(exitpoint.x, exitpoint.y, exitpoint.z); //Set the position of the exit point gizmo
            exitpointGizmos[0].SetActive(true); //Activate the gizmo

            int i = 1;
            while (visualizedIndex != new Vector3Int(-1, -1, -1))
            {
                //We loop until we hit the edge of the universe, or until the ray stops
                Vector3Int nextSectorIndex = GetNextSectorIndex(visualizedIndex, exitpoint); //Get the next sector index, so we can check if the ray hits a star in the next sector
                if (nextSectorIndex == visualizedIndex) //The ray didn't reach the next sector, so we can break out of the loop
                {
                    break;
                }
                if (nextSectorIndex == new Vector3Int(-1, -1, -1)) //If we hit the edge of the universe
                {
                    break; //Break out of the loop, we've hit the edge of the universe
                }
                if (exitpointGizmos.Length > 100) //Max number of gizmos
                {
                    break;
                }

                visualizedIndex = nextSectorIndex; //Set the visualized index to the next sector index
                entryPoint = setEntryPoint(exitpoint, visualizedIndex); //Set the new entry point, for the next cube. We know we're going in an

                exitpoint = CalculateExitPoint(entryPoint, rayDirection); //The exit point of the ray

                float distance = Vector3.Distance(entryPoint, exitpoint);
                Vector3 directionNormal = Vector3.Normalize(rayDirection);

                Vector3 adjustment = directionNormal * distance;

                //Check if there's an exit point gizmo for this index
                if (exitpointGizmos[i] == null)
                {
                    GameObject exitpointGizmo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    exitpointGizmo.transform.localScale = new Vector3(10f, 10f, 10f);
                    exitpointGizmo.GetComponent<MeshRenderer>().material = exitPointMaterial;
                    exitpointGizmos[i] = exitpointGizmo;
                }
                
                exitpointGizmos[i].transform.position = new Vector3(exitpointGizmos[i-1].transform.position.x + adjustment.x, exitpointGizmos[i - 1].transform.position.y + adjustment.y, exitpointGizmos[i - 1].transform.position.z + adjustment.z); //Set the position of the exit point gizmo
                exitpointGizmos[i].SetActive(true); //Activate the gizmo

                i++;
            }



            //Disabled for now. Needs updating for multiple sectors
            ////Create a Star in the correct spot in the cube, based on the Star Index
            //Star starDetails = stars[starIndex.x, starIndex.y, starIndex.z]; //Get the Star details from the array
            //if (visualStar == null)
            //{
            //    visualStar = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //    visualStar.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            //    visualStar.GetComponent<MeshRenderer>().enabled = false; //Hidden unless the ray is hitting a Star
            //}

            //visualStar.transform.position = new Vector3(starDetails.offSet.x * AUSize, starDetails.offSet.y * AUSize, starDetails.offSet.z * AUSize); //Set the position of the visual Star
            //float scale = baseStarRadius + (radiusIncrease * starDetails.mass); //Calculate the scale of the Star
            //visualStar.transform.localScale = new Vector3(scale, scale, scale); //Set the scale of the visual Star

            ////check if a ray between the two points hits the Star
            //Vector3 starPosition = new Vector3(starDetails.offSet.x * AUSize, starDetails.offSet.y * AUSize, starDetails.offSet.z * AUSize); //The position of the Star
            //float starSize = (baseStarRadius + (radiusIncrease * starDetails.mass)) * 0.5f; //The radius of the Star, discovered recently that the scale is actually the diameter, so we need to divide by 2

            //Vector3 rayDirNotNormalized = entryPoint - exitpoint; //The direction of the ray
            //Vector3 starDirection = (starPosition - entryPoint); //The direction of the ray

            //float a = Vector3.Dot(rayDirNotNormalized, rayDirNotNormalized);
            //float b = 2 * Vector3.Dot(starDirection, rayDirNotNormalized); //The distance between the entry point and the Star
            //float c = Vector3.Dot(starDirection, starDirection) - starSize * starSize;

            //float discriminant = b * b - 4 * a * c; //The discriminant of the quadratic equation

            //if (discriminant >= 0) {
            //    //Possible hit
            //    discriminant = Mathf.Sqrt(discriminant); //The square root of the discriminant
            //    float t1 = (-b - discriminant) / (2 * a); //The first solution of the quadratic equation
            //    float t2 = (-b + discriminant) / (2 * a); //The second solution of the quadratic equation

            //    if ((t1 >= 0 && t1 <= 1) || (t2 >= 0 && t2 <= 1)) //If both solutions are positive, then the ray hits the Star
            //    {
            //        visualStar.GetComponent<MeshRenderer>().enabled = true; //Show the visual Star
            //    }
            //    else
            //    {
            //        visualStar.GetComponent<MeshRenderer>().enabled = false; //Hide the visual Star
            //    }
            //}
            //else
            //{
            //    visualStar.GetComponent<MeshRenderer>().enabled = false; //Hide the visual Star
            //}


            
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
