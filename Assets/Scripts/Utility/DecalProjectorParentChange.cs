using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Experimental.Rendering;

public enum IKProjectorType
{
    RightArm,
    LeftArm,
    Head,
    Back
}
public class DecalProjectorParentChange : MonoBehaviour
{
    public Transform TargetParent;
    public RootMotion.FinalIK.VRIK VRIK;
    public IKProjectorType IKProjectorType;
    public DecalProjector DecalProjector;

    
    private Transform _originalParent;
    private Vector3 _originalLocalPosition;
    private Quaternion _originalLocalRotation;
    private Vector3 _startingIKBonePositionOffset;
    private Quaternion _startingIKBoneRotationOffset;
    private Vector3 _startSize;
    private bool _initialized = false;

    private Transform _decalProjTargetPos;

    private System.Reflection.MethodInfo _decalLateUpdate;

    void Start()
    {
        if(DecalProjector == null)
        {
            DecalProjector = GetComponent<DecalProjector>();
        }



        _originalParent = transform.parent;
        if (_originalParent == null)
        {
            Debug.LogError($"DecalProjectorParentChange on {gameObject.name} requires parent transform");
            this.enabled = false;
            return;
        }
        

        _originalLocalPosition = transform.localPosition;
        _originalLocalRotation = transform.localRotation;
        _startSize = DecalProjector.size;

        //GameObject targetObj = new GameObject($"Decal {gameObject.name} Target");
        //_decalProjTargetPos = targetObj.transform;
        //_decalProjTargetPos.SetParent(transform.parent, false);
        //_decalProjTargetPos.localPosition = transform.localPosition;
        //_decalProjTargetPos.localRotation = transform.localRotation;

        //transform.SetParent(TargetParent, true);


        _decalLateUpdate = typeof(DecalProjector).GetMethod("LateUpdate", 
            System.Reflection.BindingFlags.Instance | 
            System.Reflection.BindingFlags.NonPublic );
        if (_decalLateUpdate == null)
        {
            Debug.LogWarning($"Couldn't find LateUpdate method on DecalProjector");
            
        }
        


        //switch (IKProjectorType)
        //{
        //    case IKProjectorType.RightArm:
        //        _startingIKBonePositionOffset = transform.position - VRIK.solver.rightArm.position;
        //        _startingIKBoneRotationOffset = VRIK.solver.rightArm.rotation * Quaternion.Inverse(transform.rotation);
        //        break;
        //    case IKProjectorType.LeftArm:
        //        _startingIKBonePositionOffset = transform.position - VRIK.solver.leftArm.position;
        //        _startingIKBoneRotationOffset = VRIK.solver.leftArm.rotation * Quaternion.Inverse(transform.rotation);
        //        break;
        //    case IKProjectorType.Head:
        //        _startingIKBonePositionOffset = transform.position - VRIK.solver.spine.head.solverPosition;
        //        _startingIKBoneRotationOffset = VRIK.solver.spine.head.solverRotation * Quaternion.Inverse(transform.rotation);
        //        break;
        //    case IKProjectorType.Back:
        //        _startingIKBonePositionOffset = transform.position - VRIK.solver.spine.chest.solverPosition;
        //        _startingIKBoneRotationOffset = VRIK.solver.spine.chest.solverRotation * Quaternion.Inverse(transform.rotation);
        //        break;
        //    default:
        //        break;
        //}
        

        //transform.SetParent(TargetParent);
        if (VRIK != null && VRIK.solver != null)
            VRIK.solver.OnPostUpdate += AfterIK;
        //VRIK.solver.GetPoints();

        _initialized = true;
    }

    //// Update is called once per frame
    //void Update()
    //{
    //    //transform.position = _originalParent.position + _originalLocalPosition;
    //    //transform.rotation = _originalParent.rotation * _originalLocalRotation;


    //    //transform.SetParent(_originalParent);
    //    //transform.localPosition = _originalLocalPosition;
    //    //transform.localRotation = _originalLocalRotation;
    //    //transform.SetParent(TargetParent);
    //}

    void AfterIK()
    {
        /* This causes a significant performance hit & per-frame memory allocation but does fix the projector
         * 
        decalProjector.enabled = false;//This is done to force an update on the Decal Projector. I wish Unity would just give me an update function for it. It appears to be a cached process.
        decalProjector.enabled = true;
        */

        //if (_decalProjTargetPos == null)
        //    return;

        //transform.position = _decalProjTargetPos.position;
        //transform.rotation = _decalProjTargetPos.rotation;
        //transform.hasChanged = true;

        //force the decal projector to update it's cache
        if (_decalLateUpdate != null)
        {
            transform.hasChanged = true;
            _decalLateUpdate.Invoke(DecalProjector, null);
        }
        

        //transform.SetParent(_originalParent);

        //transform.localPosition = _originalLocalPosition;
        //transform.localRotation = _originalLocalRotation;


        //transform.SetParent(TargetParent);
        // VRIK.enabled = false;
        //switch (IKProjectorType)
        //{
        //    case IKProjectorType.RightArm:
        //        transform.position = VRIK.solver.rightArm.position + _startingIKBonePositionOffset;
        //        transform.rotation = VRIK.solver.rightArm.rotation * _startingIKBoneRotationOffset;
        //        break;
        //    case IKProjectorType.LeftArm:
        //        transform.position = VRIK.solver.leftArm.position + _startingIKBonePositionOffset;
        //        transform.rotation = VRIK.solver.leftArm.rotation * _startingIKBoneRotationOffset;
        //        break;
        //    case IKProjectorType.Head:
        //        transform.position = VRIK.solver.spine.head.solverPosition + _startingIKBonePositionOffset;
        //        transform.rotation = VRIK.solver.spine.head.solverRotation * _startingIKBoneRotationOffset;
        //        break;
        //    case IKProjectorType.Back:
        //        transform.position = VRIK.solver.spine.chest.solverPosition + _startingIKBonePositionOffset;
        //        transform.rotation = VRIK.solver.spine.chest.solverRotation * _startingIKBoneRotationOffset;
        //        break;
        //    default:
        //        break;
        //}


        //VRIK.enabled = true;
    }

    public void UpdateProjectorSize(float scaleValue)
    {
        if (!_initialized)
            return;

        Vector3 size = _startSize;
        size.x = scaleValue * _startSize.x;
        size.y = scaleValue * _startSize.y;
        DecalProjector.size = size;
    }
}
