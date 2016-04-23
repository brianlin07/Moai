﻿using UnityEngine;
using System.Collections;

public class WaterBody : MonoBehaviour {

    public Vector3 size;
    public Vector3 center;
    public Biome biome;
    public float waterResolution;

    private Mesh mesh;
    private float stepLength = 0.1f;
    private int terrain;
    private int maxTries = 30;
    public bool settled = false;
    public bool expanded = false;
    public bool setBelow = false;

    // References
    private MeshFilter mf;

	// Use this for initialization
	void Start () {
        mesh = GetComponent<MeshFilter>().mesh;
        terrain = LayerMask.GetMask("Terrain");
        center.y = Mathf.Infinity;
        mf = GetComponent<MeshFilter>();
        size = Vector3.right + Vector3.forward;
    }
	
	// Update is called once per frame
	void Update () {
        if (!settled || !expanded ||!setBelow) fitToTerrain();
	}

    // Returns whether this body overlaps another
    public bool overlaps(WaterBody other)
    {
        if (Vector3.Distance(center,other.center)<size.x+other.size.x)
            return true;
        return false;
    }

    public void fitToTerrain()
    {
        // Stick water to ground
        if (!settled)
            moveToLocalMinimum();
        else if (!expanded)
            moveToMaxFillHeight();
        else if (!setBelow)
            moveCornersDown();
    }

    // Drops the water body to the lwoest nearby point
    private void moveToLocalMinimum()
    {
        float minHeight = -1;
        int tries = 0;
        RaycastHit hit;
        Vector3 topLeft = new Vector3(center.x - stepLength, 10000000, center.z - stepLength);
        Vector3 topRight = new Vector3(center.x + stepLength, 10000000, center.z - stepLength);
        Vector3 bottomLeft = new Vector3(center.x - stepLength, 10000000, center.z + stepLength);
        Vector3 bottomRight = new Vector3(center.x + stepLength, 10000000, center.z + stepLength);


        while(tries < maxTries)
        {
            // Cast rays around current center
            

            topLeft = new Vector3(center.x - stepLength, 10000000, center.z - stepLength);
            topRight = new Vector3(center.x + stepLength, 10000000, center.z - stepLength);
            bottomLeft = new Vector3(center.x - stepLength, 10000000, center.z + stepLength);
            bottomRight = new Vector3(center.x + stepLength, 10000000, center.z + stepLength);

            Ray rayTopLeft      = new Ray(topLeft, Vector3.down);
            Ray rayTopRight     = new Ray(topRight, Vector3.down);
            Ray rayBottomLeft   = new Ray(bottomLeft, Vector3.down);
            Ray rayBottomRight  = new Ray(bottomRight, Vector3.down);

            if (Physics.Raycast(rayTopLeft, out hit, Mathf.Infinity, terrain))
            {
                topLeft = hit.point;
            }
                
            if (Physics.Raycast(rayTopRight, out hit, Mathf.Infinity, terrain))
            {
                topRight = hit.point;
            }
                
            if (Physics.Raycast(rayBottomLeft, out hit, Mathf.Infinity, terrain))
            {
                bottomLeft = hit.point;
            }
                
            if (Physics.Raycast(rayBottomRight, out hit, Mathf.Infinity, terrain))
            {
                bottomRight = hit.point;
            }

            minHeight = Mathf.Min(topLeft.y,topRight.y,bottomLeft.y,bottomRight.y);

            // If we have found a local minimum, exit the loop
            if (center.y <= minHeight)
            {
                settled = true;
                break;
            }
            // Otherwise, move to the new lowest point and continue searching
            else
            {
                if (minHeight == topLeft.y) center = topLeft;
                else if (minHeight == topRight.y) center = topRight;
                else if (minHeight == bottomLeft.y) center = bottomLeft;
                else center = bottomRight;
                tries++;
            }
        }
        transform.position = center;
    }

    // Gradually expands the water body to the highest it could fill this point
    private void moveToMaxFillHeight()
    {
        int tries = 0;
        RaycastHit hit;
        // Direction rays
        Vector3 topLeft = Vector3.left + Vector3.forward;
        Vector3 topRight = Vector3.right + Vector3.forward;
        Vector3 bottomLeft = Vector3.left + Vector3.back;
        Vector3 bottomRight = Vector3.right + Vector3.back;

        // Contact positions
        Vector3 topPoint,bottomPoint,leftPoint,rightPoint,topLeftPoint, topRightPoint, bottomLeftPoint, bottomRightPoint;

        // Will be asigned to center after height calculations are finished
        Vector3 physicalCenter = Vector3.zero;

        while (tries < maxTries)
        {
            topPoint = Vector3.zero;
            bottomPoint = Vector3.zero;
            leftPoint = Vector3.zero;
            rightPoint = Vector3.zero;
            topLeftPoint = Vector3.zero;
            topRightPoint = Vector3.zero;
            bottomLeftPoint = Vector3.zero;
            bottomRightPoint = Vector3.zero;

            tries++;

            Ray rayTop = new Ray(center + Vector3.up * stepLength, Vector3.forward);
            Ray rayBottom = new Ray(center + Vector3.up * stepLength, Vector3.back);
            Ray rayLeft = new Ray(center + Vector3.up * stepLength, Vector3.left);
            Ray rayRight = new Ray(center + Vector3.up * stepLength, Vector3.right);
            Ray rayTopLeft = new Ray(center + Vector3.up * stepLength, topLeft);
            Ray rayTopRight = new Ray(center + Vector3.up * stepLength, topRight);
            Ray rayBottomLeft = new Ray(center + Vector3.up * stepLength, bottomLeft);
            Ray rayBottomRight = new Ray(center + Vector3.up * stepLength, bottomRight);

           // Debug.DrawRay(rayTopLeft.origin, rayTopLeft.direction,Color.red,1000);

            // Cast rays from center to see if we have lakeable terrain
            if (Physics.Raycast(rayTopLeft, out hit, biome.lakeMaxLength, terrain))
            {
                topLeftPoint = hit.point;
            }
            if (Physics.Raycast(rayTopRight, out hit, biome.lakeMaxLength, terrain))
            {
                topRightPoint = hit.point;
            }
            if (Physics.Raycast(rayBottomLeft, out hit, biome.lakeMaxLength, terrain))
            {
                bottomLeftPoint = hit.point;
            }
            if (Physics.Raycast(rayBottomRight, out hit, biome.lakeMaxLength, terrain))
            {
                bottomRightPoint = hit.point;
            }
            if (Physics.Raycast(rayBottom, out hit, biome.lakeMaxLength, terrain))
            {
                bottomPoint = hit.point;
            }
            if (Physics.Raycast(rayTop, out hit, biome.lakeMaxLength, terrain))
            {
                topPoint = hit.point;
            }
            if (Physics.Raycast(rayLeft, out hit, biome.lakeMaxLength, terrain))
            {
                leftPoint = hit.point;
            }
            if (Physics.Raycast(rayRight, out hit, biome.lakeMaxLength, terrain))
            {
                rightPoint = hit.point;
            }

            // If any of those casts didn't find ANY terrain, this height is not lakeable
            if (topLeftPoint == Vector3.zero || topRightPoint == Vector3.zero ||
                bottomLeftPoint == Vector3.zero || bottomRightPoint == Vector3.zero||
                leftPoint == Vector3.zero || rightPoint == Vector3.zero ||
                topPoint == Vector3.zero || bottomPoint == Vector3.zero)
            {
                expanded = true;   
                break;
            }
                
            // Otherwise move up to this position, update size, and keep searching
            else
            {
                float tlDis = Vector3.Distance(center, topLeftPoint);
                float trDis = Vector3.Distance(center, topRightPoint);
                float blDis = Vector3.Distance(center, bottomLeftPoint);
                float brDis = Vector3.Distance(center, bottomRightPoint);
                float lDis = Vector3.Distance(center, leftPoint);
                float rDis = Vector3.Distance(center, rightPoint);
                float tDis = Vector3.Distance(center, topPoint);
                float bDis = Vector3.Distance(center, bottomPoint);

                size.x = (lDis + rDis);
                size.z = (bDis + tDis);
                center.y += stepLength;
                
                physicalCenter.y = center.y;
                physicalCenter.x = center.x + rDis - lDis;
                physicalCenter.z = center.z + bDis - tDis;
            }
        }
        transform.position = center;
        if (expanded)
        {
            center = physicalCenter;
            calculateVertices();
        }
    }

    private void moveCornersDown()
    {
        if (setBelow) return;
        RaycastHit hit;
        float minHeight = Mathf.Infinity;
        int xRes = Mathf.CeilToInt(size.x * waterResolution);
        int yRes = Mathf.CeilToInt(size.z * waterResolution);
        for (int iy = 0; iy < xRes; iy += mf.mesh.vertices.Length - 1)
        {
            for (int ix = 0; ix < mf.mesh.vertices.Length; ix += mf.mesh.vertices.Length -1)
            {
                Ray ray = new Ray(mf.mesh.vertices[iy * xRes + ix], Vector3.down);
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, terrain))
                {
                    if(hit.point.y < minHeight)
                        minHeight = hit.point.y;
                }
            }
        }
        setBelow = true;
        center.y = minHeight;
        transform.position = center;
    }

    private void calculateVertices()
    {
        if (size.Equals(Vector3.zero)) return;
        int xRes = Mathf.CeilToInt(size.x * waterResolution);
        int yRes = Mathf.CeilToInt(size.z * waterResolution);
        float xStepSize = size.x / (xRes - 1);
        float yStepSize = size.z / (yRes - 1);

        // Generate verticies
        Vector3[] vertices = new Vector3[(xRes * yRes)];
        for (int iy = 0; iy < yRes; iy++)
        {
            for (int ix = 0; ix < xRes; ix++)
            {
                float x = ix * xStepSize - size.x * 0.5f;
                float y = iy * yStepSize - size.z * 0.5f;
                vertices[iy * xRes + ix] = new Vector3(x, 0, y);
            }
        }
        mf.mesh.vertices = vertices;

        // Generate triangles using these vertices
        int[] triangles = new int[(xRes - 1) * (yRes - 1) * 6];
        int i = 0;
        // iterate through each quad in vertices
        for (int y = 0; y < yRes - 1; y++)
        {
            for (int x = 0; x < xRes - 1; x++)
            {
                int v1 = x + y * xRes;
                int v2 = (x + 1) + y * xRes;
                int v3 = x + (y + 1) * xRes;
                int v4 = (x + 1) + (y + 1) * xRes;

                if (Mathf.Repeat(x + y, 2) == 1)
                { //top left to bottom right
                    triangles[i] = v4;
                    triangles[i + 1] = v1;
                    triangles[i + 2] = v3;
                    triangles[i + 3] = v1;
                    triangles[i + 4] = v4;
                    triangles[i + 5] = v2;
                }
                else
                { //top right to bottom left
                    triangles[i] = v4;
                    triangles[i + 1] = v2;
                    triangles[i + 2] = v3;
                    triangles[i + 3] = v2;
                    triangles[i + 4] = v1;
                    triangles[i + 5] = v3;
                }

                i += 6;
            }
        }
        mf.mesh.triangles = triangles;

        ReCalcTriangles(mf.mesh);
    }

    private void ReCalcTriangles(Mesh mesh)
    {
        Vector3[] oldVerts = mesh.vertices;
        int[] triangles = mesh.triangles;
        Vector3[] vertices = new Vector3[triangles.Length];


        for (int i = 0; i < triangles.Length; i++)
        {
            vertices[i] = oldVerts[triangles[i]];

            triangles[i] = i;

        }
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }
}
