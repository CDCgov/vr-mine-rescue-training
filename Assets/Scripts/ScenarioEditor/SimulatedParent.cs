using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Animations;
using NIOSH_EditorLayers;

public class SimulatedParent : MonoBehaviour
{
    public ObjectInfo objectInfo;
    [HideInInspector] public BoxCollider trigger;
    [HideInInspector] public Rigidbody rb;
    bool allowParenting = true;
    [SerializeField] Button continueButton;
    BoxCollider parentTrigger;
    Transform parentTransform;
    bool forceParent;


    // Start is called before the first frame update
    void Start()
    {
        var button = GameObject.Find("Continue_Button");
        if (button != null && button.TryGetComponent(out Button continueButton)) continueButton.onClick.AddListener(StartForceParent);
        gameObject.layer = 2;


        var constraint = gameObject.AddComponent(typeof(ParentConstraint)) as ParentConstraint;
        ConstraintSource source = new ConstraintSource();
        source.sourceTransform = objectInfo.transform;
        source.weight = 1;
        constraint.AddSource(source);
        constraint.constraintActive = true;

        parentTrigger = objectInfo.GetComponent<BoxCollider>();
        parentTransform = objectInfo.transform;

        trigger = GetComponent<BoxCollider>();
        if (trigger == null) { trigger = gameObject.AddComponent(typeof(BoxCollider)) as BoxCollider; }
        trigger.isTrigger = true;

        StartCoroutine(RescaleBounds());

        rb = gameObject.AddComponent(typeof(Rigidbody)) as Rigidbody;
        rb.useGravity = false;
        rb.isKinematic = true;
        LayerManager.Instance.layerChanged += OnLayerChanged;
    }


    public void OnDestroy()
    {
        if (continueButton != null) continueButton.onClick.RemoveListener(StartForceParent);
        if (LayerManager.Instance != null) LayerManager.Instance.layerChanged -= OnLayerChanged;
    }

    //public void OnTriggerEnter(Collider other)
    //{
    //    if (allowParenting)
    //    {
    //        TryParent(other);
    //    }
    //}

    //public void OnTriggerStay(Collider other)
    //{
    //    if (forceParent) TryParent(other);
    //}

    void OnLayerChanged(LayerManager.EditorLayer currentLayer)
    {
        allowParenting = currentLayer != LayerManager.EditorLayer.Mine;
    }

    void StartForceParent()
    {
        StartCoroutine(ForceParent());
    }
    IEnumerator RescaleBounds()
    {

        yield return new WaitForSeconds(1f);
        if (parentTrigger)
        {
            trigger.size = new Vector3(parentTrigger.size.x * parentTransform.localScale.x, parentTrigger.size.y * parentTransform.localScale.y, parentTrigger.size.z * parentTransform.localScale.z);
            trigger.center = parentTrigger.center;
        }
    }

    IEnumerator ForceParent()
    {
        forceParent = true;
        yield return new WaitForSeconds(1f);
        forceParent = false;
    }

    public void TryParent(GameObject other)
    {
        ObjectInfo itemInfo;
        //float distanceToMe = -1;
        //float distanceToOther = -1;

        itemInfo = other.GetComponentInParent<ObjectInfo>();
        if (itemInfo == null)
            return;

        //var otherObj = itemInfo.gameObject;

        //if (objectInfo.hierarchyContainer == null || itemInfo.HierarchyItem == null)
        //    return;

        if (itemInfo.editorLayer != LayerManager.EditorLayer.Object &&
            itemInfo.editorLayer != LayerManager.EditorLayer.SceneControls &&
            itemInfo.editorLayer != LayerManager.EditorLayer.Ventilation)
            return;

        //itemInfo.HierarchyItem.nextContainer = objectInfo.hierarchyContainer;
        //itemInfo.HierarchyItem.Placed(false);

        if (other.transform.parent == transform) //already parented
            return;

        //SimulatedParent otherParent = null;
        //if (otherObj.transform.parent != null && 
        //    otherObj.transform.parent.TryGetComponent(out otherParent))            
        //{
        //    distanceToMe = Vector3.Distance(otherObj.transform.position, transform.position);
        //    distanceToOther = Vector3.Distance(otherObj.transform.position, otherParent.transform.position);
        //}

        //if (otherParent == null || distanceToOther > distanceToMe)
        //{
        //    otherObj.transform.parent = transform;
        //}

        other.transform.parent = transform;        

    }
}


