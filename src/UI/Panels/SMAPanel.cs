using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityExplorer.Core.Config;
using UnityExplorer.ObjectExplorer;
using UnityExplorer.UI.Widgets;
using UnityExplorer.SMA;
using UnityEngine.SceneManagement;

namespace UnityExplorer.UI.Panels
{
    public class SMAPanel : UIPanel
    {
        public static SMAPanel Instance { get; private set; }

        public SMAPanel() { Instance = this; }

        public override string Name => "SMA";
        public override UIManager.Panels PanelType => UIManager.Panels.SMA;
        public override bool ShouldSaveActiveState => false;
        public override int MinWidth => 810;
        public override int MinHeight => 350;

        public TransformTree TransformTree;
        private ScrollPool<TransformCell> transformScroll;

        public SMAList SMAList;
        private ScrollPool<SMACell> componentScroll;

        public override string GetSaveDataFromConfigManager() => ConfigManager.SMAData.Value;

        public override void DoSaveToConfigElement()
        {
            ConfigManager.SMAData.Value = this.ToSaveData();
        }

        protected internal override void DoSetDefaultPosAndAnchors()
        {
            Rect.localPosition = Vector2.zero;
            Rect.pivot = new Vector2(0f, 1f);
            Rect.anchorMin = new Vector2(0.35f, 0.175f);
            Rect.anchorMax = new Vector2(0.8f, 0.925f);
        }

        private readonly List<SMAListItem> componentEntries = new List<SMAListItem>();

        public List<SMAListItem> GetComponentEntries() =>  componentEntries;

        public void Traverse(GameObject root)
        {
            foreach (Transform t in root.transform)
            {
                var obj = t.gameObject;
                GetComponents(obj);
                Traverse(obj);
            }
        }


        public void GetComponents(GameObject parent)
        {
            foreach (Component child in parent.GetComponents<Component>())
            {
                if (!(child is Transform))
                {
                    var type = child.GetType();
                    foreach (var alteradoresKey in new string[] { "alteradores", "alteradoresB", "alteradoresC", "alteradoresD", "alteradoresE", "alteradoresF", "alteradoresG" })
                    {
                        var alteradoresProp = type.GetProperty(alteradoresKey);
                        if (alteradoresProp != null)
                        {
                            var alteradoreList = alteradoresProp.GetValue(child, null) as IList;
                            foreach (var alter in alteradoreList)
                            {

                                var typeOfAlteradore = alter.GetType();
                                foreach(string valueKey in new string[] { "valor", "a", "b", "c", "d" })
                                {
                                    var propValue = typeOfAlteradore.GetProperty(valueKey);
                                    if (propValue != null)
                                    {
                                        componentEntries.Add(new SMAListItem(alter ,valueKey));
                                    }
                                }                                
                            }

                        }
                    }
                }
            }
        }

        public void AddCamera()
        {
            List<object> currentResults = SearchProvider.UnityObjectSearch("Fps Camera Rig",
             "UnityEngine.GameObject", SearchContext.UnityObject, ChildFilter.Any, SceneFilter.Any);

            List<object> currentResults2 = SearchProvider.UnityObjectSearch("MyCustomCameras",
            "UnityEngine.GameObject", SearchContext.UnityObject, ChildFilter.Any, SceneFilter.Any);

            if (currentResults.Count >= 1 && currentResults2.Count == 0)
            {
                GameObject cameraRig = (GameObject)currentResults[0];
                var controller = cameraRig.GetComponent("com.ootii.Cameras.CameraController");
                var originalCameraGO = cameraRig.transform.FindChild("Indoor Main Camera");
                var volumeGO = originalCameraGO.transform.FindChild("Volume");
                var inputSource = controller.GetType().GetProperty("InputSource").GetValue(controller, null);
                var isEnabled = inputSource.GetType().GetProperty("IsEnabled");
                var root = new GameObject("MyCustomCameras");
                SceneManager.MoveGameObjectToScene(root, cameraRig.scene);
                var camSwitcher = root.AddComponent<CameraSwitcher>();
                var camGO = new GameObject("MyCamera");
                camGO.transform.SetParent(root.transform, false);

                Action toggleOn = () =>
                {
                    isEnabled.SetValue(inputSource, false, null);
                    volumeGO.transform.SetParent(camGO.transform, false);
                    camGO.SetActive(true);
                    
                };
                toggleOn();

                Camera camera = camGO.AddComponent<Camera>();
                camera.transform.localPosition = cameraRig.transform.localPosition;
                camera.transform.localRotation = cameraRig.transform.localRotation;
                camera.cullingMask = originalCameraGO.GetComponent<Camera>().cullingMask;
                camGO.AddComponent<FlyCamera>();
                camSwitcher.OnSwitch = () =>
                {
                    if (camGO.activeSelf)
                    {
                        camGO.SetActive(false);
                        volumeGO.transform.SetParent(originalCameraGO.transform, false);
                        isEnabled.SetValue(inputSource, true, null);
                    } else
                    {
                        toggleOn();
                    }
                };
            }
            
        }

        public void DoAll()
        {
            componentEntries.Clear();
            foreach (GameObject obj in GetTransformEntries())
            {
                Traverse(obj);
            }
            SMAList.RefreshData();
            SMAList.ScrollPool.Refresh(true, true);
        }

        public GameObject DoSearch()
        {
            componentEntries.Clear();
            SMAList.RefreshData();
            SMAList.ScrollPool.Refresh(true, true);
            List<object> currentResults = SearchProvider.UnityObjectSearch("Female Avatar Root Haired V0.826",
                "UnityEngine.GameObject", SearchContext.UnityObject, ChildFilter.Any, SceneFilter.Any);
            
            if (currentResults.Count >= 1)
            {
                GameObject avatar = (GameObject) currentResults[0];
                return avatar;
            }
            return null;
        }

        private IEnumerable<GameObject> GetTransformEntries()
        {
            GameObject avatar = DoSearch();
            if (!avatar)
                return Enumerable.Empty<GameObject>();

            var list = new List<GameObject>();
            foreach (Transform t in avatar.transform)
            {
                var obj = t.gameObject;
                if(obj.name.Contains("Alteradators"))
                {
                    list.Add(obj);
                }
            }
            return list;
        }

        public void ChangeTarget(GameObject newTarget)
        {
            componentEntries.Clear();
            GetComponents(newTarget);
            SMAList.RefreshData();
            SMAList.ScrollPool.Refresh(true, true);
        }

        public override void ConstructPanelContent()
        {
            
            var m_uiRoot = UIFactory.CreateUIObject("SMAExplorer", content);
            UIFactory.SetLayoutGroup<VerticalLayoutGroup>(m_uiRoot, true, true, true, true, 4, padLeft: 5, padRight: 5);
            UIFactory.SetLayoutElement(m_uiRoot, flexibleHeight: 9999);

            var toolbar = UIFactory.CreateVerticalGroup(m_uiRoot, "SMAToolbar", true, true, true, true, 2, new Vector4(2, 2, 2, 2),
              new Color(0.15f, 0.15f, 0.15f));

            // Filter row

            var filterRow = UIFactory.CreateHorizontalGroup(toolbar, "SMAFilterGroup", true, true, true, true, 2, new Vector4(2, 2, 2, 2));
            UIFactory.SetLayoutElement(filterRow, minHeight: 25, flexibleHeight: 0);

            var searchButton = UIFactory.CreateButton(filterRow, "SMASearchButton", "Cheat Model");
            UIFactory.SetLayoutElement(searchButton.Component.gameObject, minHeight: 25, flexibleHeight: 0);
            searchButton.OnClick += () => {
                TransformTree.RefreshData(true, false);
            };

            var cameraButton = UIFactory.CreateButton(filterRow, "SMACameraButton", "Add Fly Camera");
            UIFactory.SetLayoutElement(cameraButton.Component.gameObject, minHeight: 25, flexibleHeight: 0);
            cameraButton.OnClick += () => {
                AddCamera();
                cameraButton.ButtonText.text = "RMouse + WASD, space to switch";
                //cameraButton.Component.interactable = false;
            };

            //Filter input field
            var inputField = UIFactory.CreateInputField(filterRow, "SMAFilterInput", "Search...");
            inputField.Component.targetGraphic.color = new Color(0.2f, 0.2f, 0.2f);
            RuntimeProvider.Instance.SetColorBlock(inputField.Component, new Color(0.4f, 0.4f, 0.4f), new Color(0.2f, 0.2f, 0.2f),
                new Color(0.08f, 0.08f, 0.08f));
            UIFactory.SetLayoutElement(inputField.UIRoot, minHeight: 25);
            inputField.OnValueChanged += (s) => {
                DoAll();
                SMAList.CurrentFilter = s;
                SMAList.RefreshData();
                SMAList.ScrollPool.Refresh(true, true);
            };

            var listHolder = UIFactory.CreateUIObject("ListHolders", m_uiRoot);
            UIFactory.SetLayoutGroup<HorizontalLayoutGroup>(listHolder, false, true, true, true, 8, 2, 2, 2, 2);
            UIFactory.SetLayoutElement(listHolder, minHeight: 150, flexibleWidth: 9999, flexibleHeight: 9999);

            // Left group (Children)

            var leftGroup = UIFactory.CreateUIObject("ChildrenGroup", listHolder);
            UIFactory.SetLayoutElement(leftGroup, flexibleWidth: 9999, flexibleHeight: 9999);
            UIFactory.SetLayoutGroup<VerticalLayoutGroup>(leftGroup, false, false, true, true, 2);

            var childrenLabel = UIFactory.CreateLabel(leftGroup, "ChildListTitle", "Children", TextAnchor.MiddleCenter, default, false, 16);
            UIFactory.SetLayoutElement(childrenLabel.gameObject, flexibleWidth: 9999);

            // Transform Tree

            transformScroll = UIFactory.CreateScrollPool<TransformCell>(leftGroup, "TransformTree", out GameObject transformObj,
               out GameObject transformContent, new Color(0.11f, 0.11f, 0.11f));
            UIFactory.SetLayoutElement(transformObj, flexibleHeight: 9999);
            UIFactory.SetLayoutElement(transformContent, flexibleHeight: 9999);

            TransformTree = new TransformTree(transformScroll, GetTransformEntries);
            TransformTree.Init();
            TransformTree.OnClickOverrideHandler = ChangeTarget;

            // Right group (Components)

            var rightGroup = UIFactory.CreateUIObject("ComponentGroup", listHolder);
            UIFactory.SetLayoutElement(rightGroup, flexibleWidth: 9999, flexibleHeight: 9999);
            UIFactory.SetLayoutGroup<VerticalLayoutGroup>(rightGroup, false, false, true, true, 2);

            var compLabel = UIFactory.CreateLabel(rightGroup, "CompListTitle", "Components", TextAnchor.MiddleCenter, default, false, 16);
            UIFactory.SetLayoutElement(compLabel.gameObject, flexibleWidth: 9999);

            componentScroll = UIFactory.CreateScrollPool<SMACell>(rightGroup, "ComponentList", out GameObject compObj,
                            out GameObject compContent, new Color(0.11f, 0.11f, 0.11f));
            SMAList = new SMAList(componentScroll, GetComponentEntries);
            componentScroll.Initialize(SMAList);

            UIFactory.SetLayoutElement(compObj, flexibleHeight: 9999);
            UIFactory.SetLayoutElement(compContent, flexibleHeight: 9999);

            UIManager.SetPanelActive(PanelType, false);
        }
    }
}