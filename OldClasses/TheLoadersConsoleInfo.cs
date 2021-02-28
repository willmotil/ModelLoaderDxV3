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


namespace AssimpLoaderPbrDx
{
    public class TheLoadersConsoleInfo
    {
        RiggedModelLoader rml;

        bool showProperties = false;

        public TheLoadersConsoleInfo(RiggedModelLoader inst)
        {
            rml = inst;
        }

        public void WriteHeader(string msg)
        {
            Console.Write("\n\n@@@@@@@@@@@@");
            Console.Write($" {msg}");
            Console.Write("\n\n@@@@@@@@@@@@ \n");
        }

        #region console model output mostly

        public void DisplayConsoleInfo(Assimp.Scene scene, RiggedModel model, string filePath)
        {
            if (rml.startupDisplayLoadedModelConsoleinfo)
            {
                Console.Write("\n\n\n\n****************************************************");
                Console.WriteLine("\n\n@@@DisplayInfo \n");
                Console.WriteLine("\n");
                Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
                Console.WriteLine("Model");
                Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
                Console.WriteLine();
                Console.WriteLine("FileName");
                Console.WriteLine(rml.GetFileName(filePath, true));
                Console.WriteLine();
                Console.WriteLine("Path:");
                Console.WriteLine(filePath);
                Console.WriteLine();

                Console.WriteLine();
                Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
                Console.WriteLine("Metadata");
                Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
                Console.WriteLine("");
                var aiMetadata = scene.Metadata.ToList();
                for (int i = 0; i < aiMetadata.Count; i++)
                {
                    var aiLight = aiMetadata[i];
                    Console.Write("\n aiMetadata " + i + " of " + (aiMetadata.Count - 1) + "");
                    Console.Write("\n aiMetadata.ToString(): " + aiMetadata.ToString());

                }
                Console.WriteLine("");

                Console.WriteLine();
                Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
                Console.WriteLine("Lights");
                Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
                Console.WriteLine("");
                var aiLights = scene.Lights;
                for (int i = 0; i < aiLights.Count; i++)
                {
                    var aiLight = aiLights[i];
                    Console.Write("\n aiLight " + i + " of " + (aiLights.Count - 1) + "");
                    Console.Write("\n aiLight.Name: " + aiLight.Name);

                }
                Console.WriteLine("");

                Console.WriteLine("");
                Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
                Console.WriteLine("Cameras");
                Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
                Console.WriteLine("");
                var aiCameras = scene.Cameras;
                for (int i = 0; i < aiCameras.Count; i++)
                {
                    var aiCamera = aiCameras[i];
                    Console.Write("\n aiCamera " + i + " of " + (aiCameras.Count - 1) + "");
                    Console.Write("\n aiCamera.Name: " + aiCamera.Name);

                }
                Console.WriteLine("");

                Console.WriteLine("");
                Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
                Console.WriteLine("Meshes and Materials");
                Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
                Console.WriteLine("");
                InfoForMeshMaterials(model, rml.scene);
                Console.WriteLine("");

                Console.WriteLine("");
                Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
                Console.WriteLine("Animations");
                Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
                Console.WriteLine("");
                InfoForAnimData(scene, true);
                Console.WriteLine();

                Console.WriteLine("");
                Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
                Console.WriteLine("Node Heirarchy");
                Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
                Console.WriteLine("");
                InfoRiggedModelNodeTreeHeirarchy(model, model.sceneRootNodeOfTree, 0);
                Console.WriteLine("");
            }

            if (rml.startupMinimalConsoleinfo || rml.startupDisplayLoadedModelConsoleinfo)
            {
                MinimalInfo(model, filePath);
            }
        }

        public void InfoRiggedModelNodeTreeHeirarchy(RiggedModel model, RiggedModel.RiggedModelNode n, int tabLevel)
        {
            string ntab = "";
            for (int i = 0; i < tabLevel; i++)
                ntab += "  ";
            string rtab = "\n" + ntab;
            string msg = "\n";

            msg += rtab + $"{n.Name}  ";
            msg += rtab + $"|_children.Count: {n.children.Count} ";
            if (n.parent == null)
                msg += $"|_parent: IsRoot ";
            else
                msg += $"|_parent: " + n.parent.Name;
            msg += rtab + $"|_hasARealBone: {n.hasARealBone} ";
            msg += rtab + $"|_isThisAMeshNode: {n.isThisAMeshNode}";
            if (n.uniqueMeshBones.Count > 0)
            {
                msg += rtab + $"|_uniqueMeshBones.Count: {n.uniqueMeshBones.Count}  ";
                int i = 0;
                foreach (var bone in n.uniqueMeshBones)
                {
                    msg += rtab + $"|_node: {n.Name}  lists  uniqueMeshBone[{i}] ...  meshIndex: {bone.meshIndex}  meshBoneIndex: {bone.meshBoneIndex}   mesh[{bone.meshIndex}]bone[{bone.meshBoneIndex}].Name: {model.meshes[bone.meshIndex].modelMeshBones[bone.meshBoneIndex].Name}  in  mesh[{bone.meshIndex}].Name: {model.meshes[bone.meshIndex].Name}";
                    var nameToCompare = model.meshes[bone.meshIndex].modelMeshBones[bone.meshBoneIndex].Name;
                    int j = 0;
                    foreach (var anim in model.animations)
                    {
                        int k = 0;
                        foreach (var animNode in anim.animatedNodes)
                        {
                            if (animNode.nodeName == nameToCompare)
                                msg += rtab + $"|^has corresponding Animation[{j}].Node[{k}].Name: {animNode.nodeName}  positionKeyCount: {animNode.position.Count}  rotationKeyCount: {animNode.qrot.Count}  scaleKeyCount: {animNode.scale.Count}";
                            k++;
                        }
                        j++;
                    }
                    i++;
                }
            }

            Console.WriteLine(msg);

            for (int i = 0; i < n.children.Count; i++)
            {
                InfoRiggedModelNodeTreeHeirarchy(model, n.children[i], tabLevel + 1);
            }
        }

        public void InfoForAnimData(Scene scene, bool showDetailedInfo)
        {
            for (int i = 0; i < rml.scene.Animations.Count; i++)
            {
                var anim = rml.scene.Animations[i];
                Console.WriteLine("");
                Console.WriteLine($"_____________________________________");
                Console.WriteLine($"Anim #[{i}] Name: {anim.Name}");
                Console.WriteLine($"_____________________________________");
                Console.WriteLine($"  DurationInTicks: {anim.DurationInTicks}");
                Console.WriteLine($"  TicksPerSecond: {anim.TicksPerSecond}");
                Console.WriteLine($"  DurationInTicks / TicksPerSecond: {anim.DurationInTicks} / {anim.TicksPerSecond} sec.   total duration in seconds: {anim.DurationInTicks / anim.TicksPerSecond}");
                Console.WriteLine($"  Node Animation Channels: {anim.NodeAnimationChannelCount} ");
                Console.WriteLine($"  Mesh Animation Channels: {anim.MeshAnimationChannelCount} ");
                Console.WriteLine($"  Mesh Morph     Channels: {anim.MeshMorphAnimationChannelCount} ");
                Console.WriteLine("");
                for (int j = 0; j < anim.NodeAnimationChannels.Count; j++)
                {
                    var nodes = anim.NodeAnimationChannels[j];
                    Console.WriteLine($"  anim.NodeAnimationChannels[{j}]  NodeName: {nodes.NodeName}  PreState: {nodes.PreState}  PostState: {nodes.PostState}  Positions: {nodes.PositionKeyCount}  Rotations: {nodes.RotationKeyCount}  Scalings: {nodes.ScalingKeyCount}");
                }
                for (int j = 0; j < anim.MeshAnimationChannels.Count; j++)
                {
                    var nodes = anim.MeshAnimationChannels[j];
                    Console.WriteLine($"  anim.MeshAnimationChannels[{j}]   MeshName: {nodes.MeshName}   HasMeshKeys: {nodes.HasMeshKeys}   MeshKeyCount: {nodes.MeshKeyCount} ");
                }
                for (int j = 0; j < anim.MeshMorphAnimationChannels.Count; j++)
                {
                    var nodes = anim.MeshMorphAnimationChannels[j];
                    Console.WriteLine($"  anim.MeshMorphAnimationChannels[{j}]   Name: {nodes.Name}   HasMeshMorphKeys: {nodes.HasMeshMorphKeys}  MeshMorphKeyCount: {nodes.MeshMorphKeyCount}");
                }

            }
        }

        /*
         * https://marmoset.co/posts/physically-based-rendering-and-you-can-too/
         * 
         * 
From what I can pick up form this thread (and related), you seem to be converging on a model which effectively does the following

diffuseTerm = * ? // extension ; texture is optional.
specularTerm = * ? // extension
shineTerm = * ? // extension
emissiveTerm = * ? // part of core Materials spec

Textures are optional, and if provided, will modulate the base Factor value?

color = emissiveTerm +
ambientFactor * aL +
diffuseTerm * max(N * L, 0) +
specularTerm * max(H * N, 0)^shineTerm

where N (normal) can be geometric or can be provided by tangent-space normalTexture (part of core material spec).

Is that a fair assessment?
I like this model as it is simple (to generate and implement) and covers most cases.

I ask because we're building a pipeline and viewer based around glTF2.0 and we've got a ton of 'old' (pre-PBR) model data which we'd like to get converted, and it would be nice to have a clear definition to build against.
@UX3D-nopper
Contributor
UX3D-nopper commented on Jul 5, 2017

Basically. it is what you have written, but the ambientTerm/ambientFactor is equal to the diffuseTerm:
color = emissiveTerm +
diffuseTerm * aL +
diffuseTerm * max(N * L, 0) +
specularTerm * max(H * N, 0)^shineTerm

Please also have a look at the Khrons Blender glTF 2.0 exporter:
https://github.com/KhronosGroup/glTF-Blender-Exporter
During export, you can enable experimental Blinn-Phong export.

The reason to drop the ambient is because of a long discussion with three.js.

The following code snippet is the current formula, on how to calculate the terms:

vec3 diffuseColor = colorToLinear(texture(u_diffuseTexture, texcoord_0.st).rgb) * u_bufferMaterial_Common.diffuseFactor.rgb * color_0.rgb;
alpha = texture(u_diffuseTexture, texcoord_0.st).a * u_bufferMaterial_Common.diffuseFactor.a * color_0.a;
vec3 specularColor = colorToLinear(texture(u_specularTexture, texcoord_0.st).rgb) * u_bufferMaterial_Common.specularFactor.rgb;
float shininess = texture(u_shininessTexture, texcoord_0.st).g * u_bufferMaterial_Common.shininessFactor;

Yes, for emissive it is the same:

vec3 emissive = colorToLinear(texture(u_emissiveTexture, texcoord_0.st).rgb) * u_bufferMaterial.emissiveFactor;
float occlusion = texture(u_occlusionTexture, texcoord_0.st).r;

There will be still an ambient light in the scene. But the light needs to be adapted, as it directly uses the diffuse term.

Thanks.
I had not noticed occlusion in the core spec. Am guessing this is baked ambient occlusion, and affects everything outside of the emissiveTerm

color = emissiveTerm +
occlusionTerm * (ambient + diffuse + specular)

I note there's a pattern here; occlusionTexture.r and shininessTexture.g suggests these textures can be combined into a single map?
should shininessMap (scalar) be cooked into the alpha channel of the specularTexture?
specular = specularTexture.rgb
shininess = specularTexture.a

            https://threejs.org/docs/#api/en/materials/MeshPhongMaterial
            .lightMap : Texture
            The light map. Default is null. The lightMap requires a second set of UVs.

            .bumpMap : Texture
The texture to create a bump map. The black and white values map to the perceived depth in relation to the lights. Bump doesn't actually affect the geometry of the object, only the lighting. If a normal map is defined this will be ignored. 

            .normalMap : Texture
The texture to create a normal map. The RGB values affect the surface normal for each pixel fragment and change the way the color is lit. Normal maps do not change the actual shape of the surface, only the lighting. In case the material has a normal map authored using the left handed convention, the y component of normalScale should be negated to compensate for the different handedness. 
            
            // three js shader.
            https://github.com/mrdoob/three.js/blob/master/src/materials/MeshPhongMaterial.js

            https://github.com/assimp/assimp/pull/2640
            This is to support Maya materials properly from FBX Importer.

This adds the following texture types:

    BASE_COLOR
    NORMAL_CAMERA
    EMISSION_COLOR
    METALNESS
    DIFFUSE_ROUGHNESS
    AMBIENT_OCCLUSION

Here is an example of the work in use:
godotengine/godot#32019

Reuses existing API as it is fairly simple to use.

The existing 'ambient', 'normal' maps are not the same shading method, therefore I opted to implement proper enums for them, as they're for the legacy format only, which is easier to support than the PBR / PBS route.


            So if anyone has the same problem: GetTexture(aiTextureType_UNKNOWN) returns a path to the pbr texture. 

            https://github.com/assimp/assimp/pull/1423
            pbrMetallicRoughness  pbrSpecularGlossiness

            https://github.com/KhronosGroup/glTF/tree/master/specification/2.0

            http://github.khronos.org/glTF-Validator/

            https://github.com/KhronosGroup/glTF-Sample-Models

            // example for loading embedded textures 
            https://github.com/assimp/assimp/issues/408

            https://github.com/assimp/assimp/issues/1830
         */

        public void PrintTextureTypes(Material mat, TextureType typeToDisplay)
        {
            var name = mat.Name;
            var ttype = typeToDisplay;
            var texSlot = mat.GetMaterialTextures(ttype);
            for (int kl = 0; kl < texSlot.Length; kl++)
            {
                Console.WriteLine("      PrintTextureTypes:  Material: " + name + "   textureType: " + ttype + "   textureIndex: " + texSlot[kl].TextureIndex + "  Flags: " + texSlot[kl].Flags.ToString() + "  FilePath: " + texSlot[kl].FilePath);
            }
        }

        public void InfoForMeshMaterials(RiggedModel model, Scene scene)
        {
            //
            // Loop meshes for Vertice data.
            //

            Console.WriteLine("InfoForMaterials");

            Console.WriteLine("Each mesh has a listing of bones that apply to it this is just a reference to the bone.");
            Console.WriteLine("Each mesh has a corresponding Offset matrix for that bone.");
            Console.WriteLine("Important.");
            Console.WriteLine("This means that offsets are not common across meshes but bones can be.");
            Console.WriteLine("To say the same bone node may apply to different meshes but that same bone will have a different applicable offset per mesh.");
            Console.WriteLine("Each mesh also has a corresponding bone weight per mesh.");
            Console.WriteLine("");

            Console.WriteLine("\n" + "__________________________");
            Console.WriteLine("scene.HasTextures");
            Console.WriteLine("");

            if (scene.HasTextures)
            {
                var texturescount = scene.TextureCount;
                var textures = scene.Textures;
                Console.WriteLine("\n  Embedded Textures " + " Count " + texturescount);
                for (int i = 0; i < textures.Count; i++)
                {
                    bool hasNonCompressedData = false;
                    if (textures[i].NonCompressedData != null)
                        hasNonCompressedData = true;
                    var width = textures[i].Width;
                    var height = textures[i].Height;
                    var hasCompressedData = textures[i].HasCompressedData;
                    var compressedDataFormatHint = textures[i].CompressedFormatHint;
                    int compressedDataLength = 0;
                    if (hasCompressedData)
                    {
                        compressedDataLength = textures[i].CompressedData.Length;
                        Console.WriteLine("    Embedded Textures[" + i + "].Name: " + textures[i].Filename + "  hasCompressedData: " + hasCompressedData + "  compressedDataFormatHint: " + compressedDataFormatHint + " compressedDataLength: " + compressedDataLength);
                    }
                    if (hasNonCompressedData)
                    {
                        Console.WriteLine("    Embedded Textures[" + i + "].Name: " + textures[i].Filename + "  hasNonCompressedData: " + hasNonCompressedData + "   width: " + width + " height: " + height);
                    }
                }
            }
            else
            {
                Console.WriteLine("\n    Embedded Textures " + " None ");
            }
            Console.WriteLine("");

            Console.WriteLine("__________________________");
            Console.WriteLine("scene.Materials");
            Console.WriteLine("");
            for (int k = 0; k < scene.Materials.Count; k++)
            {
                var mat = scene.Materials[k];
                PrintTextureTypes(mat, TextureType.Ambient);
                PrintTextureTypes(mat, TextureType.AmbientOcclusion);
                PrintTextureTypes(mat, TextureType.BaseColor);
                PrintTextureTypes(mat, TextureType.Diffuse);
                PrintTextureTypes(mat, TextureType.Displacement);
                PrintTextureTypes(mat, TextureType.EmissionColor);
                PrintTextureTypes(mat, TextureType.Emissive);
                PrintTextureTypes(mat, TextureType.Height);
                PrintTextureTypes(mat, TextureType.Lightmap);
                PrintTextureTypes(mat, TextureType.Metalness);
                PrintTextureTypes(mat, TextureType.None);
                PrintTextureTypes(mat, TextureType.NormalCamera);
                PrintTextureTypes(mat, TextureType.Normals);
                PrintTextureTypes(mat, TextureType.Opacity);
                PrintTextureTypes(mat, TextureType.Reflection);
                PrintTextureTypes(mat, TextureType.Roughness);
                PrintTextureTypes(mat, TextureType.Shininess);
                PrintTextureTypes(mat, TextureType.Specular);
                PrintTextureTypes(mat, TextureType.Unknown);
            }
            Console.WriteLine("\n");

            Console.WriteLine("__________________________");
            Console.WriteLine("model.meshes");
            Console.WriteLine("");
            Console.WriteLine("Bone ([0] is a generated bone to the mesh)");
            Console.WriteLine("");
            for (int mmLoop = 0; mmLoop < model.meshes.Length; mmLoop++)
            {
                var rmMesh = model.meshes[mmLoop];
                Console.WriteLine("Model mesh # " + mmLoop + " of  " + model.meshes.Length + "   Name: " + rmMesh.Name + "   MaterialIndex: " + rmMesh.MaterialIndex + "  MaterialIndexName: " + rmMesh.MaterialIndexName + "  Bones.Count " + model.meshes[mmLoop].modelMeshBones.Count() + " TechniqueName: " + model.meshes[mmLoop].techniqueName);
                if (rmMesh.textureDiffuse != null)
                    Console.WriteLine("texture: " + rmMesh.textureDiffuse.Name);
                if (rmMesh.textureNormalMap != null)
                    Console.WriteLine("textureNormalMap: " + rmMesh.textureNormalMap.Name);
                if (rmMesh.textureHeightMap != null)
                    Console.WriteLine("textureHeightMap: " + rmMesh.textureHeightMap.Name);
                if (rmMesh.textureLightMap != null)
                    Console.WriteLine("textureLightMap: " + rmMesh.textureLightMap.Name);
            }

            Console.WriteLine("__________________________");
            Console.WriteLine("scene.Meshes");
            Console.WriteLine("");
            for (int amLoop = 0; amLoop < scene.Meshes.Count; amLoop++)
            {
                Mesh assimpMesh = scene.Meshes[amLoop];

                Console.WriteLine(
                "\n" + "__________________________" +
                "\n" + "scene.Meshes[" + amLoop + "] " + assimpMesh.Name +
                "\n" + " FaceCount: " + assimpMesh.FaceCount +
                "\n" + " VertexCount: " + assimpMesh.VertexCount +
                "\n" + " Normals.Count: " + assimpMesh.Normals.Count +
                "\n" + " Bones.Count: " + assimpMesh.Bones.Count +
                "\n" + " MaterialIndex: " + assimpMesh.MaterialIndex
                );
                Console.WriteLine(
                "\n" + " MorphMethod: " + assimpMesh.MorphMethod +
                "\n" + " HasMeshAnimationAttachments: " + assimpMesh.HasMeshAnimationAttachments
                );

                Console.WriteLine(" UVComponentCount.Length: " + assimpMesh.UVComponentCount.Length);
                for (int i = 0; i < assimpMesh.UVComponentCount.Length; i++)
                {
                    int val = assimpMesh.UVComponentCount[i];
                    if (val > 0)
                        Console.WriteLine("   mesh.UVComponentCount[" + i + "] : int value: " + val);
                }
                Console.WriteLine(" TextureCoordinateChannels.Length:" + assimpMesh.TextureCoordinateChannels.Length);
                Console.WriteLine(" TextureCoordinateChannelCount:" + assimpMesh.TextureCoordinateChannelCount);
                for (int i = 0; i < assimpMesh.TextureCoordinateChannels.Length; i++)
                {
                    var channel = assimpMesh.TextureCoordinateChannels[i];
                    if (channel.Count > 0)
                        Console.WriteLine("   mesh.TextureCoordinateChannels[" + i + "]  count " + channel.Count);
                    for (int j = 0; j < channel.Count; j++)
                    {
                        // holds uvs and ? i think
                        //Console.Write(" channel[" + j + "].Count: " + channel.Count);
                    }
                }

                int matIndex = assimpMesh.MaterialIndex;
                var assimpMaterial = scene.Materials[matIndex];
                var IsPbrprop = assimpMaterial.IsPBRMaterial;
                var pbrprop = assimpMaterial.PBR;
                var HasTextureBaseColor = pbrprop.HasTextureBaseColor;
                var HasTextureEmissionColor = pbrprop.HasTextureEmissionColor;
                var HasTextureMetalness = pbrprop.HasTextureMetalness;
                var HasTextureRoughness = pbrprop.HasTextureRoughness;
                var HasTextureNormalCamera = pbrprop.HasTextureNormalCamera;

                var TextureBaseColor = pbrprop.TextureBaseColor;
                var TextureEmissionColor = pbrprop.TextureEmissionColor;
                var TextureMetalness = pbrprop.TextureMetalness;
                var TextureNormalCamera = pbrprop.TextureNormalCamera;
                var TextureRoughness = pbrprop.TextureRoughness;

                Console.WriteLine("    ");
                Console.WriteLine("    " + "assimpMaterial.IsPBR: " + assimpMaterial.IsPBRMaterial);
                Console.WriteLine("    " + "pbrprop.HasTextureBaseColor: " + pbrprop.HasTextureBaseColor);
                Console.WriteLine("    " + "pbrprop.HasTextureEmissionColor: " + pbrprop.HasTextureEmissionColor);
                Console.WriteLine("    " + "pbrprop.HasTextureMetalness: " + pbrprop.HasTextureMetalness);
                Console.WriteLine("    " + "pbrprop.HasTextureRoughness: " + pbrprop.HasTextureRoughness);
                Console.WriteLine("    " + "pbrprop.HasTextureNormalCamera: " + pbrprop.HasTextureNormalCamera);

                //Console.WriteLine("    " + "pbrprop.TextureBaseColor.FilePath: " + pbrprop.TextureBaseColor.FilePath.ToString());
                //Console.WriteLine("    " + "pbrprop.TextureEmissionColor.FilePath: " + pbrprop.TextureEmissionColor.FilePath.ToString());
                //Console.WriteLine("    " + "pbrprop.TextureMetalness.FilePath: " + pbrprop.TextureMetalness.FilePath.ToString());
                //Console.WriteLine("    " + "pbrprop.TextureRoughness.FilePath: " + pbrprop.TextureRoughness.FilePath.ToString());
                //Console.WriteLine("    " + "pbrprop.TextureNormalCamera.FilePath: " + pbrprop.TextureNormalCamera.FilePath.ToString());

                Console.WriteLine();
                Console.WriteLine("\n    " + "__________________________");
                Console.WriteLine("\n    " + "properties for this mesh material.");
                Console.WriteLine();

                if (showProperties)
                {
                    var matprop = assimpMaterial.GetAllProperties();
                    for (int m = 0; m < matprop.Length; m++)
                    {
                        var prop = matprop[m];
                        var FullyQualifiedName = prop.FullyQualifiedName;
                        Console.WriteLine("    " + "property[" + m + "].FullyQualifiedName: " + FullyQualifiedName);
                        Console.WriteLine("    " + "     property[" + m + "].Name: " + prop.Name);
                        Console.WriteLine("    " + "     property[" + m + "].GetType(): " + prop.GetType());
                        Console.WriteLine("    " + "     property[" + m + "].PropertyType: " + prop.PropertyType);
                        Console.WriteLine("    " + "     property[" + m + "].GetStringValue: " + prop.GetStringValue());
                        Console.WriteLine("    " + "     property[" + m + "].TextureType: " + prop.TextureType);
                        Console.WriteLine("    " + "     property[" + m + "].TextureIndex: " + prop.TextureIndex);
                        Console.WriteLine("    " + "     property[" + m + "].HasRawData: " + prop.HasRawData);
                        Console.WriteLine("    " + "     property[" + m + "].ByteCount: " + prop.ByteCount);

                    }
                }

                Console.WriteLine();
                Console.WriteLine("\n    " + "__________________________");
                Console.WriteLine("\n    " + "Loaded maps for this mesh.");
                Console.WriteLine();
                if (model.meshes[amLoop].textureDiffuse != null)
                    Console.WriteLine("    " + "textureDiffuse " + model.meshes[amLoop].textureDiffuse.Name);
                if (model.meshes[amLoop].textureBaseColorMapPbr != null)
                    Console.WriteLine("    " + "textureBaseColorMap " + model.meshes[amLoop].textureBaseColorMapPbr.Name);
                if (model.meshes[amLoop].textureAmbientOcclusionOraLightMap != null)
                    Console.WriteLine("    " + "textureAmbientOcclusion " + model.meshes[amLoop].textureAmbientOcclusionOraLightMap.Name);
                if (model.meshes[amLoop].textureDisplacementMap != null)
                    Console.WriteLine("    " + "textureDisplacementMap " + model.meshes[amLoop].textureDisplacementMap.Name);
                if (model.meshes[amLoop].textureEmissiveMap != null)
                    Console.WriteLine("    " + "textureEmissiveMap " + model.meshes[amLoop].textureEmissiveMap.Name);
                if (model.meshes[amLoop].textureHeightMap != null)
                    Console.WriteLine("    " + "textureHeightMap " + model.meshes[amLoop].textureHeightMap.Name);
                if (model.meshes[amLoop].textureLightMap != null)
                    Console.WriteLine("    " + "textureLightMap " + model.meshes[amLoop].textureLightMap.Name);
                if (model.meshes[amLoop].textureMetalnessPbr != null)
                    Console.WriteLine("    " + "textureMetalness " + model.meshes[amLoop].textureMetalnessPbr.Name);
                if (model.meshes[amLoop].textureNormalMapPrbCamera != null)
                    Console.WriteLine("    " + "textureNormalCamera " + model.meshes[amLoop].textureNormalMapPrbCamera.Name);
                if (model.meshes[amLoop].textureNormalMap != null)
                    Console.WriteLine("    " + "textureNormalMap " + model.meshes[amLoop].textureNormalMap.Name);
                if (model.meshes[amLoop].textureReflectionMap != null)
                    Console.WriteLine("    " + "textureReflectionMap " + model.meshes[amLoop].textureReflectionMap.Name);
                if (model.meshes[amLoop].textureRoughnessPbr != null)
                    Console.WriteLine("    " + "textureRoughness " + model.meshes[amLoop].textureRoughnessPbr.Name);
                if (model.meshes[amLoop].textureShine != null)
                    Console.WriteLine("    " + "textureShine " + model.meshes[amLoop].textureShine.Name);
                if (model.meshes[amLoop].textureSpecular != null)
                    Console.WriteLine("    " + "textureSpecular " + model.meshes[amLoop].textureSpecular.Name);
                if (model.meshes[amLoop].textureUnknown != null)
                    Console.WriteLine("    " + "textureUnknown " + model.meshes[amLoop].textureUnknown.Name);
            }

            Console.WriteLine("\n" + "__________________________");

            if (scene.HasMaterials)
            {
                Console.WriteLine("\n    Materials scene.MaterialCount " + scene.MaterialCount + "\n");
                for (int i = 0; i < scene.Materials.Count; i++)
                {
                    Console.WriteLine();
                    Console.WriteLine("\n    " + "__________________________");
                    Console.WriteLine("    Material[" + i + "] ");
                    Console.WriteLine("    Material[" + i + "].Name " + scene.Materials[i].Name);

                    var material = scene.Materials[i];
                    var textureSlot = material.GetAllMaterialTextures();
                    Console.WriteLine("    GetAllMaterialTextures Length " + textureSlot.Length);
                    Console.WriteLine();

                    // added info
                    Console.WriteLine("    ");
                    Console.WriteLine("    Material[" + i + "] " + "  IsPBRMaterial " + material.IsPBRMaterial + "  PBR " + material.PBR.ToString() + "  PropertyCount " + material.PropertyCount);
                    Console.WriteLine("    Material[" + i + "] " + "  HasTransparencyFactor: " + material.HasTransparencyFactor + "  TransparencyFactor: " + material.TransparencyFactor + "  HasOpacity: " + material.HasOpacity + "  Opacity: " + material.Opacity + "  HasShininess:" + material.HasShininess + "  Shininess:" + material.Shininess + "  HasReflectivity: " + material.HasReflectivity + "  Reflectivity " + scene.Materials[i].Reflectivity);
                    Console.WriteLine("    Material[" + i + "] " + "  HasTwoSided: " + material.HasTwoSided + "  IsTwoSided: " + material.IsTwoSided + "  HasBlendMode:" + material.HasBlendMode + "  BlendMode:" + material.BlendMode + "  HasShadingMode: " + material.HasShadingMode + "  ShadingMode:" + material.ShadingMode + "  HasBumpScaling: " + material.HasBumpScaling);
                    Console.WriteLine("    ");
                    Console.WriteLine("    Material[" + i + "] " + "  PBR.HasTextureBaseColor " + material.PBR.HasTextureBaseColor + "  PBR.HasTextureEmissionColor " + material.PBR.HasTextureEmissionColor + "  PBR.HasTextureMetalness " + material.PBR.HasTextureMetalness);
                    Console.WriteLine("    Material[" + i + "] " + "  PBR.HasTextureNormalCamera " + material.PBR.HasTextureNormalCamera + "  PBR.HasTextureRoughness " + material.PBR.HasTextureRoughness);
                    Console.WriteLine("    ");
                    Console.WriteLine("    Material[" + i + "] " + "  HasTextureDiffuse " + material.HasTextureDiffuse + "  HasTextureAmbient " + material.HasTextureAmbient + "  HasTextureAmbientOcclusion " + material.HasTextureAmbientOcclusion + "  HasTextureSpecular " + material.HasTextureSpecular);
                    Console.WriteLine("    Material[" + i + "] " + "  HasTextureNormal " + material.HasTextureNormal + "  HasTextureHeight " + material.HasTextureHeight + "  HasTextureDisplacement:" + material.HasTextureDisplacement + "  HasTextureLightMap " + material.HasTextureLightMap);
                    Console.WriteLine("    Material[" + i + "] " + "  HasTextureReflection:" + material.HasTextureReflection + "  HasTextureOpacity " + material.HasTextureOpacity + "  HasTextureEmissive:" + material.HasTextureEmissive);
                    Console.WriteLine("    ");
                    Console.WriteLine("    Material[" + i + "] " + "  HasColorDiffuse " + material.HasColorDiffuse + "  HasColorAmbient " + material.HasColorAmbient + "  HasColorSpecular " + material.HasColorSpecular);
                    Console.WriteLine("    Material[" + i + "] " + "  HasColorReflective " + material.HasColorReflective + "  HasColorEmissive " + material.HasColorEmissive + "  HasColorTransparent " + material.HasColorTransparent);
                    Console.WriteLine("    ");
                    Console.WriteLine("    Material[" + i + "] " + "  ColorAmbient:" + material.ColorAmbient + "  ColorDiffuse: " + material.ColorDiffuse + "  ColorSpecular: " + material.ColorSpecular);
                    Console.WriteLine("    Material[" + i + "] " + "  ColorReflective:" + material.ColorReflective + "  ColorEmissive: " + material.ColorEmissive + "  ColorTransparent: " + material.ColorTransparent);
                    Console.WriteLine();

                    // https://github.com/assimp/assimp/issues/3027
                    // If the texture data is embedded, the host application can then load 'embedded' texture data directly from the aiScene.mTextures array.

                    for (int j = 0; j < textureSlot.Length; j++)
                    {
                        var tindex = textureSlot[j].TextureIndex;
                        var toperation = textureSlot[j].Operation;
                        var ttype = textureSlot[j].TextureType.ToString();
                        var flags = textureSlot[j].Flags;
                        var mapping = textureSlot[j].Mapping;
                        var uvindex = textureSlot[j].UVIndex;
                        var wrapModeU = textureSlot[j].WrapModeU;
                        var wrapModeV = textureSlot[j].WrapModeV;
                        var blendfactor = textureSlot[j].BlendFactor;
                        var tfilepath = textureSlot[j].FilePath;
                        // J matches up to the texture coordinate channel uv count it looks like.
                        Console.WriteLine("    Material[" + i + "].TextureSlot[" + j + "] " + "  TextureIndex: " + tindex + "  Type: " + ttype + "  Operation: " + toperation + "  Flags: " + flags + "  Mapping: " + mapping + "  Blendfactor: " + blendfactor + "   Filepath: " + tfilepath);
                    }
                    Console.WriteLine();
                }
            }
            else
            {
                Console.WriteLine("\n   No Materials Present. \n");
            }

            Console.WriteLine();
            Console.WriteLine("\n" + "__________________________");
            Console.WriteLine("Bones in meshes");

            for (int mindex = 0; mindex < model.meshes.Length; mindex++)
            {
                var rmMesh = model.meshes[mindex];

                Console.WriteLine();
                Console.WriteLine("\n" + "__________________________");
                Console.WriteLine("Bones in mesh[" + mindex + "]   " + rmMesh.Name);
                Console.WriteLine();

                if (rmMesh.HasBones)
                {
                    var meshBones = rmMesh.modelMeshBones;
                    Console.WriteLine(" meshBones.Length: " + meshBones.Length);
                    for (int meshBoneIndex = 0; meshBoneIndex < meshBones.Length; meshBoneIndex++)
                    {
                        var boneInMesh = meshBones[meshBoneIndex]; // ahhhh
                        var boneInMeshName = meshBones[meshBoneIndex].Name;

                        string str = "   mesh[" + mindex + "].Name: " + rmMesh.Name + "   bone[" + meshBoneIndex + "].Name: " + boneInMeshName + "   meshBoneIndex: " + meshBoneIndex.ToString() + "   WeightCount: " + boneInMesh.numberOfAssociatedWeightedVertices;
                        //str += "\n" + "   OffsetMatrix " + boneInMesh.OffsetMatrix;
                        Console.WriteLine(str);
                    }
                }
                Console.WriteLine();
            }
        }

        public void MinimalInfo(RiggedModel model, string filePath)
        {
            Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
            Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
            Console.WriteLine();
            Console.WriteLine($"Model");
            Console.WriteLine($"{rml.GetFileName(filePath, true)}  Loaded");
            Console.WriteLine();
            Console.WriteLine($"Model Scale: {rml.Scale}"); // dunno why i dont use the brackets really its so much easier.
            Console.WriteLine("Model ScaleCoefficent: " + model.ScaleCoefficent);
            Console.WriteLine("Model sceneRootNodeOfTree's Node Name:     " + model.sceneRootNodeOfTree.Name);
            Console.WriteLine("Model number of animaton: " + model.animations.Count);
            Console.WriteLine("Model number of meshes:   " + model.meshes.Length);
            Console.WriteLine();
            Console.WriteLine("End of Model info");
            Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
            Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
            Console.WriteLine("\n");
        }

        #endregion

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
    }


    public class ModelConsoleInfoOnly
    {
        public ModelConsoleInfoOnly(RiggedModel model, bool showTreeInfo, bool showMeshInfo, bool showBoneInfo)
        {
            Console.Write("\n\n\n\n****************************************************");
            Console.WriteLine("\n\n@@@DisplayInfo \n");
            Console.WriteLine("\n");
            Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
            Console.WriteLine("Model");
            Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
            Console.WriteLine();
            Console.WriteLine("FileName");
            Console.WriteLine(model.Name);
            Console.WriteLine();

            if (showTreeInfo)
            {
                Console.WriteLine();
                Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
                Console.WriteLine("Node Tree Heirarchy");
                Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
                Console.WriteLine("");
                InfoRiggedModelNodeTreeHeirarchy(model, model.sceneRootNodeOfTree, 0);
            }

            if (showMeshInfo)
            {
                Console.WriteLine();
                Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
                Console.WriteLine("Mesh Info");
                Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
                Console.WriteLine("");
                InfoRiggedModelMeshes(model, model.sceneRootNodeOfTree, 0, showBoneInfo);
            }

        }

        public void InfoRiggedModelNodeTreeHeirarchy(RiggedModel model, RiggedModel.RiggedModelNode n, int tabLevel)
        {
            string ntab = "";
            for (int i = 0; i < tabLevel; i++)
                ntab += "  ";
            string rtab = "\n" + ntab;
            string msg = "\n";

            msg += rtab + $"{n.Name}  ";
            msg += rtab + $"|_children.Count: {n.children.Count} ";
            if (n.parent == null)
                msg += $"|_parent: IsRoot ";
            else
                msg += $"|_parent: " + n.parent.Name;
            msg += rtab + $"|_hasARealBone: {n.hasARealBone} ";
            msg += rtab + $"|_isThisAMeshNode: {n.isThisAMeshNode}";
            if (n.uniqueMeshBones.Count > 0)
            {
                msg += rtab + $"|_uniqueMeshBones.Count: {n.uniqueMeshBones.Count}  ";
                int i = 0;
                foreach (var bone in n.uniqueMeshBones)
                {
                    msg += rtab + $"|_node: {n.Name}  lists  uniqueMeshBone[{i}] ...  meshIndex: {bone.meshIndex}  meshBoneIndex: {bone.meshBoneIndex}   mesh[{bone.meshIndex}]bone[{bone.meshBoneIndex}].Name: {model.meshes[bone.meshIndex].modelMeshBones[bone.meshBoneIndex].Name}  in  mesh[{bone.meshIndex}].Name: {model.meshes[bone.meshIndex].Name}";
                    var nameToCompare = model.meshes[bone.meshIndex].modelMeshBones[bone.meshBoneIndex].Name;
                    int j = 0;
                    foreach (var anim in model.animations)
                    {
                        int k = 0;
                        foreach (var animNode in anim.animatedNodes)
                        {
                            if (animNode.nodeName == nameToCompare)
                                msg += rtab + $"|^has corresponding Animation[{j}].Node[{k}].Name: {animNode.nodeName}  positionKeyCount: {animNode.position.Count}  rotationKeyCount: {animNode.qrot.Count}  scaleKeyCount: {animNode.scale.Count}";
                            k++;
                        }
                        j++;
                    }
                    i++;
                }
            }

            Console.WriteLine(msg);

            for (int i = 0; i < n.children.Count; i++)
            {
                InfoRiggedModelNodeTreeHeirarchy(model, n.children[i], tabLevel + 1);
            }
        }

        public void InfoRiggedModelMeshes(RiggedModel model, RiggedModel.RiggedModelNode n, int tabLevel, bool showBoneInfo)
        {
            Console.WriteLine("\n");

            Console.WriteLine("__________________________");
            Console.WriteLine("Mesh info");
            Console.WriteLine("");
            Console.WriteLine("Bone ([0] is a generated bone to the mesh)");
            Console.WriteLine("");


            for (int mmLoop = 0; mmLoop < model.meshes.Length; mmLoop++)
            {
                var rmMesh = model.meshes[mmLoop];

                Console.WriteLine("__________________________");
                Console.WriteLine(
                    $"\n  Model mesh # { mmLoop} of  { model.meshes.Length} " +
                    $"\n  Name: { rmMesh.Name} " +
                    $"\n  MaterialIndex: { rmMesh.MaterialIndex} " +
                    $"\n  MaterialIndexName: { rmMesh.MaterialIndexName} " +
                    $"\n  Vertices.Count {model.meshes[mmLoop].vertices.Count()} " +
                    $"\n  Indices.Count {model.meshes[mmLoop].indices.Count()} " +
                    $"\n  Bones.Count {model.meshes[mmLoop].modelMeshBones.Count()} " +
                    $"\n  TechniqueName: {model.meshes[mmLoop].techniqueName}"
                    );

                var amLoop = mmLoop;
                var mindex = mmLoop;

                Console.WriteLine();
                Console.WriteLine(    "  __________________________");
                Console.WriteLine(  $"  Textures");
                Console.WriteLine();
                string space = "  ";
                if (model.meshes[amLoop].textureDiffuse != null)
                    Console.WriteLine(space + "textureDiffuse " + model.meshes[amLoop].textureDiffuse.Name);
                if (model.meshes[amLoop].textureBaseColorMapPbr != null)
                    Console.WriteLine(space + "textureBaseColorMap " + model.meshes[amLoop].textureBaseColorMapPbr.Name);
                if (model.meshes[amLoop].textureAmbientOcclusionOraLightMap != null)
                    Console.WriteLine(space + "textureAmbientOcclusion " + model.meshes[amLoop].textureAmbientOcclusionOraLightMap.Name);
                if (model.meshes[amLoop].textureDisplacementMap != null)
                    Console.WriteLine(space + "textureDisplacementMap " + model.meshes[amLoop].textureDisplacementMap.Name);
                if (model.meshes[amLoop].textureEmissiveMap != null)
                    Console.WriteLine(space + "textureEmissiveMap " + model.meshes[amLoop].textureEmissiveMap.Name);
                if (model.meshes[amLoop].textureHeightMap != null)
                    Console.WriteLine(space + "textureHeightMap " + model.meshes[amLoop].textureHeightMap.Name);
                if (model.meshes[amLoop].textureLightMap != null)
                    Console.WriteLine(space + "textureLightMap " + model.meshes[amLoop].textureLightMap.Name);
                if (model.meshes[amLoop].textureMetalnessPbr != null)
                    Console.WriteLine(space + "textureMetalness " + model.meshes[amLoop].textureMetalnessPbr.Name);
                if (model.meshes[amLoop].textureNormalMapPrbCamera != null)
                    Console.WriteLine(space + "textureNormalCamera " + model.meshes[amLoop].textureNormalMapPrbCamera.Name);
                if (model.meshes[amLoop].textureNormalMap != null)
                    Console.WriteLine(space + "textureNormalMap " + model.meshes[amLoop].textureNormalMap.Name);
                if (model.meshes[amLoop].textureReflectionMap != null)
                    Console.WriteLine(space + "textureReflectionMap " + model.meshes[amLoop].textureReflectionMap.Name);
                if (model.meshes[amLoop].textureRoughnessPbr != null)
                    Console.WriteLine(space + "textureRoughness " + model.meshes[amLoop].textureRoughnessPbr.Name);
                if (model.meshes[amLoop].textureShine != null)
                    Console.WriteLine(space + "textureShine " + model.meshes[amLoop].textureShine.Name);
                if (model.meshes[amLoop].textureSpecular != null)
                    Console.WriteLine(space + "textureSpecular " + model.meshes[amLoop].textureSpecular.Name);
                if (model.meshes[amLoop].textureUnknown != null)
                    Console.WriteLine(space + "textureUnknown " + model.meshes[amLoop].textureUnknown.Name);

                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("    __________________________");
                Console.WriteLine("    Bones in mesh");

                if (showBoneInfo)
                {
                    if (rmMesh.HasBones)
                    {
                        var meshBones = rmMesh.modelMeshBones;
                        Console.WriteLine("    meshBones.Length: " + meshBones.Length);
                        Console.WriteLine();
                        for (int meshBoneIndex = 0; meshBoneIndex < meshBones.Length; meshBoneIndex++)
                        {
                            var boneInMesh = meshBones[meshBoneIndex]; // ahhhh
                            var boneInMeshName = meshBones[meshBoneIndex].Name;

                            string str = "    mesh[" + mindex + "].Name: " + rmMesh.Name + "   bone[" + meshBoneIndex + "].Name: " + boneInMeshName + "   meshBoneIndex: " + meshBoneIndex.ToString() + "   WeightCount: " + boneInMesh.numberOfAssociatedWeightedVertices;
                            //str += "\n" + "   OffsetMatrix " + boneInMesh.OffsetMatrix;
                            Console.WriteLine(str);
                        }
                    }
                }
                Console.WriteLine();
            }
        }
    }
}
