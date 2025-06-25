using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NIOSH_MineCreation;
using UnityEditor;
using UnityEngine.UI;

namespace NIOSH_EditorLayers
{
    public class LayerManager : MonoBehaviour
    {

        [SerializeField] Button _tilesButton;
        [SerializeField] Button _ventilationButton;
        [SerializeField] Button _cablesButton;
        [SerializeField] Button _objectButton;
        [SerializeField] Button _sceneControlsButton;


        public static LayerManager Instance { get; private set; }


        public enum EditorLayer { Mine = 0, Object = 1, LoadOnly = 2, SceneControls = 3, Ventilation = 4, VentilationBlockers = 5, Cables = 6, Curtains = 7};

        public UnityAction<EditorLayer> layerChanged;

        private static EditorLayer currentLayer;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
        }

        public void ChangeLayerFromInt(int newLayer)
        {
            ChangeLayer((EditorLayer)newLayer);
        }

        public void ChangeLayer(EditorLayer newLayer)
        {
            layerChanged?.Invoke(newLayer);

            currentLayer = newLayer;

            switch (currentLayer)
            {
                case EditorLayer.Mine:
                    ControlLayerButtons(_tilesButton);
                    break;
                case EditorLayer.Ventilation:
                    ControlLayerButtons(_ventilationButton);
                    break;
                case EditorLayer.Cables:
                    ControlLayerButtons(_cablesButton);
                    break;
                case EditorLayer.Object:
                    ControlLayerButtons(_objectButton);
                    break;
                case EditorLayer.SceneControls:
                    ControlLayerButtons(_sceneControlsButton);
                    break;
            }
        }
        public void ControlLayerButtons(Button currentButton)
        {
            _tilesButton.interactable = true;
            _ventilationButton.interactable = true;
            _cablesButton.interactable = true;
            _objectButton.interactable = true;
            _sceneControlsButton.interactable = true;
            currentButton.interactable = false;
        }
        public static EditorLayer GetCurrentLayer()
        {
            return currentLayer;
        }
    }
}