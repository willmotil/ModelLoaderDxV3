using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;



/*  TODO 
// Lots to do still and think about.
 */

/// <summary>
/// https://github.com/willmotil/MonoGameUtilityClasses
/// </summary>
namespace AssimpLoaderPbrDx
{
    /// <summary>
    /// The rigged model  i stuffed the classes the rigged model uses in it as nested classes just to keep it all together.
    /// I don't really see anything else using these classes anyways they are really just specific to the model.
    /// Even the loader should go out of scope when its done loading a model and then even it is just going to be a conversion tool.
    /// After i make a content reader and writer for the model class there will be no need for the loader except to change over new models.
    /// However you don't really have to have it in xnb form at all you could use it as is but it does a lot of processing so... meh...
    /// 
    /// Structural and Operation layout.
    /// 
    /// The model is mesh based.
    /// 1) The model.sceneRootNodeOfTree holds the node tree animations update that tree.
    /// 2) The nodes hold a array of bones uniqueMeshBones these have offsets that maybe unique to each mesh so more then one bone for the same named node may exist per mesh.
    /// 3) The nodes also keep a reference to the coresponding parent heirarchy node found in the linked node tree see #1 and its child nodes see #1
    /// 4) The model.meshes are drawn in turn 
    /// 5) Model meshes hold a list of bone nodes unique to the mesh a reference to a mesh nodebone that maybe generated to handle specific types of animations typically bone 0 to the mesh.
    /// 6) A mesh bone holds offset transforms that re-aligns displaced meshes (typically done in modeling programs) to the model world.
    /// 7) A mesh bone in the list of bone nodes has a refToCorrespondingHeirarchyNode which is in the tree see #1 
    ///         (this is primarily used for debuging editing not to run the model).
    /// 8) A mesh also keeps its own list of globalFinalTransforms these are final combinations of inverse bind pose matrices and heirarchial skeletal node tree transformations.
    /// 9) Vertices unique to the mesh are linked to the index of these global final bone transforms of which are set to the shader per frame.
    /// 10) The linking is done when loaded by assignment of the index to the vertice.BoneIndice aka weights 
    ///         (which maybe offset by +1 to facilitate the generated mesh bone processed in the loader )
    /// 10.5) The generated mesh bone is not part of standard bone data it facilitates special handling of simple mesh based animations 
    ///         (blender as far as i can tell so far only uses it, but is a efficient idea).
    /// 11) The update method iterates the transformation tree and combines parent to child transforms, 
    ///         unique inverse bind pose bone transforms are accounted for and the final value then is set to the globals per mesh.
    /// 12) The RiggedAnimationNodes hold a reference to the corresponding heirarchial tree node (see #1) RiggedNodes directly.
    /// 13) All animations nodes are named to correspond to tree nodes they in turn have a list of time nodes for rotation scaling and position.
    /// 14) These time frame key nodes are interpolated between as time is traversed across the animation duration then set to the node transform (see #1).
    /// 15) In summary the model is processed by the tree for updating such that it is in that way heirarchially skeletal based. 
    ///       However it is drawn per mesh such that it is visually and for practical purposes mesh based.
    /// 
    /// </summary>
    public class RiggedModel
    {
        #region Members

        public bool consoleDebug = true;

        public string Name { get; set; }
        public static Effect effect;
        public bool useDefaultDebugTexture = false;
        public Texture2D defaultDebugTexture;
        public int maxGlobalBones = 128; // 78       
        public RiggedModelMesh[] meshes;
        public RiggedModelNode sceneRootNodeOfTree; // The actual model root node the base node of the model from here we can locate any node in the chain.
        public float ScaleCoefficent = 1.0f;

        public List<Texture2D> textures = new List<Texture2D>();

        public int unknownTextureIs = -1; // used by the loader.
        public const int UnknownTextureIsOcclusionRoughnessMetal = 1;  // hack to manually tell the loader to set the metal rough texture that assimp doesn't read properly.

        // animations
        public List<RiggedAnimation> animations = new List<RiggedAnimation>();
        int currentAnimation = 0;
        public float animationSpeedMultiplier = 1.0f;
        public bool animationRunning = false;
        bool loopAnimation = true;
        float timeStart = 0f;
        public float currentAnimationFrameTime = 0;
        public float overrideAnimationFrameTime = -1; // mainly for testing to step thru each frame.

        public Matrix world;
        public Vector4 materialColor;
        public static Matrix view;
        public static Matrix projection;
        public static Vector3 camPos;
        public static Vector3 lightPosition1;
        public static int numberOfLightsInUse = 4;
        public static Vector3[] lights = new Vector3[4];
        
        public static TextureCube textureCubeMap;

        public static int TestValue1 =0;
        public static int TestValue2 = 0;
        public static int TestValue3 = 0;


        private Vector4 defaultAoMetalRoughness = new Vector4(.03f, 0.03f, 0.01f, 0f); //new Vector4(1f, 0.99f, 0.01f, 0f); // (1f,.97f,.03f,0f) defaults to no occulusion, rough material, dielectric, na.

        public float DefaultAmbientOcclusion { set { defaultAoMetalRoughness.X = value; } get { return defaultAoMetalRoughness.X; } }
        public float DefaultRoughness { set { defaultAoMetalRoughness.Y = value; } get { return defaultAoMetalRoughness.Y; } }
        public float DefaultMetalic { set { defaultAoMetalRoughness.Z = value; } get { return defaultAoMetalRoughness.Z; } }

        #endregion

        #region Constructor and methods

        /// <summary>
        /// Instantiates the model object and the boneShaderFinalMatrix array setting them all to identity.
        /// </summary>
        public RiggedModel()
        {
            lights[0] = new Vector3();
            lights[1] = new Vector3();
            lights[2] = new Vector3();
            lights[3] = new Vector3();
        }

        public void InitialBindPose()
        {
            for (int i = 0; i < meshes.Length; i++)
            {
                for(int j = 0; j < meshes[i].globalShaderMatrixs.Length; j++)
                {
                    meshes[i].globalShaderMatrixs[j] = Matrix.Identity;
                }
            }
        }

        public static TextureCube SetCubeFaces(GraphicsDevice gd, int size, TextureCube map, Texture2D cmLeft, Texture2D cmBottom, Texture2D cmBack, Texture2D cmRight, Texture2D cmTop, Texture2D cmFront)
        {
                 return SetIBLfaces(gd, 256, RiggedModel.textureCubeMap, cmLeft, cmBottom, cmBack, cmRight, cmTop, cmFront);
        }
        public static TextureCube SetIBLfaces(GraphicsDevice gd, int size, TextureCube map, Texture2D textureNegativeX, Texture2D textureNegativeY, Texture2D textureNegativeZ, Texture2D texturePositiveX, Texture2D texturePositiveY, Texture2D texturePositiveZ)
        {
            if(map == null)
            {
                map = new TextureCube(gd, size, true, SurfaceFormat.Color);
            }
            SetIBLface(map, textureNegativeX, CubeMapFace.NegativeX);
            SetIBLface(map, textureNegativeY, CubeMapFace.NegativeY);
            SetIBLface(map, textureNegativeZ, CubeMapFace.NegativeZ);
            SetIBLface(map, texturePositiveX, CubeMapFace.PositiveX);
            SetIBLface(map, texturePositiveY, CubeMapFace.PositiveY);
            SetIBLface(map, texturePositiveZ, CubeMapFace.PositiveZ);
            return map;
        }
        public static void SetIBLface(TextureCube map, Texture2D t, CubeMapFace face)
        {
            for (int i = 0; i < t.LevelCount; i += 1)
            {
                // Allocations here so be mindful of when you load.
                var data = new Color[(t.Width >> i) * (t.Height >> i)];
                t.GetData(i, null, data, 0, data.Length);
                map.SetData(face, i, null, data, 0, data.Length);
            }
        }
        public static void SetIblToShader(TextureCube map)
        {
            effect.Parameters["CubeMap"].SetValue(map);
        }

        ///// <summary> Todo double check texture can be set or not.
        ///// As stated
        ///// </summary>
        //public void SetEffect(Texture2D t, Matrix world, Matrix view, Matrix projection, Vector3 lightposition)
        //{
        //    //this.effect = effect;
        //    //texture = t;
        //    this.effect.Parameters["TextureA"].SetValue(t);
        //    this.effect.Parameters["World"].SetValue(world);
        //    this.effect.Parameters["View"].SetValue(view);
        //    this.effect.Parameters["Projection"].SetValue(projection);
        //}

        public void LoadEffect(Microsoft.Xna.Framework.Content.ContentManager content, string effectFileName) // , Texture2D texture, Matrix view, Matrix projection, Vector3 lightposition
        {
            effect = content.Load<Effect>(effectFileName);
            effect.CurrentTechnique = effect.Techniques["DiffuseSkinnedLighting"];
            // set up the effect initially to change how you want the shader to behave.

            effect.Parameters["MaterialColor"].SetValue(new Vector4(1f, 1f, 1f, 1.0f));
            effect.Parameters["DefaultAoMetalRoughness"].SetValue(defaultAoMetalRoughness);

            effect.Parameters["WorldLightPosition"].SetValue(lightPosition1);
            effect.Parameters["LightColor"].SetValue(new Vector4(.99f, .99f, .99f, 1.0f));
            // set up the effect initially to change how you want the shader to behave.
            effect.Parameters["World"].SetValue(Matrix.Identity);
            effect.Parameters["View"].SetValue(view);   // just add defaults here or dont add anything.
            effect.Parameters["Projection"].SetValue(projection);
            effect.Parameters["TextureA"].SetValue(defaultDebugTexture);
        }

        /// <summary>
        /// As stated
        /// </summary>
        public void SetEffectTexture(Texture2D t)
        {
            effect.Parameters["TextureA"].SetValue(t);
        }

        public void UnloadContent()
        {
            for(int i =0; i < meshes.Length;i++)
            {
                var mesh = meshes[i];
                UnloadTexture(ref mesh.textureDiffuse);
                UnloadTexture(ref mesh.textureHeightMap);
                UnloadTexture(ref mesh.textureLightMap);
                UnloadTexture(ref mesh.textureNormalMap);
                UnloadTexture(ref mesh.textureReflectionMap);
                UnloadTexture(ref mesh.textureSpecular);
                UnloadTexture(ref mesh.textureEmissiveMap);
                UnloadTexture(ref mesh.textureUnknown);
                UnloadTexture(ref mesh.textureAmbientOcclusionOraLightMap);
                UnloadTexture(ref mesh.textureBaseColorMapPbr);
                UnloadTexture(ref mesh.textureDisplacementMap);
                UnloadTexture(ref mesh.textureMetalnessPbr);
                UnloadTexture(ref mesh.textureRoughnessPbr);
                UnloadTexture(ref mesh.textureNormalMapPrbCamera);
                UnloadTexture(ref mesh.textureShine);
            }  
        }

        public void UnloadTexture(ref Texture2D t)
        {
            if (t != null)
                if (t.IsDisposed == false)
                    if ((string)(t.Tag) == "DisposeMe")
                        t.Dispose();
        }

        #endregion

        #region Update

        /// <summary>
        /// Update
        /// </summary>
        public void Update(GameTime gameTime)
        {
            if (animationRunning)
                UpdateModelAnimations(gameTime);
            UpdateNodes(sceneRootNodeOfTree);
        }

        /// <summary>
        /// Gets the animation frame corresponding to the elapsed time for all the nodes and loads them into the model node transforms.
        /// </summary>
        private void UpdateModelAnimations(GameTime gameTime)
        {
            if (animations.Count > 0 && currentAnimation < animations.Count)
            {
                AnimationTimeLogic(gameTime);
                for (int nodeLooped = 0; nodeLooped < animations[currentAnimation].animatedNodes.Count; nodeLooped++)
                {
                    var animNodeframe = animations[currentAnimation].animatedNodes[nodeLooped];
                    var node = animNodeframe.nodeRef;
                    node.LocalTransformMg = animations[currentAnimation].Interpolate(currentAnimationFrameTime, animNodeframe, loopAnimation); // use dynamic interpolated frames.
                }
            }
        }

        /// <summary>
        /// What to do at a certain animation time.
        /// </summary>
        public void AnimationTimeLogic(GameTime gameTime)
        {
            currentAnimationFrameTime = ((float)(gameTime.TotalGameTime.TotalSeconds) - timeStart) * animationSpeedMultiplier; // *.1f; // if we want to purposely slow it.
            float animationTotalDuration = (float)animations[currentAnimation].DurationInSeconds + (float)animations[currentAnimation].DurationInSecondsAdded;

            // just for seeing a single frame lets us override the current frame.
            if (overrideAnimationFrameTime >= 0f)
                currentAnimationFrameTime = overrideAnimationFrameTime;

            // Animation time exceeds total duration.
            if (currentAnimationFrameTime > animationTotalDuration)
            {
                if (loopAnimation)  // we might be looping.
                {
                    currentAnimationFrameTime = currentAnimationFrameTime - animationTotalDuration;
                    timeStart = (float)(gameTime.TotalGameTime.TotalSeconds);
                }
                else // animation completed
                {
                    //timeStart = 0;
                    animationRunning = false;
                }
            }
        }

        /// <summary>
        /// Updates the node transforms
        /// </summary>
        private void UpdateNodes(RiggedModelNode node)
        {
            if (node.parent != null)
                node.CombinedTransformMg = node.LocalTransformMg * node.parent.CombinedTransformMg;
            else
                node.CombinedTransformMg = node.LocalTransformMg;

            // Each mesh may have it's own positional offset for bones seperate from the transform heirarchy as it may have been placed that way in the model program, the offset is needed then, which is used to re-align it before the final transform.
            // A non fancy sort of set up which some models keep, will just have a bone offset of identity however we handle that here with the added effort of drawing per mesh later on.
            // It can typically reduce the bone count per mesh and its more flexable for editing but its overall arguably a bit more work.
            for (int i = 0; i < node.uniqueMeshBones.Count; i++)
            {
                var nodeUniqueBoneForMesh = node.uniqueMeshBones[i];
                meshes[nodeUniqueBoneForMesh.meshIndex].globalShaderMatrixs[nodeUniqueBoneForMesh.meshBoneIndex] = nodeUniqueBoneForMesh.OffsetMatrixMg * node.CombinedTransformMg;
            }

            foreach (RiggedModelNode n in node.children) // call children
                UpdateNodes(n);
        }


        #endregion

        #region Draws

        /// <summary>
        /// Sets the global final bone matrices to the shader and draws it.
        /// </summary>
        public void Draw(GraphicsDevice gd, Matrix world)
        {
            for(int meshindex =0; meshindex < meshes.Length; meshindex ++)
            {
                if (meshindex >= 0)
                {
                    RiggedModelMesh m = meshes[meshindex];

                    if (m.IsGeneratedSkeleton)
                    {
                        gd.RasterizerState = RasterizerState.CullNone;
                    }

                    effect.Parameters["Bones"].SetValue(m.globalShaderMatrixs);
                    Texture2D t2d;
                    if (m.textureDiffuse != null)
                        t2d = m.textureDiffuse;
                    else
                    {
                        if (useDefaultDebugTexture)
                            t2d = defaultDebugTexture;
                        else
                            t2d = null;
                    }

                    if (t2d != null)
                    {
                        if (m.vertices.Length > 0)
                        {
                            effect.Parameters["testValue1"].SetValue((int)TestValue1);
                            effect.Parameters["testValue2"].SetValue((int)TestValue2);
                            effect.Parameters["testValue3"].SetValue((int)TestValue3);

                            //effect.CurrentTechnique = effect.Techniques["SkinnedColorBaseMetalRoughEmissiveNormalMap"];  // this one works for animations but not all the other ones do aka they arent properly aligned or selsected.
                            //effect.CurrentTechnique = effect.Techniques["SkinnedColorBaseNormEmissive"];  // quick override test delete me.
                            
                            effect.CurrentTechnique = effect.Techniques[m.techniqueName];
                            effect.Parameters["TextureA"].SetValue(t2d);
                            effect.Parameters["TextureNormalMap"].SetValue(m.textureNormalMap);
                            effect.Parameters["TextureMetalRoughness"].SetValue(m.textureRoughnessPbr);
                            effect.Parameters["TextureEmissive"].SetValue(m.textureEmissiveMap);
                            effect.Parameters["World"].SetValue(world);
                            effect.Parameters["View"].SetValue(view);
                            effect.Parameters["Projection"].SetValue(projection);
                            effect.Parameters["CameraPosition"].SetValue(camPos);
                            effect.Parameters["WorldLightPosition"].SetValue(lightPosition1);
                            effect.Parameters["NumLights"].SetValue((int)numberOfLightsInUse);
                            effect.Parameters["Lights"].SetValue(lights);
                            effect.Parameters["MaterialColor"].SetValue(m.ColorDiffuse);
                            effect.Parameters["DefaultAoMetalRoughness"].SetValue(defaultAoMetalRoughness);

                            var e = effect.CurrentTechnique;
                            e.Passes[0].Apply();
                            gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, m.vertices, 0, m.vertices.Length, m.indices, 0, m.indices.Length / 3, VertexPositionColorNormalTextureTangentWeights.VertexDeclaration);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// For a special case were a person may wish to manipulate each mesh by name. Not the most elegant way to do it.
        /// </summary>
        public void DrawMesh(GraphicsDevice gd, string meshNodeName, Matrix world, Matrix view, Matrix projection)
        {
            int index = -1;
            for(int i =0; i < meshes.Length;i++)
            {
                if (meshes[i].nodeRefContainingMeshNodeAnimation.Name == meshNodeName)
                {
                    index = i;
                    i = meshes.Length;
                }
            }
            if (index > -1)
                DrawMesh(gd, index, world, view, projection);
        }

        /// <summary>
        /// draws the mesh by the index.
        /// </summary>
        public void DrawMesh(GraphicsDevice gd, int meshIndex, Matrix world, Matrix view, Matrix projection)
        {
            var m = meshes[meshIndex];

            if (m.IsGeneratedSkeleton)
                gd.RasterizerState = RasterizerState.CullNone;

            effect.Parameters["Bones"].SetValue(m.globalShaderMatrixs);

            Texture2D t2d;
            if (m.textureDiffuse != null)
                t2d = m.textureDiffuse;
            else
            {
                if (useDefaultDebugTexture)
                    t2d = defaultDebugTexture;
                else
                    t2d = null;
            }

            if (t2d != null)
            {
                if (m.vertices.Length > 0)
                {
                    effect.Parameters["TextureA"].SetValue(t2d);
                    effect.Parameters["World"].SetValue(world);
                    effect.Parameters["View"].SetValue(view);
                    effect.Parameters["Projection"].SetValue(projection);
                    effect.Parameters["WorldLightPosition"].SetValue(lightPosition1);
                    var e = effect.CurrentTechnique;
                    e.Passes[0].Apply();
                    gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, m.vertices, 0, m.vertices.Length, m.indices, 0, m.indices.Length / 3, VertexPositionColorNormalTextureTangentWeights.VertexDeclaration);
                }
            }
        }

        /// <summary>
        /// Sets the global final bone matrices to the shader and draws it.
        /// </summary>
        public string MeshBoneDebugDraw(GraphicsDevice gd, Matrix world, bool showOnlySelectedMesh, int meshIdToShow, int boneIdToShow)
        {
            string technameResult = "";
            for (int i = 0; i < meshes.Length; i++)
            {
                if ((showOnlySelectedMesh && i == meshIdToShow) || showOnlySelectedMesh == false)
                {
                    var m = meshes[i];
                    effect.Parameters["Bones"].SetValue(m.globalShaderMatrixs);

                    Texture2D t2d;
                    if (m.textureDiffuse != null)
                        t2d = m.textureDiffuse;
                    else
                    {
                        if (useDefaultDebugTexture)
                            t2d = defaultDebugTexture;
                        else
                            t2d = null;
                    }

                    if (t2d != null)
                    {
                        effect.Parameters["TextureA"].SetValue(t2d);
                        effect.CurrentTechnique = effect.Techniques["SkinnedColorBaseNormEmissiveDebugBones"];
                        effect.Parameters["boneIdToSee"].SetValue((float)boneIdToShow);
                        if (i == meshIdToShow)
                        {
                            //effect.CurrentTechnique = effect.Techniques["SkinnedColorBaseNormEmissiveDebugBones"];//effect.Techniques[m.techniqueName]; SkinnedColorBaseNormEmissiveDebugBones
                            //effect.Parameters["boneIdToSee"].SetValue((float)boneIdToShow);
                            effect.Parameters["MaterialColor"].SetValue(new Vector4(.5f, 1.0f, .5f, 1.0f));
                        }
                        else
                        {
                            effect.Parameters["MaterialColor"].SetValue(new Vector4(.5f, .5f, .5f, 1.0f));
                            //effect.CurrentTechnique = effect.Techniques[m.techniqueName];
                        }
                        technameResult = effect.Techniques[0].Name;
                        effect.Parameters["World"].SetValue(world);
                        effect.Parameters["View"].SetValue(view);
                        effect.Parameters["WorldLightPosition"].SetValue(lightPosition1);
                        var e = effect.CurrentTechnique;
                        e.Passes[0].Apply();
                        gd.DrawUserIndexedPrimitives(Microsoft.Xna.Framework.Graphics.PrimitiveType.TriangleList, m.vertices, 0, m.vertices.Length, m.indices, 0, m.indices.Length / 3, VertexPositionColorNormalTextureTangentWeights.VertexDeclaration);
                    }
                }
            }
            return technameResult;
        }

        #endregion

        #region Animation stuff

        public int CurrentPlayingAnimationIndex
        {
            get { return currentAnimation; }
            set {
                var n = value;
                if (n >= animations.Count)
                    n = 0;
                currentAnimation = n;
            }
        }

        public void BeginAnimation(int animationIndex, GameTime gametime)
        {
            timeStart = (float)gametime.TotalGameTime.TotalSeconds;
            currentAnimation = animationIndex;
            animationRunning = true;
        }

        public void StopAnimation()
        {
            animationRunning = false;
        }

        #endregion

        /// <summary>
        /// This represents a node that is a bone it contains a offset and more then one maybe linked to the same node. 
        /// There may only be one of each named bone per mesh.
        /// </summary>
        public class RiggedModelBone
        {
            public string Name { get; set; }
            /// <summary>
            /// This is a index pointing to which mesh in the model.meshes that this bones and importantly its offsets apply to.
            /// </summary>
            public int meshIndex = -1;
            /// <summary>
            /// This is the meshes bone index in its array and the index to the global shader bone index which the vertices weights for the mesh are tied to.
            /// </summary>
            public int meshBoneIndex = -1;
            public int numberOfAssociatedWeightedVertices = 0;

            /// <summary>
            /// The inverse offset takes one from model space to bone space to say it will have a position were the bone is in the world.
            /// The model node doesn't normally have or need a bind pose matrix however since ill need one later ill just return the inverted offset matrix.
            /// It is of the world space transform type from model space.
            /// </summary>
            public Matrix InvOffsetMatrixMg { get { return Matrix.Invert(OffsetMatrixMg); } }
            /// <summary>
            /// Typically a chain of local transforms from bone to bone allow one bone to build off the next. 
            /// This is the inverse bind pose position and orientation.
            /// The multiplication of this value by a full transformation chain at that specific node reveals the difference of its current model space orientations to its bind pose orientations.
            /// This is a tranformation from world space towards model space.
            /// </summary>
            public Matrix OffsetMatrixMg { get; set; }

            /// <summary>
            /// This is mostly for debuging purposes it is only so we can directly access the heirarchy transform node from the bone.
            /// </summary>
            public RiggedModelNode refToCorrespondingHeirarchyNode;
        }

        /// <summary>
        /// A node of the rigged model is really a transform joint some are bones some aren't. These form a heirarchial linked tree structure.
        /// The nodes may link to more then one mesh meaning each bone may have more then one offset.
        /// </summary>
        public class RiggedModelNode
        {
            public string Name { get; set; }
            public RiggedModelNode parent;
            public List<RiggedModelNode> children = new List<RiggedModelNode>();

            // probably don't need most of these they are from the debug phase.
            public bool isTheRootNode = false;
            public bool hasARealBone = false; // a actual bone with a bone offset.
            public bool isANodeAlongTheBoneRoute = false; // similar to is isThisNodeTransformNecessary but can include the nodes after bones.
            public bool isThisAMeshNode = false; // is this actually a mesh node.

            /// <summary>
            /// Points to the mesh bone's that corresponds to this node bone.
            /// Here in this node we are in ... we maintain, a list of bone references, to the flat bone list, that are contained in the meshes.
            /// </summary>
            public List <RiggedModelBone> uniqueMeshBones = new List<RiggedModelBone>();

            /// <summary>
            /// The simplest one to understand this is a transformation of a specific bone in relation to the previous bone.
            /// This is a world transformation that has local properties such that parent to child builds accumulations.
            /// </summary>
            public Matrix LocalTransformMg { get; set; }
            
            /// <summary>
            /// The multiplication of transforms down the tree accumulate this value tracks those accumulations.
            /// While the local transforms affect the particular orientation of a specific bone.
            /// While blender or other apps may allow some scaling or other adjustments. Special matrices can be combined with this to offset transformations from this bone on.
            /// This is a world space transformation. The final world space transform that can be uploaded to the shader after all nodes are processed.
            /// </summary>
            public Matrix CombinedTransformMg { get; set; }
        }

        /// <summary>
        /// Models are composed of meshes each with there own textures and sets of vertices associated to them.
        /// </summary>
        public class RiggedModelMesh
        {
            public string Name = "";
            public string Note = "";
            public int meshNumber = 0;
            public bool HasBones = false;
            public bool HasMeshAnimationAttachments = false;
            /// <summary>
            /// Flags that this is generated geometry this maybe created by another class and added to the model such as the skeleton mesh for example.
            /// </summary>
            public bool IsGeneratedSkeleton = false;
            public Texture2D textureDiffuse;
            public Texture2D textureBaseColorMapPbr;
            public Texture2D textureSpecular;
            public Texture2D textureNormalMap;
            public Texture2D textureNormalMapPrbCamera;
            public Texture2D textureHeightMap;
            public Texture2D textureDisplacementMap;
            public Texture2D textureReflectionMap;
            public Texture2D textureLightMap;
            public Texture2D textureAmbientOcclusionOraLightMap;
            public Texture2D textureOpacity;
            public Texture2D textureEmissiveMap;
            public Texture2D textureRoughnessPbr;
            public Texture2D textureMetalnessPbr;
            public Texture2D textureShine;
            public Texture2D textureUnknown;

            public string techniqueName = "";
            public int techniqueValue = 0;

            public VertexPositionColorNormalTextureTangentWeights[] vertices;
            public int[] indices;
            public int NumberOfIndices { get { return indices.Length; } }
            public int NumberOfVertices { get { return vertices.Length; } }

            public bool HasPreCalculatedVertexColors = false;
            public bool HasPreCalculatedNormals = false;
            public bool HasPreCalculatedTangents = false;
            public bool HasPreCalculatedBiTangents = false;

            /// <summary>
            /// If this is not null then the mesh has a node itself that transforms it when animating. To say the model has a animation node for the mesh itself this will be treated as if it were a bone or bone 0.
            /// </summary>
            public RiggedModelNode nodeRefContainingMeshNodeAnimation;
            public string GetMeshNodeName { get { return nodeRefContainingMeshNodeAnimation.Name; } }
            public Matrix MeshNodeCombinedFinalTransformMg { get { return nodeRefContainingMeshNodeAnimation.CombinedTransformMg; } }

            /// <summary>
            /// A mesh may have different offsets between each other which are applied to the general shared transformation node heirarchy applied for a still or animating bone which can affect the skin of a mesh.
            /// </summary>
            public RiggedModelBone[] modelMeshBones;
            public Matrix[] globalShaderMatrixs;

            /// <summary>
            /// Superfluous.
            /// </summary>
            public int MaterialIndex { get; set; }
            /// <summary>
            /// Superfluous. This is not really used it is left over from assimp the material values atm are stored in place at the mesh as each mesh only uses the materials it requires.
            /// as far as i can tell and see any reasoning to do otherwise as redundant this value is left in just in case im wrong later.
            /// </summary>
            public string MaterialIndexName { get; set; }

            public Vector4 ColorAmbient { get; set; } = Vector4.One;
            public Vector4 ColorDiffuse { get; set; } = Vector4.One;
            public Vector4 ColorSpecular { get; set; } = Vector4.One;
            public Vector4 ColorEmissive { get; set; } = Vector4.One;
            public Vector4 ColorReflective { get; set; } = Vector4.One;
            public Vector4 ColorTransparent { get; set; } = Vector4.One;
            public float Opacity { get; set; } = 1.0f;
            public float Transparency { get; set; } = 0.0f;
            public float Reflectivity { get; set; } = 0.0f;
            public float Shininess { get; set; } = 0.0f;
            public float ShininessStrength { get; set; } = 1.0f;
            public float BumpScaling = 0.0f;
            public bool IsTwoSided = false;
            public bool IsPbrMaterial = false;
            public string BlendMode = "Default";
            public string ShadingMode = "Default";
            public bool HasShaders = false;
            public bool IsWireFrameEnabled = false;

            /// <summary>
            /// Defines the minimum vertices extent in each direction x y z in system coordinates.
            /// </summary>
            public Vector3 Min { get; set; }
            /// <summary>
            /// Defines the mximum vertices extent in each direction x y z in system coordinates.
            /// </summary>
            public Vector3 Max { get; set; }
            /// <summary>
            /// Defines the center mass point or average of all the vertices.
            /// </summary>
            public Vector3 Centroid { get; set; }
        }

        /// <summary>
        /// Animations.
        /// All the 'animatedNodes' are in the rigged animation and the nodes have lists of frames of animations.
        /// </summary>
        public class RiggedAnimation
        {
            public string targetNodeConsoleName = "_none_"; //"L_Hand";

            public string animationName = "";
            public double DurationInTicks = 0;
            public double TicksPerSecond = 0;
            public double DurationInSeconds = 0;
            public double DurationInSecondsAdded = 0;
            //public int TotalFrames = 0;

            private int fps = 0;

            public bool HasMorphAnimations = false;
            public bool HasMeshAnimations = false;
            public bool HasNodeAnimations = false;
            public List<RiggedAnimationNode> animatedNodes;

            /// <summary>
            /// Interpolates between animation key frames.
            /// </summary>
            public Matrix Interpolate(double animTime, RiggedAnimationNode nodeAnim, bool loopAnimation)
            {
                var durationSecs = DurationInSeconds + DurationInSecondsAdded;

                while (animTime > durationSecs)
                    animTime -= durationSecs;

                Quaternion q2 = nodeAnim.qrot[0];
                Vector3 p2 = nodeAnim.position[0];
                Vector3 s2 = nodeAnim.scale[0];
                double tq2 = nodeAnim.qrotTime[0]; 
                double tp2 = nodeAnim.positionTime[0];
                double ts2 = nodeAnim.scaleTime[0]; 
                // 
                int i1 = 0;
                Quaternion q1 = nodeAnim.qrot[i1];
                Vector3 p1 = nodeAnim.position[i1];
                Vector3 s1 = nodeAnim.scale[i1];
                double tq1 = nodeAnim.qrotTime[i1];
                double tp1 = nodeAnim.positionTime[i1];
                double ts1 = nodeAnim.scaleTime[i1];
                // 
                int qindex2 = 0; int qindex1 = 0; 
                int pindex2 = 0; int pindex1 = 0;
                int sindex2 = 0; int sindex1 = 0;
                //
                var qiat = nodeAnim.qrotTime[nodeAnim.qrotTime.Count - 1];
                if (animTime > qiat)
                {
                    tq1 = nodeAnim.qrotTime[nodeAnim.qrotTime.Count - 1];
                    q1 = nodeAnim.qrot[nodeAnim.qrot.Count - 1];
                    tq2 = nodeAnim.qrotTime[0] + durationSecs;
                    q2 = nodeAnim.qrot[0];
                    qindex1 = nodeAnim.qrot.Count - 1;
                    qindex2 = 0;
                }
                else
                {
                    //
                    for (int frame2 = nodeAnim.qrot.Count - 1; frame2 > -1; frame2--)
                    {
                        var t = nodeAnim.qrotTime[frame2];
                        if (animTime <= t)
                        {
                            //1___
                            q2 = nodeAnim.qrot[frame2];
                            tq2 = nodeAnim.qrotTime[frame2];
                            qindex2 = frame2; // for output to console only
                            //2___
                            var frame1 = frame2 - 1;
                            if (frame1 < 0)
                            {
                                frame1 = nodeAnim.qrot.Count - 1;
                                tq1 = nodeAnim.qrotTime[frame1] - durationSecs;
                            }
                            else
                            {
                                tq1 = nodeAnim.qrotTime[frame1];
                            }
                            q1 = nodeAnim.qrot[frame1];
                            qindex1 = frame1; // for output to console only
                        }
                    }
                }
                //
                var piat = nodeAnim.positionTime[nodeAnim.positionTime.Count - 1];
                if (animTime > piat)
                {
                    tp1 = nodeAnim.positionTime[nodeAnim.positionTime.Count - 1];
                    p1 = nodeAnim.position[nodeAnim.position.Count - 1];
                    tp2 = nodeAnim.positionTime[0] + durationSecs;
                    p2 = nodeAnim.position[0];
                    pindex1 = nodeAnim.position.Count - 1;
                    pindex2 = 0;
                }
                else
                {
                    for (int frame2 = nodeAnim.position.Count - 1; frame2 > -1; frame2--)
                    {
                        var t = nodeAnim.positionTime[frame2];
                        if (animTime <= t)
                        {
                            //1___
                            p2 = nodeAnim.position[frame2];
                            tp2 = nodeAnim.positionTime[frame2];
                            pindex2 = frame2; // for output to console only
                            //2___
                            var frame1 = frame2 - 1;
                            if (frame1 < 0)
                            {
                                frame1 = nodeAnim.position.Count - 1;
                                tp1 = nodeAnim.positionTime[frame1] - durationSecs;
                            }
                            else
                            {
                                tp1 = nodeAnim.positionTime[frame1];
                            }
                            p1 = nodeAnim.position[frame1];
                            pindex1 = frame1; // for output to console only
                        }
                    }
                }
                // scale
                var siat = nodeAnim.scaleTime[nodeAnim.scaleTime.Count - 1];
                if (animTime > siat)
                {
                    ts1 = nodeAnim.scaleTime[nodeAnim.scaleTime.Count - 1];
                    s1 = nodeAnim.scale[nodeAnim.scale.Count - 1];
                    ts2 = nodeAnim.scaleTime[0] + durationSecs;
                    s2 = nodeAnim.scale[0];
                    sindex1 = nodeAnim.scale.Count - 1;
                    sindex2 = 0;
                }
                else
                {
                    for (int frame2 = nodeAnim.scale.Count - 1; frame2 > -1; frame2--)
                    {
                        var t = nodeAnim.scaleTime[frame2];
                        if (animTime <= t)
                        {
                            //1___
                            s2 = nodeAnim.scale[frame2];
                            ts2 = nodeAnim.scaleTime[frame2];
                            sindex2 = frame2; // for output to console only
                            //2___
                            var frame1 = frame2 - 1;
                            if (frame1 < 0)
                            {
                                frame1 = nodeAnim.scale.Count - 1;
                                ts1 = nodeAnim.scaleTime[frame1] - durationSecs;
                            }
                            else
                            {
                                ts1 = nodeAnim.scaleTime[frame1];
                            }
                            s1 = nodeAnim.scale[frame1];
                            sindex1 = frame1; // for output to console only
                        }
                    }
                }


                float tqi = 0; 
                float tpi = 0; 
                float tsi = 0; 

                Quaternion q;
                if (qindex1 != qindex2)
                {
                    tqi = (float)GetInterpolationTimeRatio(tq1, tq2, animTime);
                    q = Quaternion.Slerp(q1, q2, tqi);
                }
                else
                {
                    tqi = (float)tq2;
                    q = q2;
                }

                Vector3 p;
                if (pindex1 != pindex2)
                {
                    tpi = (float)GetInterpolationTimeRatio(tp1, tp2, animTime);
                    p = Vector3.Lerp(p1, p2, tpi);
                }
                else
                {
                    tpi = (float)tp2;
                    p = p2;
                }

                Vector3 s;
                if (sindex1 != sindex2)
                {
                    tsi = (float)GetInterpolationTimeRatio(ts1, ts2, animTime);
                    s = Vector3.Lerp(s1, s2, tsi);
                }
                else
                {
                    tsi = (float)ts2;
                    s = s2;
                }

                ////if (targetNodeConsoleName == n.nodeName || targetNodeConsoleName == "")
                ////{
                //    Console.WriteLine("" + "AnimationTime: " + animTime.ToStringTrimed());
                //    Console.WriteLine(" q : " + " index1: " + qindex1 + " index2: " + qindex2 + " time1: " + tq1.ToStringTrimed() + "  time2: " + tq2.ToStringTrimed() + "  interpolationTime: " + tqi.ToStringTrimed() + "  quaternion: " + q.ToStringTrimed());
                //    Console.WriteLine(" p : " + " index1: " + pindex1 + " index2: " + pindex2 + " time1: " + tp1.ToStringTrimed() + "  time2: " + tp2.ToStringTrimed() + "  interpolationTime: " + tpi.ToStringTrimed() + "  position: " + p.ToStringTrimed());
                //    Console.WriteLine(" s : " + " index1: " + sindex1 + " index2: " + sindex2 + " time1: " + ts1.ToStringTrimed() + "  time2: " + ts2.ToStringTrimed() + "  interpolationTime: " + tsi.ToStringTrimed() + "  scale: " + s.ToStringTrimed());
                ////}

                //s *= .01f;

                var ms = Matrix.CreateScale(s);
                var mr = Matrix.CreateFromQuaternion(q);
                var mt = Matrix.CreateTranslation(p);
                var m =  ms * mr * mt; // srt
                return m;
            }

            public double GetInterpolationTimeRatio(double s, double e, double val)
            {
                if (val < s || val > e)
                    throw new Exception("RiggedModel.cs RiggedAnimation GetInterpolationTimeRatio the value " + val + " passed to the method must be within the start and end time. ");
                return (val - s) / (e - s);
            }
            
        }

        /// <summary>
        /// Each node contains lists for Animation frame orientations. 
        /// The initial srt transforms are copied from assimp and a interpolated orientation frame time set is built.
        /// I However keep the original s r t portions for later editing.
        /// </summary>
        public class RiggedAnimationNode
        {
            public RiggedModelNode nodeRef;
            public string nodeName = "";

            // in model tick time
            public List<double> positionTime = new List<double>();
            public List<double> scaleTime = new List<double>();
            public List<double> qrotTime = new List<double>();
            public List<Vector3> position = new List<Vector3>();
            public List<Vector3> scale = new List<Vector3>();
            public List<Quaternion> qrot = new List<Quaternion>();

            // the actual calculated interpolation orientation matrice based on time.
            //public double[] frameOrientationTimes;
            //public Matrix[] frameOrientations; // ToDo last i might not even pre calculate these at all i can forsee edge cases.
        }

    }

    /// <summary>
    /// basically a wide spectrum vertice structure.
    /// </summary>
    public struct VertexPositionColorNormalTextureTangentWeights : IVertexType
    {
        public Vector3 Position;
        public Vector4 Color;
        public Vector2 TextureCoordinate;
        public Vector3 Normal;
        public Vector3 Tangent;
        public Vector3 BiTangent;
        public Vector4 BlendIndices;
        public Vector4 BlendWeights;

        public static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
              new VertexElement(VertexElementByteOffset.PositionStartOffset(), VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
              //new VertexElement(VertexElementByteOffset.OffsetColor(), VertexElementFormat.Color, VertexElementUsage.Color, 0),
              new VertexElement(VertexElementByteOffset.OffsetVector4(), VertexElementFormat.Vector4, VertexElementUsage.Color, 0),
              new VertexElement(VertexElementByteOffset.OffsetVector2(), VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
              new VertexElement(VertexElementByteOffset.OffsetVector3(), VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
              new VertexElement(VertexElementByteOffset.OffsetVector3(), VertexElementFormat.Vector3, VertexElementUsage.Normal, 1),
              new VertexElement(VertexElementByteOffset.OffsetVector3(), VertexElementFormat.Vector3, VertexElementUsage.Normal, 2),
              new VertexElement(VertexElementByteOffset.OffsetVector4(), VertexElementFormat.Vector4, VertexElementUsage.BlendIndices, 0),
              new VertexElement(VertexElementByteOffset.OffsetVector4(), VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0)
        );
        VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }
    }
    /// <summary>
    /// This is a helper struct for tallying byte offsets
    /// </summary>
    public struct VertexElementByteOffset
    {
        public static int currentByteSize = 0;
        //[STAThread]
        public static int PositionStartOffset() { currentByteSize = 0; var s = sizeof(float) * 3; currentByteSize += s; return currentByteSize - s; }
        public static int Offset(int n) { var s = sizeof(int); currentByteSize += s; return currentByteSize - s; }
        public static int Offset(float n) { var s = sizeof(float); currentByteSize += s; return currentByteSize - s; }
        public static int Offset(Vector2 n) { var s = sizeof(float) * 2; currentByteSize += s; return currentByteSize - s; }
        public static int Offset(Color n) { var s = sizeof(int); currentByteSize += s; return currentByteSize - s; }
        public static int Offset(Vector3 n) { var s = sizeof(float) * 3; currentByteSize += s; return currentByteSize - s; }
        public static int Offset(Vector4 n) { var s = sizeof(float) * 4; currentByteSize += s; return currentByteSize - s; }

        public static int OffsetInt() { var s = sizeof(int); currentByteSize += s; return currentByteSize - s; }
        public static int OffsetFloat() { var s = sizeof(float); currentByteSize += s; return currentByteSize - s; }
        public static int OffsetColor() { var s = sizeof(int); currentByteSize += s; return currentByteSize - s; }
        public static int OffsetVector2() { var s = sizeof(float) * 2; currentByteSize += s; return currentByteSize - s; }
        public static int OffsetVector3() { var s = sizeof(float) * 3; currentByteSize += s; return currentByteSize - s; }
        public static int OffsetVector4() { var s = sizeof(float) * 4; currentByteSize += s; return currentByteSize - s; }
    }
}

