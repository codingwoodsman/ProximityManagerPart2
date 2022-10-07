using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProximityManager : MonoBehaviour
{
    [SerializeField]
    int ANIMATING_DISTANCE;

    GameObject mainCamera;
    List<GameObject> npcs;
    List<List<Mesh>> npcMeshes;
    List<List<Mesh>> npcMeshes2;
    List<List<GameObject>> npcMeshesGO;
    GameObject meshTemplate;
    GameObject meshTemplate2;
    GameObject meshTemplate3;

    int updateIndex;
    List<bool> isWalk;
    int walkIndex;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = GameObject.Find("Main Camera");
        GameObject[] GOList = (GameObject[])GameObject.FindObjectsOfType(typeof(GameObject));

        //List of GameObjects that are parent to the skinned mesh renderer component gameObjects
        npcs = new List<GameObject>();

        //A list of lists of meshes. Each list represents a collection of meshes (hair, lod, weapon etc.)
        //One list of meshes per animated model
        npcMeshes = new List<List<Mesh>>();

        //The second group of meshes representing the second pose in the walk animation to go to and from
        npcMeshes2 = new List<List<Mesh>>();

        //GameObjects associated with each list of meshes (probably 1 per animated model)
        npcMeshesGO = new List<List<GameObject>>();

        //Template with components and materials for building a new object easily
        //Template gameObject must be in scene and named "MeshTemplate"
        meshTemplate = GameObject.Find("MeshTemplate");

        //2nd Template with components and materials for building a new object easily
        //Template gameObject must be in scene and named "MeshTemplate2"
        //If animated object has components with more than 2 materials, then add MeshTemplate3 etc.
        meshTemplate2 = GameObject.Find("MeshTemplate2");

        //3nd Template with components and materials for building a new object easily
        meshTemplate3 = GameObject.Find("MeshTemplate3");

        updateIndex = 0;
        isWalk = new List<bool>();
        walkIndex = 0;


        int j = 0;
        for (int i = 0; i < GOList.Length; i++)
        {
            if (GOList[i].name.Contains("nackedSet"))
            {
                npcs.Add(GOList[i]);
                isWalk.Add(true);

                //if the list doesn't exist yet, make it
                if (npcMeshes.Count < j + 1)
                {
                    npcMeshes.Add(new List<Mesh>());
                    npcMeshes2.Add(new List<Mesh>());
                    npcMeshesGO.Add(new List<GameObject>());
                }

                //for every child in the gameObject we are meshifying, create a new mesh slot
                //if there is a skinned mesh renderer 
                for (int k = 0; k < npcs[j].transform.childCount; k++)
                {
                    if (GOList[i].transform.GetChild(k).GetComponent<SkinnedMeshRenderer>() != null)
                    { 
                        Mesh tempMesh = new Mesh();
                        Mesh tempMesh2 = new Mesh();
                        tempMesh.name = "MyMesh " + j + "-" + k;
                        npcMeshes[j].Add(tempMesh);

                        tempMesh2.name = "MyMesh2 " + j + "-" + k;
                        npcMeshes2[j].Add(tempMesh2);

                        GameObject tempGO = new GameObject("Mesh" + j + "-" + k);
                        tempGO.AddComponent<MeshFilter>();
                        tempGO.AddComponent<MeshRenderer>();
                        tempGO.SetActive(false);
                        npcMeshesGO[j].Add(tempGO);
                    }
                }

                if (Vector3.Distance(mainCamera.transform.position, GOList[i].transform.position) > ANIMATING_DISTANCE)
                {
                    BakeIt(GOList[i], j);
                }
                else
                {
                    //do nothing
                }
                j++;
            }
        }

        //Start coroutine ffor swapping back and forth the walking meshes
        StartCoroutine("WalkSwap");

        StartCoroutine("SmoothWalk");
    }

    // Update is called once per frame
    void Update()
    {
            //test distance between camera and npcs
            for (int i = 0; i < npcs.Count; i++)
            {
                if (npcs[i].activeSelf == true)
                {
                    if (Vector3.Distance(mainCamera.transform.position, npcs[i].transform.position) > ANIMATING_DISTANCE)
                    {
                        BakeIt(npcs[i], i);
                    }
                }
                else
                {
                    if (Vector3.Distance(mainCamera.transform.position, npcs[i].transform.position) <= ANIMATING_DISTANCE)
                    {
                        npcs[i].SetActive(true);
                        for (int j = 0; j < npcMeshesGO[i].Count; j++)
                        {
                            npcMeshesGO[i][j].SetActive(false);
                        }
                    }
                }
            }

    }

    IEnumerator WalkSwap()
    {
        for (; ; )
        {
            //if first grouping
            if (walkIndex == 0)
            {
                AnimateGroup(0);
            }
            else if (walkIndex == 1)
            {
                AnimateGroup(1);
            }
            else if (walkIndex == 2)
            {
                AnimateGroup(2);
            }
            else if (walkIndex == 3)
            {
                AnimateGroup(3);
            }
            else
            {
                //do nothing
            }

            yield return new WaitForSeconds(.1f);
        }
    }

    IEnumerator SmoothWalk()
    {
        for (; ; )
        {
            for (int i = 0; i < npcs.Count; i++)
            {
                //if (npcs[i].activeSelf)
                //{
                //npcs[i].transform.position = new Vector3(npcs[i].transform.position.x, npcs[i].transform.position.y, npcs[i].transform.position.z + 0.0155f);
                //}
                npcs[i].transform.position = new Vector3(npcs[i].transform.position.x, npcs[i].transform.position.y, npcs[i].transform.position.z + 0.0155f);

                for (int j = 0; j < npcMeshesGO[i].Count; j++)
                {
                    npcMeshesGO[i][j].transform.position = new Vector3(npcMeshesGO[i][j].transform.position.x, npcMeshesGO[i][j].transform.position.y, npcMeshesGO[i][j].transform.position.z + 0.0155f);
                }
            }
            yield return new WaitForSeconds(.01f);
        }
    }

    /// <summary>
    /// Bakes the GameObject passed in
    /// </summary>
    /// <param name="gO">GameObject to be baked</param>
    /// <param name="j">the index of that GameObject for reference in npcMeshes list</param>
    void BakeIt(GameObject gO, int j)
    {
        
        //Vector3 npcLoc = gO.transform.position;

        //get animator and call the SampleAnimation function on the ith element
        Animator anim = gO.GetComponent<Animator>();
        anim.runtimeAnimatorController.animationClips[0].SampleAnimation(gO, 0.2f);

        //bake mesh
        //gO.GetComponentInChildren<SkinnedMeshRenderer>().BakeMesh(npcMeshes[j]);

        //bake mesh for each skinned mesh renderer component under the gameobject passed in as gO
        SkinnedMeshRenderer[] listOSkins = gO.GetComponentsInChildren<SkinnedMeshRenderer>();
        for (int i = 0; i < listOSkins.Length; i++)
        {

            Debug.Log("Before : npcMeshes[j][i]: " + npcMeshes[j][i].name);

            //listOSkins[i].BakeMesh(npcMeshesGO[j][i].GetComponent<MeshFilter>().mesh);
            listOSkins[i].BakeMesh(npcMeshes[j][i]);

            Debug.Log("After : npcMeshes[j][i]: " + npcMeshes[j][i].name);

            if (npcMeshesGO[j][i].name.Contains("-3") || npcMeshesGO[j][i].name.Contains("-6"))
            {
                npcMeshesGO[j][i].GetComponent<MeshRenderer>().materials = meshTemplate2.GetComponent<MeshRenderer>().materials;
            }
            else if(npcMeshesGO[j][i].name.Contains("-5"))
            {
                npcMeshesGO[j][i].GetComponent<MeshRenderer>().materials = meshTemplate3.GetComponent<MeshRenderer>().materials;
            }
            else
            {
                npcMeshesGO[j][i].GetComponent<MeshRenderer>().materials = meshTemplate.GetComponent<MeshRenderer>().materials;
            }

            //make the mesh and skinned mesh the same loc
            //gO.transform.position = npcLoc;

            npcMeshesGO[j][i].SetActive(true);
        }
        for(int i = 0; i < npcMeshesGO[j].Count; i ++)
        {
            npcMeshesGO[j][i].transform.position = gO.transform.position;
        }


        //Add second mesh group
        anim.runtimeAnimatorController.animationClips[0].SampleAnimation(gO, 0.6f);
        for (int i = 0; i < listOSkins.Length; i++)
        {
            //listOSkins[i].BakeMesh(npcMeshesGO[j][i].GetComponent<MeshFilter>().mesh);

            listOSkins[i].BakeMesh(npcMeshes2[j][i]);
        }
        gO.SetActive(false);
        
    }

    /// <summary>
    /// Animates the group passed in. This is used to stagger the animations so they don't look like
    /// synchronized swimmers.
    /// </summary>
    /// <param name="groupNumber">The group to animate</param>
    void AnimateGroup(int groupNumber)
    {
        //for every gameobject holding a mesh component
        for (int i = 0; i < npcMeshesGO.Count; i++)
        {
            //if it is 1, 2, 3, or group 4's turn
            if (i % 4 == groupNumber)
            {
                if (isWalk[i]) //if first walk frame
                {
                    for (int j = 0; j < npcMeshesGO[i].Count; j++)
                    {
                        Debug.Log("isWalk1 = true || npcMeshesGO[i][j].name: " + npcMeshesGO[i][j].name);
                        npcMeshesGO[i][j].GetComponent<MeshFilter>().mesh = npcMeshes[i][j];
                        //npcMeshesGO[i][j].transform.position = new Vector3(npcMeshesGO[i][j].transform.position.x, npcMeshesGO[i][j].transform.position.y, npcMeshesGO[i][j].transform.position.z + 0.5f);
                        if (!npcs[i].activeSelf)
                        {
                            //npcs[i].transform.position = new Vector3(npcs[i].transform.position.x, npcs[i].transform.position.y, npcs[i].transform.position.z + 0.5f);
                        }
                    }
                    isWalk[i] = !isWalk[i];
                }
                else
                {

                    for (int j = 0; j < npcMeshesGO[i].Count; j++)
                    {
                        Debug.Log("isWalk1 = false || npcMeshesGO[i][j].name: " + npcMeshesGO[i][j].name);
                        npcMeshesGO[i][j].GetComponent<MeshFilter>().mesh = npcMeshes2[i][j];
                        //npcMeshesGO[i][j].transform.position = new Vector3(npcMeshesGO[i][j].transform.position.x, npcMeshesGO[i][j].transform.position.y, npcMeshesGO[i][j].transform.position.z + 0.5f);
                        if (!npcs[i].activeSelf)
                        {
                            //npcs[i].transform.position = new Vector3(npcs[i].transform.position.x, npcs[i].transform.position.y, npcs[i].transform.position.z + 0.5f);
                        }
                    }
                    isWalk[i] = !isWalk[i];
                }
            }
        }
        if (groupNumber == 3)
        {
            walkIndex = 0;
        }
        else
        {
            walkIndex++;
        }
        
    }
}
