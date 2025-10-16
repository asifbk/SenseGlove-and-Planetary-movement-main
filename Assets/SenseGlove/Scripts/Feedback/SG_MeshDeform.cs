﻿using System.Collections.Generic;
using UnityEngine;

namespace SG
{
    /// <summary>
    /// Improved version with gradual deformation and distance-based falloff
    /// </summary>
    [RequireComponent(typeof(SG_Material))]
    public class SG_MeshDeform : MonoBehaviour, IOutputs01Value
    {
        //----------------------------------------------------------------------------------------------
        // Properties

        #region Properties

        [Tooltip("The filter used to extract the mesh of the object to deform.")]
        public MeshFilter meshFilter;

        [Tooltip("Determines how the Vertices respond to the collider(s)")]
        public SG.Materials.DisplaceType displaceType = SG.Materials.DisplaceType.Plane;

        [Tooltip("The Maximum that a vertex can displace from its original position, in m.")]
        public float maxDisplacement = 0.01f;

        [Header("Deformation Smoothing")]
        [Tooltip("How gradually the deformation is applied (0-1). Lower = more gradual")]
        [Range(0.1f, 1f)]
        public float deformationStrength = 0.5f;

        [Tooltip("Distance from contact point where deformation begins to fade (in meters)")]
        [Range(0.001f, 0.1f)]
        public float falloffDistance = 0.03f;

        [Tooltip("Curve controlling how deformation fades with distance")]
        public AnimationCurve falloffCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        protected Mesh myMesh;
        protected Vector3[] verts;
        protected Vector3[] deformVerts;
        protected bool atRest = true;
        protected int[] uniqueVertices;
        protected int[][] sameVertices;
        protected List<SG.Materials.Deformation> deformationQueue = new List<SG.Materials.Deformation>();
        protected bool deforms = true;

        public float AverageSqueezeDist { get; protected set; }

        #endregion Properties

        public float Get01Value()
        {
            return this.maxDisplacement == 0 ? (this.atRest ? 0 : 1) : Mathf.Clamp01(AverageSqueezeDist / maxDisplacement);
        }

        //----------------------------------------------------------------------------------------------
        // Mesh Deformation

        #region MeshDeformation

        protected void SetDeform(bool meshDeforms)
        {
            if (this.deforms)
            {
                this.ResetMesh();
            }
            this.deforms = meshDeforms;
        }

        protected void CollectMeshData()
        {
            if (this.meshFilter == null)
            {
                this.meshFilter = this.GetComponent<MeshFilter>();
            }

            if (this.meshFilter != null)
            {
                this.myMesh = this.meshFilter.mesh;
                if (myMesh != null)
                {
                    this.verts = myMesh.vertices;
                    this.deformVerts = myMesh.vertices;

                    List<int>[] samePoints = new List<int>[verts.Length];
                    int uniquePoints = 0;

                    for (int i = 0; i < this.verts.Length; i++)
                    {
                        this.deformVerts[i] = this.verts[i];
                        samePoints[i] = new List<int>();
                        for (int j = 0; j < this.verts.Length; j++)
                        {
                            if (i != j && verts[i].Equals(verts[j]))
                            {
                                samePoints[i].Add(j);
                            }
                        }

                        bool alreadyCounted = false;
                        for (int s = 0; s < samePoints[i].Count; s++)
                        {
                            if (samePoints[i][s] < i)
                            {
                                alreadyCounted = true;
                            }
                        }
                        if (!alreadyCounted)
                        {
                            uniquePoints++;
                        }
                    }

                    this.uniqueVertices = new int[uniquePoints];
                    this.sameVertices = new int[uniquePoints][];

                    int n = 0;
                    for (int i = 0; i < this.verts.Length; i++)
                    {
                        bool alreadyCounted = false;
                        for (int s = 0; s < samePoints[i].Count; s++)
                        {
                            if (samePoints[i][s] < i)
                            {
                                alreadyCounted = true;
                            }
                        }
                        if (!alreadyCounted)
                        {
                            this.uniqueVertices[n] = i;
                            this.sameVertices[n] = samePoints[i].ToArray();
                            n++;
                        }
                    }
                }
            }
        }

        public bool SameVertex(Vector3 v1, Vector3 v2)
        {
            return v1.x == v2.x && v1.y == v2.y && v1.z == v2.z;
        }

        public void AddDeformation(Vector3 absEntryVector, Vector3 absDeformPoint, float dist)
        {
            Vector3 N = this.transform.InverseTransformDirection(absEntryVector);

            for (int i = 0; i < this.deformationQueue.Count;)
            {
                if (N.Equals(this.transform.InverseTransformDirection(deformationQueue[i].absEntryVector)))
                {
                    if (dist < this.deformationQueue[i].distance)
                    {
                        return;
                    }
                    else
                    {
                        RemoveDeform(i);
                    }
                }
                else
                {
                    i++;
                }
            }

            this.AddDeform(absEntryVector, absDeformPoint, dist);
            this.atRest = this.deformationQueue.Count <= 0;
        }

        protected void AddDeform(Vector3 absEntryVector, Vector3 absDeformPoint, float dist)
        {
            this.deformationQueue.Add(new SG.Materials.Deformation(absEntryVector, absDeformPoint, dist));
        }

        protected void RemoveDeform(int index)
        {
            if (index >= 0 && index < this.deformationQueue.Count)
            {
                this.deformationQueue.RemoveAt(index);
            }
        }

        protected void ClearDeformations()
        {
            this.deformationQueue.Clear();
        }

        protected void ResetPoints(bool resetAll)
        {
            if (resetAll)
            {
                for (int i = 0; i < this.deformVerts.Length; i++)
                {
                    this.deformVerts[i] = this.verts[i];
                }
            }
            else
            {
                for (int i = 0; i < this.uniqueVertices.Length; i++)
                {
                    Vector3 originalPoint = this.verts[this.uniqueVertices[i]];
                    this.deformVerts[this.uniqueVertices[i]] = originalPoint;
                }
            }
        }

        /// <summary>
        /// IMPROVED: Actually deform the mesh with gradual deformation and distance-based falloff
        /// </summary>
        protected void DeformMesh(Vector3 absEntryVector, Vector3 absDeformPoint)
        {
            if (displaceType == SG.Materials.DisplaceType.Plane)
            {
                Vector3 localNormal = this.transform.InverseTransformDirection(absEntryVector.normalized);
                Vector3 localPoint = this.transform.InverseTransformPoint(absDeformPoint);

                for (int i = 0; i < this.uniqueVertices.Length; i++)
                {
                    Vector3 vert = this.deformVerts[this.uniqueVertices[i]];
                    Vector3 V = (vert - localPoint);
                    float dot = Vector3.Dot(localNormal, V);
                    bool abovePlane = dot > 0;

                    if (abovePlane)
                    {
                        // Calculate distance from contact point
                        float distanceFromContact = V.magnitude;
                        
                        // Calculate falloff based on distance
                        float falloff = 1f;
                        if (distanceFromContact > 0.0001f)
                        {
                            float normalizedDist = Mathf.Clamp01(distanceFromContact / falloffDistance);
                            falloff = falloffCurve.Evaluate(normalizedDist);
                        }

                        // Project the Vector onto the plane
                        Vector3 d = Vector3.Project(V, localNormal);
                        
                        // Apply gradual deformation with falloff
                        float deformAmount = deformationStrength * falloff;
                        Vector3 targetPoint = vert - (d * deformAmount);

                        // Calculate displacement from original position
                        Vector3 totalDisplacement = targetPoint - this.verts[this.uniqueVertices[i]];
                        
                        // Limit to max displacement
                        if (totalDisplacement.magnitude > this.maxDisplacement)
                        {
                            totalDisplacement = totalDisplacement.normalized * this.maxDisplacement;
                            targetPoint = this.verts[this.uniqueVertices[i]] + totalDisplacement;
                        }

                        this.UpdatePoint(i, targetPoint);
                    }
                }
                this.atRest = false;
            }
        }

        protected void UpdatePoint(int uniqueVertIndex, Vector3 newPos)
        {
            this.deformVerts[this.uniqueVertices[uniqueVertIndex]] = newPos;
            for (int i = 0; i < this.sameVertices[uniqueVertIndex].Length; i++)
            {
                this.deformVerts[this.sameVertices[uniqueVertIndex][i]] = newPos;
            }
        }

        protected void UpdateMesh()
        {
            if (this.myMesh && !this.atRest)
            {
                this.ResetPoints(false);

                float deformSum = 0;
                for (int i = 0; i < this.deformationQueue.Count; i++)
                {
                    this.DeformMesh(this.deformationQueue[i].absEntryVector, this.deformationQueue[i].absDeformPosition);
                    deformSum += deformationQueue[i].distance;
                }
                AverageSqueezeDist = deformationQueue.Count == 0 ? 0 : deformSum / (float)deformationQueue.Count;
                this.ClearDeformations();

                myMesh.vertices = deformVerts;
                myMesh.RecalculateBounds();
                myMesh.RecalculateNormals();
            }
        }

        public void ResetMesh()
        {
            if (myMesh != null)
            {
                this.ResetPoints(true);
                myMesh.vertices = deformVerts;
                myMesh.RecalculateBounds();
                myMesh.RecalculateNormals();
            }
            this.atRest = true;
            AverageSqueezeDist = 0;
        }

        #endregion MeshDeformation

        //----------------------------------------------------------------------------------------------
        // Monobehaviour

        #region Monobehaviour

        protected virtual void Start()
        {
            this.CollectMeshData();
        }

        protected virtual void FixedUpdate()
        {
            this.UpdateMesh();
        }

        protected virtual void OnDisable()
        {
            this.ResetMesh();
        }

        #endregion Monobehaviour
    }
}

namespace SG.Materials
{
    public enum DisplaceType
    {
        Plane = 0
    }

    public struct Deformation
    {
        public Vector3 absEntryVector;
        public Vector3 absDeformPosition;
        public float distance;

        public Deformation(Vector3 absEntryVect, Vector3 absDefPosition, float dist)
        {
            this.absEntryVector = absEntryVect;
            this.absDeformPosition = absDefPosition;
            this.distance = dist;
        }
    }
}