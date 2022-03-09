using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class creatureController1 : MonoBehaviour
{
    public GameObject creatureParent;

    public float timer = 1f;
    public float iTimer = 0f;
    public int mode = 1;
    public float speed = 2f;
    public float rotateSpeed = 2f;
    public float moveAdd = 0.5f;
    public float rotateAdd = 2f;
    public float pauseAdd = 1f;

    public bool ff = false;

    public float FOV = 10;
    
    public string foodTag;
    public GameObject nearestFoodObj;
    public float angleToFood = 0f;
    public float minFoodDist = Mathf.Infinity;

    public string treeTag;
    public GameObject nearestTreeObj;
    public float angleToTree = 0f;
    public float minTreeDist = Mathf.Infinity;

    public bool outOfZoneCorrDone = false;


    public float saturation = 200f;
    public float satPerFood = 20f;
    public float satPerTree = 100f;
    float maxSaturation;


    public float reproductiveUrge = 0f;
    public float reproductiveUrgeAdd = 0.3f;

    public string[] gender = new string[] {"male", "female"};
    public float minMateDist = Mathf.Infinity;
    public GameObject nearestMateObj;
    public Material maleMat;
    public Material femaleMat;
    public float reproductionTimer = 0f;
    public bool pregnant = false;
    public float iGestationTime = 0f;
    public float gestationTime = 500f;
    public float parentSpeedAv = 0f;
    public float parentFOVAv = 0f;
    public float parentHightAv = 0f;

    public bool allowFoodGathering = false;
    public bool returnToLastFood = false;
    public bool allowWandering = true;
    public bool stopForMating = false;
    public GameObject lastFoodObj1;
    public GameObject lastFoodObj2;
    public GameObject lastFoodObj3;
    public bool lastFoodObj1Destroyed =  false;
    public bool correctionRan = false;

    public float lifeTime = 3000f;
    public float iLifeTime = 0f;

    float FPScorrection = 0f;

    public float hight = 0.5f;


    void Start()
    {
        iLifeTime = lifeTime;
        maxSaturation = saturation;

        
        gameObject.tag = gender[Random.Range(0, 2)];

        transform.position = new Vector3(transform.position.x, hight, transform.position.z);
        

        if(gameObject.tag == "male"){
            GetComponent<MeshRenderer>().material = maleMat;
        }
        if(gameObject.tag == "female"){
            GetComponent<MeshRenderer>().material = femaleMat;
        }
    }

    
    void Update()
    {
        lastFoodObj1Destroyed = lastFoodObj1==null;


        FPScorrection = Time.deltaTime * 60f;

        move();

        if(Input.GetKeyDown("space")){
            ff = !ff;
        }

        if(ff){
            Time.timeScale = 20f;
        }
        else{
            Time.timeScale = 1f;
        }


        RaycastHit hit;
        if(!Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity)){
            float angleToMid = Mathf.Atan2(transform.position.x, transform.position.z) / Mathf.PI * 180f + 180f;
            transform.eulerAngles = new Vector3(0f, angleToMid, 0f);
        }





        mate();


        if(pregnant){
            if(iGestationTime >= gestationTime){
                iGestationTime = 0f;
                pregnant = false;
                reproductiveUrge = 0f;
                float speedBckp = speed;
                float FOVBckp = FOV;
                float iLifeTimeBckp = iLifeTime;
                float hightBckp = hight;
                speed = parentSpeedAv + Random.Range(-0.5f, 0.5f);
                FOV = parentFOVAv + Random.Range(-0.5f, 0.5f);
                hight = Mathf.Clamp(parentHightAv + Random.Range(-0.2f, 0.2f), 0.5f, 2f);
                iLifeTime = lifeTime;
                Instantiate(gameObject, new Vector3(transform.position.x+1f, 0.4f, transform.position.z), Quaternion.identity, creatureParent.transform);
                speed = speedBckp;
                FOV = FOVBckp;
                iLifeTime = iLifeTimeBckp;
                hight = hightBckp;
            }
            iGestationTime += FPScorrection;
        }


        transform.localScale = new Vector3(1f, hight, 1f);



        saturation -= 0.01f * speed * FOV/10 * FPScorrection;
        iLifeTime -= 0.1f * FPScorrection;
        if(iLifeTime <= 0f || saturation <= 0f){
            Destroy(gameObject);
        }
    }




    void mate(){
        if(reproductiveUrge >= 100f && gameObject.tag == gender[0]){
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, FOV);
            minMateDist = Mathf.Infinity;
            foreach (var hitCollider in hitColliders)
            {
                if(hitCollider.tag == gender[1] && !hitCollider.GetComponent<creatureController1>().pregnant){
                    float dist = Vector3.Distance(hitCollider.transform.position, transform.position);
                    if(dist <= minMateDist){
                        minMateDist = dist;
                        nearestMateObj = hitCollider.gameObject;
                    }
                }
            }
        }
        else{
            minMateDist = Mathf.Infinity;
            nearestMateObj = null;
        }

        if(minMateDist != Mathf.Infinity){
            nearestMateObj.GetComponent<creatureController1>().stopForMating = true;
            float angleToMate = Mathf.Atan2(transform.position.x - nearestMateObj.transform.position.x, transform.position.z - nearestMateObj.transform.position.z) / Mathf.PI * 180f + 180f;
            transform.eulerAngles = new Vector3(0f, angleToMate, 0f);
            if(minMateDist >= 2f){
                transform.Translate(Vector3.forward * speed * Time.deltaTime);
            }
            if(minMateDist <= 2.5f){
                reproductionTimer += FPScorrection;
            }
            if(reproductionTimer >= 500){
                nearestMateObj.GetComponent<creatureController1>().parentFOVAv = (FOV + nearestMateObj.GetComponent<creatureController1>().FOV) / 2;
                nearestMateObj.GetComponent<creatureController1>().parentSpeedAv = (speed + nearestMateObj.GetComponent<creatureController1>().speed) / 2;
                nearestMateObj.GetComponent<creatureController1>().parentHightAv = (hight + nearestMateObj.GetComponent<creatureController1>().hight) / 2;
                nearestMateObj.GetComponent<creatureController1>().pregnant = true;
                nearestMateObj.GetComponent<creatureController1>().stopForMating = false;
                reproductionTimer = 0f;
                reproductiveUrge = 0f;
            }
        }

        reproductiveUrge += reproductiveUrgeAdd * FPScorrection;
    }



    void move(){
        findFood();
        findTree();
        
        correctFoodOrder();



        if(saturation <= 70f && minFoodDist == Mathf.Infinity){
            returnToLastFood = true;
        }
        else{
            returnToLastFood = false;
        }

        if(((minFoodDist == Mathf.Infinity && !returnToLastFood && (minTreeDist == Mathf.Infinity || hight <= 1f)) || saturation >= maxSaturation/1.5f) && minMateDist == Mathf.Infinity && !stopForMating){
            allowWandering = true;
        }
        else{
            allowWandering = false;
        }

        if((minFoodDist != Mathf.Infinity || (minTreeDist != Mathf.Infinity && hight >= 1f)) && minMateDist == Mathf.Infinity && !stopForMating){
            allowFoodGathering = true;
        }
        else{
            allowFoodGathering = false;
        }


        if(allowWandering){
            wander();
        }

        if((allowFoodGathering || returnToLastFood) && saturation < maxSaturation/1.5f){
            if(((minTreeDist == Mathf.Infinity || hight <= 1f) && minFoodDist != Mathf.Infinity)/* && !nearestFoodObj == null*/){
                angleToFood = (Mathf.Atan2(transform.position.x - nearestFoodObj.transform.position.x, transform.position.z - nearestFoodObj.transform.position.z) / Mathf.PI * -180f) + 180f;
                transform.eulerAngles = new Vector3(0f, -angleToFood, 0f);
                transform.Translate(Vector3.forward * speed * Time.deltaTime);
            }
            if((minTreeDist != Mathf.Infinity && hight >= 1f)/* && !nearestTreeObj == null*/){
                angleToTree = (Mathf.Atan2(transform.position.x - nearestTreeObj.transform.position.x, transform.position.z - nearestTreeObj.transform.position.z) / Mathf.PI * -180f) + 180f;
                transform.eulerAngles = new Vector3(0f, -angleToTree, 0f);
                transform.Translate(Vector3.forward * speed * Time.deltaTime);
            }
            if(returnToLastFood && lastFoodObj1 != null){
                angleToFood = (Mathf.Atan2(transform.position.x - lastFoodObj1.transform.position.x, transform.position.z - lastFoodObj1.transform.position.z) / Mathf.PI * -180f) + 180f;
                transform.eulerAngles = new Vector3(0f, -angleToFood, 0f);
                transform.Translate(Vector3.forward * speed * Time.deltaTime);
            }
            

            if(minTreeDist < 1.5f && hight >= 1f){
                nearestTreeObj.GetComponent<treeScript2>().eaten = true;
                minTreeDist = Mathf.Infinity;
                saturation += satPerTree;
                iTimer = 0;
                mode = 2;
            }
            else if(minFoodDist < 1.5f){
                Destroy(nearestFoodObj);
                minFoodDist = Mathf.Infinity;
                saturation += satPerFood;
                iTimer = 0;
                mode = 2;
            }
        }
    }



    void findFood(){
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, FOV);
        minFoodDist = Mathf.Infinity;
        foreach (var hitCollider in hitColliders)
        {
            if(hitCollider.tag == foodTag){
                float dist = Vector3.Distance(hitCollider.transform.position, transform.position);
                if(dist <= minFoodDist){
                    minFoodDist = dist;
                    nearestFoodObj = hitCollider.gameObject;
                }
            }
        }
    }



    void findTree(){
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, FOV*2);
        minTreeDist = Mathf.Infinity;
        foreach (var hitCollider in hitColliders)
        {
            if(hitCollider.tag == treeTag && !hitCollider.gameObject.GetComponent<treeScript2>().eaten2){
                float dist = Vector3.Distance(hitCollider.transform.position, transform.position);
                if(dist <= minTreeDist){
                    minTreeDist = dist;
                    nearestTreeObj = hitCollider.gameObject;
                }
            }
        }
    }

    void correctFoodOrder(){
        if(lastFoodObj1 == null){
            correctionRan = true;
            lastFoodObj1 = lastFoodObj2;
            lastFoodObj2 = lastFoodObj3;
            lastFoodObj3 = null;
        }
        /*if(lastFoodObj2 == null){
            lastFoodObj2 = lastFoodObj3;
        }*/

        if(lastFoodObj1 != nearestFoodObj && allowFoodGathering){
            lastFoodObj3 = lastFoodObj2;
            lastFoodObj2 = lastFoodObj1;
            lastFoodObj1 = nearestFoodObj;
        }
    }



    void wander(){
        if(mode == 1){
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
            iTimer += moveAdd * FPScorrection;
        }

        if(mode == 2 || mode == 4){
            iTimer += pauseAdd * FPScorrection;
        }

        if(mode == 3){
            transform.eulerAngles = new Vector3(0f, Random.Range(0f, 360f), 0f);
            iTimer = timer;
        }


        if(iTimer < timer){
            
        }
        else{
            timer = Random.Range(50f, 200f);
            iTimer = 0f;
            mode = Random.Range(1, 4);
        }
    }
}
