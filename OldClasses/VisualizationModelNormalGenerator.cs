using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace AssimpLoaderPbrDx
{
    public class VisualizationModelNormalGenerator
    {
    }

    public class ModelsVisualNormals
    {
        //Texture2D texture;
        RiggedAlignedNormalArrows[] modelNormalArrows;

        string techniqueNameForNormalsToUse = "SkinnedColorBaseNormEmissiveDebugBones";  //"VisualNormalsDraw";//"SkinnedColorBaseNormEmissiveDebugBones";

        int workingMeshIndex = 0;
        List<RiggedModel.RiggedModelMesh> nMeshes = new List<RiggedModel.RiggedModelMesh>();
        //int workingMeshIndex = 0;


        public ModelsVisualNormals(RiggedModel model, Texture2D t, float thickness, float scale)
        {
            Console.WriteLine(" ______________________________________");
            Console.WriteLine(" ModelsVisualNormals");
            Console.WriteLine(" About to begin Generating New meshes    model.meshes.Length: " + model.meshes.Length);
            Console.WriteLine("");

            foreach (var m in model.meshes)
                Console.WriteLine(" verify ...   model.meshes[" + m.meshNumber + "]  created normals   model.meshes[i].vertices.Length: " + m.vertices.Length);

            modelNormalArrows = new RiggedAlignedNormalArrows[model.meshes.Length];

            var originalStart = 0;
            var originalEnd = model.meshes.Length;
            var newStart = model.meshes.Length;
            var newEnd = newStart + model.meshes.Length;

            nMeshes = CopyMeshes(model);
            CombineModelMeshesWithNewMeshesAndLinkBonesToNodes(model, nMeshes);

            BuildNormals(model, originalEnd, t, thickness, scale);

            ReplaceNormalsForVertices(model, t, thickness, scale, originalStart, originalEnd);
            //ReplaceNormalsForVertices(model, t, thickness, scale, newStart, newEnd);

            //AppendNormalVerticesToCurrentMeshVertices(model, t, thickness, scale);

            foreach (var m in model.meshes)
                Console.WriteLine(" verify ...   m.Note: "+m.Note+"   model.meshes[" + m.meshNumber + "]  created normals   model.meshes[i].vertices.Length: " + m.vertices.Length);

            Console.WriteLine(" Completed    model.meshes.Length: " + model.meshes.Length );
            Console.WriteLine("");
        }

        public void BuildNormals(RiggedModel model, int originalLen, Texture2D t, float thickness, float scale)
        {
            int j = 0;
            for (int meshindex = 0; meshindex < originalLen; meshindex++)
            {
                if (model.meshes[meshindex].IsGeneratedSkeleton == false)
                {
                    modelNormalArrows[j] = new RiggedAlignedNormalArrows();
                    modelNormalArrows[j].CreateVisualNormalsForPrimitiveMesh(model.meshes[meshindex].vertices, model.meshes[meshindex].indices, t, thickness, scale);
                    modelNormalArrows[j].texture = t;
                    Console.WriteLine("   Generating visual normal mesh ... modelNormalArrows.Length: " + modelNormalArrows.Length + "   model.meshes[" + meshindex + "]  created normals   modelNormalArrows[" + j + "].vertices.Length: " + modelNormalArrows[j].vertices.Length);
                    Console.WriteLine("");
                }
            }
        }

        public void AppendNormalVerticesToCurrentMeshVertices(RiggedModel model, Texture2D t, float thickness, float scale, int start, int end)
        {
            //for (int i = 0; i < model.meshes.Length; i++)
            //{
            int j = 0;
            for (int meshindex = start; meshindex < end; meshindex++)
            {
                if (model.meshes[meshindex].IsGeneratedSkeleton == false)
                {
                    Console.WriteLine("   Generating visual normal mesh ... modelNormalArrows.Length: " + modelNormalArrows.Length + "   model.meshes[" + meshindex + "]  created normals   modelNormalArrows[" + meshindex + "].vertices.Length: " + modelNormalArrows[j].vertices.Length);
                    Console.WriteLine("");
                    ModifyMeshAppendVisualNormalVerticesIndices(model, meshindex, modelNormalArrows[j]);
                    j++;
                }
            }
        }

        public void ReplaceNormalsForVertices(RiggedModel model, Texture2D t, float thickness, float scale ,int start, int end)
        {
            int j = 0;
            for (int meshindex = start; meshindex < end; meshindex++)
            {
                if (model.meshes[meshindex].IsGeneratedSkeleton == false)
                {
                    Console.WriteLine("   Generating visual normal mesh ... modelNormalArrows.Length: " + modelNormalArrows.Length + "   model.meshes[" + meshindex + "]  created normals   modelNormalArrows[" + j + "].vertices.Length: " + modelNormalArrows[j].vertices.Length);
                    Console.WriteLine("");
                    ReplaceMeshVerticesAndIndices(model, meshindex, modelNormalArrows[j], t);
                    j++;
                }
            }
        }

        public void ReplaceMeshVerticesAndIndices(RiggedModel model, int index, RiggedAlignedNormalArrows rn, Texture2D t)
        {
            var m = model.meshes[index];
            m.vertices = rn.vertices;
            m.indices = rn.indices;
            m.IsGeneratedSkeleton = true;
            m.techniqueName = techniqueNameForNormalsToUse;
            m.techniqueValue = 0;
            m.Note = "GeneratedNormals";
            m.textureDiffuse = t;
            m.meshNumber = index;
        }

        public void CombineModelMeshesWithNewMeshesAndLinkBonesToNodes(RiggedModel model, List<RiggedModel.RiggedModelMesh> newMeshes)
        {
            var start = model.meshes.Length;
            List<RiggedModel.RiggedModelMesh> MasterMeshes = new List<RiggedModel.RiggedModelMesh>();
            for (int i = 0; i < model.meshes.Length; i++)
            {
                MasterMeshes.Add(model.meshes[i]);
            }
            int j = model.meshes.Length;
            for (int i = 0; i < model.meshes.Length; i++)
            {
                newMeshes[i].Note = "GeneratedNormals";
                MasterMeshes.Add(newMeshes[i]);
                j++;
            }
            model.meshes = MasterMeshes.ToArray();

            // set up new node to mesh bone linking references.
            // nodes keep a list of bones per mesh as they contain unique offsets.
            for (int mindex = start; mindex < model.meshes.Length; mindex++)
            {
                var m = model.meshes[mindex];
                for (int bindex = 0; bindex< m.modelMeshBones.Length; bindex++)
                {
                    var bone = m.modelMeshBones[bindex];
                    bone.meshIndex = mindex;
                    var cnode = bone.refToCorrespondingHeirarchyNode; //ModelGetRefToNode(bone.refToCorrespondingHeirarchyNode.Name, model.sceneRootNodeOfTree);
                    if(cnode != null)
                      cnode.uniqueMeshBones.Add(bone);
                }
            }
        }

        public List<RiggedModel.RiggedModelMesh> CopyMeshes(RiggedModel model)
        {
            List<RiggedModel.RiggedModelMesh> newMeshes = new List<RiggedModel.RiggedModelMesh>();
            for (int i = 0; i < model.meshes.Length; i++)
            {
                var rm = model.meshes[i];
                newMeshes.Add(new RiggedModel.RiggedModelMesh());
                var nrm = newMeshes[i];
                CopyMesh(rm, nrm);
            }
            return newMeshes;
        }

        public void CopyMesh(RiggedModel.RiggedModelMesh a, RiggedModel.RiggedModelMesh b)
        {
            b.Name = a.Name;
            b.Note = a.Note;
            b.techniqueName = a.techniqueName;
            b.techniqueValue = a.techniqueValue;

            b.ColorAmbient = a.ColorAmbient;
            b.ColorDiffuse = a.ColorDiffuse;
            b.ColorEmissive = a.ColorEmissive;
            b.ColorReflective = a.ColorReflective;
            b.ColorSpecular = a.ColorSpecular;
            b.ColorTransparent = a.ColorTransparent;

            b.HasBones = a.HasBones;
            b.HasMeshAnimationAttachments = a.HasMeshAnimationAttachments;
            b.HasPreCalculatedBiTangents = a.HasPreCalculatedBiTangents;
            b.HasPreCalculatedNormals = a.HasPreCalculatedNormals;
            b.HasPreCalculatedTangents = a.HasPreCalculatedTangents;
            b.HasPreCalculatedVertexColors = a.HasPreCalculatedVertexColors;
            b.HasShaders = a.HasShaders;
            b.IsPbrMaterial = a.IsPbrMaterial;
            b.IsTwoSided = a.IsTwoSided;
            b.IsWireFrameEnabled = a.IsWireFrameEnabled;
            b.Opacity = a.Opacity;
            b.Transparency = a.Transparency;
            b.Reflectivity = a.Reflectivity;
            b.ShadingMode = a.ShadingMode;
            b.Shininess = a.Shininess;
            b.ShininessStrength = a.ShininessStrength;
            b.BlendMode = a.BlendMode;
            b.BumpScaling = a.BumpScaling;

            b.textureDiffuse = a.textureDiffuse;
            b.textureBaseColorMapPbr = a.textureBaseColorMapPbr;
            b.textureRoughnessPbr = a.textureRoughnessPbr;
            b.textureMetalnessPbr = a.textureMetalnessPbr;
            b.textureNormalMap = a.textureNormalMap;

            b.textureAmbientOcclusionOraLightMap = a.textureAmbientOcclusionOraLightMap;
            b.textureDisplacementMap = a.textureDisplacementMap;
            b.textureEmissiveMap = a.textureEmissiveMap;
            b.textureHeightMap = a.textureHeightMap;
            b.textureLightMap = a.textureLightMap;
            b.textureNormalMapPrbCamera = a.textureNormalMapPrbCamera;
            b.textureOpacity = a.textureOpacity;
            b.textureReflectionMap = a.textureReflectionMap;
            b.textureShine = a.textureShine;
            b.textureSpecular = a.textureSpecular;
            b.textureUnknown = a.textureUnknown;

            b.Centroid = a.Centroid;
            b.Max = a.Max;
            b.Min = a.Min;

            b.IsGeneratedSkeleton = a.IsGeneratedSkeleton; // note well change this later
            b.meshNumber = a.meshNumber; // note May require it to be adjusted
            b.nodeRefContainingMeshNodeAnimation = a.nodeRefContainingMeshNodeAnimation; // note may now need to be different.

            b.globalShaderMatrixs = new Matrix[a.globalShaderMatrixs.Length];
            for (int i =0; i < a.globalShaderMatrixs.Length; i++)
            {
                b.globalShaderMatrixs[i] = a.globalShaderMatrixs[i];
            }
            b.vertices = new VertexPositionColorNormalTextureTangentWeights[a.vertices.Length];
            for (int i = 0; i < a.vertices.Length; i++)
            {
                b.vertices[i] = new VertexPositionColorNormalTextureTangentWeights();
                b.vertices[i].Position = a.vertices[i].Position;
                b.vertices[i].TextureCoordinate = a.vertices[i].TextureCoordinate;
                b.vertices[i].Color = a.vertices[i].Color;
                b.vertices[i].Normal = a.vertices[i].Normal;
                b.vertices[i].Tangent = a.vertices[i].Tangent;
                b.vertices[i].BiTangent = a.vertices[i].BiTangent;
                b.vertices[i].BlendIndices = a.vertices[i].BlendIndices;
                b.vertices[i].BlendWeights = a.vertices[i].BlendWeights;
            }
            b.indices = new int[a.indices.Length];
            for (int i = 0; i < a.indices.Length; i++)
            {
                //b.indices[i] = new int();
                b.indices[i] = a.indices[i];
            }
            b.modelMeshBones = new RiggedModel.RiggedModelBone[a.modelMeshBones.Length];
            for (int i = 0; i < a.modelMeshBones.Length; i++)
            {
                b.modelMeshBones[i] = new RiggedModel.RiggedModelBone();
                b.modelMeshBones[i].Name = a.modelMeshBones[i].Name;
                b.modelMeshBones[i].OffsetMatrixMg = a.modelMeshBones[i].OffsetMatrixMg;
                b.modelMeshBones[i].meshBoneIndex = a.modelMeshBones[i].meshBoneIndex;
                b.modelMeshBones[i].meshIndex = a.modelMeshBones[i].meshIndex;    // Note will probably need to be altered by mesh count of original  + model.meshes.Length;
                b.modelMeshBones[i].numberOfAssociatedWeightedVertices = a.modelMeshBones[i].numberOfAssociatedWeightedVertices; // note will probably be modifyed
                b.modelMeshBones[i].refToCorrespondingHeirarchyNode = a.modelMeshBones[i].refToCorrespondingHeirarchyNode; // not sure but this should be ok since it points to a node.
            }
        }

        /// <summary>
        /// ModifyMesh the problem with doing it this way is the texture i use will be essentially the wrong one for these specific normals.
        /// </summary>
        public void ModifyMeshAppendVisualNormalVerticesIndices(RiggedModel model, int meshindex, RiggedAlignedNormalArrows n)
        {
            var m = model.meshes[meshindex];
            var tv = m.vertices;
            var ti = m.indices;
            var ovlen = tv.Length;
            var oilen = tv.Length;
            var nv = n.vertices;
            var ni = n.indices;
            var nvlen = nv.Length + ovlen;
            var nilen = ni.Length + oilen;
            VertexPositionColorNormalTextureTangentWeights[] v = new VertexPositionColorNormalTextureTangentWeights[nvlen];
            int[] ii = new int[nilen];
            // copy vertices
            for (int j = 0; j < ovlen; j++)
            {
                v[j] = tv[j];
            }
            int j2 = 0;
            for (int j = ovlen; j < nvlen; j++)
            {
                v[j] = nv[j2];
                j2++;
            }
            // copy indices
            for (int j = 0; j < oilen; j++)
            {
                ii[j] = ti[j];
            }
            j2 = 0;
            for (int j = oilen; j < nilen; j++)
            {
                ii[j] = ni[j2];
                j2++;
            }
            // replace mesh vertices indices.
            m.vertices = v;
            m.indices = ii;
        }
    }

    public class RiggedAlignedNormalArrows
    {
        public VertexPositionColorNormalTextureTangentWeights[] vertices;
        public int[] indices;

        public Texture2D texture;

        public RiggedAlignedNormalArrows()
        {
        }

        public void CreateVisualNormalsForPrimitiveMesh(VertexPositionColorNormalTextureTangentWeights[] inVertices, int[] inIndices, Texture2D t, float thickness, float scale)
        {
            texture = t;
            int len = inVertices.Length;

            // for each vertice in the model we will make a quad composed of 4 vertices and 6 indices.
            VertexPositionColorNormalTextureTangentWeights[] nverts = new VertexPositionColorNormalTextureTangentWeights[len * 4];
            int[] nindices = new int[len * 6];

            for (int j = 0; j < len; j++)
            {
                int v = j * 4;
                int i = j * 6;
                //
                //ReCreateForwardNormalQuad(vertices[i].Position, vertices[i].Normal);
                //
                nverts[v + 0].Position = new Vector3(0f, 0f, 0f) + inVertices[j].Position;
                nverts[v + 1].Position = new Vector3(0f, -.2f * thickness, 0f) + inVertices[j].Position;
                nverts[v + 2].Position = new Vector3(0f, 0f, 0f) + inVertices[j].Position + inVertices[j].Normal * scale;
                nverts[v + 3].Position = new Vector3(0f, -.2f * thickness, 0f) + inVertices[j].Position + inVertices[j].Normal * scale;
                //
                nverts[v + 0].TextureCoordinate = new Vector2(0f, 0f);//vertices[v + 0].TextureCoordinateA;
                nverts[v + 1].TextureCoordinate = new Vector2(0f, .33f); //vertices[v + 1].TextureCoordinateA;
                nverts[v + 2].TextureCoordinate = new Vector2(1f, .0f);//vertices[v + 2].TextureCoordinateA;
                nverts[v + 3].TextureCoordinate = new Vector2(1f, .33f);//vertices[v + 3].TextureCoordinateA;
                //
                nverts[v + 0].Color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                nverts[v + 1].Color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                nverts[v + 2].Color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                nverts[v + 3].Color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                //
                nverts[v + 0].Normal = inVertices[j].Normal;
                nverts[v + 1].Normal = inVertices[j].Normal;
                nverts[v + 2].Normal = inVertices[j].Normal;
                nverts[v + 3].Normal = inVertices[j].Normal;
                //
                nverts[v + 0].BlendIndices = inVertices[j].BlendIndices;
                nverts[v + 1].BlendIndices = inVertices[j].BlendIndices;
                nverts[v + 2].BlendIndices = inVertices[j].BlendIndices;
                nverts[v + 3].BlendIndices = inVertices[j].BlendIndices;
                //
                nverts[v + 0].BlendWeights = inVertices[j].BlendWeights;
                nverts[v + 1].BlendWeights = inVertices[j].BlendWeights;
                nverts[v + 2].BlendWeights = inVertices[j].BlendWeights;
                nverts[v + 3].BlendWeights = inVertices[j].BlendWeights;

                // indices
                nindices[i + 0] = 0 + v;
                nindices[i + 1] = 1 + v;
                nindices[i + 2] = 2 + v;
                nindices[i + 3] = 2 + v;
                nindices[i + 4] = 1 + v;
                nindices[i + 5] = 3 + v;
            }
            this.vertices = nverts;
            this.indices = nindices;
        }

        public void Draw(GraphicsDevice gd, Effect effect)
        {
            effect.Parameters["TextureA"].SetValue(texture);
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, (indices.Length / 3), VertexPositionColorNormalTextureTangentWeights.VertexDeclaration);
            }
        }
    }

}
