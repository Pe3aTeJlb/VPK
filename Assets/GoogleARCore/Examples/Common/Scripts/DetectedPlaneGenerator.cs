//-----------------------------------------------------------------------
// <copyright file="DetectedPlaneGenerator.cs" company="Google LLC">
//
// Copyright 2018 Google LLC. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------

namespace GoogleARCore.Examples.Common
{
    using System.Collections.Generic;
    using GoogleARCore;
    using UnityEngine;

    /// <summary>
    /// Manages the visualization of detected planes in the scene.
    /// </summary>
    public class DetectedPlaneGenerator : MonoBehaviour
    {
        /// <summary>
        /// A prefab for tracking and visualizing detected planes.
        /// </summary>
        public GameObject DetectedPlanePrefab;

        /// <summary>
        /// A list to hold new planes ARCore began tracking in the current frame. This object is
        /// used across the application to avoid per-frame allocations.
        /// </summary>
        private List<DetectedPlane> m_NewPlanes = new List<DetectedPlane>();
        private List<DetectedPlaneVisualizer> allPlaneObjects = new List<DetectedPlaneVisualizer>();

        public bool hideNewPlanes;
        /// <summary>
        /// The Unity Update method.
        /// </summary>
        public void Update()
        {
            // Check that motion tracking is tracking.
            if (Session.Status != SessionStatus.Tracking)
            {
                return;
            }

            // Iterate over planes found in this frame and instantiate corresponding GameObjects to
            // visualize them.
            Session.GetTrackables<DetectedPlane>(m_NewPlanes, TrackableQueryFilter.New);
            for (int i = 0; i < m_NewPlanes.Count; i++)
            {
                // Instantiate a plane visualization prefab and set it to track the new plane. The
                // transform is set to the origin with an identity rotation since the mesh for our
                // prefab is updated in Unity World coordinates.
                GameObject planeObject =
                    Instantiate(DetectedPlanePrefab, Vector3.zero, Quaternion.identity, transform);

                DetectedPlaneVisualizer planeObjectVisualizer = planeObject.GetComponent<DetectedPlaneVisualizer>();
                planeObjectVisualizer.Initialize(m_NewPlanes[i]);
                
                allPlaneObjects.Add(planeObjectVisualizer);
                

                if (hideNewPlanes) { planeObjectVisualizer.m_MeshRenderer.enabled = false; }
                else { planeObjectVisualizer.m_MeshRenderer.enabled = true; }
            }
        }

        public void HideAllPlanes() {
            for (int i = 0; i < allPlaneObjects.Count; i++)
            {
                if (allPlaneObjects[i].m_MeshRenderer != null)allPlaneObjects[i].m_MeshRenderer.enabled = false;
            }
        }

        public void ShowAllPlanes()
        {
            for (int i = 0; i < allPlaneObjects.Count; i++) {
                if(allPlaneObjects[i].m_MeshRenderer != null)allPlaneObjects[i].m_MeshRenderer.enabled = true;
            }
        }

        public void CreateCollider() 
        {
            foreach (DetectedPlaneVisualizer dpv in allPlaneObjects) 
            {
                if (!dpv.gameObject.AddComponent<MeshCollider>())
                {
                    dpv.gameObject.AddComponent<MeshCollider>();
                    dpv.GetComponent<MeshCollider>().sharedMesh = dpv.m_Mesh;
                }
                else 
                {
                    dpv.GetComponent<MeshCollider>().enabled = true;
                    dpv.GetComponent<MeshCollider>().sharedMesh = dpv.m_Mesh;
                }
            }

        }

        public void RemoveAllCollider()
        {
            foreach (DetectedPlaneVisualizer dpv in allPlaneObjects)
            {
                dpv.GetComponent<MeshCollider>().enabled = false;
            }
        }

        public DetectedPlane GetMaxAreaPlane(float minX, float minZ) 
        {
            DetectedPlane resultPlane = allPlaneObjects[0].m_DetectedPlane;
            float resultPlaneArea = resultPlane.ExtentX * resultPlane.ExtentZ;

            for (int i = 1; i < allPlaneObjects.Count; i++) 
            {
                float buffArea = allPlaneObjects[i].m_DetectedPlane.ExtentX * allPlaneObjects[i].m_DetectedPlane.ExtentZ;

                if (buffArea > resultPlaneArea) 
                {
                    resultPlane = allPlaneObjects[i].m_DetectedPlane;
                    resultPlaneArea = buffArea;
                }

            }

            Debug.LogWarning(resultPlane.ExtentX);
            Debug.LogWarning(resultPlane.ExtentZ);

            if (resultPlane.ExtentX >= minX && resultPlane.ExtentZ >= minZ)
            {
                return resultPlane;
            }
            else { return null; }
        }



    }

}
