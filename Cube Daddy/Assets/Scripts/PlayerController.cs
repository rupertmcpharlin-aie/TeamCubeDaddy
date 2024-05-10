using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;
using Pixelplacement;
using System;
using UnityEngine.InputSystem;
using Unity.VisualScripting;
using System.Linq;

public class PlayerController : MonoBehaviour
{
    //**********************************************************************************************************//
    #region Variables
    [Header("Player Input")]
    [SerializeField] InputActionAsset inputActions;
    private InputAction move;
    private InputAction camera_Mouse;
    private InputAction camera_Gamepad;
    [Space]
    [SerializeField] public Vector2 inputVector;
    [SerializeField] public Vector2 cameraVector_Mouse;
    [SerializeField] public Vector2 cameraVector_Gamepad;

    [Space]
    [Space]
    [Header("Tags")]
    [SerializeField] string tag_Environment;
    [SerializeField] string tag_MagenticEnvironment;
    [SerializeField] string tag_player;
    [SerializeField] string tag_currentPlayer;

    [Space]
    [Space]
    [Header("Movement")]
    [SerializeField] bool isMoving;
    [SerializeField] bool isFalling;
    [Space]
    [SerializeField] bool isMagnetic;
    [SerializeField] bool onMagneticCube;
    [Space]
    [SerializeField] RollType rollType;
    [Header("Rotation")]
    [SerializeField] float remainingAngle;
    [SerializeField] Vector3 rotationAnchor;
    [SerializeField] Vector3 rotationAxis;
    [Space]
    [Header("Cube variables")]
    [SerializeField] float scale;
    [SerializeField] float rollSpeed;
    [SerializeField] AnimationCurve rollCurve;
    [Space]
    [SerializeField] float bonkAngle;
    [SerializeField] float bonkSpeed;
    [SerializeField] AnimationCurve bonkCurve;
    [Space]
    [SerializeField] float headBonkAngle;
    [SerializeField] float headBonkSpeed;
    [SerializeField] AnimationCurve headBonkCurve;
    [Space]
    [SerializeField] float step1BonkAngle;
    [SerializeField] float step1BonkSpeed;
    [SerializeField] AnimationCurve step1BonkCurve;
    [Space]
    [SerializeField] float step2BonkAngle;
    [SerializeField] float step2BonkSpeed;
    [SerializeField] AnimationCurve step2BonkCurve;
    [Space]
    [SerializeField] float step3BonkAngle;
    [SerializeField] float step3BonkSpeed;
    [SerializeField] AnimationCurve step3BonkCurve;
    [Space]

    [Header("Fall")]
    [SerializeField] float fallDistance;
    [SerializeField] float fallJourneyLength;
    [SerializeField] AnimationCurve fallCurve;
    [SerializeField] RaycastHit hit;

    [Space]
    [Space]
    [Header("Cube")]
    [SerializeField] Transform cubeTransform;

    [Space]
    [Space]
    [Header("Larger Cubes")]
    [SerializeField] int cubes_index;
    [SerializeField] CubeData[] cubeDatas;

    [Space]
    [Space]
    [Header("Camera")]
    [SerializeField] CameraController cameraController;
    [SerializeField] public Transform vc_transform;
    [SerializeField] CameraFollow cameraFollow;
    [SerializeField] float cameraTrackingSpeed;

    [Space]
    [Space]
    [Header("Pressure Plates")]
    [SerializeField] PressurePlate[] pressurePlates;

    [Space]
    [Space]
    [Header("Enemies")]
    [SerializeField] EnemyController[] enemies;
    #endregion

    //**********************************************************************************************************//
    #region ENUM
    public enum RollType
    {
        stuck,
        bonk_flat,
        bonk_head,
        bonk_stepUp1,
        bonk_stepUp2,
        bonk_stepUp3,
        bonk_stepDown1,
        bonk_stepDown2,
        bonk_stepDown3,
        bonk_stepLeft1,
        bonk_stepLeft2,
        bonk_stepLeft3,
        bonk_stepRight1,
        bonk_stepRight2,
        bonk_stepRight3,
        bonk_climbUp_flat,
        bonk_climbUp_head,
        bonk_climbDown,
        bonk_climbLeft_flat,
        bonk_climbLeft_head,
        bonk_climbRight_flat,
        bonk_climbRight_head,
        flat,
        step_Up,
        step_Down,
        step_Left,
        step_Right,
        climb_Up,
        climb_Down,
        climb_Left,
        climb_Right
    }
    #endregion

    //**********************************************************************************************************//
    #region Start
    // Start is called before the first frame update
    void Start()
    {
        //pressure plates
        pressurePlates = FindObjectsOfType<PressurePlate>();

        //eneies
        enemies = FindObjectsOfType<EnemyController>();

        //cube transform
        cubeTransform = GameObject.FindGameObjectWithTag(tag_currentPlayer).transform;

        //camera controller
        cameraController = FindObjectOfType<CameraController>();

        //camera follow
        cameraFollow = FindObjectOfType<CameraFollow>();
        cameraFollow.currentCubeTransform = cubeTransform;

        //cubes
        cubeDatas = FindObjectsOfType<CubeData>();
        SortCubeDatasByScale(cubeDatas);

        //input
        inputActions.Enable();
        move = inputActions.FindAction("move");
        move.Enable();

        camera_Mouse = inputActions.FindAction("camera Mouse");
        camera_Mouse.Enable();

        camera_Gamepad = inputActions.FindAction("camera Gamepad");
        camera_Gamepad.Enable();
    }

    public void SortCubeDatasByScale(CubeData[] cubeDatas)
    {
        // Create a list to hold the cube datas
        List<CubeData> cubeDataList = new List<CubeData>(cubeDatas);

        // Sort the cube datas based on the scale variable
        cubeDataList.Sort((a, b) => a.scale.CompareTo(b.scale));

        // Copy the sorted cube datas back to the original array
        cubeDataList.CopyTo(cubeDatas);
    }

    #endregion

    //**********************************************************************************************************//
    #region Update
    // Update is called once per frame
    void Update()
    {
        //update player game object position
        transform.position = cubeTransform.position;

        //get player input
        #region Player Input
        //get player input
        inputVector = move.ReadValue<Vector2>();
        inputVector.x = Mathf.Round(inputVector.x);
        inputVector.y = Mathf.Round(inputVector.y);

        cameraVector_Mouse = camera_Mouse.ReadValue<Vector2>();
        cameraVector_Gamepad = camera_Gamepad.ReadValue<Vector2>();
        #endregion

        //camera 3 debugging
        #region camera debug
        //camera control
        if (cameraController.cameraState == CameraController.CameraState.camera3_DynamicIsometric_Unlocked)
        {
            if (Mathf.Abs(cameraVector_Gamepad.x) >= cameraController.camera3_gamepadThreshold)
            {
                if (cameraVector_Gamepad.x > 0)
                {
                    cameraController.DecreaseCamera3Index();
                }
                else
                {
                    cameraController.IncreaseCamera3Index();
                }
            }
            else if (Mathf.Abs(cameraVector_Mouse.x) >= cameraController.camera3_mouseThreshold)
            {
                if (cameraVector_Mouse.x > 0)
                {
                    cameraController.DecreaseCamera3Index();
                }
                else
                {
                    cameraController.IncreaseCamera3Index();
                }
            }
        }

        #endregion

        //control movement
        #region Movement
        //if current moving or falling do nothing
        if (isMoving) return;
        if (isFalling) return;

        //forward
        if (inputVector.y == 1)
        {
            StartCoroutine(Roll(Vector3.forward));
        }
        //back
        else if (inputVector.y == -1)
        {
            StartCoroutine(Roll(Vector3.back));
        }
        //right
        else if (inputVector.x == 1)
        {
            StartCoroutine(Roll(Vector3.right));
        }
        //left
        else if (inputVector.x == -1)
        {
            StartCoroutine(Roll(Vector3.left));
        }
        #endregion

        //quit
        #region quit
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        #endregion
    }
    #endregion

    //**********************************************************************************************************//
    #region Roll Coroutine
    //coroutine
    IEnumerator Roll(Vector3 direction)
    {
        //translate relative direction;
        direction = TranslateRelativeDirection(direction);

        //figure out roll type
        rollType = CalculateRollType(direction, cubeTransform.position, scale);

        //is moving = true
        isMoving = true;
        onMagneticCube = false;

        //set the roll type information
        #region Roll Type
        //get roll information
        switch (rollType)
        {
            #region flat
            //stuck
            case RollType.stuck:
                isMoving = false;
                yield break;

            //bonk
            case RollType.bonk_flat:
                remainingAngle = bonkAngle;
                rotationAnchor = cubeTransform.position - direction * scale / 2 + Vector3.down * scale / 2;
                break;

            //head bonk
            case RollType.bonk_head:
                remainingAngle = headBonkAngle;
                rotationAnchor = cubeTransform.position + direction * scale / 2 + Vector3.down * scale / 2;
                direction = -direction;
                break;

            //flat
            case RollType.flat:
                remainingAngle = 90f;
                rotationAnchor = cubeTransform.position + direction * scale / 2 + Vector3.down * scale / 2;
                break;

            #endregion

            #region Step

            #region Step Up
            //step up
            case RollType.step_Up:
                remainingAngle = 180f;
                rotationAnchor = cubeTransform.position + direction * scale / 2 + Vector3.up * scale / 2;
                break;

            //step up bonk 1
            case RollType.bonk_stepUp1:
                remainingAngle = headBonkAngle;
                rotationAnchor = cubeTransform.position + direction * scale / 2 + Vector3.up * scale / 2;
                direction = -direction;
                break;

            //step up bonk 2
            case RollType.bonk_stepUp2:
                remainingAngle = step2BonkAngle;
                rotationAnchor = cubeTransform.position + direction * scale / 2 + Vector3.up * scale / 2;
                direction = -direction;
                break;

            //step up bonk 3
            case RollType.bonk_stepUp3:
                remainingAngle = step3BonkAngle;
                rotationAnchor = cubeTransform.position + direction * scale / 2 + Vector3.up * scale / 2;
                direction = -direction;
                break;

            #endregion

            #region Step Down
            //step down
            case RollType.step_Down:
                remainingAngle = 180f;
                rotationAnchor = cubeTransform.position + direction * scale / 2 + Vector3.down * scale / 2;
                break;

            //step down bonk 1
            case RollType.bonk_stepDown1:
                remainingAngle = step1BonkAngle;
                rotationAnchor = cubeTransform.position + direction * scale / 2 + Vector3.down * scale / 2;
                direction = -direction;
                break;

            //step down bonk 2
            case RollType.bonk_stepDown2:
                remainingAngle = step2BonkAngle;
                rotationAnchor = cubeTransform.position + direction * scale / 2 + Vector3.down * scale / 2;
                direction = -direction;
                break;

            //step down bonk 3
            case RollType.bonk_stepDown3:
                remainingAngle = step3BonkAngle;
                rotationAnchor = cubeTransform.position + direction * scale / 2 + Vector3.down * scale / 2;
                direction = -direction;
                break;

            #endregion

            #region Step Left
            //step left
            case RollType.step_Left:
                remainingAngle = 180f;
                rotationAnchor = cubeTransform.position + direction * scale / 2 + -Vector3.Cross(direction, Vector3.up) * scale / 2;
                break;

            //step left bonk 1
            case RollType.bonk_stepLeft1:
                remainingAngle = step1BonkAngle;
                rotationAnchor = cubeTransform.position + direction * scale / 2 + -Vector3.Cross(direction, Vector3.up) * scale / 2;
                break;

            //step left bonk 2
            case RollType.bonk_stepLeft2:
                remainingAngle = step2BonkAngle;
                rotationAnchor = cubeTransform.position + direction * scale / 2 + -Vector3.Cross(direction, Vector3.up) * scale / 2;
                break;

            //step left bonk 2
            case RollType.bonk_stepLeft3:
                remainingAngle = step3BonkAngle;
                rotationAnchor = cubeTransform.position + direction * scale / 2 + -Vector3.Cross(direction, Vector3.up) * scale / 2;
                break;

            #endregion

            #region Step Right
            //step right
            case RollType.step_Right:
                remainingAngle = 180f;
                rotationAnchor = cubeTransform.position + direction * scale / 2 + Vector3.Cross(direction, Vector3.up) * scale / 2;
                break;

            //step right bonk 1
            case RollType.bonk_stepRight1:
                remainingAngle = step1BonkAngle;
                rotationAnchor = cubeTransform.position + direction * scale / 2 + Vector3.Cross(direction, Vector3.up) * scale / 2;
                direction = -direction;
                break;

            //step right bonk 2
            case RollType.bonk_stepRight2:
                remainingAngle = step2BonkAngle;
                rotationAnchor = cubeTransform.position + direction * scale / 2 + Vector3.Cross(direction, Vector3.up) * scale / 2;
                direction = -direction;
                break;

            //step right bonk 3
            case RollType.bonk_stepRight3:
                remainingAngle = step3BonkAngle;
                rotationAnchor = cubeTransform.position + direction * scale / 2 + Vector3.Cross(direction, Vector3.up) * scale / 2;
                direction = -direction;
                break;

            #endregion

            #endregion

            #region Climb

            #region Climb Up
            //climb up
            case RollType.climb_Up:
                remainingAngle = 90f;
                rotationAnchor = cubeTransform.position + direction * scale / 2 + Vector3.up * scale / 2;
                break;

            //climb up bonk flat
            case RollType.bonk_climbUp_flat:
                remainingAngle = 90f;
                rotationAnchor = cubeTransform.position - direction * scale / 2 + Vector3.up * scale / 2;
                break;

            //climb up bonk head
            case RollType.bonk_climbUp_head:
                remainingAngle = headBonkAngle;
                rotationAnchor = cubeTransform.position + direction * scale / 2 + Vector3.up * scale / 2;
                direction = -direction;
                break;

            #endregion

            #region Climb Down
            //climb down
            case RollType.climb_Down:
                remainingAngle = 90f;
                rotationAnchor = cubeTransform.position + -direction * scale / 2 + Vector3.down * scale / 2;
                break;

            //climb down bonk
            case RollType.bonk_climbDown:
                remainingAngle = headBonkAngle;
                rotationAnchor = cubeTransform.position + -direction * scale / 2 + Vector3.down * scale / 2;
                direction = -direction;
                break;

            #endregion

            #region Climb Left
            //climb left
            case RollType.climb_Left:
                remainingAngle = 90f;
                rotationAnchor = cubeTransform.position + direction * scale / 2 + -Vector3.Cross(direction, Vector3.up) * scale / 2;
                break;

            //climb left bonk flat
            case RollType.bonk_climbLeft_flat:
                remainingAngle = bonkAngle;
                rotationAnchor = cubeTransform.position - direction * scale / 2 + -Vector3.Cross(direction, Vector3.up) * scale / 2;
                break;

            //climb left bonk head
            case RollType.bonk_climbLeft_head:
                remainingAngle = bonkAngle;
                rotationAnchor = cubeTransform.position + direction * scale / 2 + -Vector3.Cross(direction, Vector3.up) * scale / 2;
                break;

            #endregion

            #region Climb Right
            //climb right
            case RollType.climb_Right:
                remainingAngle = 90f;
                rotationAnchor = cubeTransform.position + direction * scale / 2 + Vector3.Cross(direction, Vector3.up) * scale / 2;
                break;

            //climb right bonk flat
            case RollType.bonk_climbRight_flat:
                remainingAngle = bonkAngle;
                rotationAnchor = cubeTransform.position - direction * scale / 2 + Vector3.Cross(direction, Vector3.up) * scale / 2;
                break;

            //climb right bonk flat
            case RollType.bonk_climbRight_head:
                remainingAngle = bonkAngle;
                rotationAnchor = cubeTransform.position + direction * scale / 2 + Vector3.Cross(direction, Vector3.up) * scale / 2;
                break;

                #endregion

                #endregion
        }
        #endregion

        //execute roll rotation
        #region Rotation
        //////////////////////////////////////////////////////////////////////////////

        //find roll axis
        //LEFT
        if (rollType == RollType.climb_Left || 
            rollType == RollType.step_Left ||
            rollType == RollType.bonk_climbLeft_flat ||
            rollType == RollType.bonk_climbLeft_head)
        {
            rotationAxis = Vector3.up;
        }
        else if(rollType == RollType.bonk_stepLeft1 ||
                rollType == RollType.bonk_stepLeft2 ||
                rollType == RollType.bonk_stepLeft3)
        {
            rotationAxis = Vector3.down;
        }
        //RIGHT
        else if (rollType == RollType.climb_Right || 
                 rollType == RollType.step_Right ||
                 rollType == RollType.bonk_climbRight_flat ||
                 rollType == RollType.bonk_climbRight_head)
        {
            rotationAxis = Vector3.down;
        }
        else if(rollType == RollType.bonk_stepRight1 ||
                rollType == RollType.bonk_stepRight2 ||
                rollType == RollType.bonk_stepRight3)
        {
            rotationAxis = Vector3.up;
        }
        //NORMAL
        else
        {
            rotationAxis = Vector3.Cross(Vector3.up, direction);
        }
        //reset timer
        float timer = 0;

        //while there is still angle to go
        while (remainingAngle > 0)
        {
            float rotationAngle;
            //calculate rotation angle
            //uses min so that it goes exactly to the angle specified
            //bonk flat
            if (rollType == RollType.bonk_flat || rollType == RollType.bonk_climbLeft_flat || rollType == RollType.bonk_climbRight_flat)
            {
                rotationAngle = Mathf.Min(bonkCurve.Evaluate(timer) * bonkSpeed * Time.deltaTime, remainingAngle);
            }
            //head bonk
            else if (rollType == RollType.bonk_head || rollType == RollType.bonk_climbUp_head || rollType == RollType.bonk_climbLeft_head || rollType == RollType.bonk_climbRight_head)
            {
                rotationAngle = Mathf.Min(headBonkCurve.Evaluate(timer) * headBonkSpeed * Time.deltaTime, remainingAngle);
            }
            //bonk step 1
            else if (rollType == RollType.bonk_stepUp1 || rollType == RollType.bonk_stepDown1 || rollType == RollType.bonk_stepLeft1 || rollType == RollType.bonk_stepRight1)
            {
                rotationAngle = Mathf.Min(step1BonkCurve.Evaluate(timer) * step1BonkSpeed * Time.deltaTime, remainingAngle);
            }
            //bonk step 2
            else if (rollType == RollType.bonk_stepUp2 || rollType == RollType.bonk_stepDown2 || rollType == RollType.bonk_stepLeft2 || rollType == RollType.bonk_stepRight2)
            {
                rotationAngle = Mathf.Min(step2BonkCurve.Evaluate(timer) * step2BonkSpeed * Time.deltaTime, remainingAngle);
            }
            //bonk step 3
            else if (rollType == RollType.bonk_stepUp3 || rollType == RollType.bonk_stepDown3 || rollType == RollType.bonk_stepLeft3 || rollType == RollType.bonk_stepRight3)
            {
                rotationAngle = Mathf.Min(step3BonkCurve.Evaluate(timer) * step3BonkSpeed * Time.deltaTime, remainingAngle);
            }
            //normal
            else
            {
                rotationAngle = Mathf.Min(rollCurve.Evaluate(timer) * rollSpeed * Time.deltaTime, remainingAngle);
            }

            //rotation
            cubeTransform.RotateAround(rotationAnchor, rotationAxis, rotationAngle);

            //decrement remaing angle by the amount rotated
            remainingAngle -= rotationAngle;

            //increment timer
            timer += Time.deltaTime;
            yield return null;
        }

        if (rollType == RollType.bonk_flat || rollType == RollType.bonk_climbLeft_flat || rollType == RollType.bonk_climbRight_flat)
        {
            cubeTransform.RotateAround(rotationAnchor, rotationAxis, -bonkAngle);
        }
        else if (rollType == RollType.bonk_head || rollType == RollType.bonk_climbUp_head || rollType == RollType.bonk_climbLeft_head || rollType == RollType.bonk_climbRight_head)
        {
            cubeTransform.RotateAround(rotationAnchor, rotationAxis, -headBonkAngle);
        }
        else if (rollType == RollType.bonk_stepUp1 || rollType == RollType.bonk_stepDown1 || rollType == RollType.bonk_stepLeft1 || rollType == RollType.bonk_stepRight1)
        {
            cubeTransform.RotateAround(rotationAnchor, rotationAxis, -step1BonkAngle);
        }
        else if (rollType == RollType.bonk_stepUp2 || rollType == RollType.bonk_stepDown2 || rollType == RollType.bonk_stepLeft2 || rollType == RollType.bonk_stepRight2)
        {
            cubeTransform.RotateAround(rotationAnchor, rotationAxis, -step2BonkAngle);
        }
        else if (rollType == RollType.bonk_stepUp3 || rollType == RollType.bonk_stepDown3 || rollType == RollType.bonk_stepLeft3 || rollType == RollType.bonk_stepRight3)
        {
            cubeTransform.RotateAround(rotationAnchor, rotationAxis, -step3BonkAngle);
        }


        ///////////////////////////////////////////////////////////////////////////////
        #endregion

        //fix round errors
        #region Round Errors
        FixFloatingPointErrors();
        #endregion

        //falling behaviour
        #region Fall
        //fall
        //if there is nothing below at the end of a move
        if (!isBlock_down1)
        {
            //check not on a magnetic tile
            CheckIfOnMagneticCube();

            if (!onMagneticCube)
            {
                //draw a debug ray down
                Debug.DrawRay(cubeTransform.position, Vector3.down, Color.white, Mathf.Infinity);

                //raycast straight down
                Physics.Raycast(cubeTransform.position + Vector3.down * scale / 2, Vector3.down, out hit, Mathf.Infinity);

                //get distance
                fallDistance = hit.distance;

                //move cube down 
                Tween.Position(cubeTransform, cubeTransform.position + Vector3.down * fallDistance, fallJourneyLength * fallDistance / scale, 0, fallCurve, Tween.LoopType.None, HandleStartFallTween, HandleEndFallTween);

                //set falling to true
                isFalling = true;
            }
        }
        #endregion

        //final checks to end movement
        #region end movement
        //if is falling do nothing
        if (isFalling)
        {
            yield return new WaitForEndOfFrame();
        }
        else
        {
            //pressure plates
            CheckAllPressurePlates();

            //check if on magnetic cube
            CheckIfOnMagneticCube();

            //enemy moves
            CheckEnemyMovement();

            //check that cube hasnt been completed
            #region check completed cube
            if (cubeDatas.Count() > 0)
            {
                if (cubeTransform.position == cubeDatas[cubes_index].missingPosition.position)
                {
                    //set current small cubes parent to be large cubes transform
                    cubeTransform.gameObject.SetActive(false);

                    //set cube transform to large cube transform
                    cubeTransform = cubeDatas[cubes_index].transform;

                    //update camera follow transform
                    cameraFollow.currentCubeTransform = cubeDatas[cubes_index].transform;

                    //set scale
                    scale = cubeDatas[cubes_index].scale;

                    //set active = false incomplete meshes
                    cubeDatas[cubes_index].incompleteMesh.SetActive(false);

                    //set active = true complete meshes
                    cubeDatas[cubes_index].completeMesh.SetActive(true);

                    //increment index
                    if (cubes_index < cubeDatas.Length - 1)
                    {
                        cubes_index++;
                    }
                }
            }

            #endregion

            //set isMoving to false
            isMoving = false;
        }

        #endregion
    }

    //**********************************************************************************************************//
    //methods that assist the roll coroutine
    #region Roll Helper Methods

    //**********************************************************************************************************//
    //methods that help with magnetic cube calculations
    #region Magnetic Methods
    private void CheckIfOnMagneticCube()
    {
        #region Debug
        /*Debug.DrawRay(cubeTransform.position, Vector3.forward, Color.black, scale);
        Debug.Log("Forward" + Physics.Raycast(cubeTransform.position, Vector3.forward, scale));
        Debug.DrawRay(cubeTransform.position, Vector3.back, Color.black, scale);
        Debug.Log("back" + Physics.Raycast(cubeTransform.position, Vector3.back, scale));
        Debug.DrawRay(cubeTransform.position, Vector3.left, Color.black, scale);
        Debug.Log("left" + Physics.Raycast(cubeTransform.position, Vector3.left, scale));
        Debug.DrawRay(cubeTransform.position, Vector3.right, Color.black, scale);
        Debug.Log("right" + Physics.Raycast(cubeTransform.position, Vector3.right, scale));*/
        #endregion

        //forward
        RaycastHit hitForward;
        if (Physics.Raycast(cubeTransform.position, Vector3.forward, out hitForward, scale))
        {
            if (hitForward.collider.gameObject.tag == tag_MagenticEnvironment)
            {
                onMagneticCube = true;
            }
        }

        //back
        RaycastHit hitBackward;
        if (Physics.Raycast(cubeTransform.position, Vector3.back, out hitBackward, scale))
        {
            if (hitBackward.collider.gameObject.tag == tag_MagenticEnvironment)
            {
                onMagneticCube = true;
            }
        }

        //left
        RaycastHit hitLeft;
        if (Physics.Raycast(cubeTransform.position, Vector3.left, out hitLeft, scale))
        {
            if (hitLeft.collider.gameObject.tag == tag_MagenticEnvironment)
            {
                onMagneticCube = true;
            }
        }

        //right
        RaycastHit hitRight;
        if (Physics.Raycast(cubeTransform.position, Vector3.right, out hitRight, scale))
        {
            if (hitRight.collider.gameObject.tag == tag_MagenticEnvironment)
            {
                onMagneticCube = true;
            }
        }

        //down
        RaycastHit hitDown;
        if (Physics.Raycast(cubeTransform.position, Vector3.down, out hitDown, scale))
        {
            if (hitDown.collider.gameObject.tag == tag_MagenticEnvironment)
            {
                onMagneticCube = true;
            }
        }

        //up
        RaycastHit hitUp;
        if (Physics.Raycast(cubeTransform.position, Vector3.up, out hitUp, scale))
        {
            if (hitUp.collider.gameObject.tag == tag_MagenticEnvironment)
            {
                onMagneticCube = true;
            }
        }
    }

    #endregion

    //**********************************************************************************************************//
    //pressure plate methods
    #region Pressure Plates
    public void CheckAllPressurePlates()
    {
        foreach (PressurePlate pressurePlate in pressurePlates)
        {
            pressurePlate.CheckIfTriggered(cubeTransform.position, scale);
        }
    }


    #endregion

    //**********************************************************************************************************//
    //enemy
    #region Enemy Handlers

    public void CheckEnemyMovement()
    {
        foreach (EnemyController enemy in enemies)
        {
            enemy.EnemyBehaviour();
        }
    }

    #endregion

    //**********************************************************************************************************//
    //methods that control what happens at the start and end of the fall tween
    #region Fall Tween Handlers
    public void HandleStartFallTween()
    {

    }


    public void HandleEndFallTween()
    {
        isFalling = false;
        isMoving = false;
    }
    #endregion

    //**********************************************************************************************************//
    //fix floating point errors made by the computer
    #region Precision
    private void FixFloatingPointErrors()
    {
        //rotation ~ ROUND TO NEAREST INTEGER
        cubeTransform.rotation = Quaternion.Euler(Mathf.Round(cubeTransform.rotation.eulerAngles.x), Mathf.Round(cubeTransform.rotation.eulerAngles.y), Mathf.Round(cubeTransform.rotation.eulerAngles.z));

        //position ~ ROUND TO NEAREST TENTH
        Vector3 positionVector = cubeTransform.position;
        positionVector = new Vector3(positionVector.x * 10, positionVector.y * 10, positionVector.z * 10);
        positionVector = new Vector3(Mathf.Round(positionVector.x), Mathf.Round(positionVector.y), Mathf.Round(positionVector.z));
        positionVector = new Vector3(positionVector.x / 10, positionVector.y / 10, positionVector.z / 10);
        cubeTransform.position = positionVector;
    }
    #endregion

    //**********************************************************************************************************//
    //get the corrent roll type
    #region Roll Type Calculation
    //calculate roll type
    private RollType CalculateRollType(Vector3 direction, Vector3 position, float scale)
    {
        #region Debug

        #endregion

        ////////////////////////////////////////////////
        RaycastHit hit_leftForward1;
        RaycastHit hit_rightForward1;

        //////////////// IS BLOCK ///////////
        //direction 1
        bool isBlock_direction1 = Physics.Raycast(position, direction, scale);
        bool isBlock_direction1up1 = Physics.Raycast(position + Vector3.up * scale, direction, scale);
        bool isBlock_direction1up2 = Physics.Raycast(position + Vector3.up * scale * 2, direction, scale);
        bool isBlock_direction1down1 = Physics.Raycast(position + direction * scale, Vector3.down, scale);

        //direction 2
        bool isBlock_direction2 = Physics.Raycast(position + direction * scale, direction, scale);
        bool isBlock_direction2down1 = Physics.Raycast(position + direction * scale * 2, Vector3.down, scale);

        //block 1
        bool isBlock_up1 = Physics.Raycast(position, Vector3.up, scale);

        //block 2
        bool isBlock_up2 = Physics.Raycast(position + Vector3.up * scale, Vector3.up, scale);

        //direction minus 1
        bool isBlock_directionMinus1 = Physics.Raycast(position, -direction, scale);
        bool isBlock_directionMinus1up1 = Physics.Raycast(position + Vector3.up * scale, -direction, scale);

        //down 1
        bool isBlock_down1 = Physics.Raycast(position, Vector3.down, scale);

        //left forward 1
        bool isBlock_leftForward1 = Physics.Raycast(position, -Vector3.Cross(direction, Vector3.up), out hit_leftForward1, scale);
        bool isBlock_leftForward1direction1 = Physics.Raycast(position + direction * scale, -Vector3.Cross(direction, Vector3.up), scale);

        //left forward 2
        bool isBlock_leftForward2direction1 = Physics.Raycast(position + -Vector3.Cross(direction, Vector3.up) * scale * 2, direction, scale);

        //left back 1
        bool isBlock_leftBack1 = Physics.Raycast(position, Vector3.Cross(direction, Vector3.up), scale);
        bool isBlock_leftBack1direction1 = Physics.Raycast(position + Vector3.Cross(direction, Vector3.up) * scale, direction, scale);

        //right forward 1
        bool isBlock_rightForward1 = Physics.Raycast(position, Vector3.Cross(direction, Vector3.up), out hit_rightForward1, scale);
        bool isBlock_rightForward1direction1 = Physics.Raycast(position + direction * scale, Vector3.Cross(direction, Vector3.up), scale);

        //right back 1
        bool isBlock_rightBack1 = Physics.Raycast(position, -Vector3.Cross(direction, Vector3.up), scale);
        bool isBlock_rightBack1direction1 = Physics.Raycast(position + -Vector3.Cross(direction, Vector3.up) * scale, direction, scale);

        //right forward 2
        bool isBlock_rightForward2direction1 = Physics.Raycast(position + Vector3.Cross(direction, Vector3.up) * scale * 2, direction, scale);
        ////////////////////// IS MAGNETIC ///////////////////
        //left forward 1
        bool isMagnetic_leftForward1 = hit_leftForward1.collider.gameObject.tag == tag_MagenticEnvironment;

        //right forward 1
        bool isMagnetic_rightForward1 = hit_rightForward1.collider.gameObject.tag == tag_MagenticEnvironment;


        //**********************************************************************************************************//
        // STEP
        #region STEP

        // STEP UP
        #region STEP UP
        ////////////// STEP UP //////////////
        // IS cube direction 1
        // &&
        // IS NOT cube direction 1 + up 1 
        // &&
        // IS NOT cube up 1
        // &&
        // IS NOT cube direction -1
        // &&
        // IS NOT cube direction -1 + up 1
        // &&
        // IS NOT cube up + 2
        // &&
        // IS NOT cube direction 1 + up + 2
        if (isBlock_direction1 &&
            !isBlock_direction1up1 &&
            !isBlock_up1 &&
            !isBlock_directionMinus1 &&
            !isBlock_directionMinus1up1 &&
            !isBlock_up2 &&
            !isBlock_direction1up2)
        {

            return RollType.step_Up;
        }

        ////////////// STEP UP 1 BONK //////////////
        // IS cube direction 1
        // &&
        // IS NOT cube direction 1 + up 1 
        // &&
        // IS NOT cube up 1
        // &&
        // IS NOT cube direction -1
        // &&
        // IS cube direction -1 + up 1
        if (isBlock_direction1 &&
            !isBlock_direction1up1 &&
            !isBlock_up1 &&
            !isBlock_directionMinus1 &&
            isBlock_directionMinus1up1)
        {

            return RollType.bonk_stepUp1;
        }

        ////////////// STEP UP 2 BONK //////////////
        // IS cube direction 1
        // &&
        // IS NOT cube direction 1 + up 1 
        // &&
        // IS NOT cube up 1
        // &&
        // IS NOT cube direction -1
        // &&
        // IS NOT cube direction -1 + up 1
        // &&
        // IS cube up + 2

        if (isBlock_direction1 &&
            !isBlock_direction1up1 &&
            !isBlock_up1 &&
            !isBlock_directionMinus1 &&
            !isBlock_directionMinus1up1 &&
            isBlock_up2)
        {

            return RollType.bonk_stepUp2;
        }

        ////////////// STEP UP 3 BONK //////////////
        // IS cube direction 1
        // &&
        // IS NOT cube direction 1 + up 1 
        // &&
        // IS NOT cube up 1
        // &&
        // IS NOT cube direction -1
        // &&
        // IS NOT cube direction -1 + up 1
        // &&
        // IS NOT cube up + 2
        // &&
        // IS cube direction 1 + up + 2
        if (isBlock_direction1 &&
            !isBlock_direction1up1 &&
            !isBlock_up1 &&
            !isBlock_directionMinus1 &&
            !isBlock_directionMinus1up1 &&
            !isBlock_up2 &&
            isBlock_direction1up2)
        {

            return RollType.bonk_stepUp3;
        }

        #endregion

        // STEP DOWN
        #region STEP DOWN
        ////////////// STEP DOWN //////////////
        // IS cube down 1
        // &&
        // IS NOT cube direction 1
        // &&
        // IS NOT cube direction 1 + down 1
        // &&
        // IS NOT cube up 1
        // &&
        // IS NOT cube direction 1 + up 1
        // &&
        // IS NOT cube in direction 2
        // &&
        // IS NOT cube in direction 2 + down 1
        else if (isBlock_down1 &&
                 !isBlock_direction1 &&
                 !isBlock_direction1down1 &&
                 !isBlock_up1 &&
                 !isBlock_direction1up1 &&
                 !isBlock_direction2 &&
                 !isBlock_direction2down1)
        {
            return RollType.step_Down;
        }

        ////////////// STEP DOWN BONK 1 //////////////
        // IS cube down 1
        // &&
        // IS NOT cube direction 1
        // &&
        // IS NOT cube direction 1 + down 1
        // &&
        // IS NOT cube up 1
        // &&
        // IS cube direction 1 + up 1
        else if (isBlock_down1 &&
                 !isBlock_direction1 &&
                 !isBlock_direction1down1 &&
                 !isBlock_up1 &&
                 isBlock_direction1up1)
        {
            return RollType.bonk_stepDown1;
        }

        ////////////// STEP DOWN BONK 2 //////////////
        // IS cube down 1
        // &&
        // IS NOT cube direction 1
        // &&
        // IS NOT cube direction 1 + down 1
        // &&
        // IS NOT cube up 1
        // &&
        // IS NOT cube direction 1 + up 1
        // &&
        // IS cube in direction 2
        else if (isBlock_down1 &&
                 !isBlock_direction1 &&
                 !isBlock_direction1down1 &&
                 !isBlock_up1 &&
                 !isBlock_direction1up1 &&
                 isBlock_direction2)
        {
            return RollType.flat;
        }

        ////////////// STEP DOWN BONK 3 //////////////
        // IS cube down 1
        // &&
        // IS NOT cube direction 1
        // &&
        // IS NOT cube direction 1 + down 1
        // &&
        // IS NOT cube up 1
        // &&
        // IS NOT cube direction 1 + up 1
        // &&
        // IS NOT cube in direction 2
        // &&
        // IS cube in direction 2 + down 1
        else if (isBlock_down1 &&
                 !isBlock_direction1 &&
                 !isBlock_direction1down1 &&
                 !isBlock_up1 &&
                 !isBlock_direction1up1 &&
                 !isBlock_direction2 &&
                 isBlock_direction2down1)
        {
            return RollType.flat;
        }

        #endregion

        //STEP LEFT
        #region Step Left
        ////////////// STEP LEFT //////////////
        // IS cube 'forward' 1 && IS magnetic
        // &&
        // IS NOT cube direction 1
        // &&
        // IS NOT cube direction 1 && 'forward' 1
        // &&
        // IS NOT cube down 1
        // &&
        // IS NOT cube 'forward' -1 
        // && 
        // IS NOT cube 'forward' -1 + direction 1
        // &&
        // IS NOT cube direction 2
        // &&
        // IS NOT cube 'forward' 2 + direction 1
        else if (isBlock_leftForward1 &&
                 isMagnetic_leftForward1 &&
                !isBlock_direction1 &&
                !isBlock_leftForward1direction1 &&
                !isBlock_down1 &&
                !isBlock_leftBack1 &&
                !isBlock_leftBack1direction1 &&
                !isBlock_direction2 &&
                !isBlock_leftForward2direction1)

        {
            return RollType.step_Left;
        }

        ////////////// STEP LEFT BONK 1 //////////////
        // IS cube 'forward' 1 && IS magnetic
        // &&
        // IS NOT cube direction 1
        // &&
        // IS NOT cube direction 1 && 'forward' 1
        // &&
        // IS NOT cube down 1
        // &&
        // IS NOT cube 'forward' -1 
        // && 
        // IS cube 'forward' -1 + direction 1
        else if (isBlock_leftForward1 &&
                 isMagnetic_leftForward1 &&
                !isBlock_direction1 &&
                !isBlock_leftForward1direction1 &&
                !isBlock_down1 &&
                !isBlock_leftBack1 &&
                 isBlock_leftBack1direction1)

        {
            return RollType.bonk_stepLeft1;
        }

        ////////////// STEP LEFT BONK 2//////////////
        // IS cube 'forward' 1 && IS magnetic
        // &&
        // IS NOT cube direction 1
        // &&
        // IS NOT cube direction 1 && 'forward' 1
        // &&
        // IS NOT cube down 1
        // &&
        // IS NOT cube 'forward' -1 
        // && 
        // IS NOT cube 'forward' -1 + direction 1
        // &&
        // IS cube direction 2
        else if (isBlock_leftForward1 &&
                 isMagnetic_leftForward1 &&
                !isBlock_direction1 &&
                !isBlock_leftForward1direction1 &&
                !isBlock_down1 &&
                !isBlock_leftBack1 &&
                !isBlock_leftBack1direction1 &&
                 isBlock_direction2)

        {
            return RollType.bonk_stepLeft2;
        }

        ////////////// STEP LEFT BONK 3 //////////////
        // IS cube 'forward' 1 && IS magnetic
        // &&
        // IS NOT cube direction 1
        // &&
        // IS NOT cube direction 1 && 'forward' 1
        // &&
        // IS NOT cube down 1
        // &&
        // IS NOT cube 'forward' -1 
        // && 
        // IS NOT cube 'forward' -1 + direction 1
        // &&
        // IS NOT cube direction 2
        // &&
        // IS cube 'forward' 1 + direction 2
        else if (isBlock_leftForward1 &&
                 isMagnetic_leftForward1 &&
                !isBlock_direction1 &&
                !isBlock_leftForward1direction1 &&
                !isBlock_down1 &&
                !isBlock_leftBack1 &&
                !isBlock_leftBack1direction1 &&
                !isBlock_direction2 &&
                 isBlock_leftForward2direction1)

        {
            return RollType.bonk_stepLeft3;
        }



        #endregion

        //step right
        #region Step Right
        ////////////// STEP RIGHT //////////////
        //if IS cube 'forward' 1 && IS magnetic
        // &&
        //if IS NOT cube direction 1
        // &&
        //if IS NOT cube direction 1 && forward 1
        // &&
        //if IS NOT cube down 1
        // &&
        // IS NOT cube 'forward' -1 
        // && 
        // IS NOT cube 'forward' -1 + direction 1
        // &&
        // IS NOT cube direction 2
        // &&
        // IS NOT cube 'forward' 1 + direction 2
        else if (isBlock_rightForward1 &&
                isMagnetic_rightForward1 &&
                !isBlock_direction1 &&
                !isBlock_rightForward1direction1 &&
                !isBlock_down1 &&
                !isBlock_rightBack1 &&
                !isBlock_rightBack1direction1 &&
                !isBlock_direction2 &&
                !isBlock_rightForward2direction1)
        {
            return RollType.step_Right;
        }

        ////////////// STEP RIGHT BONK 0 //////////////
        //if IS cube 'forward' 1 && IS magnetic
        // &&
        //if IS NOT cube direction 1
        // &&
        //if IS NOT cube direction 1 && forward 1
        // &&
        //if IS NOT cube down 1
        // &&
        // IS cube 'forward' -1 
        else if (isBlock_rightForward1 &&
                isMagnetic_rightForward1 &&
                !isBlock_direction1 &&
                !isBlock_rightForward1direction1 &&
                !isBlock_down1 &&
                isBlock_rightBack1)
        {
            return RollType.stuck;
        }

        ////////////// STEP RIGHT BONK 1 //////////////
        //if IS cube 'forward' 1 && IS magnetic
        // &&
        //if IS NOT cube direction 1
        // &&
        //if IS NOT cube direction 1 && forward 1
        // &&
        //if IS NOT cube down 1
        // &&
        // IS NOT cube 'forward' -1 
        // && 
        // IS cube 'forward' -1 + direction 1
        else if (isBlock_rightForward1 &&
                isMagnetic_rightForward1 &&
                !isBlock_direction1 &&
                !isBlock_rightForward1direction1 &&
                !isBlock_down1 &&
                !isBlock_rightBack1 &&
                isBlock_rightBack1direction1)
        {
            return RollType.bonk_stepRight1;
        }

        ////////////// STEP RIGHT BONK 2 //////////////
        //if IS cube 'forward' 1 && IS magnetic
        // &&
        //if IS NOT cube direction 1
        // &&
        //if IS NOT cube direction 1 && forward 1
        // &&
        //if IS NOT cube down 1
        // &&
        // IS NOT cube 'forward' -1 
        // && 
        // IS NOT cube 'forward' -1 + direction 1
        // &&
        // IS cube direction 2
        else if (isBlock_rightForward1 &&
                isMagnetic_rightForward1 &&
                !isBlock_direction1 &&
                !isBlock_rightForward1direction1 &&
                !isBlock_down1 &&
                !isBlock_rightBack1 &&
                !isBlock_rightBack1direction1 &&
                isBlock_direction2)
        {
            return RollType.bonk_stepRight2;
        }

        ////////////// STEP RIGHT BONK 3 //////////////
        //if IS cube 'forward' 1 && IS magnetic
        // &&
        //if IS NOT cube direction 1
        // &&
        //if IS NOT cube direction 1 && forward 1
        // &&
        //if IS NOT cube down 1
        // &&
        // IS NOT cube 'forward' -1 
        // && 
        // IS NOT cube 'forward' -1 + direction 1
        // &&
        // IS NOT cube direction 2
        // &&
        // IS cube 'forward' 1 + direction 2
        else if (isBlock_rightForward1 &&
                isMagnetic_rightForward1 &&
                !isBlock_direction1 &&
                !isBlock_rightForward1direction1 &&
                !isBlock_down1 &&
                !isBlock_rightBack1 &&
                !isBlock_rightBack1direction1 &&
                !isBlock_direction2 &&
                isBlock_rightForward2direction1)
        {
            return RollType.bonk_stepRight3;
        }

        #endregion

        #endregion

        //**********************************************************************************************************//
        //CLIMB
        #region CLIMB

        #region Climb Up
        ////////////// CLIMB UP //////////////
        // IS cube direction 1 && IS magnetic
        // &&
        // IS cube direction 1 + up 1
        // &&
        // IS NOT cube up 1
        // &&
        // IS NOT cube direction -1
        // &&
        // IS NOT cube direction -1 + up 1
        else if (Physics.Raycast(position, direction, out hit_climbUp, scale) &&
                 hit_climbUp.collider.gameObject.tag == tag_MagenticEnvironment &&
                 isBlock_direction1up1 &&
                 !isBlock_up1 &&
                 !isBlock_directionMinus1 &&
                 !isBlock_directionMinus1up1)
        {
            return RollType.climb_Up;
        }

        ////////////// CLIMB UP FLAT BONK //////////////
        // IS cube direction 1 && IS magnetic
        // &&
        // IS cube direction 1 + up 1
        // &&
        // IS cube up 1
        // &&
        // IS NOT cube direction -1
        // &&
        // IS NOT cube direction -1 + up 1
        else if (Physics.Raycast(position, direction, out hit_climbUp, scale) &&
                 hit_climbUp.collider.gameObject.tag == tag_MagenticEnvironment &&
                 isBlock_direction1up1 &&
                 Physics.Raycast(position, Vector3.up, out hit_climbUp_flat, scale) &&
                 hit_climbUp_flat.collider.gameObject.tag != tag_MagenticEnvironment &&
                 !isBlock_directionMinus1)
        {
            return RollType.bonk_climbUp_flat;
        }

        ////////////// CLIMB UP HEAD BONK //////////////
        // IS cube direction 1 && IS magnetic
        // &&
        // IS cube direction 1 + up 1
        // &&
        // IS NOT cube up 1
        // &&
        // IS NOT cube direction -1
        // &&
        // IS cube direction -1 + up 1 && IS NOT magnetic
        else if (Physics.Raycast(position, direction, out hit_climbUp, scale) &&
                 hit_climbUp.collider.gameObject.tag == tag_MagenticEnvironment &&
                 isBlock_direction1up1 &&
                 !isBlock_up1 &&
                 !isBlock_directionMinus1 &&
                 isBlock_directionMinus1up1)
        {
            return RollType.bonk_climbUp_head;
        }

        #endregion

        #region climb down
        ////////////// CLIMB DOWN //////////////
        // IS cube back 1 && IS magnetic
        // &&
        // IS cube direction 1 + down 1
        // &&
        // IS NOT cube down 1
        // &&
        // IS NOT cube direction -1
        // &&
        // IS NOT cube direction -1 + down 1
        else if (Physics.Raycast(position, -direction, out hit_climbDown, scale) &&
                 hit_climbDown.collider.gameObject.tag == tag_MagenticEnvironment &&
                 Physics.Raycast(position + Vector3.down * scale, -direction, scale) &&
                 !isBlock_down1 &&
                 !isBlock_direction1 &&
                 !Physics.Raycast(position + Vector3.down * scale, direction, scale))
        {
            return RollType.climb_Down;
        }

        ////////////// CLIMB DOWN BONK //////////////
        // IS cube back 1 && IS magnetic
        // &&
        // IS cube direction 1 + down 1
        // &&
        // IS NOT cube down 1
        // &&
        // IS NOT cube direction -1
        // &&
        // IS cube direction -1 + down 1
        else if (Physics.Raycast(position, -direction, out hit_climbDown, scale) &&
                 hit_climbDown.collider.gameObject.tag == tag_MagenticEnvironment &&
                 Physics.Raycast(position + Vector3.down * scale, -direction, scale) &&
                 !isBlock_down1 &&
                 !isBlock_direction1 &&
                 Physics.Raycast(position + Vector3.down * scale, direction, scale))
        {
            return RollType.flat;
        }
        #endregion

        #region Climb left
        ////////////// CLIMB LEFT //////////////
        // IS cube 'forward' 1 && is magnetic
        // &&
        // IS cube direction 1 && 'forward' 1
        // &&
        // IS NOT cube down 1
        // &&
        // IS NOT cube direction 1
        // &&
        // IS NOT cube 'forward' -1
        // &&
        // IS NOT cube 'forward' -1 + direction 1

        else if (Physics.Raycast(position, -Vector3.Cross(direction, Vector3.up), out hit_climbLeft, scale) &&
                 hit_climbLeft.collider.gameObject.tag == tag_MagenticEnvironment &&
                 Physics.Raycast(position + direction * scale, -Vector3.Cross(direction, Vector3.up), scale) &&
                 !isBlock_down1 &&
                 !isBlock_direction1 &&
                 !Physics.Raycast(position, Vector3.Cross(direction, Vector3.up), scale) &&
                 !isBlock_rightForward1direction1)
        {
            return RollType.climb_Left;
        }

        ////////////// CLIMB LEFT BONK FLAT //////////////
        // IS cube 'forward' 1 && is magnetic
        // &&
        // IS cube direction 1 && 'forward' 1
        // &&
        // IS NOT cube down 1
        // &&
        // IS cube direction 1
        // &&
        // IS cube direction 1 + up 1
        // &&
        // IS NOT cube 'forward' -1
        else if (Physics.Raycast(position, -Vector3.Cross(direction, Vector3.up), out hit_climbLeft, scale) &&
                 hit_climbLeft.collider.gameObject.tag == tag_MagenticEnvironment &&
                 Physics.Raycast(position + direction * scale, -Vector3.Cross(direction, Vector3.up), scale) &&
                 !isBlock_down1 &&
                 isBlock_direction1 &&
                 isBlock_direction1up1 &&
                 !Physics.Raycast(position, Vector3.Cross(direction, Vector3.up), scale))
        {
            return RollType.bonk_climbLeft_flat;
        }

        ////////////// CLIMB LEFT BONK HEAD //////////////
        // IS cube 'forward' 1 && is magnetic
        // &&
        // IS cube direction 1 && 'forward' 1
        // &&
        // IS NOT cube down 1
        // &&
        // IS NOT cube direction 1
        // &&
        // IS NOT cube 'forward' -1
        // &&
        // IS cube 'forward' -1 + direction 1
        else if (Physics.Raycast(position, -Vector3.Cross(direction, Vector3.up), out hit_climbLeft, scale) &&
                 hit_climbLeft.collider.gameObject.tag == tag_MagenticEnvironment &&
                 Physics.Raycast(position + direction * scale, -Vector3.Cross(direction, Vector3.up), scale) &&
                 !isBlock_down1 &&
                 !isBlock_direction1 &&
                 !Physics.Raycast(position, Vector3.Cross(direction, Vector3.up), scale) &&
                 isBlock_rightForward1direction1)
        {
            return RollType.bonk_climbLeft_head;
        }

        #endregion



        ////////////// CLIMB RIGHT //////////////
        // IS cube 'forward' 1 && is magnetic
        // &&
        // IS cube direction 1 && 'forward' 1
        // &&
        // IS NOT cube down 1
        // &&
        // IS NOT cube direction 1
        // &&
        // IS NOT cube 'forward' -1
        // &&
        // IS NOT cube 'forward' -1 + direction 1
        else if (Physics.Raycast(position, Vector3.Cross(direction, Vector3.up), out hit_climbRight, scale) &&
                 hit_climbRight.collider.gameObject.tag == tag_MagenticEnvironment &&
                 isBlock_rightForward1direction1 &&
                 !isBlock_down1 &&
                 !isBlock_direction1 &&
                 !isBlock_rightBack1 &&
                 !Physics.Raycast(position + direction * scale, -Vector3.Cross(direction, Vector3.up), scale))
        {
            return RollType.climb_Right;
        }

        ////////////// CLIMB RIGHT BONK FLAT //////////////
        // IS cube 'forward' 1 && is magnetic
        // &&
        // IS cube direction 1 && 'forward' 1
        // &&
        // IS NOT cube down 1
        // &&
        // IS cube direction 1
        // &&  
        // IS cube direction 1 + up 1
        // &&
        // IS NOT cube 'forward' -1
        else if (Physics.Raycast(position, Vector3.Cross(direction, Vector3.up), out hit_climbRight, scale) &&
                 hit_climbRight.collider.gameObject.tag == tag_MagenticEnvironment &&
                 isBlock_rightForward1direction1 &&
                 !Physics.Raycast(position, -Vector3.down, scale) &&
                 isBlock_direction1 &&
                 Physics.Raycast(position + -Vector3.up * scale, direction, scale) &&
                 !isBlock_rightBack1)
        {
            return RollType.bonk_climbRight_flat;
        }

        ////////////// CLIMB RIGHT BONK HEAD //////////////
        // IS cube 'forward' 1 && is magnetic
        // &&
        // IS cube direction 1 && 'forward' 1
        // &&
        // IS NOT cube down 1
        // &&
        // IS NOT cube direction 1
        // &&
        // IS NOT cube 'forward' -1
        // &&
        // IS cube 'forward' -1 + direction 1
        else if (Physics.Raycast(position, Vector3.Cross(direction, Vector3.up), out hit_climbRight, scale) &&
                 hit_climbRight.collider.gameObject.tag == tag_MagenticEnvironment &&
                 isBlock_rightForward1direction1 &&
                 !isBlock_down1 &&
                 !isBlock_direction1 &&
                 !isBlock_rightBack1 &&
                 Physics.Raycast(position + direction * scale, -Vector3.Cross(direction, Vector3.up), scale))
        {
            return RollType.bonk_climbRight_head;
        }

        #endregion

        //**********************************************************************************************************//
        //FLAT
        #region
        ////////////// BONK //////////////
        // IS cube direction 1 && IS NOT magnetic
        // && 
        // IS cube direction 1 + up 1
        // &&
        // IS NOT cube direction -1
        //
        // OR
        //
        // IS cube direction 1
        // &&
        // IS cube up 1
        // &&
        // IS NOT cube direction -1
        else if (Physics.Raycast(position, direction, out hit_bonk, scale) &&
                 isBlock_direction1up1 &&
                 hit_bonk.collider.gameObject.tag != tag_MagenticEnvironment &&
                 !isBlock_directionMinus1)
        {
            return RollType.bonk_flat;
        }
        else if (isBlock_direction1 &&
                isBlock_up1 &&
                !isBlock_directionMinus1)
        {
            return RollType.bonk_flat;
        }

        ////////////// HEAD BONK //////////////
        // IS NOT cube direction 1
        // &&
        // IS cube direction 1 + up 1
        // &&
        // IS NOT cube dirction -1
        // &&
        // IS NOT cube up 1
        else if (!isBlock_direction1 &&
                 isBlock_direction1up1 &&
                 !isBlock_directionMinus1 &&
                 !isBlock_up1)
        {
            return RollType.bonk_head;
        }

        ////////////// STUCK //////////////
        //if cant bonk or head bonk because of cubes around it
        //bonk 1
        //bonk 2
        //headbonk 1
        else if (Physics.Raycast(position, direction, out hit_bonk, scale) &&
                 isBlock_direction1up1 &&
                 hit_bonk.collider.gameObject.tag != tag_MagenticEnvironment &&
                 isBlock_directionMinus1
                 ||
                 isBlock_direction1 &&
                 isBlock_up1 &&
                 isBlock_directionMinus1
                 ||
                 !isBlock_direction1 &&
                 isBlock_direction1up1 &&
                 isBlock_directionMinus1 &&
                 isBlock_up1)
        {
            return RollType.stuck;
        }

        ////////////// FLAT //////////////
        //IS NOT cube direction 1
        // &&
        //IS NOT cube up 1
        // &&
        //IS NOT cube direction 1 + up 1
        // &&
        //IS cube direction 1 + down 1
        else if (!isBlock_direction1 &&
                !isBlock_up1 &&
                !Physics.Raycast(position + direction * scale, Vector3.up, scale) &&
                isBlock_direction1down1)
        {
            return RollType.flat;
        }
        else
        {
            return RollType.stuck;
        }

        #endregion

    }

    //calcualate the direction vector based on the camera
    #region Camera Relative Direction
    public Vector3 TranslateRelativeDirection(Vector3 direction)
    {
        //forward
        if (direction == Vector3.forward)
        {
            return RoundToClosestCardinalDirection(vc_transform.forward);
        }
        //back
        else if (direction == Vector3.back)
        {
            return RoundToClosestCardinalDirection(-vc_transform.forward);
        }
        //right
        else if (direction == Vector3.right)
        {
            return RoundToClosestCardinalDirection(vc_transform.right);
        }
        //left
        else if (direction == Vector3.left)
        {
            return RoundToClosestCardinalDirection(-vc_transform.right);
        }
        //oopsie
        else
        {
            return Vector3.zero;
        }
    }

    public Vector3 RoundToClosestCardinalDirection(Vector3 inputDirection)
    {
        //Debug.Log(inputDirection);
        Vector3 output = inputDirection;
        output.y = 0;
        output.x = Mathf.Round(output.x);
        output.z = Mathf.Round(output.z);

        if (Mathf.Abs(inputDirection.x) > Mathf.Abs(inputDirection.z))
        {
            output.z = 0;
        }
        else
        {
            output.x = 0;
        }

        //Debug.Log(output);
        return output;
    }
    #endregion

    #endregion

    #endregion

    #endregion
}