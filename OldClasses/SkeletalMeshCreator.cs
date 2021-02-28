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
    class SkeletalMeshCreator
    {
    }

    // were gonna make this skeleton a model mesh instead as part of the model.
    public class VisualSkeleton
    {
        int workingMeshIndex = 0;
        int newBoneIndex = 0;
        bool invertNormalsOnCreation = false;

        // We will be generating the bones piecemeal for a new mesh that displays the skeleton.
        // The below will be a temporary list of named bones for this new skeletal mesh that line up with the heirarchy nodes names which have bones.
        // This will at the end of all this get turned into a array and added to this new mesh.
        // We also need to assign the weights and indices for the vertices we generate to line up with these bones,,,  (which should just be local transforms translation i believe).
        // Well be generating vertices for bone nodes as we traverse the node heirarchy.
        List<RiggedModel.RiggedModelBone> modelMeshBones = new List<RiggedModel.RiggedModelBone>();

        // This will also be converted to a array.
        List<Matrix> modelMeshGlobalTransforms = new List<Matrix>();

        List<VertexPositionColorNormalTextureTangentWeights> verticesList = new List<VertexPositionColorNormalTextureTangentWeights>();
        List<int> indicesList = new List<int>();

        VertexPositionColorNormalTextureTangentWeights[] vertices;
        int[] indices;

        int roundness = 5; // this can be a input rather it should be a input.
        float lineThickness = 1.0f;
        int vertCountTotal = 0;
        int primCountTotal = 0;
        int indexCountTotal = 0;

        public Matrix World = Matrix.Identity;
        public Vector3 camUp = Vector3.Up;

        public void CreateSkeletonFromModel(RiggedModel model, int roundness, float thickness, bool invertNormalsOnCreation)
        {
            Console.WriteLine("\n CreateSkeletonFromModel \n");

            string indent = "";
            this.roundness = roundness;
            this.lineThickness = thickness;
            this.invertNormalsOnCreation = invertNormalsOnCreation;
            roundness = 3;

            var startnode = model.sceneRootNodeOfTree;
            var rmSkelMesh = CreateMesh(model, startnode.Name);

            int lastIndex = CreateModelBone(model, startnode);
            var lastActualNodeThatIsaBone = startnode;

            for (int i = 0; i < lastActualNodeThatIsaBone.children.Count; i++)
            {
                RiggedModel.RiggedModelNode child = lastActualNodeThatIsaBone.children[i];
                IterateModelHeirarchyForBones(model, child, lastActualNodeThatIsaBone, lastIndex, indent);
            }

            // Finish setting up the mesh.
            rmSkelMesh.modelMeshBones = modelMeshBones.ToArray();
            rmSkelMesh.globalShaderMatrixs = modelMeshGlobalTransforms.ToArray();
            rmSkelMesh.vertices = verticesList.ToArray();
            rmSkelMesh.indices = indicesList.ToArray();

            TallyWeightsPerBone(rmSkelMesh);

            FinishBoneSetUp(model);

            //vertices = CreateSmoothNormals(vertices, indices);
        }

        /// <summary>
        /// CreateMesh.
        /// </summary>
        public RiggedModel.RiggedModelMesh CreateMesh(RiggedModel model, string name)
        {
            // add a new mesh to the model this will be the skeleton mesh.
            RiggedModel.RiggedModelMesh skelmesh = new RiggedModel.RiggedModelMesh();
            int oldLen = model.meshes.Length;
            workingMeshIndex = oldLen;
            int newLen = model.meshes.Length + 1;
            var tmp = new RiggedModel.RiggedModelMesh[newLen];
            for (int i = 0; i < oldLen; i++)
            {
                tmp[i] = model.meshes[i];
                tmp[workingMeshIndex] = skelmesh;
            }
            model.meshes = tmp;

            var mesh = model.meshes[workingMeshIndex];
            // set up the default values for the mesh TODO this is peicemeal work here have to set up the basic stuff.
            mesh.Name = name;
            mesh.textureDiffuse = model.defaultDebugTexture;
            mesh.meshNumber = workingMeshIndex;
            mesh.IsGeneratedSkeleton = true;
            mesh.techniqueName = "SkinnedColorBaseNormEmissiveDebugBones"; //  SkinnedColorBaseNormEmissive  SkinnedColorBaseNormEmissiveDebugBones SkinedDebugModelDraw
            mesh.techniqueValue = 0;
            mesh.Note = "GeneratedSkeleton";
            return mesh;
        }

        /// <summary>
        /// Returns the index to the bone in the mesh.
        /// </summary>
        public int CreateModelBone(RiggedModel model, RiggedModel.RiggedModelNode node)
        {
            var newBone = new RiggedModel.RiggedModelBone();
            newBone.Name = node.Name;
            newBone.meshIndex = workingMeshIndex;
            newBone.meshBoneIndex = newBoneIndex;
            newBoneIndex++;
            newBone.OffsetMatrixMg = Matrix.Identity;
            node.uniqueMeshBones.Add(newBone);
            modelMeshBones.Add(newBone);
            modelMeshGlobalTransforms.Add(node.LocalTransformMg);
            model.meshes[workingMeshIndex].HasBones = true;
            return newBone.meshBoneIndex;
        }

        private void IterateModelHeirarchyForBones(RiggedModel model, RiggedModel.RiggedModelNode currentNode, RiggedModel.RiggedModelNode lastActualNodeThatIsaBone, int meshBoneIndexToLastBone, string indent)
        {
            indent += " ";
            if (currentNode.hasARealBone)
            {
                Console.Write($"\n{indent}currentNode: {currentNode.Name} pos {currentNode.CombinedTransformMg.Translation.ToStringTrimed()} IsaRealBone:  True");
                int currentIndex = CreateModelBone(model, currentNode);

                //if (lastActualNodeThatIsaBone != null)
                    CreateBonePyramidGeometry(model, lineThickness, Color.Yellow, currentNode, currentIndex, indent);

                meshBoneIndexToLastBone = currentIndex;
                lastActualNodeThatIsaBone = currentNode;
            }
            else
            {
                Console.Write($"\n{indent}currentNode: {currentNode.Name} pos {currentNode.CombinedTransformMg.Translation.ToStringTrimed()} IsaRealBone:  False");
            }
            for (int i = 0; i < currentNode.children.Count; i++)
            {
                RiggedModel.RiggedModelNode child = currentNode.children[i];
                IterateModelHeirarchyForBones(model, child, lastActualNodeThatIsaBone, meshBoneIndexToLastBone, indent);
            }
        }

        private void CreateBonePyramidGeometry(RiggedModel model, float linewidth, Color c, RiggedModel.RiggedModelNode currentNode, int meshBoneStartIndex, string indent)
        {
            var len = currentNode.uniqueMeshBones.Count;
            Console.Write($"\n{indent} > Creating geometry  # of bones {len}");

            var startPos = currentNode.CombinedTransformMg.Translation;
            var mesh = model.meshes[workingMeshIndex];

            var v0 = new VertexPositionColorNormalTextureTangentWeights();
            var v1 = new VertexPositionColorNormalTextureTangentWeights();
            var v2 = new VertexPositionColorNormalTextureTangentWeights();
            var v3 = new VertexPositionColorNormalTextureTangentWeights();

            v0.Position = startPos + new Vector3(0f, linewidth, 0f);
            v1.Position = startPos + new Vector3(0f, -linewidth, linewidth);
            v2.Position = startPos + new Vector3(linewidth, -linewidth, -linewidth);
            v3.Position = startPos + new Vector3(-linewidth, -linewidth, -linewidth);

            v0.Color = c.ToVector4();
            v1.Color = c.ToVector4();
            v2.Color = c.ToVector4();
            v3.Color = c.ToVector4();

            v0.BlendIndices = new Vector4(meshBoneStartIndex, 0, 0, 0);
            v1.BlendIndices = new Vector4(meshBoneStartIndex, 0, 0, 0);
            v2.BlendIndices = new Vector4(meshBoneStartIndex, 0, 0, 0);
            v3.BlendIndices = new Vector4(meshBoneStartIndex, 0, 0, 0);

            v0.BlendWeights = new Vector4(1.0f, 0, 0, 0);
            v1.BlendWeights = new Vector4(1.0f, 0, 0, 0);
            v2.BlendWeights = new Vector4(1.0f, 0, 0, 0);
            v3.BlendWeights = new Vector4(1.0f, 0, 0, 0);

            int vs = verticesList.Count;

            int vi0 = vs + 0;
            int vi1 = vs + 1;
            int vi2 = vs + 2;
            int vi3 = vs + 3;

            int si = indicesList.Count;

            int i0 = vs + vi0;  int i1 = vs + vi1; int i2 = vs + vi2;
            int i3 = vs + vi0; int i4 = vs + vi2; int i5 = vs + vi3;
            int i6 = vs + vi0; int i7 = vs + vi3; int i8 = vs + vi1;
            int i9 = vs + vi1; int i10 = vs + vi2; int i11 = vs + vi3;

            v0.Normal = CrossProduct3d(v0.Position, v1.Position, v2.Position);
            v1.Normal = CrossProduct3d(v0.Position, v2.Position, v3.Position);
            v2.Normal = CrossProduct3d(v0.Position, v3.Position, v1.Position);
            v3.Normal = CrossProduct3d(v1.Position, v2.Position, v3.Position);

            verticesList.Add(v0);
            verticesList.Add(v1);
            verticesList.Add(v2);
            verticesList.Add(v3);

            indicesList.Add(i0); indicesList.Add(i1); indicesList.Add(i2);
            indicesList.Add(i3); indicesList.Add(i4); indicesList.Add(i5);
            indicesList.Add(i6); indicesList.Add(i7); indicesList.Add(i8);
            indicesList.Add(i9); indicesList.Add(i10); indicesList.Add(i11);
        }

        ///// <summary>   ERROR GETTING POSITIONAL NANS
        ///// 
        ///// </summary>
        //private void CreateBoneLineGeometry(float linewidth, Color c, RiggedModel.RiggedModelNode startNode, int meshBoneStartIndex, RiggedModel.RiggedModelNode endNode, int meshBoneEndIndex, string indent)
        //{
        //    Vector3 startPos = startNode.CombinedTransformMg.Translation;
        //    Vector3 endPos = endNode.CombinedTransformMg.Translation;

        //    Random rnd = new Random();
        //    float rx = rnd.Next(0, 100) * .01f;
        //    float ry = rnd.Next(0, 100) * .01f;
        //    float rz = rnd.Next(0, 100) * .01f;

        //    //startPos = new Vector3(0f, 0f, 0f);
        //    //endPos = new Vector3(rx, ry, rz);

        //    // prevent nan.
        //    if (endPos == startPos)
        //        endPos = startPos + new Vector3(0, 0f, 1f);

        //    Console.Write($"\n{indent}lastBoneNode: {startNode.Name} pos {startPos.ToStringTrimed()} >  to currentBoneNode:  {endNode.Name}  pos {endPos.ToStringTrimed()}");
        //    indent += " ";

        //    // int roundness = 5; // this can be a input rather it should be a input.
        //    int vertCountTemp = roundness * 2 + 2;
        //    int primCountTemp = roundness * 2 * 2;
        //    int indexCountTemp = primCountTemp * 3;

        //    int vertCountTotalOld = vertCountTotal;
        //    int primCountTotalOld = primCountTotal;
        //    int indexCountTotalOld = indexCountTotal;

        //    vertCountTotal += vertCountTemp;
        //    primCountTotal += primCountTemp;
        //    indexCountTotal += indexCountTemp;

        //    VertexPositionColorNormalTextureTangentWeights[] verticesTemp = new VertexPositionColorNormalTextureTangentWeights[vertCountTemp];
        //    int[] indicesTemp = new int[indexCountTemp];

        //    int startVertOffset = 0;
        //    int endVertsOffset = roundness;

        //    float rotinc = (float)(Math.PI * 2 / (float)roundness);
        //    int v0 = startVertOffset;
        //    int v1 = endVertsOffset;
        //    for (int i = 0; i < roundness; i++)
        //    {
        //        //var normDir = Vector3.Normalize(endPos - startPos);
        //        var normDir = new Vector3(0f, 0f, 1f);
        //        //var world = Matrix.CreateFromAxisAngle(endPos - startPos, rotinc * i);
        //        //var qworld = Quaternion.CreateFromAxisAngle(endPos - startPos, rotinc * i);
        //        var qworld = Quaternion.CreateFromAxisAngle(new Vector3(0f,0f,1f) , rotinc * i);
        //        var world = Matrix.CreateFromQuaternion(qworld);
        //        //world.Translation = new Vector3(r, r, r);
        //        var sAbove = world.Up * linewidth;

        //        verticesTemp[v0].Position = sAbove + startPos;
        //        verticesTemp[v0].Color = c.ToVector4();
        //        verticesTemp[v0].BlendIndices = new Vector4(meshBoneStartIndex, 0, 0, 0);
        //        verticesTemp[v0].BlendWeights = new Vector4(1.0f, 0, 0, 0);
        //        v0++;
        //        verticesTemp[v1].Position = sAbove + endPos;
        //        verticesTemp[v1].Color = c.ToVector4();
        //        verticesTemp[v1].BlendIndices = new Vector4(meshBoneEndIndex, 0, 0, 0);
        //        verticesTemp[v1].BlendWeights = new Vector4(1.0f, 0, 0, 0);
        //        v1++;
        //    }
        //    // cap end centers themselves.
        //    verticesTemp[v1].Position = startPos;
        //    verticesTemp[v1].Color = c.ToVector4();
        //    verticesTemp[v1].BlendIndices = new Vector4(meshBoneStartIndex, 0, 0, 0);
        //    verticesTemp[v1].BlendWeights = new Vector4(1.0f, 0, 0, 0);
        //    v1++;
        //    verticesTemp[v1].Position = endPos;
        //    verticesTemp[v1].Color = c.ToVector4();
        //    verticesTemp[v1].BlendIndices = new Vector4(meshBoneEndIndex, 0, 0, 0);
        //    verticesTemp[v1].BlendWeights = new Vector4(1.0f, 0, 0, 0);
        //    v1++;



        //    // index
        //    v0 = startVertOffset;
        //    v1 = endVertsOffset;
        //    //  v0 -- v1
        //    //   a  /   b
        //    //  v2 -- v3
        //    int index = 0;
        //    for (int i = 0; i < roundness; i++)
        //    {
        //        var v2 = v0 + 1;
        //        if (v2 >= roundness)
        //            v2 = 0;
        //        var v3 = v1 + 1;
        //        if (v3 >= roundness * 2)
        //            v3 = roundness;
        //        //___

        //        indicesTemp[index] = v0; index++;
        //        indicesTemp[index] = v2; index++;
        //        indicesTemp[index] = v1; index++;

        //        indicesTemp[index] = v1; index++;
        //        indicesTemp[index] = v2; index++;
        //        indicesTemp[index] = v3; index++;

        //        // end caps.

        //        // start side.
        //        indicesTemp[index] = v0; index++;
        //        indicesTemp[index] = v2; index++;
        //        indicesTemp[index] = roundness * 2; index++;

        //        // end side.
        //        indicesTemp[index] = v1; index++;
        //        indicesTemp[index] = v3; index++;
        //        indicesTemp[index] = roundness * 2 + 1; index++;

        //        //___
        //        v0++;
        //        v1++;
        //    }

        //    VertexPositionColorNormalTextureTangentWeights[] verticesNew = new VertexPositionColorNormalTextureTangentWeights[vertCountTotal];
        //    int[] indicesNew = new int[indexCountTotal];

        //    // copy old to new.
        //    for (int v = 0; v < vertCountTotalOld; v++)
        //    {
        //        verticesNew[v] = vertices[v];
        //    }
        //    for (int i = 0; i < indexCountTotalOld; i++)
        //    {
        //        indicesNew[i] = indices[i];
        //    }
        //    // copy in these new temp vert indice to the new total
        //    int vi = 0;
        //    for (int v = vertCountTotalOld; v < vertCountTotal; v++)
        //    {
        //        verticesNew[v] = verticesTemp[vi];
        //        vi++;
        //    }
        //    int ti = 0;
        //    for (int i = indexCountTotalOld; i < indexCountTotal; i++)
        //    {
        //        indicesNew[i] = indicesTemp[ti];
        //        ti++;
        //    }
        //    // set it.
        //    vertices = verticesNew;
        //    indices = indicesNew;
        //}

        /**/


        ///// <summary>   ERROR GETTING POSITIONAL NANS
        ///// 
        ///// </summary>
        //private void CreateBoneLineGeometry(float linewidth, Color c, RiggedModel.RiggedModelNode startNode, int meshBoneStartIndex, RiggedModel.RiggedModelNode endNode, int meshBoneEndIndex, string indent)
        //{
        //    Vector3 startPos = startNode.CombinedTransformMg.Translation;
        //    Vector3 endPos = endNode.CombinedTransformMg.Translation;

        //    // prevent nan.
        //    if (endPos == startPos)
        //        endPos = startPos + new Vector3(0, 0f, 1f);

        //    Console.Write($"\n{indent}lastBoneNode: {startNode.Name} pos {startPos.ToStringTrimed()} >  to currentBoneNode:  {endNode.Name}  pos {endPos.ToStringTrimed()}");
        //    indent += " ";

        //    // int roundness = 5; // this can be a input rather it should be a input.
        //    int vertCountTemp = roundness * 2 + 2;
        //    int primCountTemp = roundness * 2 * 2;
        //    int indexCountTemp = primCountTemp * 3;

        //    int vertCountTotalOld = vertCountTotal;
        //    int primCountTotalOld = primCountTotal;
        //    int indexCountTotalOld = indexCountTotal;

        //    vertCountTotal += vertCountTemp;
        //    primCountTotal += primCountTemp;
        //    indexCountTotal += indexCountTemp;

        //    VertexPositionColorNormalTextureTangentWeights[] verticesTemp = new VertexPositionColorNormalTextureTangentWeights[vertCountTemp];
        //    int[] indicesTemp = new int[indexCountTemp];

        //    int startVertOffset = 0;
        //    int endVertsOffset = roundness;

        //    float rotinc = (float)(Math.PI * 2 / (float)roundness);
        //    int v0 = startVertOffset;
        //    int v1 = endVertsOffset;
        //    for (int i = 0; i < roundness; i++)
        //    {
        //        var normDir = Vector3.Normalize(endPos - startPos);
        //        //var world = Matrix.CreateFromAxisAngle(endPos - startPos, rotinc * i);
        //        var qworld = Quaternion.CreateFromAxisAngle(endPos - startPos, rotinc * i);
        //        var world = Matrix.CreateFromQuaternion(qworld);
        //        var sAbove = world.Up * linewidth;

        //        verticesTemp[v0].Position = sAbove + startPos;
        //        verticesTemp[v0].Color = c.ToVector4();
        //        verticesTemp[v0].BlendIndices = new Vector4(meshBoneStartIndex, 0, 0, 0);
        //        verticesTemp[v0].BlendWeights = new Vector4(1.0f, 0, 0, 0);
        //        v0++;
        //        verticesTemp[v1].Position = sAbove + endPos;
        //        verticesTemp[v1].Color = c.ToVector4();
        //        verticesTemp[v1].BlendIndices = new Vector4(meshBoneEndIndex, 0, 0, 0);
        //        verticesTemp[v1].BlendWeights = new Vector4(1.0f, 0, 0, 0);
        //        v1++;
        //    }
        //    // cap end centers themselves.
        //    verticesTemp[v1].Position = startPos;
        //    verticesTemp[v1].Color = c.ToVector4();
        //    verticesTemp[v1].BlendIndices = new Vector4(meshBoneStartIndex, 0, 0, 0);
        //    verticesTemp[v1].BlendWeights = new Vector4(1.0f, 0, 0, 0);
        //    v1++;
        //    verticesTemp[v1].Position = endPos;
        //    verticesTemp[v1].Color = c.ToVector4();
        //    verticesTemp[v1].BlendIndices = new Vector4(meshBoneEndIndex, 0, 0, 0);
        //    verticesTemp[v1].BlendWeights = new Vector4(1.0f, 0, 0, 0);
        //    v1++;



        //    // index
        //    v0 = startVertOffset;
        //    v1 = endVertsOffset;
        //    //  v0 -- v1
        //    //   a  /   b
        //    //  v2 -- v3
        //    int index = 0;
        //    for (int i = 0; i < roundness; i++)
        //    {
        //        var v2 = v0 + 1;
        //        if (v2 >= roundness)
        //            v2 = 0;
        //        var v3 = v1 + 1;
        //        if (v3 >= roundness * 2)
        //            v3 = roundness;
        //        //___

        //        indicesTemp[index] = v0; index++;
        //        indicesTemp[index] = v2; index++;
        //        indicesTemp[index] = v1; index++;

        //        indicesTemp[index] = v1; index++;
        //        indicesTemp[index] = v2; index++;
        //        indicesTemp[index] = v3; index++;

        //        // end caps.

        //        // start side.
        //        indicesTemp[index] = v0; index++;
        //        indicesTemp[index] = v2; index++;
        //        indicesTemp[index] = roundness * 2; index++;

        //        // end side.
        //        indicesTemp[index] = v1; index++;
        //        indicesTemp[index] = v3; index++;
        //        indicesTemp[index] = roundness * 2 + 1; index++;

        //        //___
        //        v0++;
        //        v1++;
        //    }

        //    VertexPositionColorNormalTextureTangentWeights[] verticesNew = new VertexPositionColorNormalTextureTangentWeights[vertCountTotal];
        //    int[] indicesNew = new int[indexCountTotal];

        //    // copy old to new.
        //    for (int v = 0; v < vertCountTotalOld; v++)
        //    {
        //        verticesNew[v] = vertices[v];
        //    }
        //    for (int i = 0; i < indexCountTotalOld; i++)
        //    {
        //        indicesNew[i] = indices[i];
        //    }
        //    // copy in these new temp vert indice to the new total
        //    int vi = 0;
        //    for (int v = vertCountTotalOld; v < vertCountTotal; v++)
        //    {
        //        verticesNew[v] = verticesTemp[vi];
        //        vi++;
        //    }
        //    int ti = 0;
        //    for (int i = indexCountTotalOld; i < indexCountTotal; i++)
        //    {
        //        indicesNew[i] = indicesTemp[ti];
        //        ti++;
        //    }
        //    // set it.
        //    vertices = verticesNew;
        //    indices = indicesNew;
        //}


        public void TallyWeightsPerBone(RiggedModel.RiggedModelMesh rmSkelMesh)
        {
            var len = rmSkelMesh.vertices.Length;
            for (int vi = 0; vi < len; vi++)
            {
                float blendIndice0 = rmSkelMesh.vertices[vi].BlendIndices.X;
                if (rmSkelMesh.vertices[vi].BlendWeights.X > 0f)
                    rmSkelMesh.modelMeshBones[(int)blendIndice0].numberOfAssociatedWeightedVertices++;

                float blendIndice1 = rmSkelMesh.vertices[vi].BlendIndices.Y;
                if (rmSkelMesh.vertices[vi].BlendWeights.Y > 0f)
                    rmSkelMesh.modelMeshBones[(int)blendIndice1].numberOfAssociatedWeightedVertices++;

                float blendIndice2 = rmSkelMesh.vertices[vi].BlendIndices.Z;
                if (rmSkelMesh.vertices[vi].BlendWeights.Z > 0f)
                    rmSkelMesh.modelMeshBones[(int)blendIndice2].numberOfAssociatedWeightedVertices++;

                float blendIndice3 = rmSkelMesh.vertices[vi].BlendIndices.W;
                if (rmSkelMesh.vertices[vi].BlendWeights.W > 0f)
                    rmSkelMesh.modelMeshBones[(int)blendIndice3].numberOfAssociatedWeightedVertices++;
            }
        }

        public void FinishBoneSetUp(RiggedModel model)
        {
            var len = model.meshes.Length;
            for (int i = 0; i < len; i++)
            {
                var mesh = model.meshes[i];
                var bones = mesh.modelMeshBones;
                for (int j = 0; j < bones.Length; j++)
                {
                    var bone = bones[j];
                    var node = ModelSearchIterateNodeTreeForNameGetRefToNode(bone.Name, model.sceneRootNodeOfTree);
                    if (node != null)
                    {
                        bone.refToCorrespondingHeirarchyNode = node;
                    }
                }
            }
        }

        public static Vector3 CrossProduct3d(Vector3 a, Vector3 b, Vector3 c)
        {
            return new Vector3
                (
                ((b.Y - a.Y) * (c.Z - b.Z)) - ((c.Y - b.Y) * (b.Z - a.Z)),
                ((b.Z - a.Z) * (c.X - b.X)) - ((c.Z - b.Z) * (b.X - a.X)),
                ((b.X - a.X) * (c.Y - b.Y)) - ((c.X - b.X) * (b.Y - a.Y))
                );
        }

        /// <summary>
        /// Searches the model for the name of the node if found it returns the model node if not it returns null.
        /// </summary>
        private static RiggedModel.RiggedModelNode ModelSearchIterateNodeTreeForNameGetRefToNode(string name, RiggedModel.RiggedModelNode node)
        {
            RiggedModel.RiggedModelNode result = null;
            if (node.Name == name)
                result = node;
            if (result == null && node.children.Count > 0)
            {
                for (int i = 0; i < node.children.Count; i++)
                {
                    var res = ModelSearchIterateNodeTreeForNameGetRefToNode(name, node.children[i]);
                    if (res != null)
                    {
                        // set result and break if the named node was found
                        result = res;
                        i = node.children.Count;
                    }
                }
            }
            return result;
        }


        VertexPositionColorNormalTextureTangentWeights[] CreateSmoothNormals(VertexPositionColorNormalTextureTangentWeights[] vertices, int[] indexs)
        {
            // For each vertice we must calculate the surrounding triangles normals, average them and set the normal.
            int tvertmultiplier = 3;
            int triangles = (int)(indexs.Length / tvertmultiplier);
            for (int currentTestedVerticeIndex = 0; currentTestedVerticeIndex < vertices.Length; currentTestedVerticeIndex++)
            {
                Vector3 sum = Vector3.Zero;
                float total = 0;
                for (int t = 0; t < triangles; t++)
                {
                    int tvstart = t * tvertmultiplier;
                    int tindex0 = tvstart + 0;
                    int tindex1 = tvstart + 1;
                    int tindex2 = tvstart + 2;
                    var vindex0 = indices[tindex0];
                    var vindex1 = indices[tindex1];
                    var vindex2 = indices[tindex2];
                    if (vindex0 == currentTestedVerticeIndex || vindex1 == currentTestedVerticeIndex || vindex2 == currentTestedVerticeIndex)
                    {
                        var n0 = (vertices[vindex1].Position - vertices[vindex0].Position) * 10f; // * 10 math artifact avoidance.
                        var n1 = (vertices[vindex2].Position - vertices[vindex1].Position) * 10f;
                        var cnorm = Vector3.Cross(n0, n1);
                        sum += cnorm;
                        total += 1;
                    }
                }
                if (total > 0)
                {
                    var averagednormal = sum / total;
                    averagednormal.Normalize();
                    if (invertNormalsOnCreation)
                        averagednormal = -averagednormal;
                    vertices[currentTestedVerticeIndex].Normal = averagednormal;
                }
            }
            return vertices;
        }
    }

}
