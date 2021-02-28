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
    

    //public class VisualSkeletalOutput
    //{
    //    public bool disable = false;
    //    RiggedModel model;
    //    public float lineThickness = .1f;
    //    public float arrowScale = 2f;
    //    public OrientationArrows orientationArrow = new OrientationArrows();

    //    public List<SkeletalBoneLineTriplets> nodeTriplet = new List<SkeletalBoneLineTriplets>();
    //    public List<RoundLine3D> boneLinesCombinedTransform = new List<RoundLine3D>();
    //    public List<Matrix> boneArrowWorldMatricesCombined = new List<Matrix>();

    //    public Color boneLineCombinedTransformColor = Color.Aqua;

    //    //int currentIndex = 0;
    //    //public int GetSelectedIndex
    //    //{
    //    //    get { return lastSelectedPointerlineIndex; }
    //    //}
    //    int lastSelectedPointerlineIndex = 0;
    //    public RoundLine3D pointerLine;
    //    //RiggedModel.RiggedModelNode currentSelectedNode;

    //    bool buildBoneList_DirtyFlag = true;
    //    //BoneListSelector listSelector = new BoneListSelector();

    //    public void CreateBoneLinkages(RiggedModel _model, float linethickness, float arrowScale)
    //    {
    //        this.model = _model;
    //        lineThickness = linethickness;
    //        this.arrowScale = arrowScale;
    //        CreateBoneLinkages(model);
    //    }
    //    public void CreateBoneLinkages(RiggedModel _model)
    //    {
    //        this.model = _model;
    //        if(model.sceneRootNodeOfTree.children.Count ==0  || model.flatListToBoneNodesVisualDebug.Count < 2)
    //        {
    //            disable = true;
    //        }
    //        if (disable == false)
    //        {
    //            nodeTriplet.Clear();
    //            boneLinesCombinedTransform.Clear();
    //            boneArrowWorldMatricesCombined.Clear();
    //            IterateCreatingBones(model.sceneRootNodeOfTree, null, null);
    //            RecreateUpdateLines(model);
    //            if (buildBoneList_DirtyFlag)
    //            {
    //                //listSelector.BuildBoneNameList(model);
    //                buildBoneList_DirtyFlag = false;
    //            }
    //        }
    //    }
    //    private void IterateCreatingBones(RiggedModel.RiggedModelNode node, RiggedModel.RiggedModelNode parent, RiggedModel.RiggedModelNode lastActualBone)
    //    {

    //        var last = lastActualBone;
    //        if (last != null && parent != null)
    //        {
    //            if (node.isThisARealBone)
    //            {
    //                nodeTriplet.Add(new SkeletalBoneLineTriplets(node.parent, node, last));
    //                boneArrowWorldMatricesCombined.Add(node.CombinedTransformMg);
    //            }
    //        }
    //        else
    //        {
    //            if (parent == null && node.isThisARealBone)
    //            {
    //                // in this case we are going to reason this is the first entry and it is a bone because we want things to line up with offsets.
    //                nodeTriplet.Add(new SkeletalBoneLineTriplets(node, node, node));
    //                boneArrowWorldMatricesCombined.Add(node.CombinedTransformMg);
    //            }
    //        }
    //        if (node.isThisARealBone)
    //            last = node;
    //        for (int k = 0; k < node.children.Count; k++)
    //        {
    //            var c = node.children[k];
    //            IterateCreatingBones(c, node, last);
    //        }
    //    }

    //    public void RecreateUpdateLines(RiggedModel _model)
    //    {
    //        if (disable == false)
    //        {
    //            this.model = _model;
    //            if (nodeTriplet.Count == 0)
    //                CreateBoneLinkages(model);
    //            // clear the list of bone lines.
    //            boneLinesCombinedTransform.Clear();
    //            boneArrowWorldMatricesCombined.Clear();

    //            // Create drawing lines using the combined transforms.
    //            for (int i = 0; i < nodeTriplet.Count; i++)
    //            {
    //                var v0 = Vector3.Zero;
    //                if (nodeTriplet[i].parent != null)
    //                    v0 = nodeTriplet[i].lastTranslationNode.CombinedTransformMg.Translation;
    //                var v1 = nodeTriplet[i].child.CombinedTransformMg.Translation;
    //                //var line = new Line(lineThickness, boneLineCombinedTransformColor, v0, v1);
    //                var line = new RoundLine3D(5, lineThickness, boneLineCombinedTransformColor, v0, v1);
    //                line.World = Matrix.Identity;
    //                boneLinesCombinedTransform.Add(line);// boneLines[i] = line;
    //                boneArrowWorldMatricesCombined.Add(nodeTriplet[i].child.CombinedTransformMg); // nodeTriplet[i].lastTranslationNode.CombinedTransformMg
    //            }
    //        }
    //    }

    //    public bool Draw(GameTime gameTime, GraphicsDevice gd, bool useDefaultTextPositions, Matrix cameraWorld)
    //    {
    //        //var test = listSelector.DrawBoneListSelector(model, gd, useDefaultTextPositions);
    //        //if (test != lastSelectedPointerlineIndex && test > -1 && test < boneArrowWorldMatrices.Count)
    //        //{
    //        //    lastSelectedPointerlineIndex = currentIndex;
    //        //    currentIndex = test;
    //        //}
    //        //if (currentIndex > -1 && currentIndex < boneArrowWorldMatrices.Count)
    //        //{
    //        //    boneLines[lastSelectedPointerlineIndex].ReColorLine(boneLineInvOffsetTransformColor);
    //        //    boneLines[currentIndex].ReColorLine(Color.MonoGameOrange);
    //        //    invBoneLines[lastSelectedPointerlineIndex].ReColorLine(boneLineOffsetTransformColor);
    //        //    invBoneLines[currentIndex].ReColorLine(Color.MonoGameOrange);
    //        //    boneLinesCombinedTransform[lastSelectedPointerlineIndex].ReColorLine(boneLineCombinedTransformColor);
    //        //    boneLinesCombinedTransform[currentIndex].ReColorLine(Color.Purple);
    //        //    invBoneLinesCombinedTransform[lastSelectedPointerlineIndex].ReColorLine(boneLineInvCombinedTransformColor);
    //        //    invBoneLinesCombinedTransform[currentIndex].ReColorLine(Color.Purple);
    //        //    // well make another method or class in the list selector or similar to click and rotate
    //        //    currentSelectedNode = nodeTriplet[currentIndex].child;
    //        //    int colelement = 0;
    //        //    colelement = (int)(Gu.Timer7SecondOcillating.GetElapsedPercentageAsOscillation * 205f + 50f);
    //        //    var col = new Color(0, colelement, 0, colelement);
    //        //    var p1 = cameraWorld.Translation + cameraWorld.Right * 2.2f + cameraWorld.Down * 2.2f + cameraWorld.Forward * 1.8f;
    //        //    //pointerLine = new Line(.05f, col, p1, boneArrowWorldMatrices[currentIndex].Translation);
    //        //    pointerLine = new Line(.05f, col, p1, boneArrowWorldMatricesCombined[currentIndex].Translation);
    //        //    lastSelectedPointerlineIndex = currentIndex;
    //        //}
    //        //if (listSelector.skeleton_Dirty)
    //        //{
    //        //    //buildBoneList_DirtyFlag = false;
    //        //    listSelector.skeleton_Dirty = false;
    //        //    return true;
    //        //}
    //        //else
    //        //    return false;

    //        return false;
    //    }

    //    public void DrawLines(GraphicsDevice graphicsDevice, Effect effect)
    //    {
    //        if (disable == false)
    //        {
    //            // draw bone lines 
    //            foreach (var blct in boneLinesCombinedTransform)
    //            {
    //                blct.Draw(graphicsDevice, effect, blct.World);
    //            }
    //            // draw the special line pointing at the bone.  pulled.
    //            if (lastSelectedPointerlineIndex != -1 && pointerLine != null)
    //            {
    //                pointerLine.Draw(graphicsDevice, effect, pointerLine.World);
    //            }
    //        }
    //    }
    //    public void DrawOffsetArrows(GraphicsDevice graphicsDevice, Effect effect, Texture2D t)
    //    {
    //        if (disable == false)
    //        {
    //            // draw arrows
    //            effect.Parameters["TextureA"].SetValue(t);
    //            foreach (var bawmc in boneArrowWorldMatricesCombined)
    //            {
    //                effect.Parameters["World"].SetValue(Matrix.CreateScale(arrowScale) * bawmc);
    //                orientationArrow.Draw(graphicsDevice, effect);
    //            }
    //        }
    //    }
    //    public class SkeletalBoneLineTriplets
    //    {
    //        public RiggedModel.RiggedModelNode parent;
    //        public RiggedModel.RiggedModelNode child;
    //        public RiggedModel.RiggedModelNode lastTranslationNode;
    //        public SkeletalBoneLineTriplets(RiggedModel.RiggedModelNode parent, RiggedModel.RiggedModelNode child, RiggedModel.RiggedModelNode lastActualBoneNode)
    //        {
    //            this.parent = parent;
    //            this.child = child;
    //            this.lastTranslationNode = lastActualBoneNode;
    //        }
    //    }

    //    //________________________________________________________________________________
    //    //@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
    //    //
    //    // Starts a simple ui class to manipulate the above class and make changes to what is viewed or alter it.
    //    //________________________________________________________________________________
    //    //@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

    //    //public class BoneListSelector
    //    //{
    //    //    public bool skeleton_Dirty = false;
    //    //    RiggedModel.RiggedModelNode currentSelectedNode;
    //    //    public int nodeSelectionIndex = -1;
    //    //    public int nodeSelectionScrollIndex = 0;
    //    //    public int nodeSelectionNumOfVisibleItems = 10;
    //    //    public Vector2 mSelectedNodeListItemInfo = Vector2.Zero;
    //    //    public Vector2 mClickListPos = Vector2.Zero;
    //    //    public Vector2 mpos0 = Vector2.Zero;
    //    //    public Vector2 mpos1 = Vector2.Zero;
    //    //    public Vector2 mpos2 = Vector2.Zero;
    //    //    public Vector2 mpos3 = Vector2.Zero;
    //    //    public Vector2 mpos4 = Vector2.Zero;
    //    //    public int mClickBoxWidthListClicker = 140;

    //    //    public Rectangle xRotCw;
    //    //    public Rectangle yRotCw;
    //    //    public Rectangle zRotCw;

    //    //    List<MgStringBuilder> boneSelectionNames = new List<MgStringBuilder>();
    //    //    MgStringBuilder boneSelectedInfo = new MgStringBuilder();
    //    //    MgStringBuilder boneSelectedOffsetInfo = new MgStringBuilder();
    //    //    MgStringBuilder boneSelectedInvOffsetInfo = new MgStringBuilder();
    //    //    MgStringBuilder boneSelectedLocalInfo = new MgStringBuilder();
    //    //    MgStringBuilder boneSelectedCombinedInfo = new MgStringBuilder();
    //    //    MgStringBuilder boneSelectedFinalInfo = new MgStringBuilder();

    //    //    MgStringBuilder boneSelectedRotatorLabelX = new MgStringBuilder("X+");
    //    //    MgStringBuilder boneSelectedRotatorLabelY = new MgStringBuilder("Y+");
    //    //    MgStringBuilder boneSelectedRotatorLabelZ = new MgStringBuilder("Z+");

    //    //    public void BuildBoneNameList(RiggedModel model)
    //    //    {
    //    //        var len = model.numberOfBonesInUse;
    //    //        if (nodeSelectionNumOfVisibleItems >= len)
    //    //            nodeSelectionNumOfVisibleItems = len;
    //    //        for (int i = 0; i < model.flatListToBoneNodes.Count; i++)
    //    //        {
    //    //            var s = model.flatListToBoneNodes[i].name;
    //    //            boneSelectionNames.Add(new MgStringBuilder(s));
    //    //        }
    //    //    }

    //    //    /// <summary>
    //    //    /// returns the currently selected bone or -1 if not applicable.
    //    //    /// </summary>
    //    //    public int DrawBoneListSelector(RiggedModel model, GraphicsDevice gd, bool useDefaultPositions)
    //    //    {
    //    //        if (useDefaultPositions)
    //    //        {
    //    //            var x = gd.Viewport.Width - 200;
    //    //            var y = gd.Viewport.Height - 550;

    //    //            mSelectedNodeListItemInfo = new Vector2(x, y);
    //    //            mClickListPos = new Vector2(x + 50, y + 300);
    //    //            mClickBoxWidthListClicker = 140;

    //    //            var y_mat_pos = gd.Viewport.Height - 170;

    //    //            var x_mat_pos_label_spacing = 280;
    //    //            mpos0 = new Vector2(10 + 0 * x_mat_pos_label_spacing, y_mat_pos);
    //    //            mpos1 = new Vector2(10 + 1 * x_mat_pos_label_spacing, y_mat_pos);
    //    //            mpos2 = new Vector2(10 + 2 * x_mat_pos_label_spacing, y_mat_pos);
    //    //            mpos3 = new Vector2(10 + 3 * x_mat_pos_label_spacing, y_mat_pos);
    //    //            mpos4 = new Vector2(10 + 4 * x_mat_pos_label_spacing, y_mat_pos);

    //    //            int lnHeight = Gu.currentFont.LineSpacing + 3;
    //    //            var size = new Vector2(30, Gu.currentFont.LineSpacing);
    //    //            var rotposXcw = new Vector2(x, mClickListPos.Y);
    //    //            var rotposYcw = new Vector2(x, mClickListPos.Y + lnHeight);
    //    //            var rotposZcw = new Vector2(x, mClickListPos.Y + lnHeight * 2);

    //    //            xRotCw = new Rectangle(rotposXcw.ToPoint(), size.ToPoint());
    //    //            yRotCw = new Rectangle(rotposYcw.ToPoint(), size.ToPoint());
    //    //            zRotCw = new Rectangle(rotposZcw.ToPoint(), size.ToPoint());
    //    //        }
    //    //        DrawBoneListSelector(model);
    //    //        DrawBoneRotationClickBoxes(model);
    //    //        return nodeSelectionIndex;
    //    //    }
    //    //    public void DrawBoneListSelector(RiggedModel model)
    //    //    {
    //    //        var len = boneSelectionNames.Count;
    //    //        int lnHeight = Gu.currentFont.LineSpacing + 3;
    //    //        for (int i = 0; i < nodeSelectionNumOfVisibleItems; i++)
    //    //        {
    //    //            var rect = new Rectangle((int)mClickListPos.X, (int)mClickListPos.Y + i * lnHeight, mClickBoxWidthListClicker, lnHeight - 3);
    //    //            if (UserMouseInput.mousewheel_upordown < 0)
    //    //            {
    //    //                int testNextIndexoffset = nodeSelectionScrollIndex + 1;
    //    //                int lastItemVisible = nodeSelectionNumOfVisibleItems - 1;
    //    //                if (testNextIndexoffset > -1 && testNextIndexoffset + (lastItemVisible) < len)
    //    //                {
    //    //                    nodeSelectionScrollIndex += 1;
    //    //                }
    //    //            }
    //    //            if (UserMouseInput.mousewheel_upordown > 0)
    //    //            {
    //    //                int indexoffset = nodeSelectionScrollIndex - 1;
    //    //                if (indexoffset > -1 && indexoffset + nodeSelectionNumOfVisibleItems < len)
    //    //                {
    //    //                    nodeSelectionScrollIndex -= 1;
    //    //                }
    //    //            }
    //    //            if (i + nodeSelectionScrollIndex == nodeSelectionIndex)
    //    //                Gu.DrawTextOnBackgroundRectangle(boneSelectionNames[i + nodeSelectionScrollIndex], rect, Color.Green, Color.Moccasin);
    //    //            else
    //    //                Gu.DrawTextOnBackgroundRectangle(boneSelectionNames[i + nodeSelectionScrollIndex], rect, Color.Blue, Color.Beige);
    //    //            if (UserMouseInput.is_left_clicked && rect.Contains(UserMouseInput.Pos))
    //    //            {
    //    //                nodeSelectionIndex = i + nodeSelectionScrollIndex;
    //    //                currentSelectedNode = model.flatListToBoneNodes[nodeSelectionIndex];

    //    //                skeleton_Dirty = true;
    //    //                boneSelectedInfo.Clear();
    //    //                boneSelectedOffsetInfo.Clear();
    //    //                boneSelectedInvOffsetInfo.Clear();
    //    //                boneSelectedLocalInfo.Clear();
    //    //                boneSelectedCombinedInfo.Clear();
    //    //                boneSelectedFinalInfo.Clear();
    //    //                if (currentSelectedNode.isThisARealBone)
    //    //                    boneSelectedInfo.Append(Helpers.InfoAboutNodeLongScreenForm(currentSelectedNode));
    //    //                else
    //    //                    boneSelectedInfo.Append("");
    //    //                boneSelectedOffsetInfo.Append("OffsetMatrixMg").Append(Helpers.MatrixToStringCompacted(currentSelectedNode.OffsetMatrixMg, "", true));
    //    //                boneSelectedInvOffsetInfo.Append("InvOffsetMatrixMg").Append(Helpers.MatrixToStringCompacted(currentSelectedNode.InvOffsetMatrixMg, "", true));
    //    //                boneSelectedLocalInfo.Append("LocalTransformMg").Append(Helpers.MatrixToStringCompacted(currentSelectedNode.LocalTransformMg, "", true));
    //    //                boneSelectedCombinedInfo.Append("CombinedTransformMg").Append(Helpers.MatrixToStringCompacted(currentSelectedNode.CombinedTransformMg, "", true));
    //    //                boneSelectedFinalInfo.Append("FinalTransformMg").Append(Helpers.MatrixToStringCompacted(model.globalShaderMatrixs[currentSelectedNode.boneShaderFinalTransformIndex], "", true));
    //    //            }
    //    //        }
    //    //        if (nodeSelectionIndex > -1 && nodeSelectionIndex < boneSelectionNames.Count)
    //    //        {
    //    //            Gu.spriteBatch.DrawString(Gu.currentFont, boneSelectedInfo, mSelectedNodeListItemInfo, Color.White);
    //    //            Gu.spriteBatch.DrawString(Gu.currentFont, boneSelectedOffsetInfo, mpos0, Color.AntiqueWhite);
    //    //            Gu.spriteBatch.DrawString(Gu.currentFont, boneSelectedInvOffsetInfo, mpos1, Color.AntiqueWhite);
    //    //            Gu.spriteBatch.DrawString(Gu.currentFont, boneSelectedLocalInfo, mpos2, Color.Moccasin);
    //    //            Gu.spriteBatch.DrawString(Gu.currentFont, boneSelectedCombinedInfo, mpos3, Color.NavajoWhite);
    //    //            Gu.spriteBatch.DrawString(Gu.currentFont, boneSelectedFinalInfo, mpos4, Color.FloralWhite);
    //    //        }
    //    //    }
    //    //    public void DrawBoneRotationClickBoxes(RiggedModel model)
    //    //    {
    //    //        if (currentSelectedNode != null)
    //    //        {
    //    //            //boneSelectedRotatorLabelX.Clear();
    //    //            //boneSelectedRotatorLabelY.Clear();
    //    //            //boneSelectedRotatorLabelZ.Clear();
    //    //            //boneSelectedRotatorLabelX.Append("X+");
    //    //            //boneSelectedRotatorLabelY.Append("Y+");
    //    //            //boneSelectedRotatorLabelZ.Append("Z+");

    //    //            bool isActive = false;
    //    //            float rotAmount = .001f;
    //    //            if (UserMouseInput.is_left_held && xRotCw.Contains(UserMouseInput.Pos))
    //    //            {
    //    //                currentSelectedNode.LocalTransformMg *= Matrix.CreateRotationX(rotAmount);
    //    //                //currentSelectedNode.OffsetMatrixMg *= Matrix.CreateRotationX(rotAmount);
    //    //                Gu.DrawTextOnBackgroundRectangle(boneSelectedRotatorLabelX, xRotCw, Color.Green, Color.Moccasin);
    //    //                isActive = true;
    //    //            }
    //    //            else
    //    //                Gu.DrawTextOnBackgroundRectangle(boneSelectedRotatorLabelX, xRotCw, Color.Red, Color.Moccasin);
    //    //            if (UserMouseInput.is_left_held && yRotCw.Contains(UserMouseInput.Pos))
    //    //            {
    //    //                currentSelectedNode.LocalTransformMg *= Matrix.CreateRotationY(rotAmount);
    //    //                //currentSelectedNode.OffsetMatrixMg *= Matrix.CreateRotationY(rotAmount);
    //    //                Gu.DrawTextOnBackgroundRectangle(boneSelectedRotatorLabelY, yRotCw, Color.Green, Color.Moccasin);
    //    //                isActive = true;
    //    //            }
    //    //            else
    //    //                Gu.DrawTextOnBackgroundRectangle(boneSelectedRotatorLabelY, yRotCw, Color.Red, Color.Moccasin);
    //    //            if (UserMouseInput.is_left_held && zRotCw.Contains(UserMouseInput.Pos))
    //    //            {
    //    //                currentSelectedNode.LocalTransformMg *= Matrix.CreateRotationZ(rotAmount);
    //    //                //currentSelectedNode.OffsetMatrixMg *= Matrix.CreateRotationY(rotAmount);
    //    //                Gu.DrawTextOnBackgroundRectangle(boneSelectedRotatorLabelZ, zRotCw, Color.Green, Color.Moccasin);
    //    //                isActive = true;
    //    //            }
    //    //            else
    //    //                Gu.DrawTextOnBackgroundRectangle(boneSelectedRotatorLabelZ, zRotCw, Color.Red, Color.Moccasin);

    //    //            if (isActive)
    //    //            {
    //    //                skeleton_Dirty = true;
    //    //                boneSelectedInfo.Clear();
    //    //                boneSelectedOffsetInfo.Clear();
    //    //                boneSelectedInvOffsetInfo.Clear();
    //    //                boneSelectedLocalInfo.Clear();
    //    //                boneSelectedCombinedInfo.Clear();
    //    //                boneSelectedFinalInfo.Clear();
    //    //                if (currentSelectedNode.isThisARealBone)
    //    //                    boneSelectedInfo.Append(Helpers.InfoAboutNodeLongScreenForm(currentSelectedNode));
    //    //                else
    //    //                    boneSelectedInfo.Append("");
    //    //                boneSelectedOffsetInfo.Append("OffsetMatrixMg").Append(Helpers.MatrixToStringCompacted(currentSelectedNode.OffsetMatrixMg, "", true));
    //    //                boneSelectedInvOffsetInfo.Append("InvOffsetMatrixMg").Append(Helpers.MatrixToStringCompacted(currentSelectedNode.InvOffsetMatrixMg, "", true));
    //    //                boneSelectedLocalInfo.Append("LocalTransformMg").Append(Helpers.MatrixToStringCompacted(currentSelectedNode.LocalTransformMg, "", true));
    //    //                boneSelectedCombinedInfo.Append("CombinedTransformMg").Append(Helpers.MatrixToStringCompacted(currentSelectedNode.CombinedTransformMg, "", true));
    //    //                boneSelectedFinalInfo.Append("FinalTransformMg").Append(Helpers.MatrixToStringCompacted(model.globalShaderMatrixs[currentSelectedNode.boneShaderFinalTransformIndex], "", true));
    //    //            }
    //    //        }

    //    //    }
    //    //}
    //}

    public class NormalArrow
    {
        VertexPositionColorNormalTexture[] vertices;
        int[] indices;
        public Texture2D texture;

        public NormalArrow(VertexPositionNormalTexture[] inVertices, int[] inIndices, Texture2D t, float scale)
        {
            CreateVisualNormalsForPrimitiveMesh(inVertices, inIndices, t, scale);
        }
        public NormalArrow(VertexPositionColorNormalTextureTangent[] inVertices, int[] inIndices, Texture2D t, float scale)
        {
            VertexPositionNormalTexture[] v = new VertexPositionNormalTexture[inVertices.Length];
            int[] i = new int[inIndices.Length];
            for(int n =0; n < inVertices.Length; n++)
            {
                v[n].Position = inVertices[n].Position;
                v[n].Normal = inVertices[n].Normal;
            }
            for (int n = 0; n < inIndices.Length; n++)
            {
                i[n] = inIndices[n];
            }
            CreateVisualNormalsForPrimitiveMesh(v, i, t, scale);
        }
        public NormalArrow(VertexPositionColorNormalTextureTangentWeights[] inVertices, int[] inIndices, Texture2D t, float scale)
        {
            VertexPositionNormalTexture[] v = new VertexPositionNormalTexture[inVertices.Length];
            int[] i = new int[inIndices.Length];
            for (int n = 0; n < inVertices.Length; n++)
            {
                v[n].Position = inVertices[n].Position;
                v[n].Normal = inVertices[n].Normal;
            }
            for (int n = 0; n < inIndices.Length; n++)
            {
                i[n] = inIndices[n];
            }
            CreateVisualNormalsForPrimitiveMesh(v, i, t, scale);
        }     

        public void CreateVisualNormalsForPrimitiveMesh(VertexPositionNormalTexture[] inVertices, int[] inIndices , Texture2D t, float scale)
        {
            texture = t;
            int len = inVertices.Length;

            VertexPositionColorNormalTexture[] nverts = new VertexPositionColorNormalTexture[len * 4];
            int[] nindices = new int[len * 6];

            for (int j = 0; j < len; j++)
            {
                int v = j * 4;
                int i = j * 6;
                //ReCreateForwardNormalQuad(vertices[i].Position, vertices[i].Normal);
                nverts[v + 0].Color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                nverts[v + 1].Color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                nverts[v + 2].Color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                nverts[v + 3].Color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                //
                nverts[v + 0].TextureCoordinate = new Vector2(0f, 0f);//vertices[v + 0].TextureCoordinateA;
                nverts[v + 1].TextureCoordinate = new Vector2(0f, .33f); //vertices[v + 1].TextureCoordinateA;
                nverts[v + 2].TextureCoordinate = new Vector2(1f, .0f);//vertices[v + 2].TextureCoordinateA;
                nverts[v + 3].TextureCoordinate = new Vector2(1f, .33f);//vertices[v + 3].TextureCoordinateA;
                //
                nverts[v + 0].Position = new Vector3(0f, 0f, 0f) + inVertices[j].Position;
                nverts[v + 1].Position = new Vector3(0f, -.2f, 0f) + inVertices[j].Position;
                nverts[v + 2].Position = new Vector3(0f, 0f, 0f) + inVertices[j].Position + inVertices[j].Normal * scale;
                nverts[v + 3].Position = new Vector3(0f, -.2f, 0f) + inVertices[j].Position + inVertices[j].Normal * scale;
                //
                nverts[v + 0].Normal = inVertices[j].Normal;
                nverts[v + 1].Normal = inVertices[j].Normal;
                nverts[v + 2].Normal = inVertices[j].Normal;
                nverts[v + 3].Normal = inVertices[j].Normal;

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
                gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, (indices.Length / 3), VertexPositionColorNormalTexture.VertexDeclaration);
            }
        }
    }

    public class RoundLine3D
    {
        VertexPositionColor[] vertices;
        int[] indices;
        public Matrix World = Matrix.Identity;

        public Vector3 camUp = Vector3.Up;

        public RoundLine3D(int roundness, float linewidth, Color c, Vector3 start, Vector3 end)
        {
            CreateLine(roundness, linewidth, c, start, end);
        }

        private void CreateLine(int roundness, float linewidth, Color c, Vector3 start, Vector3 end)
        {
            // int roundness = 5; // this can be a input rather it should be a input.
            int vertCount = roundness * 2 + 2;
            int primCount = roundness * 2 * 2;
            int indexCount = primCount * 3;

            vertices = new VertexPositionColor[vertCount];
            indices = new int[indexCount];

            int startVertOffset = 0;
            int endVertsOffset = roundness;

            float rotinc = (float)(Math.PI * 2 / (float)roundness);
            int v0 = startVertOffset;
            int v1 = endVertsOffset;
            for (int i =0; i < roundness; i++)
            {
                var world = Matrix.CreateFromAxisAngle(Vector3.Normalize(end - start), rotinc * i);
                var sAbove = world.Up * linewidth;

                vertices[v0].Position = sAbove + start; vertices[v0].Color = c;
                v0++;
                vertices[v1].Position = sAbove + end; vertices[v1].Color = c;
                v1++;
            }
            // cap end centers themselves.
            vertices[v1].Position = start; vertices[v1].Color = c;
            v1++;
            vertices[v1].Position = end; vertices[v1].Color = c;
            v1++;

            // index
            v0 = startVertOffset;
            v1 = endVertsOffset;
            //  v0 -- v1
            //   a  /   b
            //  v2 -- v3
            int index = 0;
            for (int i = 0; i < roundness; i++)
            {
                var v2= v0 + 1;
                if (v2 >= roundness)
                    v2 = 0;
                var v3 = v1 +1;
                if (v3 >= roundness *2)
                    v3 = roundness;
                //___

                indices[index] = v0; index++;
                indices[index] = v2; index++;
                indices[index] = v1; index++;

                indices[index] = v1; index++;
                indices[index] = v2; index++;
                indices[index] = v3; index++;

                // end caps.

                // start side.
                indices[index] = v0; index++;
                indices[index] = v2; index++;
                indices[index] = roundness * 2; index++;

                // end side.
                indices[index] = v1; index++;
                indices[index] = v3; index++;
                indices[index] = roundness * 2 +1; index++;

                //___
                v0++;
                v1++;
            }
        }

        public void LineOld(float linewidth, Color c, Vector3 start, Vector3 end)
        {
            CreateLineOld(linewidth, c, start, end);
        }
        private void CreateLineOld(float linewidth, Color c, Vector3 start, Vector3 end)
        {
            var a = end - start;
            a.Normalize();
            var b = Vector3.Up;
            float n = Vector3.Dot(a, b);
            if (n * n > .95f)
                b = Vector3.Right;
            var su = Vector3.Cross(a, b);
            var sr = Vector3.Cross(a, su);
            var offsetup = su * linewidth;
            var offsetright = sr * linewidth;

            Vector3 s0 = start + offsetright - offsetup;
            Vector3 s1 = start - offsetright - offsetup;
            Vector3 s2 = start + offsetup;

            Vector3 e0 = end + offsetright - offsetup;
            Vector3 e1 = end - offsetright - offsetup;
            Vector3 e2 = end + offsetup;

            var calpha = c.A;
            var cs = c * .6f;
            cs.A = calpha;

            vertices = new VertexPositionColor[12];
            indices = new int[18];

            int v = 0;
            int i = 0;
            // q1
            vertices[v].Position = s0; vertices[v].Color = cs; v++;
            vertices[v].Position = s1; vertices[v].Color = cs; v++;
            vertices[v].Position = e0; vertices[v].Color = c; v++;
            vertices[v].Position = e1; vertices[v].Color = c; v++;

            var vi = v - 4;
            indices[i + 0] = vi + 0; indices[i + 1] = vi + 1; indices[i + 2] = vi + 2;
            indices[i + 3] = vi + 2; indices[i + 4] = vi + 1; indices[i + 5] = vi + 3;
            i += 6;

            // q2
            vertices[v].Position = s1; vertices[v].Color = cs; v++;
            vertices[v].Position = s2; vertices[v].Color = cs; v++;
            vertices[v].Position = e1; vertices[v].Color = c; v++;
            vertices[v].Position = e2; vertices[v].Color = c; v++;

            vi = v - 4;
            indices[i + 0] = vi + 0; indices[i + 1] = vi + 1; indices[i + 2] = vi + 2;
            indices[i + 3] = vi + 2; indices[i + 4] = vi + 1; indices[i + 5] = vi + 3;
            i += 6;

            //q3
            vertices[v].Position = s2; vertices[v].Color = cs; v++;
            vertices[v].Position = s0; vertices[v].Color = cs; v++;
            vertices[v].Position = e2; vertices[v].Color = c; v++;
            vertices[v].Position = e0; vertices[v].Color = c; v++;

            vi = v - 4;
            indices[i + 0] = vi + 0; indices[i + 1] = vi + 1; indices[i + 2] = vi + 2;
            indices[i + 3] = vi + 2; indices[i + 4] = vi + 1; indices[i + 5] = vi + 3;
            i += 6;
        }

        public void ReColorLineOld(Color c)
        {
            var calpha = c.A;
            var cs = c * .6f;
            cs.A = calpha;
            int v = 0;
            // q1
            vertices[v].Color = cs; v++;
            vertices[v].Color = cs; v++;
            vertices[v].Color = c; v++;
            vertices[v].Color = c; v++;
            // q2
            vertices[v].Color = cs; v++;
            vertices[v].Color = cs; v++;
            vertices[v].Color = c; v++;
            vertices[v].Color = c; v++;
            //q3
            vertices[v].Color = cs; v++;
            vertices[v].Color = cs; v++;
            vertices[v].Color = c; v++;
            vertices[v].Color = c; v++;
        }

        public void Draw(GraphicsDevice gd, Effect effect)
        {
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, (indices.Length / 3), VertexPositionColor.VertexDeclaration);
            }
        }
        public void Draw(GraphicsDevice gd, Effect effect, Matrix world)
        {
            effect.Parameters["World"].SetValue(world);
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, (indices.Length / 3), VertexPositionColor.VertexDeclaration);
            }
        }
    }

    public class Line
    {
        VertexPositionColor[] vertices;
        int[] indices;
        public Matrix World = Matrix.Identity;

        public Vector3 camUp = Vector3.Up;

        public Line(float linewidth, Color c, Vector3 start, Vector3 end)
        {
            CreateLine(linewidth, c, start, end);
        }
        private void CreateLine(float linewidth, Color c, Vector3 start, Vector3 end)
        {
            var a = end - start;
            a.Normalize();
            var b = Vector3.Up;
            float n = Vector3.Dot(a, b);
            if (n * n > .95f)
                b = Vector3.Right;
            var su = Vector3.Cross(a, b);
            var sr = Vector3.Cross(a, su);
            var offsetup = su * linewidth;
            var offsetright = sr * linewidth;

            Vector3 s0 = start + offsetright - offsetup;
            Vector3 s1 = start - offsetright - offsetup;
            Vector3 s2 = start + offsetup;

            Vector3 e0 = end + offsetright - offsetup;
            Vector3 e1 = end - offsetright - offsetup;
            Vector3 e2 = end + offsetup;

            var calpha = c.A;
            var cs = c * .6f;
            cs.A = calpha;

            vertices = new VertexPositionColor[12];
            indices = new int[18];

            int v = 0;
            int i = 0;
            // q1
            vertices[v].Position = s0; vertices[v].Color = cs; v++;
            vertices[v].Position = s1; vertices[v].Color = cs; v++;
            vertices[v].Position = e0; vertices[v].Color = c; v++;
            vertices[v].Position = e1; vertices[v].Color = c; v++;

            var vi = v - 4;
            indices[i + 0] = vi + 0; indices[i + 1] = vi + 1; indices[i + 2] = vi + 2;
            indices[i + 3] = vi + 2; indices[i + 4] = vi + 1; indices[i + 5] = vi + 3;
            i += 6;

            // q2
            vertices[v].Position = s1; vertices[v].Color = cs; v++;
            vertices[v].Position = s2; vertices[v].Color = cs; v++;
            vertices[v].Position = e1; vertices[v].Color = c; v++;
            vertices[v].Position = e2; vertices[v].Color = c; v++;

            vi = v - 4;
            indices[i + 0] = vi + 0; indices[i + 1] = vi + 1; indices[i + 2] = vi + 2;
            indices[i + 3] = vi + 2; indices[i + 4] = vi + 1; indices[i + 5] = vi + 3;
            i += 6;

            //q3
            vertices[v].Position = s2; vertices[v].Color = cs; v++;
            vertices[v].Position = s0; vertices[v].Color = cs; v++;
            vertices[v].Position = e2; vertices[v].Color = c; v++;
            vertices[v].Position = e0; vertices[v].Color = c; v++;

            vi = v - 4;
            indices[i + 0] = vi + 0; indices[i + 1] = vi + 1; indices[i + 2] = vi + 2;
            indices[i + 3] = vi + 2; indices[i + 4] = vi + 1; indices[i + 5] = vi + 3;
            i += 6;
        }

        public void ReColorLine(Color c)
        {
            var calpha = c.A;
            var cs = c * .6f;
            cs.A = calpha;
            int v = 0;
            // q1
            vertices[v].Color = cs; v++;
            vertices[v].Color = cs; v++;
            vertices[v].Color = c; v++;
            vertices[v].Color = c; v++;
            // q2
            vertices[v].Color = cs; v++;
            vertices[v].Color = cs; v++;
            vertices[v].Color = c; v++;
            vertices[v].Color = c; v++;
            //q3
            vertices[v].Color = cs; v++;
            vertices[v].Color = cs; v++;
            vertices[v].Color = c; v++;
            vertices[v].Color = c; v++;
        }

        public void Draw(GraphicsDevice gd, Effect effect)
        {
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, (indices.Length / 3), VertexPositionColor.VertexDeclaration);
            }
        }
        public void Draw(GraphicsDevice gd, Effect effect, Matrix world)
        {
            effect.Parameters["World"].SetValue(world);
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, (indices.Length / 3), VertexPositionColor.VertexDeclaration);
            }
        }
    }

    public class LinePCT
    {
        VertexPositionColorTexture[] vertices;
        int[] indices;

        public Vector3 camUp = Vector3.Up;

        public LinePCT(float linewidth, Color c, Vector3 start, Vector3 end)
        {
            CreateLine(linewidth, c, c, start, end);
        }
        public LinePCT(float linewidth, Color colorStart, Color colorEnd, Vector3 start, Vector3 end)
        {
            CreateLine(linewidth, colorStart, colorEnd, start, end);
        }

        private void CreateLine(float linewidth, Color cs, Color ce, Vector3 start, Vector3 end)
        {
            var a = end - start;
            a.Normalize();
            var b = Vector3.Up;
            float n = Vector3.Dot(a, b);
            if (n * n > .95f)
                b = Vector3.Right;
            var su = Vector3.Cross(a, b);
            var sr = Vector3.Cross(a, su);
            var offsetup = su * linewidth;
            var offsetright = sr * linewidth;

            Vector3 s0 = start + offsetright - offsetup;
            Vector3 s1 = start - offsetright - offsetup;
            Vector3 s2 = start + offsetup;

            Vector3 e0 = end + offsetright - offsetup;
            Vector3 e1 = end - offsetright - offsetup;
            Vector3 e2 = end + offsetup;

            Vector2 uv0 = new Vector2(0f, 1f);
            Vector2 uv1 = new Vector2(0f, 0f);
            Vector2 uv2 = new Vector2(1f, 0f);
            Vector2 uv3 = new Vector2(1f, 1f);

            vertices = new VertexPositionColorTexture[12];
            indices = new int[18];

            int v = 0;
            int i = 0;
            // q1
            vertices[v].Position = s0; vertices[v].Color = cs; vertices[v].TextureCoordinate = uv0; v++;
            vertices[v].Position = s1; vertices[v].Color = cs; vertices[v].TextureCoordinate = uv1; v++;
            vertices[v].Position = e0; vertices[v].Color = ce; vertices[v].TextureCoordinate = uv2; v++;
            vertices[v].Position = e1; vertices[v].Color = ce; vertices[v].TextureCoordinate = uv3; v++;

            var vi = v - 4;
            indices[i + 0] = vi + 0; indices[i + 1] = vi + 1; indices[i + 2] = vi + 2;
            indices[i + 3] = vi + 2; indices[i + 4] = vi + 1; indices[i + 5] = vi + 3;
            i += 6;

            // q2
            vertices[v].Position = s1; vertices[v].Color = cs; vertices[v].TextureCoordinate = uv0; v++;
            vertices[v].Position = s2; vertices[v].Color = cs; vertices[v].TextureCoordinate = uv1; v++;
            vertices[v].Position = e1; vertices[v].Color = ce; vertices[v].TextureCoordinate = uv2; v++;
            vertices[v].Position = e2; vertices[v].Color = ce; vertices[v].TextureCoordinate = uv3; v++;

            vi = v - 4;
            indices[i + 0] = vi + 0; indices[i + 1] = vi + 1; indices[i + 2] = vi + 2;
            indices[i + 3] = vi + 2; indices[i + 4] = vi + 1; indices[i + 5] = vi + 3;
            i += 6;

            //q3
            vertices[v].Position = s2; vertices[v].Color = cs; vertices[v].TextureCoordinate = uv0; v++;
            vertices[v].Position = s0; vertices[v].Color = cs; vertices[v].TextureCoordinate = uv1; v++;
            vertices[v].Position = e2; vertices[v].Color = ce; vertices[v].TextureCoordinate = uv2; v++;
            vertices[v].Position = e0; vertices[v].Color = ce; vertices[v].TextureCoordinate = uv3; v++;

            vi = v - 4;
            indices[i + 0] = vi + 0; indices[i + 1] = vi + 1; indices[i + 2] = vi + 2;
            indices[i + 3] = vi + 2; indices[i + 4] = vi + 1; indices[i + 5] = vi + 3;
            i += 6;
        }
        public void Draw(GraphicsDevice gd, Effect effect)
        {
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, (indices.Length / 3), VertexPositionColorTexture.VertexDeclaration);
            }
        }
    }

    public class OrientationArrows
    {
        VertexPositionColorTexture[] vertices;
        int[] indices;

        public OrientationArrows()
        {
            CreateOrientationArrows(Color.White);
        }
        public OrientationArrows(Color color)
        {
            CreateOrientationArrows(color);
        }

        private void CreateOrientationArrows(Color color)
        {
            //float z = 0.0f;

            Vector3 center = new Vector3(0, 0, 0);
            //
            Vector3 endForward_fromcenter = new Vector3(0, 0, -1f);
            Vector3 endUp_fromcenter = new Vector3(0, 1f, 0);
            Vector3 endRight_fromcenter = new Vector3(1f, 0, 0);
            //
            Vector3 offCenterForward = new Vector3(0, 0, -.2f);
            Vector3 offCenterRight = new Vector3(.2f, 0, 0);
            Vector3 offCenterUp = new Vector3(0, .2f, 0);
            //
            Vector3 endForward_fromoffcenter = offCenterRight + endForward_fromcenter;
            Vector3 endUp_fromoffcenter = offCenterRight + endUp_fromcenter;
            Vector3 endRight_fromoffcenter = offCenterUp + endRight_fromcenter;


            // order is  0,1,2 ,2,1,3  of vertices 0,1,2,3   0 will always be center

            vertices = new VertexPositionColorTexture[12];

            // forward
            vertices[0].Position = center; vertices[0].Color = color; vertices[0].TextureCoordinate = new Vector2(0f, 0f);
            vertices[1].Position = endForward_fromcenter; vertices[1].Color = color; vertices[1].TextureCoordinate = new Vector2(1f, 0f);
            vertices[2].Position = offCenterRight; vertices[2].Color = color; vertices[2].TextureCoordinate = new Vector2(0f, .33f);
            vertices[3].Position = endForward_fromoffcenter; vertices[3].Color = color; vertices[3].TextureCoordinate = new Vector2(1f, .33f);

            // right
            vertices[4].Position = center; vertices[4].Color = color; vertices[4].TextureCoordinate = new Vector2(0f, .66f);
            vertices[5].Position = endRight_fromcenter; vertices[5].Color = color; vertices[5].TextureCoordinate = new Vector2(1f, .66f);
            vertices[6].Position = offCenterUp; vertices[6].Color = color; vertices[6].TextureCoordinate = new Vector2(0f, .33f);
            vertices[7].Position = endRight_fromoffcenter; vertices[7].Color = color; vertices[7].TextureCoordinate = new Vector2(1f, .33f);

            // up square
            vertices[8].Position = center; vertices[8].Color = color; vertices[8].TextureCoordinate = new Vector2(0f, .66f);
            vertices[9].Position = endUp_fromcenter; vertices[9].Color = color; vertices[9].TextureCoordinate = new Vector2(1f, .66f);
            vertices[10].Position = offCenterRight; vertices[10].Color = color; vertices[10].TextureCoordinate = new Vector2(0f, 1f);
            vertices[11].Position = endUp_fromoffcenter; vertices[11].Color = color; vertices[11].TextureCoordinate = new Vector2(1f, 1f);

            indices = new int[18];
            indices[0] = 0; indices[1] = 1; indices[2] = 2;
            indices[3] = 2; indices[4] = 1; indices[5] = 3;

            indices[6] = 4; indices[7] = 5; indices[8] = 6;
            indices[9] = 6; indices[10] = 5; indices[11] = 7;

            indices[12] = 8; indices[13] = 9; indices[14] = 10;
            indices[15] = 10; indices[16] = 9; indices[17] = 11;
        }
        public void Draw(GraphicsDevice gd, Effect effect)
        {
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, (indices.Length / 3), VertexPositionColorTexture.VertexDeclaration);
            }
        }
    }

    /// <summary>
    /// This is basically my super sphere created by and last updated willmotil 2019.
    /// This is a sphere or a sky sphere. A face resolution of 2 is also a cube or sky cube.
    /// It can use 6 seperate images on 6 faces or a cross or blender block type texture..
    /// Both Sphere and skyShere Uses CCW culling in regular operation.
    /// It generates positions normals texture and tangents for normal maping.
    /// It tesselates face points into sphereical coordinates on creation.
    /// It can also switch tangent or normal directions or u v that shouldn't be needed though.
    /// </summary>
    public class SpherePNTT
    {
        bool changeToSkySphere = false;
        bool useSingleImageTexture = false;
        bool blenderStyleElseCross = false;
        bool flipTangentSign = false;
        bool flipVerticeWindingToCW = false;
        bool flipNormalDirection = false;
        bool flipU = false;
        bool flipV = false;
        int verticeFaceResolution = 3;
        float scale = 1f;

        int verticeFaceDrawOffset = 0;
        int indiceFaceDrawOffset = 0;
        int verticesPerFace = 0;
        int indicesPerFace = 0;
        int primitivesPerFace = 0;

        // face identifiers
        const int FaceFront = 0;
        const int FaceBack = 1;
        const int FaceLeft = 2;
        const int FaceRight = 3;
        const int FaceTop = 4;
        const int FaceBottom = 5;

        public VertexPositionColorNormalTextureTangent[] vertices = new VertexPositionColorNormalTextureTangent[24];
        public int[] indices = new int[36];

        /// <summary>
        /// Defaults to a single image hexahedron used on all faces. 
        /// Use the other overloads if you want something more specific like a sphere by increasing vertexResolutionPerFace.
        /// The spheres are counter clockwise wound that can be changed by setting the skysphere bool or fliping the winding direction.
        /// The skySphere is clockwise wound normally 
        /// if you are using opposite or strange orders for normals or whatever this has option to match just about everything.
        /// </summary>
        public SpherePNTT()
        {
            CreateSixFaceSphere(true, false, false, false, false,false, false, false, verticeFaceResolution, scale);
        }
        // seperate faces
        public SpherePNTT(bool changeToSkySphere)
        {
            CreateSixFaceSphere(changeToSkySphere, false, false, false,false, false, false, false, verticeFaceResolution, scale);
        }
        // seperate faces at resolution
        public SpherePNTT(bool changeToSkySphere, int vertexResolutionPerFace, float scale)
        {
            CreateSixFaceSphere(changeToSkySphere, false, false, false, false, false, false, false, vertexResolutionPerFace, scale);
        }
        public SpherePNTT(bool changeToSkySphere, int vertexResolutionPerFace, float scale, bool flipWindingDirection)
        {
            CreateSixFaceSphere(changeToSkySphere, false, false, false, flipWindingDirection,false, false, false, vertexResolutionPerFace, scale);
        }
        public SpherePNTT(bool changeToSkySphere, int vertexResolutionPerFace, float scale, bool flipNormalDirection, bool flipWindingDirection)
        {
            CreateSixFaceSphere(changeToSkySphere, false, false, flipNormalDirection, flipWindingDirection, false, false, false, vertexResolutionPerFace, scale);
        }
        /// <summary>
        /// Set the type, if the faces are in a single image or six seperate images and if the single image is a cross or blender type image.
        /// Additionally specify the number of vertices per face this value is squared as it is used for rows and columns.
        /// </summary>
        public SpherePNTT(bool changeToSkySphere, bool changeToSingleImageTexture, bool blenderStyleSkyBox, int vertexResolutionPerFace, float scale)
        {
            CreateSixFaceSphere(changeToSkySphere, changeToSingleImageTexture, blenderStyleSkyBox, false,false, false, false, false, vertexResolutionPerFace, scale);
        }
        public SpherePNTT(bool changeToSkySphere, bool changeToSingleImageTexture, bool blenderStyleSkyBox, int vertexResolutionPerFace, float scale, bool flipWindingDirection)
        {
            CreateSixFaceSphere(changeToSkySphere, changeToSingleImageTexture, blenderStyleSkyBox, false, flipWindingDirection, false, false, false, vertexResolutionPerFace, scale);
        }
        public SpherePNTT(bool changeToSkySphere, bool changeToSingleImageTexture, bool blenderStyleSkyBox, int vertexResolutionPerFace, float scale, bool flipNormalDirection, bool flipWindingDirection)
        {
            CreateSixFaceSphere(changeToSkySphere, changeToSingleImageTexture, blenderStyleSkyBox, flipNormalDirection, flipWindingDirection, false, false, false, vertexResolutionPerFace, scale);
        }
        public SpherePNTT(bool changeToSkySphere, bool changeToSingleImageTexture, bool blenderStyleSkyBox, bool flipNormalDirection , bool flipWindingDirection, bool flipTangentDirection, bool flipTextureDirectionU, bool flipTextureDirectionV, int vertexResolutionPerFace, float scale)
        {
            CreateSixFaceSphere(changeToSkySphere, changeToSingleImageTexture, blenderStyleSkyBox, flipNormalDirection, flipWindingDirection, flipTangentDirection, flipTextureDirectionU, flipTextureDirectionV, vertexResolutionPerFace, scale);
        }

        void CreateSixFaceSphere(bool changeToSkySphere, bool changeToSingleImageTexture , bool blenderStyleElseCross, bool flipNormalDirection , bool flipWindingDirection, bool flipTangentDirection, bool flipU, bool flipV, int vertexResolutionPerFace, float scale)
        {
            this.scale = scale;
            this.changeToSkySphere = changeToSkySphere;
            this.useSingleImageTexture = changeToSingleImageTexture;
            this.blenderStyleElseCross = blenderStyleElseCross;
            this.flipVerticeWindingToCW = flipWindingDirection;
            this.flipNormalDirection = flipNormalDirection;
            this.flipTangentSign = flipTangentDirection;
            this.flipU = flipU;
            this.flipV = flipV;
            if (vertexResolutionPerFace < 2)
                vertexResolutionPerFace = 2;
            this.verticeFaceResolution = vertexResolutionPerFace;
            Vector3 offset = new Vector3(.5f, .5f, .5f);
            // 8 vertice points ill label them, then reassign them for clarity.
            Vector3 LT_f = new Vector3(0, 1, 0) - offset; Vector3 A = LT_f * scale;
            Vector3 LB_f = new Vector3(0, 0, 0) - offset; Vector3 B = LB_f * scale;
            Vector3 RT_f = new Vector3(1, 1, 0) - offset; Vector3 C = RT_f * scale;
            Vector3 RB_f = new Vector3(1, 0, 0) - offset; Vector3 D = RB_f * scale;
            Vector3 LT_b = new Vector3(0, 1, 1) - offset; Vector3 E = LT_b * scale;
            Vector3 LB_b = new Vector3(0, 0, 1) - offset; Vector3 F = LB_b * scale;
            Vector3 RT_b = new Vector3(1, 1, 1) - offset; Vector3 G = RT_b * scale;
            Vector3 RB_b = new Vector3(1, 0, 1) - offset; Vector3 H = RB_b * scale;
            if (flipVerticeWindingToCW)
            {
                LT_f = new Vector3(0, 1, 0) - offset; H = LT_f * scale;
                LB_f = new Vector3(0, 0, 0) - offset; G = LB_f * scale;
                RT_f = new Vector3(1, 1, 0) - offset; F = RT_f * scale;
                RB_f = new Vector3(1, 0, 0) - offset; E = RB_f * scale;
                LT_b = new Vector3(0, 1, 1) - offset; D = LT_b * scale;
                LB_b = new Vector3(0, 0, 1) - offset; C = LB_b * scale;
                RT_b = new Vector3(1, 1, 1) - offset; B = RT_b * scale;
                RB_b = new Vector3(1, 0, 1) - offset; A = RB_b * scale;
            }

            // Six faces to a cube or sphere
            // each face of the cube wont actually share vertices as each will use its own texture.
            // unless it is actually using single skybox texture

            // we will need to precalculate the grids size now
            int vw = vertexResolutionPerFace;
            int vh = vertexResolutionPerFace;
            int vlen = vw * vh * 6; // the extra six here is the number of faces
            int iw = vw - 1;
            int ih = vh - 1;
            int ilen = iw * ih * 6 * 6; // the extra six here is the number of faces
            vertices = new VertexPositionColorNormalTextureTangent[vlen];
            indices = new int[ilen];
            verticeFaceDrawOffset = vlen = vw * vh;
            indiceFaceDrawOffset = ilen = iw * ih * 6;
            verticesPerFace = vertexResolutionPerFace * vertexResolutionPerFace;
            indicesPerFace = iw * ih * 6;
            primitivesPerFace = iw * ih * 2; // 2 triangles per quad

            if (changeToSkySphere)
            {
                // passed uv texture coordinates.
                Vector2 uv0 = new Vector2(1f, 1f);
                Vector2 uv1 = new Vector2(0f, 1f);
                Vector2 uv2 = new Vector2(1f, 0f);
                Vector2 uv3 = new Vector2(0f, 0f);
                SetFaceGrid(FaceFront, D, B, C, A, uv0, uv1, uv2, uv3, vertexResolutionPerFace);
                SetFaceGrid(FaceBack, F, H, E, G, uv0, uv1, uv2, uv3, vertexResolutionPerFace);
                SetFaceGrid(FaceLeft, B, F, A, E, uv0, uv1, uv2, uv3, vertexResolutionPerFace);
                SetFaceGrid(FaceRight, H, D, G, C, uv0, uv1, uv2, uv3, vertexResolutionPerFace);
                SetFaceGrid(FaceTop, C, A, G, E, uv0, uv1, uv2, uv3, vertexResolutionPerFace);
                SetFaceGrid(FaceBottom, H, F, D, B, uv0, uv1, uv2, uv3, vertexResolutionPerFace);
            }
            else // regular cube 
            {
                Vector2 uv0 = new Vector2(0f, 0f);
                Vector2 uv1 = new Vector2(0f, 1f);
                Vector2 uv2 = new Vector2(1f, 0f);
                Vector2 uv3 = new Vector2(1f, 1f);
                SetFaceGrid(FaceFront, A, B, C, D, uv0, uv1, uv2, uv3, vertexResolutionPerFace);
                SetFaceGrid(FaceBack, G, H, E, F, uv0, uv1, uv2, uv3, vertexResolutionPerFace);
                SetFaceGrid(FaceLeft, E, F, A, B, uv0, uv1, uv2, uv3, vertexResolutionPerFace);
                SetFaceGrid(FaceRight, C, D, G, H, uv0, uv1, uv2, uv3, vertexResolutionPerFace);
                SetFaceGrid(FaceTop, E, A, G, C, uv0, uv1, uv2, uv3, vertexResolutionPerFace);
                SetFaceGrid(FaceBottom, B, F, D, H, uv0, uv1, uv2, uv3, vertexResolutionPerFace);
            }
        }

        void SetFaceGrid(int faceMultiplier, Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, Vector2 uv0, Vector2 uv1, Vector2 uv2, Vector2 uv3, int vertexResolution)
        {
            if (useSingleImageTexture)
                UvSkyTextureReassignment(faceMultiplier, ref uv0, ref uv1, ref uv2, ref uv3);
            int vw = vertexResolution;
            int vh = vertexResolution;
            int vlen = vw * vh;
            int iw = vw - 1;
            int ih = vh - 1;
            int ilen = iw * ih * 6;
            // actual start index's
            int vIndex = faceMultiplier * vlen;
            int iIndex = faceMultiplier * ilen;
            // we now must build the grid/
            float ratio = 1f / (float)(vertexResolution - 1);
            // well do it all simultaneously no point in spliting it up
            for (int y = 0; y < vertexResolution; y++)
            {
                float ratioY = (float)y * ratio;
                for (int x = 0; x < vertexResolution; x++)
                {
                    // index
                    int index = vIndex + (y * vertexResolution + x);
                    float ratioX = (float)x * ratio;
                    // calculate uv_n_p tangent comes later
                    var uv = InterpolateUv(uv0, uv1, uv2, uv3, ratioX, ratioY);
                    var n = InterpolateToNormal(v0, v1, v2, v3, ratioX, ratioY);
                    var p = n * .5f * scale; // displace to distance
                    if (changeToSkySphere)
                        n = -n;
                    if (flipNormalDirection)
                        n = -n;
                    // handle u v fliping if its desired.
                    if (flipU)
                        uv.X = 1.0f - uv.X;
                    if (flipV)
                        uv.Y = 1.0f - uv.Y;
                    // assign
                    vertices[index].Position = p;
                    vertices[index].Color = new Vector4(1.0f,1.0f,1.0f,1.0f);
                    vertices[index].TextureCoordinate = uv;
                    vertices[index].Normal = n;
                }
            }

            // ToDo... 
            // We could loop all the vertices which are nearly the exact same and make sure they are the same place but seperate.
            // sort of redundant but floating point errors happen under interpolation, well get back to that later on.
            // not sure i really need to it looks pretty spot on.

            // ok so now we have are positions our normal and uv per vertice we need to loop again and handle the tangents
            for (int y = 0; y < (vertexResolution - 1); y++)
            {
                for (int x = 0; x < (vertexResolution - 1); x++)
                {
                    //
                    int indexV0 = vIndex + (y * vertexResolution + x);
                    int indexV1 = vIndex + ((y + 1) * vertexResolution + x);
                    int indexV2 = vIndex + (y * vertexResolution + (x + 1));
                    int indexV3 = vIndex + ((y + 1) * vertexResolution + (x + 1));
                    var p0 = vertices[indexV0].Position;
                    var p1 = vertices[indexV1].Position;
                    var p2 = vertices[indexV2].Position;
                    var p3 = vertices[indexV3].Position;
                    var t = -(p0 - p1);
                    if (changeToSkySphere)
                        t = -t;
                    t.Normalize();
                    if (flipTangentSign)
                        t = -t;
                    vertices[indexV0].Tangent = t; vertices[indexV1].Tangent = t; vertices[indexV2].Tangent = t; vertices[indexV3].Tangent = t;
                    //
                    // set our indices while were at it.
                    int indexI = iIndex + ((y * (vertexResolution - 1) + x) * 6);
                    int via = indexV0, vib = indexV1, vic = indexV2, vid = indexV3;
                    indices[indexI + 0] = via; indices[indexI + 1] = vib; indices[indexI + 2] = vic;
                    indices[indexI + 3] = vic; indices[indexI + 4] = vib; indices[indexI + 5] = vid;
                }
            }
        }

        // this allows for the use of a single texture skybox.
        void UvSkyTextureReassignment(int faceMultiplier, ref Vector2 uv0, ref Vector2 uv1, ref Vector2 uv2, ref Vector2 uv3)
        {
            if (useSingleImageTexture)
            {
                Vector2 tupeBuvwh = new Vector2(.250000000f, .333333333f); // this is a 8 square left sided skybox
                Vector2 tupeAuvwh = new Vector2(.333333333f, .500000000f); // this is a 6 square blender type skybox
                Vector2 currentuvWH = tupeBuvwh;
                Vector2 uvStart = Vector2.Zero;
                Vector2 uvEnd = Vector2.Zero;

                // crossstyle
                if (blenderStyleElseCross == false)
                {
                    currentuvWH = tupeBuvwh;
                    switch (faceMultiplier)
                    {
                        case FaceFront:
                            uvStart = new Vector2(currentuvWH.X * 1f, currentuvWH.Y * 1f);
                            uvEnd = uvStart + currentuvWH;
                            break;
                        case FaceBack:
                            uvStart = new Vector2(currentuvWH.X * 3f, currentuvWH.Y * 1f);
                            uvEnd = uvStart + currentuvWH;
                            break;
                        case FaceRight:
                            uvStart = new Vector2(currentuvWH.X * 2f, currentuvWH.Y * 1f);
                            uvEnd = uvStart + currentuvWH;
                            break;
                        case FaceLeft:
                            uvStart = new Vector2(currentuvWH.X * 0f, currentuvWH.Y * 1f);
                            uvEnd = uvStart + currentuvWH;
                            break;
                        case FaceTop:
                            uvStart = new Vector2(currentuvWH.X * 1f, currentuvWH.Y * 0f);
                            uvEnd = uvStart + currentuvWH;
                            break;
                        case FaceBottom:
                            uvStart = new Vector2(currentuvWH.X * 1f, currentuvWH.Y * 2f);
                            uvEnd = uvStart + currentuvWH;
                            break;
                    }
                    if (changeToSkySphere)
                    {
                        uv0 = new Vector2(uvEnd.X, uvEnd.Y); uv1 = new Vector2(uvStart.X, uvEnd.Y); uv2 = new Vector2(uvEnd.X, uvStart.Y); uv3 = new Vector2(uvStart.X, uvStart.Y);
                    }
                    else
                    {
                        uv0 = new Vector2(uvStart.X, uvStart.Y); uv1 = new Vector2(uvStart.X, uvEnd.Y); uv2 = new Vector2(uvEnd.X, uvStart.Y); uv3 = new Vector2(uvEnd.X, uvEnd.Y);
                    }
                }
                else
                {
                    currentuvWH = tupeAuvwh;
                    switch (faceMultiplier)
                    {
                        case FaceLeft:
                            uvStart = new Vector2(currentuvWH.X * 0f, currentuvWH.Y * 0f);
                            uvEnd = uvStart + currentuvWH;
                            break;
                        case FaceBack:
                            uvStart = new Vector2(currentuvWH.X * 1f, currentuvWH.Y * 0f);
                            uvEnd = uvStart + currentuvWH;
                            break;
                        case FaceRight:
                            uvStart = new Vector2(currentuvWH.X * 2f, currentuvWH.Y * 0f);
                            uvEnd = uvStart + currentuvWH;
                            break;
                        case FaceBottom:
                            uvStart = new Vector2(currentuvWH.X * 0f, currentuvWH.Y * 1f);
                            uvEnd = uvStart + currentuvWH;
                            break;
                        case FaceTop:
                            uvStart = new Vector2(currentuvWH.X * 1f, currentuvWH.Y * 1f);
                            uvEnd = uvStart + currentuvWH;
                            break;
                        case FaceFront:
                            uvStart = new Vector2(currentuvWH.X * 2f, currentuvWH.Y * 1f);
                            uvEnd = uvStart + currentuvWH;
                            break;
                    }
                    if (changeToSkySphere)
                    {
                        uv0 = new Vector2(uvEnd.X, uvEnd.Y); uv2 = new Vector2(uvEnd.X, uvStart.Y); uv1 = new Vector2(uvStart.X, uvEnd.Y); uv3 = new Vector2(uvStart.X, uvStart.Y);
                    }
                    else
                    {
                        uv0 = new Vector2(uvStart.X, uvStart.Y); uv1 = new Vector2(uvStart.X, uvEnd.Y); uv2 = new Vector2(uvEnd.X, uvStart.Y); uv3 = new Vector2(uvEnd.X, uvEnd.Y);
                    }
                }
            }
        }

        Vector3 InterpolateToNormal(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, float timeX, float timeY)
        {
            var y0 = ((v1 - v0) * timeY + v0);
            var y1 = ((v3 - v2) * timeY + v2);
            var n = ((y1 - y0) * timeX + y0) * 10f; // * 10f ensure its sufficiently denormalized.
            n.Normalize();
            return n;
        }
        Vector2 InterpolateUv(Vector2 v0, Vector2 v1, Vector2 v2, Vector2 v3, float timeX, float timeY)
        {
            var y0 = ((v1 - v0) * timeY + v0);
            var y1 = ((v3 - v2) * timeY + v2);
            return ((y1 - y0) * timeX + y0);
        }

        public void Draw(GraphicsDevice gd, Effect effect)
        {
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, (indices.Length / 3), VertexPositionColorNormalTextureTangent.VertexDeclaration);
            }
        }

        /// <summary>
        /// Seperate faced cube or sphere or sky
        /// This method is pretty dependant on being able to pass to textureA not good but....
        /// </summary>
        public void Draw(GraphicsDevice gd, Effect effect, Texture2D front, Texture2D back, Texture2D left, Texture2D right, Texture2D top, Texture2D bottom)
        {
            int FaceFront = 0;
            int FaceBack = 1;
            int FaceLeft = 2;
            int FaceRight = 3;
            int FaceTop = 4;
            int FaceBottom = 5;
            for (int t = 0; t < 6; t++)
            {
                if (t == FaceFront) effect.Parameters["TextureA"].SetValue(front);
                if (t == FaceBack) effect.Parameters["TextureA"].SetValue(back);
                if (t == FaceLeft) effect.Parameters["TextureA"].SetValue(left);
                if (t == FaceRight) effect.Parameters["TextureA"].SetValue(right);
                if (t == FaceTop) effect.Parameters["TextureA"].SetValue(top);
                if (t == FaceBottom) effect.Parameters["TextureA"].SetValue(bottom);
                int ifoffset = t * indicesPerFace;
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, ifoffset, primitivesPerFace, VertexPositionColorNormalTextureTangent.VertexDeclaration);
                }
            }
        }

        /// <summary>
        /// Single texture multi faced cube or sphere or sky
        /// This method is pretty dependant on being able to pass to textureA not good but....
        /// </summary>
        public void Draw(GraphicsDevice gd, Effect effect, Texture2D cubeTexture)
        {
            effect.Parameters["TextureA"].SetValue(cubeTexture);
            for (int t = 0; t < 6; t++)
            {
                int ifoffset = t * indicesPerFace;
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, ifoffset, primitivesPerFace, VertexPositionColorNormalTextureTangent.VertexDeclaration);
                }
            }
        }

        /// <summary>
        /// This method is pretty dependant on being able to pass to textureA not good but....
        /// </summary>
        public void DrawWithBasicEffect(GraphicsDevice gd, BasicEffect effect, Texture2D front, Texture2D back, Texture2D left, Texture2D right, Texture2D top, Texture2D bottom)
        {
            int FaceFront = 0;
            int FaceBack = 1;
            int FaceLeft = 2;
            int FaceRight = 3;
            int FaceTop = 4;
            int FaceBottom = 5;
            for (int t = 0; t < 6; t++)
            {
                if (t == FaceFront) effect.Texture = front;
                if (t == FaceBack) effect.Texture = back;
                if (t == FaceLeft) effect.Texture = left;
                if (t == FaceRight) effect.Texture = right;
                if (t == FaceTop) effect.Texture = top;
                if (t == FaceBottom) effect.Texture = bottom;
                int ifoffset = t * indicesPerFace;
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, ifoffset, primitivesPerFace, VertexPositionColorNormalTextureTangent.VertexDeclaration);
                }
            }
        }

        /// <summary>
        /// Single texture multi faced cube or sphere or sky
        /// This method is pretty dependant on being able to pass to textureA not good but....
        /// </summary>
        public void DrawWithBasicEffect(GraphicsDevice gd, BasicEffect effect, Texture2D cubeTexture)
        {
            effect.Texture = cubeTexture;
            for (int t = 0; t < 6; t++)
            {
                int ifoffset = t * indicesPerFace;
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, ifoffset, primitivesPerFace, VertexPositionColorNormalTextureTangent.VertexDeclaration);
                }
            }
        }

        public Vector3 Norm(Vector3 n)
        {
            return Vector3.Normalize(n);
        }

        /// <summary>
        /// Positional cross product, Counter Clock wise positive.
        /// </summary>
        public static Vector3 CrossVectors3d(Vector3 a, Vector3 b, Vector3 c)
        {
            // no point in doing reassignments the calculation is straight forward.
            return new Vector3
                (
                ((b.Y - a.Y) * (c.Z - b.Z)) - ((c.Y - b.Y) * (b.Z - a.Z)),
                ((b.Z - a.Z) * (c.X - b.X)) - ((c.Z - b.Z) * (b.X - a.X)),
                ((b.X - a.X) * (c.Y - b.Y)) - ((c.X - b.X) * (b.Y - a.Y))
                );
        }

        /// <summary>
        /// use the vector3 cross
        /// </summary>
        public static Vector3 CrossXna(Vector3 a, Vector3 b, Vector3 c)
        {
            var v1 = a - b;
            var v2 = c - b;

            return Vector3.Cross(v1, v2);
        }

        //// vertex structure data.
        //public struct VertexPositionColorNormalTextureTangent : IVertexType
        //{
        //    public Vector3 Position;
        //    public Vector4 Color;
        //    public Vector3 Normal;
        //    public Vector2 TextureCoordinate;
        //    public Vector3 Tangent;

        //    public static VertexDeclaration VertexDeclaration = new VertexDeclaration
        //    (
        //          new VertexElement(VertexElementByteOffset.PositionStartOffset(), VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
        //          new VertexElement(VertexElementByteOffset.OffsetVector4(), VertexElementFormat.Vector4, VertexElementUsage.Color, 0),
        //          new VertexElement(VertexElementByteOffset.OffsetVector3(), VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
        //          new VertexElement(VertexElementByteOffset.OffsetVector2(), VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
        //          new VertexElement(VertexElementByteOffset.OffsetVector3(), VertexElementFormat.Vector3, VertexElementUsage.Normal, 1)
        //    );
        //    VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }
        //}
        ///// <summary>
        ///// This is a helper struct for tallying byte offsets
        ///// </summary>
        //public struct VertexElementByteOffset
        //{
        //    public static int currentByteSize = 0;
        //    [STAThread]
        //    public static int PositionStartOffset() { currentByteSize = 0; var s = sizeof(float) * 3; currentByteSize += s; return currentByteSize - s; }
        //    public static int Offset(float n) { var s = sizeof(float); currentByteSize += s; return currentByteSize - s; }
        //    public static int Offset(Vector2 n) { var s = sizeof(float) * 2; currentByteSize += s; return currentByteSize - s; }
        //    public static int Offset(Color n) { var s = sizeof(int); currentByteSize += s; return currentByteSize - s; }
        //    public static int Offset(Vector3 n) { var s = sizeof(float) * 3; currentByteSize += s; return currentByteSize - s; }
        //    public static int Offset(Vector4 n) { var s = sizeof(float) * 4; currentByteSize += s; return currentByteSize - s; }

        //    public static int OffsetFloat() { var s = sizeof(float); currentByteSize += s; return currentByteSize - s; }
        //    public static int OffsetColor() { var s = sizeof(int); currentByteSize += s; return currentByteSize - s; }
        //    public static int OffsetVector2() { var s = sizeof(float) * 2; currentByteSize += s; return currentByteSize - s; }
        //    public static int OffsetVector3() { var s = sizeof(float) * 3; currentByteSize += s; return currentByteSize - s; }
        //    public static int OffsetVector4() { var s = sizeof(float) * 4; currentByteSize += s; return currentByteSize - s; }
        //}
    }

    public struct VertexPositionColorNormalTextureTangent : IVertexType
    {
        public Vector3 Position;
        public Vector4 Color;
        public Vector3 Normal;
        public Vector2 TextureCoordinate;
        public Vector3 Tangent;

        public static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
              new VertexElement(VertexElementByteOffset.PositionStartOffset(), VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
              new VertexElement(VertexElementByteOffset.OffsetVector4(), VertexElementFormat.Vector4, VertexElementUsage.Color, 0),
              new VertexElement(VertexElementByteOffset.OffsetVector3(), VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
              new VertexElement(VertexElementByteOffset.OffsetVector2(), VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
              new VertexElement(VertexElementByteOffset.OffsetVector3(), VertexElementFormat.Vector3, VertexElementUsage.Normal, 1)
        );
        VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }
    }
    public struct VertexPositionColorNormalTexture : IVertexType
    {
        public Vector3 Position;
        public Vector4 Color;
        public Vector3 Normal;
        public Vector2 TextureCoordinate;

        public static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
              new VertexElement(VertexElementByteOffset.PositionStartOffset(), VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
              new VertexElement(VertexElementByteOffset.OffsetVector4(), VertexElementFormat.Vector4, VertexElementUsage.Color, 0),
              new VertexElement(VertexElementByteOffset.OffsetVector3(), VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
              new VertexElement(VertexElementByteOffset.OffsetVector2(), VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
        );
        VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }
    }

}
