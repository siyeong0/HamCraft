using Unity.Mathematics;
using UnityEngine;

using System.Runtime.InteropServices;
using UnityEngine.UIElements;
using System;
using UnityEditor;

namespace HamCraft
{
    public class Fluid : MonoBehaviour
    {
        [Header("Simulation")]
        [SerializeField] int numParcels = 10000;
        [SerializeField] float interactionRadius = 1.0f;
        [SerializeField] float mass = 0.1f;
        [SerializeField] float targetDensity = 1.0f;
        [SerializeField] float pressureCoefficient = 0.5f;

        [Header("Visualization")]
        [SerializeField] float radius = 1.0f;
        [SerializeField] Color color = Color.white;

        [Header("Shaders")]
        [SerializeField] ComputeShader fluidComputeShader;
        [SerializeField] Material fluidMaterialShader;

        [Header("Debug")]
        [SerializeField] Material visualizeDensityMaterialShader;
        [SerializeField] Color positiveDensityColor = Color.red;
        [SerializeField] Color negativeDensityColor = Color.blue;
        [SerializeField] Color zeroDensityColor = Color.white;
        [SerializeField] Color forceColor = Color.black;

        // parcel data buffer
        Parcel[] hostParcelBuffer;
        ComputeBuffer deviceParcelBuffer;
        float[] hostDensityBuffer;
        float[] hostPressureBuffer;

        ComputeBuffer argsBuffer;

        int solveKernel;

        Mesh mesh;
        Vector2 halfSize;

        void Start()
        {
            halfSize = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0));

            // init kernels
            solveKernel = fluidComputeShader.FindKernel("Solve");

            // init parcels
            int numParcelsPerLine = Mathf.CeilToInt(Mathf.Sqrt(numParcels));
            float spacePerParcel = (2 * radius + 0.1f);
            float basePos = -numParcelsPerLine / 2 * spacePerParcel;
            hostParcelBuffer = new Parcel[numParcels];
            for (int i = 0; i < numParcels; i++)
            {
                //float x = i % numParcelsPerLine * spacePerParcel + basePos;
                //float y = i / numParcelsPerLine * spacePerParcel + basePos;
                float x = UnityEngine.Random.Range(-halfSize.x, halfSize.x);
                float y = UnityEngine.Random.Range(-halfSize.y, halfSize.y);
                hostParcelBuffer[i].Position = new float2(x, y);
            }
            hostDensityBuffer = new float[numParcels];
            hostPressureBuffer = new float[numParcels];

            // init buffers
            deviceParcelBuffer = new ComputeBuffer(numParcels, Marshal.SizeOf<Parcel>());
            deviceParcelBuffer.SetData(hostParcelBuffer);

            // set buffers
            fluidComputeShader.SetBuffer(solveKernel, "parcelBuffer", deviceParcelBuffer);
            fluidMaterialShader.SetBuffer("parcelBuffer", deviceParcelBuffer);

            // init other members
            mesh = createMesh();

            // init instance indirect args
            uint[] args = new uint[5] { mesh.GetIndexCount(0), (uint)numParcels, 0, 0, 0 };
            argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);

            // debug
            visualizeDensityMaterialShader.SetBuffer("parcelBuffer", deviceParcelBuffer);
        }

        void Update()
        {
            uint[] args = new uint[5] { mesh.GetIndexCount(0), (uint)numParcels, 0, 0, 0 };
            argsBuffer.SetData(args);

            // draw debug info
            visualizeDensityMaterialShader.SetInt("numParcels", numParcels);
            visualizeDensityMaterialShader.SetFloat("smoothingRadius", interactionRadius);
            visualizeDensityMaterialShader.SetFloat("targetDensity", targetDensity);
            visualizeDensityMaterialShader.SetColor("positiveColor", positiveDensityColor);
            visualizeDensityMaterialShader.SetColor("negativeColor", negativeDensityColor);
            visualizeDensityMaterialShader.SetColor("zeroColor", zeroDensityColor);
            Graphics.DrawProcedural(visualizeDensityMaterialShader, new Bounds(Vector3.zero, Vector3.one * 1000f), MeshTopology.Triangles, 6, 1);

            // draw parcels
            fluidMaterialShader.SetFloat("radius", radius);
            fluidMaterialShader.SetColor("color", color);
            Graphics.DrawMeshInstancedIndirect(mesh, 0, fluidMaterialShader, new Bounds(Vector3.zero, Vector3.one * 1000f), argsBuffer);
        }

        void FixedUpdate()
        {
            fluidComputeShader.SetFloat("deltaTime", Time.deltaTime);
            fluidComputeShader.SetFloat("radius", interactionRadius);
            fluidComputeShader.SetFloat("mass", mass);
            fluidComputeShader.SetInt("numParcels", numParcels);

            fluidComputeShader.Dispatch(solveKernel, Mathf.CeilToInt(numParcels / 1024f), 1, 1);

            deviceParcelBuffer.GetData(hostParcelBuffer);

            for (int i = 0; i < numParcels; i++)
            {
                hostDensityBuffer[i] = sampleDensity(hostParcelBuffer[i].Position);
                hostPressureBuffer[i] = cvtDensityToPressure(hostDensityBuffer[i]);
            }

            float2[] accelerations = new float2[numParcels];
            for (int i = 0; i < numParcels; i++)
            {
                float2 pos = hostParcelBuffer[i].Position;
                accelerations[i] += calcPressureForce(i) / hostDensityBuffer[i];
            }

            for (int i = 0; i < numParcels; i++)
            {
                hostParcelBuffer[i].Velocity = accelerations[i] * Time.deltaTime;
                hostParcelBuffer[i].Position += hostParcelBuffer[i].Velocity * Time.deltaTime;
            }

            handleCollision();

            deviceParcelBuffer.SetData(hostParcelBuffer);
        }

        void OnDestroy()
        {
            deviceParcelBuffer?.Release();
            argsBuffer?.Release();
        }

        void OnGUI()
        {
            return;
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            float2 samplePoint = new float2(mousePosition.x, mousePosition.y);
            float pressure = cvtDensityToPressure(sampleDensity(samplePoint));

            string message = "Pressure : " + pressure;

            GUIStyle style = new GUIStyle();
            style.fontSize = 96;
            style.normal.textColor = Color.black;
            GUI.Label(new Rect(10, 10, 300, 20), message, style);
        }

        void OnDrawGizmos()
        {
            return;
            const float SLICING = 0.5f;
            Gizmos.color = forceColor;
			for (float xPos = -halfSize.x + SLICING; xPos < halfSize.x; xPos += SLICING)
            {
                for (float yPos = -halfSize.y + SLICING; yPos < halfSize.y; yPos += SLICING)
                {
                    // float2 pf = samplePressureForce(new float2(xPos, yPos));
                    float2 pf = samplePressureForce(new float2(xPos, yPos));
                    Vector3 start = new Vector3(xPos, yPos, 0);
                    Vector3 direction = new Vector3(pf.x, pf.y, 0) * 0.5f;
					float length = direction.magnitude;
                    if (length > 0.3f)
                        direction = direction / length * 0.3f;
					Vector3 end = start + direction;
					Gizmos.DrawRay(start, direction);

                    Vector2 right = rotate(direction, 150f);
                    right.Normalize();
                    right *= 0.2f;
					Gizmos.DrawRay(end, right * 0.5f);
                    Vector2 left = rotate(direction, -150f);
                    left.Normalize();
                    left *= 0.2f;
					Gizmos.DrawRay(end, left * 0.5f);
				}
            }
        }
        Vector2 rotate(Vector2 v, float degrees)
        {
            float rad = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            return new Vector2(cos * v.x - sin * v.y, sin * v.x + cos * v.y);
        }

        Mesh createMesh()
        {
            // init meshe
            Mesh m = new Mesh();
            m.name = "FluidQuadMesh";
            Vector3[] vertices = new Vector3[6]
               {
                    new Vector3(-1, -1, 0), // 0
					new Vector3( 1, -1, 0), // 1
					new Vector3(-1,  1, 0), // 2

					new Vector3(-1,  1, 0), // 2
					new Vector3( 1, -1, 0), // 1
					new Vector3( 1,  1, 0)  // 3
			   };

            Vector2[] uv = new Vector2[6]
                {
                    new Vector2(0, 0), // 0
					new Vector2(1, 0), // 1
					new Vector2(0, 1), // 2

					new Vector2(0, 1), // 2
					new Vector2(1, 0), // 1
					new Vector2(1, 1)  // 3
				};
            int[] indices = new int[6] { 0, 1, 2, 3, 4, 5 };

            m.vertices = vertices;
            m.uv = uv;
            m.SetIndices(indices, MeshTopology.Triangles, 0);
            m.RecalculateNormals();
            m.RecalculateBounds();

            return m;
        }

        float sampleDensity(float2 samplePoint)
        {
            float density = 0.0f;
            foreach (var parcel in hostParcelBuffer)
            {
                float dist = math.length(parcel.Position - samplePoint);
                density += mass * spikyKernel(dist);
            }
            return density;
        }

        float cvtDensityToPressure(float density)
        {
            float densityError = density - targetDensity;
            return pressureCoefficient * densityError;
        }

        float2 calcPressureForce(int sampleIdx)
        {
            float2 pressureForce = new float2(0.0f, 0.0f);
            float2 positionA = hostParcelBuffer[sampleIdx].Position;
            float densityA = hostDensityBuffer[sampleIdx];
            float pressureA = hostPressureBuffer[sampleIdx];
            for (int j = 0; j < numParcels; j++)
            {
                if (j == sampleIdx) continue;

                float2 vec = (hostParcelBuffer[j].Position - positionA);
                float dist = math.length(vec);
                if (dist > interactionRadius || dist < 0.1f) continue;

                float densityB = hostDensityBuffer[j];
                float pressureB = hostPressureBuffer[j];
                float pressureTerm = -mass * (pressureA + pressureB) / (2.0f * densityB);
				pressureForce += pressureTerm * spikyGradient(vec, dist);
			}

            return pressureForce;
        }

        float2 samplePressureForce(float2 samplePosition)
        {
            float2 pressureForce = new float2(0.0f, 0.0f);
            float densityA = sampleDensity(samplePosition);
            float pressureA = cvtDensityToPressure(densityA);
            for (int j = 0; j < numParcels; j++)
            {
                float2 vec = (hostParcelBuffer[j].Position - samplePosition);
                float dist = math.length(vec);
                if (dist > interactionRadius || dist < 0.1f) continue;

                float densityB = hostDensityBuffer[j];
                float pressureB = hostPressureBuffer[j];
                float pressureTerm = -mass * (pressureA + pressureB) / (2.0f * densityB);
				pressureForce += pressureTerm * spikyGradient(vec, dist);
			}

            return pressureForce;
        }

        float2 grad(float2 samplePosition)
        {
            float stepSize = 0.001f;
            float pivot = cvtDensityToPressure(sampleDensity(samplePosition));
            float dx = cvtDensityToPressure(sampleDensity(samplePosition + new float2(1.0f, 0.0f) * stepSize)) - pivot;
            float dy = cvtDensityToPressure(sampleDensity(samplePosition + new float2(0.0f, 1.0f) * stepSize)) - pivot;

            return new float2(dx, dy) / -stepSize;
        }


        const float PI = 3.141592f;

		float smoothKernel(float dist)
		{
			float volume = PI * Mathf.Pow(interactionRadius, 4) / 6;
			float value = Mathf.Max(0, interactionRadius - dist);
			return value * value / volume;
		}

		float2 smoothGradient(float2 vec, float dist)
		{
			float volume = PI * Mathf.Pow(interactionRadius, 4) / 6;
			float value = Mathf.Max(0, interactionRadius - dist);
			return (-2 * dist * value / volume) * (vec / dist);
		}

		float spikyKernel(float dist)
        {
            if (dist > interactionRadius) return 0.0f;
            float coeff = 15 / (3.14f * Mathf.Pow(interactionRadius, 6));
            return coeff * Mathf.Pow(interactionRadius - dist, 3);
        }

        float2 spikyGradient(float2 vec, float dist)
        {
            if (dist > interactionRadius || dist < 0.001f) return new float2(0,0);
            float coeff = -45 / (3.14f * Mathf.Pow(interactionRadius, 6));
            return coeff * Mathf.Pow(interactionRadius - dist, 2) * (vec / (dist));
        }

        void handleCollision()
        {
            float damping = 0.8f;
            for (int i = 0; i < numParcels; ++i)
            {
                if (hostParcelBuffer[i].Position.x > halfSize.x)
                {
                    hostParcelBuffer[i].Position.x = halfSize.x;
                    hostParcelBuffer[i].Velocity.x *= -1 * damping;
                }
                else if (hostParcelBuffer[i].Position.x < -halfSize.x)
                {
                    hostParcelBuffer[i].Position.x = -halfSize.x;
                    hostParcelBuffer[i].Velocity.x *= -1 * damping;
                }
                if (hostParcelBuffer[i].Position.y > halfSize.y)
                {
                    hostParcelBuffer[i].Position.y = halfSize.y;
                    hostParcelBuffer[i].Velocity.y *= -1 * damping;
                }
                else if (hostParcelBuffer[i].Position.y < -halfSize.y)
                {
                    hostParcelBuffer[i].Position.y = -halfSize.y;
                    hostParcelBuffer[i].Velocity.y *= -1 * damping;
                }
            }
        }
    }
}