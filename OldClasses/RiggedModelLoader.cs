using System;
using System.IO;                //Required by Assimp-net
using System.Reflection;        //Required by Assimp-net
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Assimp;                               // Note: install AssimpNET 5.01 via nuget
using Assimp.Configs;                    // Required by Assimp-net

// Uses assimp.net 5.01 beta nuget,  to load a model.
// 
//
// TODO's  see the model class for more.  
// organize the reading order so it looks a little more readable and clear.
// 0) Materials ... https://github.com/KhronosGroup/glTF-Asset-Generator
// 1) spring cleaning start re-organizing this so its clearer for the next round of changes.
// 2) read the deformers
// 3) read the extra colors (i might not do this as its a lot of extra data) its got a weird setup for that with channels there can apparently be more then one color per vertex.
// 4) to much to list.
//
// add back in errmm make a better bone visualization class because im going to want to edit the models later on.
//
// resources
// https://github.khronos.org/glTF-Validator/
//
// https://github.com/KhronosGroup/glTF/tree/master/specification/2.0 // see appendix b for math model.
// https://kcoley.github.io/glTF/extensions/  
//
// https://github.com/KhronosGroup/glTF-Sample-Models/tree/master/2.0
// https://github.com/KhronosGroup/glTF-Asset-Generator
//
// https://github.com/KhronosGroup/glTF-Blender-Exporter/blob/master/docs/developer.md
// https://github.com/KhronosGroup/glTF
// https://blackthread.io/gltf-converter/
// https://marketplace.visualstudio.com/items?itemName=cesium.gltf-vscode
// https://github.com/KhronosGroup/glTF#tutorials
// https://github.com/KhronosGroup/glTF-Sample-Viewer/#physically-based-materials-in-gltf-20  // see Appendix for code
// https://academy.substance3d.com/courses/the-pbr-guide-part-1
// https://academy.substance3d.com/courses/the-pbr-guide-part-2
// https://marmoset.co/posts/physically-based-rendering-and-you-can-too/
// https://github.com/vpenades/SharpGLTF/tree/master/examples
// https://developer.apple.com/documentation/modelio/mdlmaterialsemantic
//
// SiGraph Physics reference.
// https://blog.selfshadow.com/publications/s2014-shading-course/hoffman/s2014_pbs_physics_math_slides.pdf
//
// https://github.com/willmotil/MonoGameUtilityClasses
//
namespace AssimpLoaderPbrDx
{
    /// <summary>
    /// Uses assimp.net, to load a model.  http://sir-kimmi.de/assimp/lib_html/index.html
    /// </summary>
    public class RiggedModelLoader
    {
        #region console display bools

        public bool startupDisplayLoadedModelConsoleinfo = true;
        public bool startupMinimalConsoleinfo = true;

        #endregion

        #region class members

        public Scene scene;

        public string FilePathName { get; set; } = "";
        public string FilePathNameWithoutExtension { get; set; } = "";
        public string AltTextureDirectory { get; set; } = "";
        public GraphicsDevice graphicsDevice;

        public static ContentManager content;
        public static Effect effectToUse;  // This is simply going to have to be ReDone. Im going to have to include some shaders especially for Gtlf glb prb can see that already, mines cool but this is standardized to some extent.
        public static bool UseDefaultTexture { get; set; } = true;
        public static Texture2D DefaultTexture { get; set; }

        public const int LoadingLevel_Default = -1;
        public const int LoadingLevel_TargetRealTimeMaximumQuality = 0;
        public const int LoadingLevel_TargetRealTimeQuality = 1;
        public const int LoadingLevel_TargetRealTimeFast = 2;

        /// <summary>
        /// 0 for TargetRealTimeMaximumQuality, 1 for TargetRealTimeQuality, 2 for TargetRealTimeFast, 3 for custom this does it's best to squash meshes down.
        /// Default is custom many and some older models need this to simplify things.
        /// </summary>
        public static int LoadingLevelPreset = LoadingLevel_Default;

        /// <summary>
        /// Reverses the models winding typically this will change the model vertices to counter clockwise winding ccw.
        /// </summary>
        public bool ReverseVerticeWinding = false;

        /// <summary>
        /// Artificially adds a small amount of looping duration to the end of a animation. This helps to fix animations that aren't properly looped.
        /// Turn on AddAdditionalLoopingTime to use this.
        /// </summary>
        public float AddedLoopingDuration = 0f;

        /// <summary>
        /// Dunno if this will actually work.
        /// </summary>
        public float Scale = 1.0f;

        /// <summary>
        /// Setting this to true once will output all the models embedded textures to file it can be set false afterwards and they will load from file.
        /// </summary>
        public bool OutputEmbededTextures { get; set; } = false;

        public int UnknownTextureOverride { get; set; } = -1;

        /* https://github.com/assimp/assimp-net/blob/master/AssimpNet/Configs/PropertyConfig.cs
         * MeasureTimeConfig(bool measureTime)  MultithreadingConfig(int value) MDLColorMapConfig(String fileName)
         * MaterialExcludeListConfig(String[] materialNames) NodeExcludeListConfig(params String[] nodeNames)
         * TangentSmoothingAngleConfig(float angle) NormalSmoothingAngleConfig(float angle) NormalizeVertexComponentsConfig(bool normalizeVertexComponents)
         * TangentTextureChannelIndexConfig(int textureChannelIndex) DeboneThresholdConfig(float threshold) MD3KeyFrameImportConfig(int keyFrame)
         * VertexBoneWeightLimitConfig(int maxBoneWeights)
         * NoSkeletonMeshesConfig(bool disableDummySkeletonMeshes) 
         * KeepSceneHierarchyConfig(bool keepHierarchy) 
         * RemoveDegeneratePrimitivesConfig(bool removeDegenerates)
         * SortByPrimitiveTypeConfig(PrimitiveType typesToRemove)
         * MaxBoneCountConfig(int maxBones)
         * RootTransformationConfig(Matrix4x4 rootTransform)
         * GlobalKeyFrameImportConfig(int keyFrame)
         * FBXImportAllGeometryLayersConfig(bool importAllGeometryLayers) FBXImportAllMaterialsConfig(bool importAllMaterials) FBXImportAllMaterialsConfig(bool importAllMaterials)
         * FBXImportCamerasConfig(bool importCameras) FBXImportLightsConfig(bool importLights) FBXImportAnimationsConfig(bool importAnimations) 
         * FBXStrictModeConfig(bool useStrictMode)
         * FBXPreservePivotsConfig(bool preservePivots)
         * FBXOptimizeEmptyAnimationCurvesConfig(bool optimizeEmptyAnimations)
         * 
         */
        public List<PropertyConfig> configurations = new List<PropertyConfig>()
        {
            new NoSkeletonMeshesConfig(true),
            new FBXImportCamerasConfig(false),
            new SortByPrimitiveTypeConfig(Assimp.PrimitiveType.Point | Assimp.PrimitiveType.Line),
            new VertexBoneWeightLimitConfig(4),
            new FBXStrictModeConfig(false)
        };

        public TheLoadersConsoleInfo info;

        #endregion

        #region Constructor and primary model loading methods

        /// <summary>
        /// Loading content here is just for visualizing but it wont be requisite if we load all the textures in from xnb's at runtime in completed model.
        /// </summary>
        public RiggedModelLoader(GraphicsDevice device, ContentManager Content, Effect defaulteffect)
        {
            graphicsDevice = device;
            effectToUse = defaulteffect;
            content = Content;
        }

        /// <summary> 
        /// Primary loading method.
        /// </summary>
        public RiggedModel LoadAsset(string filePathorFileName)
        {
            return ImportAssetAndCreateModel(filePathorFileName);
        }

        /// <summary> 
        /// Primary loading method.
        /// </summary>
        public RiggedModel LoadAsset(string filePathorFileName, string altTextureDirectory)
        {
            AltTextureDirectory = altTextureDirectory;
            return ImportAssetAndCreateModel(filePathorFileName);
        }

        /// <summary> 
        /// Primary loading method. 
        /// </summary>
        public RiggedModel LoadAsset(string filePathorFileName, string altTextureDirectory, bool useDefaultTexture)
        {
            UseDefaultTexture = useDefaultTexture;
            AltTextureDirectory = altTextureDirectory;
            Scale = 1.0f;
            return ImportAssetAndCreateModel(filePathorFileName);
        }

        /// <summary> 
        /// Primary loading method. 
        /// </summary>
        public RiggedModel LoadAsset(string filePathorFileName, string altTextureDirectory, bool useDefaultTexture, int loadingLevelPreset)
        {
            LoadingLevelPreset = loadingLevelPreset;
            UseDefaultTexture = useDefaultTexture;
            AltTextureDirectory = altTextureDirectory;
            Scale = 1.0f;
            return ImportAssetAndCreateModel(filePathorFileName);
        }

        /// <summary> 
        /// Primary loading method. 
        /// </summary>
        public RiggedModel LoadAsset(string filePathorFileName, string altTextureDirectory, bool useDefaultTexture, int loadingLevelPreset, float modelScale)
        {
            LoadingLevelPreset = loadingLevelPreset;
            UseDefaultTexture = useDefaultTexture;
            AltTextureDirectory = altTextureDirectory;
            Scale = modelScale;
            return ImportAssetAndCreateModel(filePathorFileName);
        }
        /// <summary> 
        /// Primary loading method. 
        /// </summary>
        public RiggedModel LoadAsset(string filePathorFileName, string altTextureDirectory, bool useDefaultTexture, int loadingLevelPreset, float modelScale, int unknownTextureOverride)
        {
            LoadingLevelPreset = loadingLevelPreset;
            UseDefaultTexture = useDefaultTexture;
            AltTextureDirectory = altTextureDirectory;
            Scale = modelScale;
            UnknownTextureOverride = unknownTextureOverride;
            return ImportAssetAndCreateModel(filePathorFileName);
        }

        //public static Effect LoadDefaultEffects(Microsoft.Xna.Framework.Content.ContentManager content, Matrix View, Matrix Projection, Vector3 lightPosition, Texture2D defaultTexture)
        //{
        //    DefaultTexture = defaultTexture;
        //    RiggedModelLoader.UseDefaultTexture = true;

        //    effectToUse = content.Load<Effect>("RiggedModelEffect");
        //    effectToUse.CurrentTechnique = effectToUse.Techniques["DiffuseSkinnedLighting"];

        //    effectToUse.Parameters["World"].SetValue(Matrix.Identity);
        //    effectToUse.Parameters["View"].SetValue(View);
        //    effectToUse.Parameters["Projection"].SetValue(Projection);
        //    effectToUse.Parameters["WorldLightPosition"].SetValue(lightPosition);
        //    effectToUse.Parameters["TextureA"].SetValue(defaultTexture);
        //    effectToUse.Parameters["LightColor"].SetValue(new Vector4(.99f, .99f, .99f, 1.0f));

        //    // set up the effect initially to change how you want the shader to behave.
        //    effectToUse.Parameters["AmbientAmt"].SetValue(.35f);
        //    effectToUse.Parameters["DiffuseAmt"].SetValue(.4f);
        //    effectToUse.Parameters["SpecularAmt"].SetValue(.25f);
        //    effectToUse.Parameters["SpecularSharpness"].SetValue(.88f);
        //    effectToUse.Parameters["SpecularLightVsTexelInfluence"].SetValue(.40f);
        //    effectToUse.Parameters["MaterialColor"].SetValue(new Vector4(.99f, .99f, .99f, 1.0f));
        //    return effectToUse;
        //}

        #endregion

        /// <summary> 
        /// Private primary model file loading method. This method first looks in the Assets folder then in the Content folder for the file.
        /// If that fails it will look to see if the filepath is actually the full path to the file.
        /// The texture itself is expected to be loaded and then attached to the effect atm.
        /// </summary>
        private RiggedModel ImportAssetAndCreateModel(string filePathorFileName)
        {
            var fullFilePath = AlterPath(filePathorFileName);
            var importer = SetupConfig();
            ImportWithSettings(importer, fullFilePath);
            info = new TheLoadersConsoleInfo(this);
            return CreateModel(fullFilePath);
        }

        /// <summary>
        /// Alters the file path if it doesn't exist to a default.
        /// </summary>
        private string AlterPath(string filePathorFileName)
        {
            FilePathName = filePathorFileName;
            FilePathNameWithoutExtension = Path.GetFileNameWithoutExtension(filePathorFileName);

            string s = Path.Combine(Path.Combine(Environment.CurrentDirectory, "Assets"), filePathorFileName);
            if (File.Exists(s) == false)
                s = Path.Combine(Path.Combine(Environment.CurrentDirectory, "Content"), filePathorFileName);
            if (File.Exists(s) == false)
                s = Path.Combine(Environment.CurrentDirectory, filePathorFileName);
            if (File.Exists(s) == false)
                s = filePathorFileName;
            Debug.Assert(File.Exists(s), "Could not find the file to load: " + s);
            string fullFilePath = s;

            return fullFilePath;
        }

        /// <summary>
        /// Sets up the assimp context to some standard stuff.
        /// </summary>
        private AssimpContext SetupConfig()
        {
            var importer = new AssimpContext();
            if (Scale != 1.0f)
                importer.Scale = Scale;
            foreach(var p in configurations)
                importer.SetConfig(p);
            return importer;
        }

        /// <summary>
        /// Imports the .fbx or model with the importer settings.
        /// </summary>
        private void ImportWithSettings(AssimpContext importer, string fullFilePath)
        {
            // load the file at path to the scene
            try
            {
                switch (LoadingLevelPreset)
                {
                    case LoadingLevel_TargetRealTimeMaximumQuality:
                        scene = importer.ImportFile(fullFilePath, PostProcessPreset.TargetRealTimeMaximumQuality);
                        break;
                    case LoadingLevel_TargetRealTimeQuality:
                        scene = importer.ImportFile(fullFilePath, PostProcessPreset.TargetRealTimeQuality);
                        break;
                    case LoadingLevel_TargetRealTimeFast:
                        scene = importer.ImportFile(fullFilePath, PostProcessPreset.TargetRealTimeFast);
                        break;
                    default:
                        scene = importer.ImportFile
                                               (
                                                fullFilePath
                                                ,
                                                PostProcessSteps.ValidateDataStructure
                                                | PostProcessSteps.RemoveRedundantMaterials
                                                | PostProcessSteps.FindInstances
                                                | PostProcessSteps.FindDegenerates
                                                | PostProcessSteps.GenerateUVCoords
                                                | PostProcessSteps.Triangulate
                                                | PostProcessSteps.SortByPrimitiveType
                                                | PostProcessSteps.FindInvalidData
                                                | PostProcessSteps.OptimizeMeshes
                                                | PostProcessSteps.OptimizeGraph // normal
                                                | PostProcessSteps.FlipUVs            // appears necessary
                                                | PostProcessSteps.FixInFacingNormals
                                                | PostProcessSteps.JoinIdenticalVertices
                                                | PostProcessSteps.ImproveCacheLocality
                                                //| PostProcessSteps.FlipWindingOrder
                                                //| PostProcessSteps.GlobalScale
                                                //| PostProcessSteps.RemoveRedundantMaterials // sketchy
                                                //| PostProcessSteps.PreTransformVertices
                                                );
                        break;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Debug.Assert(false, fullFilePath + "\n\n" + "A problem loading the model occured: \n " + fullFilePath + " \n" + e.Message);
                scene = null;
            }
        }

        /// <summary> Begins the flow to call methods and do the actual loading from the assimp data to our models data.
        /// </summary>
        private RiggedModel CreateModel(string filepathorname)
        {
            // create model
            RiggedModel model = new RiggedModel();
            model.Name = FilePathNameWithoutExtension;
            model.defaultDebugTexture = DefaultTexture;
            model.useDefaultDebugTexture = UseDefaultTexture;
            model.LoadEffect(content, "RiggedModelEffect");

            // set the models effect to use.
            if (effectToUse != null && RiggedModel.effect == null)
                RiggedModel.effect = effectToUse;

            // prep to build a models tree.
            CreateModelTreeRootNode(model, scene);

            // create the models meshes
            CreateModelMeshesAndBones(model, scene, 0);

            // setup mesh mats textures.
            SetUpMeshMaterialsAndLoadTextures(model, scene, 0);

            // recursively search and add the nodes to our model from the scene, this includes adding to the flat bone and node lists.
            CreateModelNodeTreeTransformsRecursively(model, model.sceneRootNodeOfTree, scene.RootNode, 0);

            // create node refs to link bones to nodes directly.
            FinishBoneSetUp(model);

            // get the animations in the file into each nodes animations framelist
            AnimationsCreateNodesAndCopy(model, scene);

            // get the vertice data from the meshes. this is the last thing we will do because we need the nodes set up first.
            CopyVerticeIndiceData(model, scene, 0);

            // primary final info is outputed this might or probably will eventually replace all or most of the console output.
            info.DisplayConsoleInfo(scene, model, filepathorname);

            return model;
        }

        public void FinishBoneSetUp(RiggedModel model)
        {
            var len = model.meshes.Length;
            for(int i = 0;i < len; i++)
            {
                var mesh = model.meshes[i];
                var bones = mesh.modelMeshBones;
                for(int j =0; j < bones.Length; j++)
                {
                    var bone = bones[j];
                    var node = ModelSearchIterateNodeTreeForNameGetRefToNode(bone.Name, model.sceneRootNodeOfTree);
                    if(node != null)
                    {
                        bone.refToCorrespondingHeirarchyNode = node;
                    }
                }
            }
        }

        /// <summary> Create a root node. 
        /// </summary>
        private void CreateModelTreeRootNode(RiggedModel model, Scene scene)
        {
            // set the rootnode up.
            model.sceneRootNodeOfTree = new RiggedModel.RiggedModelNode();
            model.sceneRootNodeOfTree.Name = scene.RootNode.Name;
            model.sceneRootNodeOfTree.LocalTransformMg = scene.RootNode.Transform.ToMgTransposed();
            model.sceneRootNodeOfTree.CombinedTransformMg = model.sceneRootNodeOfTree.LocalTransformMg;
        }

        /// <summary>We create model mesh instances for each mesh in scene.meshes. This is just set up it doesn't load any data.
        /// </summary>
        private void CreateModelMeshesAndBones(RiggedModel model, Scene scene, int meshIndex)
        {
            // The material used by this mesh.
            // A mesh uses only a single material. If an imported model uses multiple materials, the import splits up the mesh. Use this value as index into the scene's material list. 
            // http://sir-kimmi.de/assimp/lib_html/structai_mesh.html#aa2807c7ba172115203ed16047ad65f9e

            model.meshes = new RiggedModel.RiggedModelMesh[scene.Meshes.Count];

            // create a new model.mesh per mesh in scene
            for (int mloop = 0; mloop < scene.Meshes.Count; mloop++)
            {
                Mesh assimpMesh = scene.Meshes[mloop];
                var rmMesh = new RiggedModel.RiggedModelMesh();
                rmMesh.Name = assimpMesh.Name;
                rmMesh.meshNumber = mloop;
                //rmMesh.texture = DefaultTexture;
                //rmMesh.textureName = "Default";
                rmMesh.MaterialIndex = assimpMesh.MaterialIndex;
                rmMesh.MaterialIndexName = scene.Materials[rmMesh.MaterialIndex].Name;

                // new stuff per mesh.  shoud be off of create meshes.
                var assimpMeshBones = assimpMesh.Bones;
                rmMesh.HasBones = assimpMesh.HasBones;
                rmMesh.globalShaderMatrixs = new Matrix[assimpMeshBones.Count + 1];
                rmMesh.globalShaderMatrixs[0] = Matrix.Identity; // default this to identity, for the no bone or no node animation case this will then act as a static value for the duration.
                rmMesh.modelMeshBones = new RiggedModel.RiggedModelBone[assimpMesh.BoneCount + 1];
                // dummy bone
                // We cannot however yet link this to the node that has to wait as it has not yet been created and only exists in the model not in the assimp bone list.
                rmMesh.modelMeshBones[0] = new RiggedModel.RiggedModelBone();
                var modelMeshFlatBone = rmMesh.modelMeshBones[0];
                rmMesh.modelMeshBones[0].OffsetMatrixMg = Matrix.Identity;
                rmMesh.modelMeshBones[0].Name = assimpMesh.Name; //"DummyBone0";
                rmMesh.modelMeshBones[0].meshIndex = mloop;
                rmMesh.modelMeshBones[0].meshBoneIndex = 0;

                for (int assimpMeshBoneIndex = 0; assimpMeshBoneIndex < assimpMeshBones.Count; assimpMeshBoneIndex++)
                {
                    var assimpBoneInMesh = assimpMeshBones[assimpMeshBoneIndex]; // ahhhh
                    var assimpBoneInMeshName = assimpMeshBones[assimpMeshBoneIndex].Name;

                    // new stuff per mesh.
                    var modelMeshBoneIndex = assimpMeshBoneIndex + 1;
                    rmMesh.globalShaderMatrixs[modelMeshBoneIndex] = Matrix.Identity;
                    rmMesh.modelMeshBones[modelMeshBoneIndex] = new RiggedModel.RiggedModelBone();
                    modelMeshFlatBone = rmMesh.modelMeshBones[modelMeshBoneIndex];
                    modelMeshFlatBone.Name = assimpBoneInMesh.Name;
                    modelMeshFlatBone.OffsetMatrixMg = assimpBoneInMesh.OffsetMatrix.ToMgTransposed();
                    modelMeshFlatBone.meshIndex = mloop;
                    modelMeshFlatBone.meshBoneIndex = modelMeshBoneIndex;
                    modelMeshFlatBone.numberOfAssociatedWeightedVertices = assimpBoneInMesh.VertexWeightCount;
                    rmMesh.modelMeshBones[modelMeshBoneIndex] = modelMeshFlatBone;
                }
                model.meshes[mloop] = rmMesh;
            }
        }

        /// <summary>
        /// Loads textures and sets material values to each model mesh.
        /// </summary>
        private void SetUpMeshMaterialsAndLoadTextures(RiggedModel model, Scene scene, int meshIndex)
        {
            var savedDir = content.RootDirectory;

            List<Texture2D> textureList = new List<Texture2D>();
            string tempDir = Path.Combine(savedDir, "Temp", model.Name);
            Directory.CreateDirectory(tempDir);

            // create a new model.mesh per mesh in scene
            for (int mloop = 0; mloop < scene.Meshes.Count; mloop++)
            {
                var rmMesh = model.meshes[mloop];
                int matIndex = rmMesh.MaterialIndex;
                var assimpMaterial = scene.Materials[matIndex];

                rmMesh.IsPbrMaterial = assimpMaterial.IsPBRMaterial;
                rmMesh.ColorAmbient = assimpMaterial.ColorAmbient.ToMg();
                rmMesh.ColorDiffuse = assimpMaterial.ColorDiffuse.ToMg();
                rmMesh.ColorSpecular = assimpMaterial.ColorSpecular.ToMg();
                rmMesh.ColorEmissive = assimpMaterial.ColorEmissive.ToMg();
                rmMesh.ColorReflective = assimpMaterial.ColorReflective.ToMg();
                rmMesh.ColorTransparent = assimpMaterial.ColorTransparent.ToMg();
                rmMesh.Opacity = assimpMaterial.Opacity;
                rmMesh.Transparency = assimpMaterial.TransparencyFactor;
                rmMesh.Reflectivity = assimpMaterial.Reflectivity;
                rmMesh.Shininess = assimpMaterial.Shininess;
                rmMesh.ShininessStrength = assimpMaterial.ShininessStrength;
                rmMesh.Shininess = assimpMaterial.Shininess;
                rmMesh.BumpScaling = assimpMaterial.BumpScaling;
                rmMesh.HasShaders = assimpMaterial.HasShaders;
                rmMesh.ShadingMode = assimpMaterial.ShadingMode.ToString();
                rmMesh.BlendMode = assimpMaterial.BlendMode.ToString();
                rmMesh.IsWireFrameEnabled = assimpMaterial.IsWireFrameEnabled;

                //var pbrprop = assimpMaterial.PBR;
                //var HasTextureBaseColor = pbrprop.HasTextureBaseColor;
                //var HasTextureEmissionColor = pbrprop.HasTextureEmissionColor;
                //var HasTextureMetalness = pbrprop.HasTextureMetalness;
                //var HasTextureNormalCamera = pbrprop.HasTextureNormalCamera;
                //var HasTextureRoughness = pbrprop.HasTextureRoughness;
                //var TextureBaseColor = pbrprop.TextureBaseColor;
                //var TextureEmissionColor = pbrprop.TextureEmissionColor;
                //var TextureMetalness = pbrprop.TextureMetalness;
                //var TextureNormalCamera = pbrprop.TextureNormalCamera;
                //var TextureRoughness = pbrprop.TextureRoughness;


                var assimpMaterialTextures = assimpMaterial.GetAllMaterialTextures();
               
                for (int matTextureIndex = 0; matTextureIndex < assimpMaterialTextures.Length; matTextureIndex++)
                {
                    // Texture types available via assimp are.
                    // None = 0, Diffuse = 1,Specular = 2, Ambient = 3,Emissive = 4, Height = 5,Normals = 6,Shininess = 7,Opacity = 8, Displacement = 9, Lightmap = 10,AmbientOcclusion = 17,Reflection = 11,
                    // BaseColor = 12 /*PBR*/, NormalCamera = 13/*PBR normal map workflow*/,EmissionColor = 14/*PBR emissive*/,Metalness = 15 /*PBR shininess*/  , Roughness = 16, /*PRB*/

                    var tindex = assimpMaterialTextures[matTextureIndex].TextureIndex;
                    var toperation = assimpMaterialTextures[matTextureIndex].Operation;
                    var ttype = assimpMaterialTextures[matTextureIndex].TextureType.ToString();
                    var tfilepath = assimpMaterialTextures[matTextureIndex].FilePath;
                    bool tfileexists = false;
                    string tfilename = "";
                    string tfullfilepath = "";
                    bool isInContentXnb = false;

                    // check if the file exist in the content folder or off it or in a expected place
                    if (tfilepath != null)
                    {
                        tfilename = GetFileName(tfilepath, true);
                        tfullfilepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, savedDir, tfilename + ".xnb");
                        tfileexists = File.Exists(tfullfilepath);
                        if (tfileexists == true)
                        {
                            content.RootDirectory = savedDir;
                            isInContentXnb = true;
                        }
                        else
                        {
                            tfullfilepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, content.RootDirectory,  tfilename + ".xnb");
                            tfileexists = File.Exists(tfullfilepath);
                            if (tfileexists == true)
                            {
                                isInContentXnb = true;
                            }
                            else
                            {
                                tfullfilepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, content.RootDirectory, FilePathNameWithoutExtension, tfilename + ".xnb");
                                tfileexists = File.Exists(tfullfilepath);
                                if (tfileexists == true)
                                {
                                    content.RootDirectory = Path.Combine("Content", FilePathNameWithoutExtension);
                                    isInContentXnb = true;
                                }
                                else
                                {
                                    tfullfilepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, content.RootDirectory, AltTextureDirectory, tfilename + ".xnb");
                                    tfileexists = File.Exists(tfullfilepath);
                                    if (tfileexists == true)
                                    {
                                        content.RootDirectory = Path.Combine("Content", AltTextureDirectory);
                                        isInContentXnb = true;
                                    }
                                }
                            }
                        }
                    }

                    // http://sir-kimmi.de/assimp/lib_html/materials.html

                    // load the textures into the model lots of checks and double checks as we wind thru these calls.
                    LoadTexture(ref model, ref model.meshes[mloop].textureDiffuse , ttype, TextureType.Diffuse.ToString(), mloop, tfileexists, tindex, matTextureIndex, tempDir, tfilename);
                    LoadTexture(ref model, ref model.meshes[mloop].textureNormalMap, ttype, TextureType.Normals.ToString(), mloop, tfileexists, tindex, matTextureIndex, tempDir, tfilename);
                    LoadTexture(ref model, ref model.meshes[mloop].textureHeightMap, ttype, TextureType.Height.ToString(), mloop, tfileexists, tindex, matTextureIndex, tempDir, tfilename);
                    LoadTexture(ref model, ref model.meshes[mloop].textureDisplacementMap, ttype, TextureType.Displacement.ToString(), mloop, tfileexists, tindex, matTextureIndex, tempDir, tfilename);
                    LoadTexture(ref model, ref model.meshes[mloop].textureReflectionMap, ttype, TextureType.Reflection.ToString(), mloop, tfileexists, tindex, matTextureIndex, tempDir, tfilename);
                    LoadTexture(ref model, ref model.meshes[mloop].textureEmissiveMap, ttype, TextureType.Emissive.ToString(), mloop, tfileexists, tindex, matTextureIndex, tempDir, tfilename);
                    LoadTexture(ref model, ref model.meshes[mloop].textureSpecular, ttype, TextureType.Specular.ToString(), mloop, tfileexists, tindex, matTextureIndex, tempDir, tfilename);
                    LoadTexture(ref model, ref model.meshes[mloop].textureLightMap, ttype, TextureType.Lightmap.ToString(), mloop, tfileexists, tindex, matTextureIndex, tempDir, tfilename);
                    LoadTexture(ref model, ref model.meshes[mloop].textureAmbientOcclusionOraLightMap, ttype, TextureType.AmbientOcclusion.ToString(), mloop, tfileexists, tindex, matTextureIndex, tempDir, tfilename);
                    LoadTexture(ref model, ref model.meshes[mloop].textureOpacity, ttype, TextureType.Opacity.ToString(), mloop, tfileexists, tindex, matTextureIndex, tempDir, tfilename);
                    // not sure if this is a gloss map pbr or what not.
                    LoadTexture(ref model, ref model.meshes[mloop].textureShine, ttype, TextureType.Shininess.ToString(), mloop, tfileexists, tindex, matTextureIndex, tempDir, tfilename);
                    // i think these ones are the actual prb types.
                    LoadTexture(ref model, ref model.meshes[mloop].textureNormalMapPrbCamera, ttype, TextureType.NormalCamera.ToString(), mloop, tfileexists, tindex, matTextureIndex, tempDir, tfilename);
                    LoadTexture(ref model, ref model.meshes[mloop].textureBaseColorMapPbr, ttype, TextureType.BaseColor.ToString(), mloop, tfileexists, tindex, matTextureIndex, tempDir, tfilename);
                    LoadTexture(ref model, ref model.meshes[mloop].textureRoughnessPbr, ttype, TextureType.Roughness.ToString(), mloop, tfileexists, tindex, matTextureIndex, tempDir, tfilename);
                    LoadTexture(ref model, ref model.meshes[mloop].textureMetalnessPbr, ttype, TextureType.Metalness.ToString(), mloop, tfileexists, tindex, matTextureIndex, tempDir, tfilename);
                    // this appears to be the roughness or metal texture but for some reason assimp here is not detecting it properly.
                    if(UnknownTextureOverride == RiggedModel.UnknownTextureIsOcclusionRoughnessMetal)
                        LoadTexture(ref model, ref model.meshes[mloop].textureRoughnessPbr, ttype, TextureType.Unknown.ToString(), mloop, tfileexists, tindex, matTextureIndex, tempDir, tfilename);
                    else
                        LoadTexture(ref model, ref model.meshes[mloop].textureUnknown, ttype, TextureType.Unknown.ToString(), mloop, tfileexists, tindex, matTextureIndex, tempDir, tfilename);
                }

                // ok so im going to have to come up with a shader scheme to handle all these different possiblitys.
                var m = model.meshes[mloop];
                var techVal = 0;
                if (m.textureNormalMap != null)
                    techVal += 1;
                if (m.textureRoughnessPbr != null)
                    techVal += 2;
                //if (m.textureEmissiveMap != null)
                //    techVal += 4;

                m.techniqueValue = techVal;

                switch (techVal)
                {
                    case 0:
                        m.techniqueName = "SkinnedColorBaseNormEmissive"; 
                        break;
                    case 1:
                        m.techniqueName = "SkinnedColorBaseNormalMapEmissive"; 
                        break;
                    case 2:
                        m.techniqueName = "SkinnedColorBaseMetalRoughEmissive"; 
                        break;
                    case 3:
                        m.techniqueName = "SkinnedColorBaseMetalRoughEmissiveNormalMap"; 
                        break;
                    default:
                        m.techniqueName = "SkinnedColorBaseNormEmissive";
                        break;
                }

            }
            Console.WriteLine();

            content.RootDirectory = savedDir;
        }

        /// <summary> We recursively walk the nodes here this drops all the info from scene nodes to model nodes.
        /// This gets if its a bone mesh index, global index, marks parents necessary ect..
        /// We mark neccessary also is this a bone, also is this part of a bone chain, children parents ect.
        /// Additionally this sets offset matrices however that might not be right to do that here i don't think it is now and i think thats a obsolete notion in this method. 
        /// Add node to model
        /// </summary>
        private void CreateModelNodeTreeTransformsRecursively(RiggedModel model, RiggedModel.RiggedModelNode modelNode, Node curAssimpNode, int tabLevel)
        {
            modelNode.Name = curAssimpNode.Name;
            modelNode.LocalTransformMg = curAssimpNode.Transform.ToMgTransposed(); // set the initial local node transform.
            modelNode.CombinedTransformMg = curAssimpNode.Transform.ToMgTransposed();
            if (curAssimpNode.HasMeshes)
            { 
                modelNode.isThisAMeshNode = true;
                foreach (var nMeshIndice in curAssimpNode.MeshIndices)
                {
                    var rmMesh = model.meshes[nMeshIndice]; // get the applicable model mesh reference.
                    rmMesh.nodeRefContainingMeshNodeAnimation = modelNode; // set the current node reference to each applicable mesh node ref that uses it so each meshes can reference it' the node transform.  
                }
            }

            // model bone building here.
            for (int mIndex = 0; mIndex < scene.Meshes.Count; mIndex++)
            {
                // there wont be an assimp bone named with a mesh node animation however we added mesh bones 0 to our model class so it exists there.
                RiggedModel.RiggedModelBone bone;
                int boneIndexInMesh = 0;
                if (GetRiggedBoneForTheMesh(model.meshes[mIndex], modelNode.Name, out bone, out boneIndexInMesh))
                {
                    modelNode.hasARealBone = true;
                    modelNode.isANodeAlongTheBoneRoute = true;
                    // new stuff for the changes.
                    var riggedBone = bone;
                    riggedBone.meshIndex = mIndex;
                    riggedBone.meshBoneIndex = boneIndexInMesh;
                    modelNode.uniqueMeshBones.Add(riggedBone);
                }
            }

            // access children
            for (int i = 0; i < curAssimpNode.Children.Count; i++)
            {
                var asimpChildNode = curAssimpNode.Children[i];
                var childNode = new RiggedModel.RiggedModelNode();
                // set parent before passing.
                childNode.parent = modelNode;
                childNode.Name = curAssimpNode.Children[i].Name;
                if (childNode.parent.isANodeAlongTheBoneRoute)
                    childNode.isANodeAlongTheBoneRoute = true;
                modelNode.children.Add(childNode);
                CreateModelNodeTreeTransformsRecursively(model, modelNode.children[i], asimpChildNode, tabLevel + 1);
            }
        }

        /// <summary>Get Scene Model Mesh Vertices. Gets all the mesh data into a mesh array. 
        ///  http://sir-kimmi.de/assimp/lib_html/structai_mesh.html#aa2807c7ba172115203ed16047ad65f9e
        /// </summary>
        private void CopyVerticeIndiceData(RiggedModel model, Scene scene, int meshIndex) // RiggedModel
        {
            // Loop meshes for Vertice data.
            for (int mloop = 0; mloop < scene.Meshes.Count; mloop++)
            {
                Mesh mesh = scene.Meshes[mloop];
                var rmMesh = model.meshes[mloop];

                // indices
                int[] indexs = new int[mesh.Faces.Count * 3];
                int loopindex = 0;
                for (int k = 0; k < mesh.Faces.Count; k++)
                {
                    var f = mesh.Faces[k];
                    for (int j = 0; j < f.IndexCount; j++)
                    {
                        var ind = f.Indices[j];
                        indexs[loopindex] = ind;
                        loopindex++;
                    }
                }

                // vertices 
                VertexPositionColorNormalTextureTangentWeights[] v = new VertexPositionColorNormalTextureTangentWeights[mesh.Vertices.Count];
                for (int k = 0; k < mesh.Vertices.Count; k++)
                {
                    var f = mesh.Vertices[k];
                    v[k].Position = new Vector3(f.X, f.Y, f.Z);
                }

                // normals
                for (int k = 0; k < mesh.Normals.Count; k++)
                {
                    rmMesh.HasPreCalculatedNormals = true;
                    var f = mesh.Normals[k];
                    v[k].Normal = new Vector3(f.X, f.Y, f.Z);
                }

                // Check whether the mesh contains tangent and bitangent vectors It is not possible that it contains tangents and no bitangents (or the other way round). 
                // http://sir-kimmi.de/assimp/lib_html/structai_mesh.html#aa2807c7ba172115203ed16047ad65f9e
                //
                //// TODO need to add this to the vertex declaration or calculate it on the shader.

                // tangents
                for (int k = 0; k < mesh.Tangents.Count; k++)
                {
                    rmMesh.HasPreCalculatedTangents = true;
                    var f = mesh.Tangents[k];
                    v[k].Tangent = new Vector3(f.X, f.Y, f.Z);
                }

                // bi tangents  
                for (int k = 0; k < mesh.BiTangents.Count; k++)
                {
                    rmMesh.HasPreCalculatedBiTangents = true;
                    var f = mesh.BiTangents[k];
                    v[k].BiTangent = f.ToMg();
                }

                // A mesh may contain 0 to AI_MAX_NUMBER_OF_COLOR_SETS vertex colors per vertex. NULL if not present. Each array is mNumVertices in size if present. 
                // http://sir-kimmi.de/assimp/lib_html/structai_mesh.html#aa2807c7ba172115203ed16047ad65f9e

                // TODO needs to be done up proper or at least completely figured out what to do with multiple vertex channels.

                // vertex colors.
                var c = mesh.VertexColorChannels[0];
                bool hascolors = false;
                if (mesh.HasVertexColors(mloop))
                    hascolors = true;
                for (int k = 0; k < mesh.Vertices.Count; k++)
                {
                    if (hascolors == false)
                    {
                        v[k].Color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                        //var m = scene.Materials[mesh.MaterialIndex];
                        //if (m.HasColorDiffuse)
                        //    v[k].Color = new Vector4(m.ColorDiffuse.R, m.ColorDiffuse.G, m.ColorDiffuse.B, m.ColorDiffuse.A);
                    }
                    else
                    {
                        rmMesh.HasPreCalculatedVertexColors = true;
                        v[k].Color = new Vector4(c[k].R, c[k].G, c[k].B, c[k].A);
                    }
                }

                // Check whether the mesh contains a texture coordinate set. 
                // mNumUVComponents
                // unsigned int aiMesh::mNumUVComponents[AI_MAX_NUMBER_OF_TEXTURECOORDS]
                // Specifies the number of components for a given UV channel.
                // Up to three channels are supported(UVW, for accessing volume or cube maps).
                //   If the value is 2 for a given channel n, the component p.z of mTextureCoords[n][p] is set to 0.0f. 
                //   If the value is 1 for a given channel, p.y is set to 0.0f, too.
                // Note 4D coords are not supported

                // Uv
                var uvchannels = mesh.TextureCoordinateChannels;
                for (int k = 0; k < uvchannels.Length; k++)
                {
                    var verticeUvCoords = uvchannels[k];
                    int loopIndex = 0;
                    for (int j = 0; j < verticeUvCoords.Count; j++)
                    {
                        var uv = verticeUvCoords[j];
                        v[loopIndex].TextureCoordinate = new Microsoft.Xna.Framework.Vector2(uv.X, uv.Y);
                        loopIndex++;
                    }
                }

                // find the min max vertices for a bounding box.
                // this is useful for other stuff which i need right now.

                Vector3 min = Vector3.Zero;
                Vector3 max = Vector3.Zero;
                Vector3 centroid = Vector3.Zero;
                foreach (var vert in v)
                {
                    if (vert.Position.X < min.X) { min.X = vert.Position.X; }
                    if (vert.Position.Y < min.Y) { min.Y = vert.Position.Y; }
                    if (vert.Position.Z < min.Z) { min.Z = vert.Position.Z; }
                    if (vert.Position.X > max.X) { max.X = vert.Position.X; }
                    if (vert.Position.Y > max.Y) { max.Y = vert.Position.Y; }
                    if (vert.Position.Z > max.Z) { max.Z = vert.Position.Z; }
                    centroid += vert.Position;
                }
                model.meshes[mloop].Centroid = centroid / (float)v.Length;
                model.meshes[mloop].Min = min;
                model.meshes[mloop].Max = max;

                // Prep blend weight and indexs this one is just prep for later on.
                for (int k = 0; k < mesh.Vertices.Count; k++)
                {
                    var f = mesh.Vertices[k];
                    v[k].BlendIndices = new Vector4(0f, 0f, 0f, 0f);
                    v[k].BlendWeights = new Vector4(0f, 0f, 0f, 0f);
                }

                // Restructure vertice data to conform to a shader.
                // Iterate mesh bone offsets set the bone Id's and weights to the vertices.
                // This also entails correlating the mesh local bone index names to the flat bone list.
                TempVerticeWeightsIndexs[] verts = new TempVerticeWeightsIndexs[mesh.Vertices.Count];
                if (mesh.HasBones)
                {
                    model.meshes[mloop].HasBones = mesh.HasBones;
                    model.meshes[mloop].HasMeshAnimationAttachments = mesh.HasMeshAnimationAttachments;
                    var assimpMeshBones = mesh.Bones;
                    for (int assimpMeshBoneIndex = 0; assimpMeshBoneIndex < assimpMeshBones.Count; assimpMeshBoneIndex++)
                    {
                        var assimpBoneInMesh = assimpMeshBones[assimpMeshBoneIndex]; // ahhhh
                        var assimpBoneInMeshName = assimpMeshBones[assimpMeshBoneIndex].Name;

                        // new stuff per mesh.
                        var modelMeshBoneIndex = assimpMeshBoneIndex + 1;

                        // new per mesh.
                        // loop thru this bones vertice listings with the weights for it.
                        for (int vertWeightIndex = 0; vertWeightIndex < assimpBoneInMesh.VertexWeightCount; vertWeightIndex++)
                        {
                            var verticeIndexTheBoneIsFor = assimpBoneInMesh.VertexWeights[vertWeightIndex].VertexID;
                            var boneWeightVal = assimpBoneInMesh.VertexWeights[vertWeightIndex].Weight;
                            if (verts[verticeIndexTheBoneIsFor] == null)
                            {
                                verts[verticeIndexTheBoneIsFor] = new TempVerticeWeightsIndexs();
                            }
                            // add this vertice its weight and the bone id to the temp verts list.
                            verts[verticeIndexTheBoneIsFor].verticeIndexs.Add(verticeIndexTheBoneIsFor);
                            verts[verticeIndexTheBoneIsFor].verticesFlatBoneId.Add(modelMeshBoneIndex);
                            verts[verticeIndexTheBoneIsFor].verticeBoneWeights.Add(boneWeightVal);
                            verts[verticeIndexTheBoneIsFor].countOfBoneEntrysForThisVertice++;
                        }
                    }
                }
                else // mesh has no bones
                {
                    // if there is no bone data we will make it set to bone zero.
                    // this is basically a safety measure as if there is no bone data there is no bones.
                    // however in that case the vertices need to have a weight more then 0 and be set to bone zero which is really identity.
                    // this allows us to draw a boneless mesh as if the entire mesh were attached to a single identity world bone.
                    // if there is a actual world mesh node we can combine the animated transform and set it to that bone as well.
                    // so this will work for boneless node mesh transforms which assimp doesn't mark as a actual mesh transform when it is.
                    for (int i = 0; i < verts.Length; i++)
                    {
                        verts[i] = new TempVerticeWeightsIndexs();
                        var ve = verts[i];
                        if (ve.verticeIndexs.Count == 0)
                        {
                            // there is no bone data for this vertice at all then we should set it to bone zero.
                            verts[i].verticeIndexs.Add(i);
                            verts[i].verticesFlatBoneId.Add(0);
                            verts[i].verticeBoneWeights.Add(1.0f);
                        }
                    }
                }

                // Ill need up to 4 values per bone list so if some of the values are empty ill copy zero to them with weight 0.
                // This is to ensure the full key vector4 is populated.
                // The bone weight data aligns to the bones not nodes so it aligns to the offset matrices bone names.

                // loop each temp vertice add the temporary structure we have to the model vertices in sequence.
                for (int i = 0; i < verts.Length; i++)
                {
                    if (verts[i] != null)
                    {
                        var ve = verts[i];
                        //int maxbones = 4;
                        var arrayIndex = ve.verticeIndexs.ToArray();
                        var arrayBoneId = ve.verticesFlatBoneId.ToArray();
                        var arrayWeight = ve.verticeBoneWeights.ToArray();
                        if (arrayBoneId.Count() > 3)
                        {
                            v[arrayIndex[3]].BlendIndices.W = arrayBoneId[3];
                            v[arrayIndex[3]].BlendWeights.W = arrayWeight[3];
                        }
                        if (arrayBoneId.Count() > 2)
                        {
                            v[arrayIndex[2]].BlendIndices.Z = arrayBoneId[2];
                            v[arrayIndex[2]].BlendWeights.Z = arrayWeight[2];
                        }
                        if (arrayBoneId.Count() > 1)
                        {
                            v[arrayIndex[1]].BlendIndices.Y = arrayBoneId[1];
                            v[arrayIndex[1]].BlendWeights.Y = arrayWeight[1];
                        }
                        if (arrayBoneId.Count() > 0)
                        {
                            v[arrayIndex[0]].BlendIndices.X = arrayBoneId[0];
                            v[arrayIndex[0]].BlendWeights.X = arrayWeight[0];
                        }
                    }
                }

                model.meshes[mloop].vertices = v;
                model.meshes[mloop].indices = indexs;

                // last things reverse the winding if specified.
                if (ReverseVerticeWinding)
                {
                    for (int k = 0; k < model.meshes[mloop].indices.Length; k += 3)
                    {
                        var i0 = model.meshes[mloop].indices[k + 0];
                        var i1 = model.meshes[mloop].indices[k + 1];
                        var i2 = model.meshes[mloop].indices[k + 2];
                        model.meshes[mloop].indices[k + 0] = i0;
                        model.meshes[mloop].indices[k + 1] = i2;
                        model.meshes[mloop].indices[k + 2] = i1;
                    }
                }

            }

            // Bugged even more unfortunately i have to calculate this properly by applying the inverse bind pose on all the vertices per mesh first shit...
            // that means a first run to find the accurate coefficient scalars..... aaarrrrrgggggg   i might as well make full cpu run on everything that sucks ...... later ill do it. 
            // Finds the model scalar coefficent.
            // Unfortunately i can't find this automatically without running thru every single vertice twice thats simply too much extra calculation on load.
            // The recomendation then is to look at your model after you load it for this value and set the scale to this coefficient value manually on load.
            Vector3 modelMin = model.meshes[0].Min;
            Vector3 modelMax = model.meshes[0].Max;
            for(int mloop =0; mloop < model.meshes.Length;mloop++)
            {
                var min = model.meshes[mloop].Min;
                var max = model.meshes[mloop].Max;
                if (min.X < modelMin.X) { modelMin.X = min.X; }
                if (min.Y< modelMin.Y) { modelMin.Y = min.Y; }
                if (min.Z < modelMin.Z) { modelMin.Z = min.Z; }
                if (max.X > modelMax.X) { modelMin.X = max.X ; }
                if (max.Y > modelMax.Y) { modelMin.Y = max.Y; }
                if (max.Z > modelMax.Z) { modelMin.Z = max.Z; }
            }
            Vector3 diff = modelMax - modelMin;
            float coeMax = diff.X;
            if (diff.Y > coeMax)
                coeMax = diff.Y;
            if (diff.Z > coeMax)
                coeMax = diff.Z;
            model.ScaleCoefficent = 1f / coeMax;
        }

        /// <summary> Gets the assimp animations as the original does it into the model.
        /// </summary>
        private void AnimationsCreateNodesAndCopy(RiggedModel model, Scene scene)
        {
            // Nice now i find it after i already figured it out.
            // http://sir-kimmi.de/assimp/lib_html/_animation_overview.html
            // http://sir-kimmi.de/assimp/lib_html/structai_animation.html
            // http://sir-kimmi.de/assimp/lib_html/structai_anim_mesh.html
            // Animations

            // Copy over as assimp has it set up.
            for (int i = 0; i < scene.Animations.Count; i++)
            {
                var assimpAnim = scene.Animations[i];
                //________________________________________________
                // Initial copy over.
                var modelAnim = new RiggedModel.RiggedAnimation();
                modelAnim.animationName = assimpAnim.Name;
                modelAnim.DurationInTicks = assimpAnim.DurationInTicks;
                modelAnim.TicksPerSecond = assimpAnim.TicksPerSecond;
                if (modelAnim.TicksPerSecond == 0)
                {
                    modelAnim.TicksPerSecond = 1000;
                    Console.WriteLine("*@!#! TicksPerSecond was 0  setting default to 1000    specified default");
                }
                if (modelAnim.DurationInTicks == 0)
                {
                    modelAnim.DurationInTicks = 1234;
                    Console.WriteLine("*@!#! Model Read Error,   DurationInTicks was 0    setting default to 1234   undefined default.");
                }
                modelAnim.DurationInSeconds = modelAnim.DurationInTicks / modelAnim.TicksPerSecond;
                modelAnim.DurationInSecondsAdded = AddedLoopingDuration;
                //
                modelAnim.HasNodeAnimations = assimpAnim.HasNodeAnimations;
                modelAnim.HasMeshAnimations = assimpAnim.HasMeshAnimations;
                //modelAnim.HasMorphAnimations = assimpAnim.MeshMorphAnimationChannelCount > 0;

                // create new animation node list per animation
                modelAnim.animatedNodes = new List<RiggedModel.RiggedAnimationNode>();
                // Loop the node channels.
                for (int j = 0; j < assimpAnim.NodeAnimationChannels.Count; j++)
                {
                    var nodeAnimLists = assimpAnim.NodeAnimationChannels[j];
                    var nodeAnim = new RiggedModel.RiggedAnimationNode();
                    nodeAnim.nodeName = nodeAnimLists.NodeName;

                    // Set the reference to the node for node name by the model method that searches for it.
                    var modelnoderef = ModelGetRefToNode(nodeAnimLists.NodeName, model.sceneRootNodeOfTree);
                    nodeAnim.nodeRef = modelnoderef;

                    // Place all the different keys lists rot scale pos into this nodes elements lists.
                    foreach (var keyList in nodeAnimLists.RotationKeys)
                    {
                        var oaq = keyList.Value;
                        nodeAnim.qrotTime.Add(keyList.Time / assimpAnim.TicksPerSecond);
                        nodeAnim.qrot.Add(oaq.ToMg());
                    }
                    foreach (var keyList in nodeAnimLists.PositionKeys)
                    {
                        var oap = keyList.Value.ToMg();
                        nodeAnim.positionTime.Add(keyList.Time / assimpAnim.TicksPerSecond);
                        nodeAnim.position.Add(oap);
                    }
                    foreach (var keyList in nodeAnimLists.ScalingKeys)
                    {
                        var oas = keyList.Value.ToMg();
                        nodeAnim.scaleTime.Add(keyList.Time / assimpAnim.TicksPerSecond);
                        nodeAnim.scale.Add(oas);
                    }
                    // Place this populated node into this model animation
                    modelAnim.animatedNodes.Add(nodeAnim);
                }
                // Place the animation into the model.
                model.animations.Add(modelAnim); 
            }
        }

        #region helper methods.

        /// <summary> Custom get file name.
        /// </summary>
        public string GetFileName(string s, bool useBothSeperators)
        {
            var tpathsplit = s.Split(new char[] { '.' });
            string f = tpathsplit[0];
            if (tpathsplit.Length > 1)
            {
                f = tpathsplit[tpathsplit.Length - 2];
            }
            if (useBothSeperators)
                tpathsplit = f.Split(new char[] { '/', '\\' });
            else
                tpathsplit = f.Split(new char[] { '/' });
            s = tpathsplit[tpathsplit.Length - 1];
            return s;
        }
        
        /// <summary>
        /// Gets the named bone in the model mesh.
        /// </summary>
        private bool GetRiggedBoneForTheMesh(RiggedModel.RiggedModelMesh mesh, string name, out RiggedModel.RiggedModelBone node, out int boneIndexInMesh)
        {
            bool found = false;
            node = null;
            boneIndexInMesh = 0;
            for (int j = 0; j < mesh.modelMeshBones.Length; j++)
            {
                if (mesh.modelMeshBones[j].Name == name)
                {
                    found = true;
                    node = mesh.modelMeshBones[j];
                    boneIndexInMesh = j;
                }
            }
            return found;
        }

        /// <summary>
        /// Gets a name node in the assimp tree.
        /// </summary>
        private bool GetAssimpTreeNode(Assimp.Node treenode, string name, out Assimp.Node node)
        {
            bool found = false;
            node = null;
            if (treenode.Name == name)
            {
                found = true;
                node = treenode;
            }
            else
            {
                foreach (var n in treenode.Children)
                {
                    found = GetAssimpTreeNode(n, name, out node);
                }
            }
            return found;
        }

        /// <summary>
        /// Same as ModelSearchIterateNodeTreeForNameGetRefToNode
        /// </summary>
        private static RiggedModel.RiggedModelNode ModelGetRefToNode(string name, RiggedModel.RiggedModelNode rootNodeOfTree)
        {
            return ModelSearchIterateNodeTreeForNameGetRefToNode(name, rootNodeOfTree);
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

        /// <summary>
        /// Load the textures either from file or from the embeded texture in the model.
        /// </summary>
        public void LoadTexture(ref RiggedModel model,ref Texture2D tex, string ttype, string typeToCheck, int mloop, bool tfileexists, int tindex, int matTextureIndex, string tempDir, string tfilename) //  bool isInContentFolder,
        {
            if (ttype == typeToCheck)
            {
                bool alreadyLoaded = false;
                for(int i =0; i <  model.textures.Count; i++)
                {
                    if (model.textures[i].Name == tfilename)
                    {
                        tex = model.textures[i];
                        alreadyLoaded = true;
                        Console.Write($"\n      MaterialTextureIndex {matTextureIndex}  Type {ttype}  fileName: {tfilename}   Texture[{i}] Assigned....");
                    }
                }
                // ...
                if (alreadyLoaded == false)
                {
                    if (content != null && tfileexists)
                    {
                        var t = content.Load<Texture2D>(tfilename);
                        t.Name = tfilename;
                        model.textures.Add(t);
                        tex = model.textures[model.textures.Count -1];
                        Console.Write($"\n      MaterialTextureIndex {matTextureIndex}  Type {ttype}  fileName: {tfilename}   From disk LOADED....");
                    }
                    else
                    {
                        var t = LoadOrSaveImages(tempDir, ttype, matTextureIndex, tfilename);
                        if (t != null)
                        {
                            t.Name = tfilename;
                            model.textures.Add(t);
                            tex = model.textures[model.textures.Count - 1];
                        }
                    }
                }
            }
        }

        private Texture2D LoadOrSaveImages(string tempDir, string textype, int materialTextureIndex, string tfilename)
        {
            // http://sir-kimmi.de/assimp/lib_html/materials.html
            // maybe can just directly use this here ??

            Texture2D texture = null;
            var texdata = scene.GetEmbeddedTexture(tfilename);
            if (texdata != null)
            {
                var name = texdata.Filename;
                var compressedDataFormatHint = texdata.CompressedFormatHint;
                string filenameext = tfilename.Replace("*", "").ToString() + "." + compressedDataFormatHint;
                string filepath = Path.Combine(tempDir, filenameext);

                // check to make sure the texture isn't loaded already if it is then we need to just reference it.

                if (OutputEmbededTextures == false)
                {
                    // in the advent that the textures were embeded we may have already saved them to disk.
                    if (File.Exists(filepath))
                    {
                        using (FileStream fs = new FileStream(filepath, FileMode.Open))
                        {
                            texture = Texture2D.FromStream(graphicsDevice, fs); // load from disk folders if embedded have been saved and are present.
                            Console.Write($"\n      MaterialTextureIndex {materialTextureIndex}  Type {textype}  fileName: {tfilename}   Compressed Texture Found in file LOADED....");
                        };
                    }
                    else
                    {
                        texture = LoadBytesToTexture(tempDir, texdata.CompressedData); // load the embedded texture from the model.
                        Console.Write($"\n      MaterialTextureIndex {materialTextureIndex}  Type {textype}  fileName: {tfilename}   Compressed Texture from model LOADED....");
                    }

                }
                else
                {
                    File.WriteAllBytes(filepath, texdata.CompressedData); // save the file to disk.
                    using (FileStream fs = new FileStream(filepath, FileMode.Open))
                    {
                        texture = Texture2D.FromStream(graphicsDevice, fs); // load after saving
                        Console.Write($"\n      MaterialTextureIndex {materialTextureIndex}  Type {textype}  fileName: {tfilename}   Compressed Texture Saved to File then LOADED....");
                    };
                }
                texture.Tag = "DisposeMe";
            }
            return texture;
        }
        private Texture2D LoadBytesToTexture(string filepath, byte[] imgByteData)
        {
            Texture2D texture;
            using (var m = new MemoryStream(imgByteData))
            {
                texture = Texture2D.FromStream(graphicsDevice, m);
            }
            return texture;
        }
        /**/
        //private Texture2D SaveLoadThenDeleteImage(string filepath, byte[] imgByteData)
        //{
        //    File.WriteAllBytes(filepath, imgByteData);
        //    Texture2D texture;
        //    using (FileStream fs = new FileStream(filepath, FileMode.Open))
        //    {
        //        texture = Texture2D.FromStream(graphicsDevice, fs);
        //    };
        //    File.Delete(filepath);
        //    return texture;
        //}



        #endregion

        public class TempVerticeWeightsIndexs
        {
            public int countOfBoneEntrysForThisVertice = 0;
            public List<float> verticesFlatBoneId = new List<float>();
            public List<int> verticeIndexs = new List<int>();
            public List<float> verticeBoneWeights = new List<float>();
        }
    }
}
