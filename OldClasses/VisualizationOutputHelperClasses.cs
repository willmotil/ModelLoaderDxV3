using System;
using System.Text;
//using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Framework
{

    public static class Extensions
    {
        // exensions

        public static Vector3 SubtractAllBy(this Vector3 v, float n)
        {
            return new Vector3(v.X - n, v.Y - n, v.Z - n);
        }
        public static Vector3 SubtractBy(this Vector3 v, Vector3 n)
        {
            return new Vector3(v.X - n.X, v.Y - n.Y, v.Z - n.X);
        }

        public static Color ToColor(this Vector4 v)
        {
            return new Color(v.X, v.Y, v.Z, v.W);
        }
        public static Vector4 ToVector4(this Color v)
        {
            return new Vector4(1f / v.R, 1f / v.G, 1f / v.B, 1f / v.A);
        }
        public static int PaddingAmount = 6;

        public static string ToStringTrimed(this int v)
        {
            string d = "+0.000;-0.000"; // "0.00";
            return (v.ToString(d).PadRight(PaddingAmount));
        }
        public static string ToStringTrimed(this float v)
        {
            string d = "+0.000;-0.000"; // "0.00";
            return (v.ToString(d).PadRight(PaddingAmount));
        }
        public static string ToStringTrimed(this double v)
        {
            string d = "+0.000;-0.000"; // "0.00";
            return (v.ToString(d).PadRight(PaddingAmount));
        }
        public static string ToStringTrimed(this Vector2 v)
        {
            string d = "+0.000;-0.000"; // "0.00";
            return ( "[" + v.X.ToString(d).PadRight(PaddingAmount) + ", " + v.Y.ToString(d).PadRight(PaddingAmount) + "]");
        }
        public static string ToStringTrimed(this Vector3 v)
        {
            string d = "+0.000;-0.000"; // "0.00";
            return ( "[" + v.X.ToString(d).PadRight(PaddingAmount) + ", " + v.Y.ToString(d).PadRight(PaddingAmount) + ", " + v.Z.ToString(d).PadRight(PaddingAmount) + "]" );
        }
        public static string ToStringTrimed(this Vector4 v)
        {
            string d = "+0.000;-0.000"; // "0.00";
            return ( "["+ v.X.ToString(d).PadRight(PaddingAmount) + ", " + v.Y.ToString(d).PadRight(PaddingAmount) + ", " + v.Z.ToString(d).PadRight(PaddingAmount) + ", " + v.W.ToString(d).PadRight(PaddingAmount) +"]");
        }
        public static string ToStringTrimed(this Microsoft.Xna.Framework.Quaternion q)
        {
            string d = "+0.000;-0.000"; // "0.00";
            return ("[" + "x: " + q.X.ToString(d).PadRight(PaddingAmount) + "y: " + q.Y.ToString(d).PadRight(PaddingAmount) + "z: " + q.Z.ToString(d).PadRight(PaddingAmount) + "w: " + q.W.ToString(d).PadRight(PaddingAmount) + "]");
        }

        public static Matrix Invert(this Matrix m)
        {
            return Matrix.Invert(m);
        }
        public static Matrix Inverse(this Matrix m)
        {
            return Matrix.Invert(m);
        }
    }

    /// <summary>
    /// This is a camera i basically remade to make it work. 
    /// Using quite a bit of stuff from my camera class its nearly the same as mine but its a bit simpler. 
    /// I have bunches of cameras at this point and i need to combine them into a fully hard core non basic camera.
    /// That said simple makes for a better example and a better basis to combine them later.
    /// </summary>
    public class Basic3dExampleCamera
    {
        private GraphicsDevice graphicsDevice = null;
        private GameWindow gameWindow = null;

        private MouseState oldmouseState = default(MouseState);
        private KeyboardState oldkeyboardState = default(KeyboardState);
        MouseState mouseState = default(MouseState);
        KeyboardState keyboardState = default(KeyboardState);

        public float MovementUnitsPerSecond { get; set; } = 30f;
        public float RotationDegreesPerSecond { get; set; } = 60f;

        public float FieldOfViewDegrees { get; set; } = 80f;
        public float NearClipPlane { get; set; } = .05f;
        public float FarClipPlane { get; set; } = 999900f;

        private float yMouseAngle = 0f;
        private float xMouseAngle = 0f;
        private bool mouseLookIsUsed = true;

        private int KeyboardLayout = 1;
        private int cameraTypeOption = 1;

        /// <summary>
        /// operates pretty much like a fps camera.
        /// </summary>
        public const int CAM_UI_OPTION_FPS_STRAFE_LAYOUT = 0;
        /// <summary>
        /// I put this one on by default.
        /// free cam i use this for editing its more like a air plane or space camera.
        /// the horizon is not corrected for in this one so use the z and c keys to roll 
        /// hold the right mouse to look with it.
        /// </summary>
        public const int CAM_UI_OPTION_EDIT_LAYOUT = 1;
        /// <summary>
        /// Determines how the camera behaves fixed 0  free 1
        /// </summary>

        /// <summary>
        /// A fixed camera is typically used in fps games. It is called a fixed camera because the up is stabalized to the system vectors up.
        /// However be aware that this means if the forward vector or were you are looking is directly up or down you will gimble lock.
        /// Typically this is not allowed in many fps or rather it is faked so you can never truely look directly up or down.
        /// </summary>
        public const int CAM_TYPE_OPTION_FIXED = 0;
        /// <summary>
        /// A free camera has its up vector unlocked perfect for a space sim fighter game or editing. 
        /// It won't gimble lock. Provided the up is crossed from the right forward occasionally it can't gimble lock.
        /// The draw back is the horizon stabilization needs to be handled for some types of games.
        /// </summary>
        public const int CAM_TYPE_OPTION_FREE = 1;


        public bool UseOthorgraphic = false;


        /// <summary>
        /// Constructs the camera.
        /// </summary>
        public Basic3dExampleCamera(GraphicsDevice gfxDevice, GameWindow window, bool useOthographic)
        {
            UseOthorgraphic = useOthographic;
            graphicsDevice = gfxDevice;
            gameWindow = window;
            ReCreateWorldAndView();
            ReCreateThePerspectiveProjectionMatrix(gfxDevice, FieldOfViewDegrees);
            oldmouseState = default(MouseState);
            oldkeyboardState = default(KeyboardState);
        }

        /// <summary>
        /// Select how you want the ui to feel or how to control the camera using Basic3dExampleCamera. CAM_UI_ options
        /// </summary>
        /// <param name="UiOption"></param>
        public void CameraUi(int UiOption)
        {
            KeyboardLayout = UiOption;
        }
        /// <summary>
        /// Select a camera type fixed free or other. using Basic3dExampleCamera. CAM_TYPE_ options
        /// </summary>
        /// <param name="cameraOption"></param>
        public void CameraType(int cameraOption)
        {
            cameraTypeOption = cameraOption;
        }

        /// <summary>
        /// This serves as the cameras up for fixed cameras this might not change at all ever for free cameras it changes constantly.
        /// A fixed camera keeps a fixed horizon but can gimble lock under normal rotation when looking straight up or down.
        /// A free camera has no fixed horizon but can't gimble lock under normal rotation as the up changes as the camera moves.
        /// Most hybrid cameras are a blend of the two but all are based on one or both of the above.
        /// </summary>
        private Vector3 up = Vector3.Up;
        /// <summary>
        /// this serves as the cameras world orientation 
        /// it holds all orientational values and is used to move the camera properly thru the world space as well.
        /// </summary>
        private Matrix camerasWorld = Matrix.Identity;
        /// <summary>
        /// The view matrix is created from the cameras world matrixs but it has special propertys.
        /// Using create look at to create this matrix you move from the world space into the view space.
        /// If you are working on world objects you should not take individual elements from this to directly operate on world matrix components.
        /// As well the multiplication of a view matrix by a world matrix moves the resulting matrix into view space itself.
        /// </summary>
        private Matrix viewMatrix = Matrix.Identity;
        /// <summary>
        /// The projection matrix.
        /// </summary>
        private Matrix projectionMatrix = Matrix.Identity;

        /// <summary>
        /// Gets or sets the the camera's position in the world.
        /// </summary>
        public Vector3 Position
        {
            set
            {
                camerasWorld.Translation = value;
                // since we know here that a change has occured to the cameras world orientations we can update the view matrix.
                ReCreateWorldAndView();
            }
            get { return camerasWorld.Translation; }
        }
        /// <summary>
        /// Gets or Sets the direction the camera is looking at in the world.
        /// The forward is the same as the look at direction it i a directional vector not a position.
        /// </summary>
        public Vector3 Forward
        {
            set
            {
                camerasWorld = Matrix.CreateWorld(camerasWorld.Translation, value, up);
                // since we know here that a change has occured to the cameras world orientations we can update the view matrix.
                ReCreateWorldAndView();
            }
            get { return camerasWorld.Forward; }
        }
        /// <summary>
        /// Get or Set the cameras up vector. Don't set the up unless you understand gimble lock.
        /// </summary>
        public Vector3 Up
        {
            set
            {
                up = value;
                camerasWorld = Matrix.CreateWorld(camerasWorld.Translation, camerasWorld.Forward, value);
                // since we know here that a change has occured to the cameras world orientations we can update the view matrix.
                ReCreateWorldAndView();
            }
            get { return up; }
        }

        /// <summary>
        /// Gets or Sets the direction the camera is looking at in the world as a directional vector.
        /// </summary>
        public Vector3 LookAtDirection
        {
            set
            {
                camerasWorld = Matrix.CreateWorld(camerasWorld.Translation, value, up);
                // since we know here that a change has occured to the cameras world orientations we can update the view matrix.
                ReCreateWorldAndView();
            }
            get
            {
                return camerasWorld.Forward;
            }
        }
        /// <summary>
        /// Sets a positional target in the world to look at.
        /// </summary>
        public Vector3 LookAtTargetPosition
        {
            set
            {
                camerasWorld = Matrix.CreateWorld(camerasWorld.Translation, Vector3.Normalize(value - camerasWorld.Translation), up);
                // since we know here that a change has occured to the cameras world orientations we can update the view matrix.
                ReCreateWorldAndView();
            }
        }
        /// <summary>
        /// Turns the camera to face the target this method just takes in the targets matrix for convienience.
        /// </summary>
        public Matrix LookAtTheTargetMatrix
        {
            set
            {
                camerasWorld = Matrix.CreateWorld(camerasWorld.Translation, Vector3.Normalize(value.Translation - camerasWorld.Translation), up);
                // since we know here that a change has occured to the cameras world orientations we can update the view matrix.
                ReCreateWorldAndView();
            }
        }

        /// <summary>
        /// Directly set or get the world matrix this also updates the view matrix
        /// </summary>
        public Matrix World
        {
            get
            {
                return camerasWorld;
            }
            set
            {
                camerasWorld = value;
                viewMatrix = Matrix.CreateLookAt(camerasWorld.Translation, camerasWorld.Forward + camerasWorld.Translation, camerasWorld.Up);
            }
        }

        /// <summary>
        /// Gets the view matrix we never really set the view matrix ourselves outside this method just get it.
        /// The view matrix is remade internally when we know the world matrix forward or position is altered.
        /// </summary>
        public Matrix View
        {
            get
            {
                return viewMatrix;
            }
        }

        /// <summary>
        /// Gets the projection matrix.
        /// </summary>
        public Matrix Projection
        {
            get
            {
                return projectionMatrix;
            }
        }

        /// <summary>
        /// When the cameras position or orientation changes, we call this to ensure that the cameras world matrix is orthanormal.
        /// We also set the up depending on our choices of is fix or free camera and we then update the view matrix.
        /// </summary>
        private void ReCreateWorldAndView()
        {
            if (cameraTypeOption == 0)
                up = Vector3.Up;
            if (cameraTypeOption == 1)
                up = camerasWorld.Up;

            if (camerasWorld.Forward.X == float.NaN)
                camerasWorld.Forward = new Vector3(.01f, .1f, 1f);

            camerasWorld = Matrix.CreateWorld(camerasWorld.Translation, camerasWorld.Forward, up);
            viewMatrix = Matrix.CreateLookAt(camerasWorld.Translation, camerasWorld.Forward + camerasWorld.Translation, camerasWorld.Up);
        }

        /// <summary>
        /// Changes the perspective matrix to a new near far and field of view.
        /// </summary>
        public void ReCreateThePerspectiveProjectionMatrix(GraphicsDevice gd, float fovInDegrees)
        {
            if (UseOthorgraphic)
                projectionMatrix = Matrix.CreateScale(1, -1, 1) * Matrix.CreateOrthographicOffCenter(0, gd.Viewport.Width, gd.Viewport.Height, 0, 0, FarClipPlane);
            else
                projectionMatrix = Matrix.CreatePerspectiveFieldOfView(fovInDegrees * (float)((3.14159265358f) / 180f), gd.Viewport.Width / gd.Viewport.Height, NearClipPlane, FarClipPlane);
        }
        /// <summary>
        /// Changes the perspective matrix to a new near far and field of view.
        /// The projection matrix is typically only set up once at the start of the app.
        /// </summary>
        public void ReCreateThePerspectiveProjectionMatrix(GraphicsDevice gd, float fieldOfViewInDegrees, float nearPlane, float farPlane)
        {
            this.FieldOfViewDegrees = MathHelper.ToRadians(fieldOfViewInDegrees);
            NearClipPlane = nearPlane;
            FarClipPlane = farPlane;
            float aspectRatio = graphicsDevice.Viewport.Width / (float)graphicsDevice.Viewport.Height;

            // create the projection matrix.          
            if (UseOthorgraphic)
                projectionMatrix = Matrix.CreateScale(1, -1, 1) * Matrix.CreateOrthographicOffCenter(0, gd.Viewport.Width, gd.Viewport.Height, 0, 0, FarClipPlane);
            else
            {
                projectionMatrix = Matrix.CreatePerspectiveFieldOfView(this.FieldOfViewDegrees, aspectRatio, NearClipPlane, FarClipPlane);
            }
        }

        /// <summary>
        /// update the camera.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            oldmouseState = mouseState;
            oldkeyboardState = keyboardState;
            mouseState = Mouse.GetState();
            keyboardState = Keyboard.GetState();
            if (KeyboardLayout == CAM_UI_OPTION_FPS_STRAFE_LAYOUT)
                FpsStrafeUiControlsLayout(gameTime);
            if (KeyboardLayout == CAM_UI_OPTION_EDIT_LAYOUT)
                EditingUiControlsLayout(gameTime);
        }

        /// <summary>
        /// like a fps game.
        /// </summary>
        /// <param name="gameTime"></param>
        private void FpsStrafeUiControlsLayout(GameTime gameTime)
        {
            if (keyboardState.IsKeyDown(Keys.W))
            {
                MoveUp(gameTime);
            }
            else if (keyboardState.IsKeyDown(Keys.S) == true)
            {
                MoveDown(gameTime);
            }
            // strafe. 
            if (keyboardState.IsKeyDown(Keys.A) == true)
            {
                MoveLeft(gameTime);
            }
            else if (keyboardState.IsKeyDown(Keys.D) == true)
            {
                MoveRight(gameTime);
            }

            if (keyboardState.IsKeyDown(Keys.Q) == true)
            {
                MoveBackward(gameTime);
            }
            else if (keyboardState.IsKeyDown(Keys.E) == true)
            {
                MoveForward(gameTime);
            }

            // roll counter clockwise
            if (keyboardState.IsKeyDown(Keys.Z) == true)
            {
                RotateRollCounterClockwise(gameTime);
            }
            // roll clockwise
            else if (keyboardState.IsKeyDown(Keys.C) == true)
            {
                RotateRollClockwise(gameTime);
            }


            if (mouseState.RightButton == ButtonState.Pressed)
            {
                if (mouseLookIsUsed == false)
                    mouseLookIsUsed = true;
                else
                    mouseLookIsUsed = false;
            }

            if (mouseLookIsUsed)
            {
                if (oldmouseState.RightButton == ButtonState.Pressed)
                {
                    Vector2 diff = MouseChange(graphicsDevice, oldmouseState, mouseLookIsUsed, 2.0f);
                    if (diff.X != 0f)
                        RotateLeftOrRight(gameTime, diff.X);
                    if (diff.Y != 0f)
                        RotateUpOrDown(gameTime, diff.Y);
                }
                else
                {
                    var pos = new Point(graphicsDevice.Viewport.Width / 2, graphicsDevice.Viewport.Height / 2);
                    Mouse.SetPosition(pos.X, pos.Y);
                }
            }
        }

        /// <summary>
        /// when working like programing editing and stuff.
        /// </summary>
        /// <param name="gameTime"></param>
        private void EditingUiControlsLayout(GameTime gameTime)
        {
            if (keyboardState.IsKeyDown(Keys.E))
            {
                MoveForward(gameTime);
            }
            else if (keyboardState.IsKeyDown(Keys.Q) == true)
            {
                MoveBackward(gameTime);
            }
            if (keyboardState.IsKeyDown(Keys.W))
            {
                RotateUp(gameTime);
            }
            else if (keyboardState.IsKeyDown(Keys.S) == true)
            {
                RotateDown(gameTime);
            }
            if (keyboardState.IsKeyDown(Keys.A) == true)
            {
                RotateLeft(gameTime);
            }
            else if (keyboardState.IsKeyDown(Keys.D) == true)
            {
                RotateRight(gameTime);
            }

            if (keyboardState.IsKeyDown(Keys.Left) == true)
            {
                MoveLeft(gameTime);
            }
            else if (keyboardState.IsKeyDown(Keys.Right) == true)
            {
                MoveRight(gameTime);
            }
            // rotate 
            if (keyboardState.IsKeyDown(Keys.Up) == true)
            {
                MoveUp(gameTime);
            }
            else if (keyboardState.IsKeyDown(Keys.Down) == true)
            {
                MoveDown(gameTime);
            }

            // roll counter clockwise
            if (keyboardState.IsKeyDown(Keys.Z) == true)
            {
                if (cameraTypeOption == CAM_TYPE_OPTION_FREE)
                    RotateRollCounterClockwise(gameTime);
            }
            // roll clockwise
            else if (keyboardState.IsKeyDown(Keys.C) == true)
            {
                if (cameraTypeOption == CAM_TYPE_OPTION_FREE)
                    RotateRollClockwise(gameTime);
            }

            if (mouseState.RightButton == ButtonState.Pressed)
            {
                mouseLookIsUsed = true;
            }
            else
                mouseLookIsUsed = false;
            if (mouseLookIsUsed)
            {
                if (oldmouseState.RightButton == ButtonState.Pressed)
                {
                    Vector2 diff = MouseChange(graphicsDevice, oldmouseState, mouseLookIsUsed, 2.0f);
                    if (diff.X != 0f)
                        RotateLeftOrRight(gameTime, diff.X);
                    if (diff.Y != 0f)
                        RotateUpOrDown(gameTime, diff.Y);
                }
                else
                {
                    var pos = new Point(graphicsDevice.Viewport.Width / 2, graphicsDevice.Viewport.Height / 2);
                    Mouse.SetPosition(pos.X, pos.Y);
                }
            }
        }

        public Vector2 MouseChange(GraphicsDevice gd, MouseState m, bool isCursorSettingPosition, float sensitivity)
        {
            var center = new Point(gd.Viewport.Width / 2, gd.Viewport.Height / 2);
            var delta = m.Position.ToVector2() - center.ToVector2();
            if (isCursorSettingPosition)
            {
                Mouse.SetPosition((int)center.X, center.Y);
            }
            return delta * sensitivity;
        }

        /// <summary>
        /// This function can be used to check if gimble is about to occur in a fixed camera.
        /// If this value returns 1.0f you are in a state of gimble lock, However even as it gets near to 1.0f you are in danger of problems.
        /// In this case you should interpolate towards a free camera. Or begin to handle it.
        /// Earlier then .9 in some manner you deem to appear fitting otherwise you will get a hard spin effect. Though you may want that.
        /// </summary>
        public float GetGimbleLockDangerValue()
        {
            var c0 = Vector3.Dot(World.Forward, World.Up);
            if (c0 < 0f) c0 = -c0;
            return c0;
        }

        #region Local Translations and Rotations.

        public void MoveForward(GameTime gameTime)
        {
            Position += (camerasWorld.Forward * MovementUnitsPerSecond) * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
        public void MoveBackward(GameTime gameTime)
        {
            Position += (camerasWorld.Backward * MovementUnitsPerSecond) * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
        public void MoveLeft(GameTime gameTime)
        {
            Position += (camerasWorld.Left * MovementUnitsPerSecond) * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
        public void MoveRight(GameTime gameTime)
        {
            Position += (camerasWorld.Right * MovementUnitsPerSecond) * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
        public void MoveUp(GameTime gameTime)
        {
            Position += (camerasWorld.Up * MovementUnitsPerSecond) * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
        public void MoveDown(GameTime gameTime)
        {
            Position += (camerasWorld.Down * MovementUnitsPerSecond) * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public void RotateUp(GameTime gameTime)
        {
            var radians = RotationDegreesPerSecond * (float)gameTime.ElapsedGameTime.TotalSeconds;
            Matrix matrix = Matrix.CreateFromAxisAngle(camerasWorld.Right, MathHelper.ToRadians(radians));
            LookAtDirection = Vector3.TransformNormal(LookAtDirection, matrix);
            ReCreateWorldAndView();
        }
        public void RotateDown(GameTime gameTime)
        {
            var radians = -RotationDegreesPerSecond * (float)gameTime.ElapsedGameTime.TotalSeconds;
            Matrix matrix = Matrix.CreateFromAxisAngle(camerasWorld.Right, MathHelper.ToRadians(radians));
            LookAtDirection = Vector3.TransformNormal(LookAtDirection, matrix);
            ReCreateWorldAndView();
        }
        public void RotateLeft(GameTime gameTime)
        {
            var radians = RotationDegreesPerSecond * (float)gameTime.ElapsedGameTime.TotalSeconds;
            Matrix matrix = Matrix.CreateFromAxisAngle(camerasWorld.Up, MathHelper.ToRadians(radians));
            LookAtDirection = Vector3.TransformNormal(LookAtDirection, matrix);
            ReCreateWorldAndView();
        }
        public void RotateRight(GameTime gameTime)
        {
            var radians = -RotationDegreesPerSecond * (float)gameTime.ElapsedGameTime.TotalSeconds;
            Matrix matrix = Matrix.CreateFromAxisAngle(camerasWorld.Up, MathHelper.ToRadians(radians));
            LookAtDirection = Vector3.TransformNormal(LookAtDirection, matrix);
            ReCreateWorldAndView();
        }
        public void RotateRollClockwise(GameTime gameTime)
        {
            var radians = RotationDegreesPerSecond * (float)gameTime.ElapsedGameTime.TotalSeconds;
            var pos = camerasWorld.Translation;
            camerasWorld *= Matrix.CreateFromAxisAngle(camerasWorld.Forward, MathHelper.ToRadians(radians));
            camerasWorld.Translation = pos;
            ReCreateWorldAndView();
        }
        public void RotateRollCounterClockwise(GameTime gameTime)
        {
            var radians = -RotationDegreesPerSecond * (float)gameTime.ElapsedGameTime.TotalSeconds;
            var pos = camerasWorld.Translation;
            camerasWorld *= Matrix.CreateFromAxisAngle(camerasWorld.Forward, MathHelper.ToRadians(radians));
            camerasWorld.Translation = pos;
            ReCreateWorldAndView();
        }

        // just for example this is the same as the above rotate left or right.
        public void RotateLeftOrRight(GameTime gameTime, float amount)
        {
            var radians = amount * -RotationDegreesPerSecond * (float)gameTime.ElapsedGameTime.TotalSeconds;
            Matrix matrix = Matrix.CreateFromAxisAngle(camerasWorld.Up, MathHelper.ToRadians(radians));
            LookAtDirection = Vector3.TransformNormal(LookAtDirection, matrix);
            ReCreateWorldAndView();
        }
        public void RotateUpOrDown(GameTime gameTime, float amount)
        {
            var radians = amount * -RotationDegreesPerSecond * (float)gameTime.ElapsedGameTime.TotalSeconds;
            Matrix matrix = Matrix.CreateFromAxisAngle(camerasWorld.Right, MathHelper.ToRadians(radians));
            LookAtDirection = Vector3.TransformNormal(LookAtDirection, matrix);
            ReCreateWorldAndView();
        }

        #endregion

        #region Non Local System Translations and Rotations.

        public void MoveForwardInNonLocalSystemCoordinates(GameTime gameTime)
        {
            Position += (Vector3.Forward * MovementUnitsPerSecond) * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
        public void MoveBackwardsInNonLocalSystemCoordinates(GameTime gameTime)
        {
            Position += (Vector3.Backward * MovementUnitsPerSecond) * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
        public void MoveUpInNonLocalSystemCoordinates(GameTime gameTime)
        {
            Position += (Vector3.Up * MovementUnitsPerSecond) * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
        public void MoveDownInNonLocalSystemCoordinates(GameTime gameTime)
        {
            Position += (Vector3.Down * MovementUnitsPerSecond) * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
        public void MoveLeftInNonLocalSystemCoordinates(GameTime gameTime)
        {
            Position += (Vector3.Left * MovementUnitsPerSecond) * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
        public void MoveRightInNonLocalSystemCoordinates(GameTime gameTime)
        {
            Position += (Vector3.Right * MovementUnitsPerSecond) * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        /// <summary>
        /// These aren't typically useful and you would just use create world for a camera snap to a new view. I leave them for completeness.
        /// </summary>
        public void NonLocalRotateLeftOrRight(GameTime gameTime, float amount)
        {
            var radians = amount * -RotationDegreesPerSecond * (float)gameTime.ElapsedGameTime.TotalSeconds;
            Matrix matrix = Matrix.CreateFromAxisAngle(Vector3.Up, MathHelper.ToRadians(radians));
            LookAtDirection = Vector3.TransformNormal(LookAtDirection, matrix);
            ReCreateWorldAndView();
        }
        /// <summary>
        /// These aren't typically useful and you would just use create world for a camera snap to a new view.  I leave them for completeness.
        /// </summary>
        public void NonLocalRotateUpOrDown(GameTime gameTime, float amount)
        {
            var radians = amount * -RotationDegreesPerSecond * (float)gameTime.ElapsedGameTime.TotalSeconds;
            Matrix matrix = Matrix.CreateFromAxisAngle(Vector3.Right, MathHelper.ToRadians(radians));
            LookAtDirection = Vector3.TransformNormal(LookAtDirection, matrix);
            ReCreateWorldAndView();
        }

        #endregion
    }

    /// <summary>
    /// No garbage stringbuilder William Motill, last fix or change Feb 10, 2019.
    /// 
    /// The purpose of this class is to eliminate garbage collections. 
    /// Primarily bypassing numeric conversions to string.
    /// While this is not for high precision, Performance is to be considered.
    /// This class can be used in place of stringbuilder it was primarily designed for use with monogame.
    /// 
    ///  Notes
    ///  To use this as a regular c# class simply remove the vector and color overloads.
    ///  It's more like a string now ability wise, since over time ive slowly changed things to add overloads properly.
    ///  So while you can use it with the + "" + operators id avoid that when adding dynamic number variable and stick with .append()
    ///   
    /// ...
    /// Change log 2017
    /// ...
    /// March to December
    /// Added chained append funtionality that was getting annoying not having it.
    /// Cut out excess trailing float and double zeros. This was a partial fix. 
    /// Fixed a major floating point error.  Shifted the remainder of floats doubles into the higher integer range.
    /// Fixed a second edge case for trailing zeros.  
    /// Fixed n = -n;  when values were negative in float double appends.
    /// that would lead to a bug were - integer portions didn't get returned.
    /// yanked some redundant stuff for a un-needed reference swap hack.
    /// ...
    ///  Change log  2018
    /// ...
    /// Added a Indexer to index into the underlying stringbuilder to directly access chars.
    /// Added a insert char overload.
    /// Appendline was adding the new line to the beginning not the end.
    /// Added a method to directly link via reference to the internal string builder this will probably stay in.
    /// The original AppendAt was fixed and renamed to OverWriteAt, The new AppendAt's works as a Insert.
    /// Multiple overloads were added and tested in relation.
    /// Standardized capacity and length checks to a method.
    /// Added AppendTrim overloads to allow setting the decimal place.
    /// Added a rectangle why i never added this before.
    /// ...
    /// Change log  2019
    /// ...
    /// Feb 10 
    /// Added Familiar insert methods that use the AppendAt
    /// ...
    /// </summary>
    public sealed class MgStringBuilder
    {
        private static char decimalseperator = '.';
        private static char minus = '-';
        private static char plus = '+';
        private StringBuilder stringbuilder;

        /// <summary>
        /// This was sort of a iffy thing to add i was superstitious it might make garbage, after all this time i guess its pretty safe.
        /// </summary>
        public StringBuilder StringBuilder
        {
            get { return stringbuilder; }
            private set { if (stringbuilder == null) { stringbuilder = value; } else { stringbuilder.Clear(); stringbuilder.Append(value); } }
        }
        /// <summary>
        /// Clears the length without clearing the capacity to prevent garbage deallocation.
        /// </summary>
        public int Length
        {
            get { return stringbuilder.Length; }
            set { stringbuilder.Length = value; }
        }
        /// <summary>
        /// Clears the length without clearing the capacity to prevent garbage deallocation.
        /// </summary>
        public void Clear()
        {
            stringbuilder.Length = 0;
        }
        /// <summary>
        /// Typically you only increase this setting it to say zero would cause a garbage collection to occur.
        /// </summary>
        public int Capacity
        {
            get { return stringbuilder.Capacity; }
            set { stringbuilder.Capacity = value; }
        }

        public static void CheckSeperator()
        {
            decimalseperator = Convert.ToChar(System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
        }

        // indexer

        /// <summary>
        /// Indexer to chars in the underlying array
        /// </summary>
        public char this[int i]
        {
            get { return stringbuilder[i]; }
            set { stringbuilder[i] = value; }
        }

        // constructors

        public MgStringBuilder()
        {
            StringBuilder = StringBuilder;
            if (stringbuilder == null) { stringbuilder = new StringBuilder(); }
        }
        public MgStringBuilder(int capacity)
        {
            StringBuilder = new StringBuilder(capacity);
            if (stringbuilder == null) { stringbuilder = new StringBuilder(); }
        }
        public MgStringBuilder(StringBuilder sb)
        {
            StringBuilder = sb;
            if (sb == null) { sb = new StringBuilder(); }
        }
        public MgStringBuilder(string s)
        {
            StringBuilder = new StringBuilder(s);
            if (stringbuilder == null) { stringbuilder = new StringBuilder(); }
        }

        // operators

        public static implicit operator MgStringBuilder(String s)
        {
            return new MgStringBuilder(s);
        }
        public static implicit operator MgStringBuilder(StringBuilder sb)
        {
            return new MgStringBuilder(sb);
        }
        public static implicit operator StringBuilder(MgStringBuilder msb)
        {
            return msb.StringBuilder;
        }
        public static MgStringBuilder operator +(MgStringBuilder sbm, MgStringBuilder s)
        {
            sbm.StringBuilder.Append(s);
            return sbm;
        }
        public static MgStringBuilder operator +(MgStringBuilder sbm, string s)
        {
            sbm.StringBuilder.Append(s);
            return sbm;
        }

        // Methods all the appends are unrolled to squeeze out speed.

        public MgStringBuilder Append(StringBuilder s)
        {
            int len = this.StringBuilder.Length;
            CheckAppendCapacityAndLength(stringbuilder.Length, s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                this.StringBuilder[i + len] = (char)(s[i]);
            }
            return this;
        }
        public MgStringBuilder Append(string s)
        {
            stringbuilder.Append(s);
            return this;
        }
        public MgStringBuilder Append(char value)
        {
            stringbuilder.Append(value);
            return this;
        }
        public MgStringBuilder Append(bool value)
        {
            stringbuilder.Append(value);
            return this;
        }
        public MgStringBuilder Append(byte value)
        {
            // basics
            int num = value;
            if (num == 0)
            {
                stringbuilder.Append('0');
                return this;
            }
            int place = 100;
            if (num >= place * 10)
            {
                // just append it
                stringbuilder.Append(num);
                return this;
            }
            // part 1 pull integer digits
            bool addzeros = false;
            while (place > 0)
            {
                if (num >= place)
                {
                    addzeros = true;
                    int modulator = place * 10;
                    int val = num % modulator;
                    int dc = val / place;
                    stringbuilder.Append((char)(dc + 48));
                }
                else
                {
                    if (addzeros) { stringbuilder.Append('0'); }
                }
                place = (int)(place * .1);
            }
            return this;
        }
        public MgStringBuilder Append(short value)
        {
            int num = value;
            // basics
            if (num < 0)
            {
                // Negative.
                stringbuilder.Append(minus);
                num = -num;
            }
            if (value == 0)
            {
                stringbuilder.Append('0');
                return this;
            }

            int place = 10000;
            if (num >= place * 10)
            {
                // just append it, if its this big, this isn't a science calculator, its a edge case.
                stringbuilder.Append(num);
                return this;
            }
            // part 1 pull integer digits
            bool addzeros = false;
            while (place > 0)
            {
                if (num >= place)
                {
                    addzeros = true;
                    int modulator = place * 10;
                    int val = num % modulator;
                    int dc = val / place;
                    stringbuilder.Append((char)(dc + 48));
                }
                else
                {
                    if (addzeros) { stringbuilder.Append('0'); }
                }
                place = (int)(place * .1);
            }
            return this;
        }
        public MgStringBuilder Append(int value)
        {
            // basics
            if (value < 0)
            {
                // Negative.
                stringbuilder.Append(minus);
                value = -value;
            }
            if (value == 0)
            {
                stringbuilder.Append('0');
                return this;
            }

            int place = 1000000000;
            if (value >= place * 10)
            {
                // just append it
                stringbuilder.Append(value);
                return this;
            }
            // part 1 pull integer digits
            int n = (int)(value);
            bool addzeros = false;
            while (place > 0)
            {
                if (n >= place)
                {
                    addzeros = true;
                    int modulator = place * 10;
                    int val = n % modulator;
                    int dc = val / place;
                    stringbuilder.Append((char)(dc + 48));
                }
                else
                {
                    if (addzeros) { stringbuilder.Append('0'); }
                }
                place = (int)(place * .1);
            }
            return this;
        }
        public MgStringBuilder Append(long value)
        {
            // basics
            if (value < 0)
            {
                // Negative.
                stringbuilder.Append(minus);
                value = -value;
            }
            if (value == 0)
            {
                stringbuilder.Append('0');
                return this;
            }

            long place = 10000000000000000L;
            if (value >= place * 10)
            {
                // just append it,
                stringbuilder.Append(value);
                return this;
            }
            // part 1 pull integer digits
            long n = (long)(value);
            bool addzeros = false;
            while (place > 0)
            {
                if (n >= place)
                {
                    addzeros = true;
                    long modulator = place * 10L;
                    long val = n % modulator;
                    long dc = val / place;
                    stringbuilder.Append((char)(dc + 48));
                }
                else
                {
                    if (addzeros) { stringbuilder.Append('0'); }
                }
                place = (long)(place * .1);
            }
            return this;
        }

        public MgStringBuilder Append(float value)
        {
            // basics
            bool addZeros = false;
            int n = (int)(value);
            int place = 100000000;
            if (value < 0)
            {
                // Negative.
                stringbuilder.Append(minus);
                value = -value;
                n = -n;
            }
            if (value == 0)
            {
                stringbuilder.Append('0');
                return this;
            }
            // fix march 18-17
            // values not zero value is at least a integer
            if (value <= -1f || value >= 1f)
            {

                place = 100000000;
                if (value >= place * 10)
                {
                    // just append it, if its this big its a edge case.
                    stringbuilder.Append(value);
                    return this;
                }
                // part 1 pull integer digits
                // int n =  // moved
                addZeros = false;
                while (place > 0)
                {
                    if (n >= place)
                    {
                        addZeros = true;
                        int modulator = place * 10;
                        int val = n % modulator;
                        int dc = val / place;
                        stringbuilder.Append((char)(dc + 48));
                    }
                    else
                    {
                        if (addZeros) { stringbuilder.Append('0'); }
                    }
                    place = (int)(place * .1);
                }
            }
            else
                stringbuilder.Append('0');

            stringbuilder.Append(decimalseperator);

            // part 2 

            // floating point part now it can have about 28 digits but uh ya.. nooo lol
            place = 1000000;
            // pull decimal to integer digits, based on the number of place digits
            int dn = (int)((value - (float)(n)) * place * 10);
            // ... march 17 testing... cut out extra zeros case 1
            if (dn == 0)
            {
                stringbuilder.Append('0');
                return this;
            }
            addZeros = true;
            while (place > 0)
            {
                if (dn >= place)
                {
                    //addzeros = true;
                    int modulator = place * 10;
                    int val = dn % modulator;
                    int dc = val / place;
                    stringbuilder.Append((char)(dc + 48));
                    if (val - dc * place == 0) // && trimEndZeros this would be a acstetic
                    {
                        return this;
                    }
                }
                else
                {
                    if (addZeros) { stringbuilder.Append('0'); }
                }
                place = (int)(place * .1);
            }
            return this;
        }
        public MgStringBuilder Append(double value)
        {
            // basics
            bool addZeros = false;
            long n = (long)(value);
            long place = 10000000000000000L;
            if (value < 0) // is Negative.
            {
                stringbuilder.Append(minus);
                value = -value;
                n = -n;
            }
            if (value == 0) // is Zero
            {
                stringbuilder.Append('0');
                return this;
            }
            if (value <= -1d || value >= 1d) // is a Integer
            {
                if (value >= place * 10)
                {
                    stringbuilder.Append(value); // is big, just append its a edge case.
                    return this;
                }
                // part 1 pull integer digits
                addZeros = false;
                while (place > 0)
                {
                    if (n >= place)
                    {
                        addZeros = true;
                        long modulator = place * 10;
                        long val = n % modulator;
                        long dc = val / place;
                        stringbuilder.Append((char)(dc + 48));
                    }
                    else
                        if (addZeros) { stringbuilder.Append('0'); }

                    place = (long)(place * .1d);
                }
            }
            else
                stringbuilder.Append('0');

            stringbuilder.Append(decimalseperator);

            // part 2 
            // floating point part now it can have about 28 digits but uh ya.. nooo lol
            place = 1000000000000000L;
            // pull decimal to integer digits, based on the number of place digits
            long dn = (long)((value - (double)(n)) * place * 10);
            if (dn == 0)
            {
                stringbuilder.Append('0');
                return this;
            }
            addZeros = true;
            while (place > 0)
            {
                if (dn >= place)
                {
                    long modulator = place * 10;
                    long val = dn % modulator;
                    long dc = val / place;
                    stringbuilder.Append((char)(dc + 48));
                    if (val - dc * place == 0) // && trimEndZeros  aectetic
                    {
                        return this;
                    }
                }
                else
                    if (addZeros) { stringbuilder.Append('0'); }

                place = (long)(place * .1);
            }
            return this;
        }

        public MgStringBuilder Append(Point value)
        {
            Append("(");
            Append(value.X);
            Append(", ");
            Append(value.Y);
            Append(")");
            return this;
        }
        public MgStringBuilder Append(Vector2 value)
        {
            Append("(");
            Append(value.X);
            Append(", ");
            Append(value.Y);
            Append(")");
            return this;
        }
        public MgStringBuilder Append(Vector3 value)
        {
            Append("(");
            Append(value.X);
            Append(", ");
            Append(value.Y);
            Append(", ");
            Append(value.Z);
            Append(")");
            return this;
        }
        public MgStringBuilder Append(Vector4 value)
        {
            Append("(");
            Append(value.X);
            Append(", ");
            Append(value.Y);
            Append(", ");
            Append(value.Z);
            Append(", ");
            Append(value.W);
            Append(")");
            return this;
        }
        public MgStringBuilder Append(Color value)
        {
            Append("(");
            Append(value.R);
            Append(", ");
            Append(value.G);
            Append(", ");
            Append(value.B);
            Append(", ");
            Append(value.A);
            Append(")");
            return this;
        }

        public MgStringBuilder AppendTrim(float value)
        {
            // basics
            bool addZeros = false;
            int n = (int)(value);
            int place = 100000000;
            if (value < 0)
            {
                // Negative.
                stringbuilder.Append(minus);
                value = -value;
                n = -n;
            }
            if (value == 0)
            {
                stringbuilder.Append('0');
                return this;
            }
            // fix march 18-17
            // values not zero value is at least a integer
            if (value <= -1f || value >= 1f)
            {

                place = 100000000;
                if (value >= place * 10)
                {
                    // just append it, if its this big its a edge case.
                    stringbuilder.Append(value);
                    return this;
                }
                // part 1 pull integer digits
                // int n =  // moved
                addZeros = false;
                while (place > 0)
                {
                    if (n >= place)
                    {
                        addZeros = true;
                        int modulator = place * 10;
                        int val = n % modulator;
                        int dc = val / place;
                        stringbuilder.Append((char)(dc + 48));
                    }
                    else
                    {
                        if (addZeros) { stringbuilder.Append('0'); }
                    }
                    place = (int)(place * .1);
                }
            }
            else
                stringbuilder.Append('0');

            stringbuilder.Append(decimalseperator);

            // part 2 

            // floating point part now it can have about 28 digits but uh ya.. nooo lol
            place = 100;
            // pull decimal to integer digits, based on the number of place digits
            int dn = (int)((value - (float)(n)) * place * 10);
            // ... march 17 testing... cut out extra zeros case 1
            if (dn == 0)
            {
                stringbuilder.Append('0');
                return this;
            }
            addZeros = true;
            while (place > 0)
            {
                if (dn >= place)
                {
                    //addzeros = true;
                    int modulator = place * 10;
                    int val = dn % modulator;
                    int dc = val / place;
                    stringbuilder.Append((char)(dc + 48));
                    if (val - dc * place == 0) // && trimEndZeros this would be a acstetic
                    {
                        return this;
                    }
                }
                else
                {
                    if (addZeros) { stringbuilder.Append('0'); }
                }
                place = (int)(place * .1);
            }
            return this;
        }
        public MgStringBuilder AppendTrim(double value)
        {
            // basics
            bool addZeros = false;
            long n = (long)(value);
            long place = 10000000000000000L;
            if (value < 0) // is Negative.
            {
                stringbuilder.Append(minus);
                value = -value;
                n = -n;
            }
            if (value == 0) // is Zero
            {
                stringbuilder.Append('0');
                return this;
            }
            if (value <= -1d || value >= 1d) // is a Integer
            {
                if (value >= place * 10)
                {
                    stringbuilder.Append(value); // is big, just append its a edge case.
                    return this;
                }
                // part 1 pull integer digits
                addZeros = false;
                while (place > 0)
                {
                    if (n >= place)
                    {
                        addZeros = true;
                        long modulator = place * 10;
                        long val = n % modulator;
                        long dc = val / place;
                        stringbuilder.Append((char)(dc + 48));
                    }
                    else
                        if (addZeros) { stringbuilder.Append('0'); }

                    place = (long)(place * .1);
                }
            }
            else
                stringbuilder.Append('0');

            stringbuilder.Append(decimalseperator);

            // part 2 
            // floating point part now it can have about 28 digits but uh ya.. nooo lol
            place = 100L;
            // pull decimal to integer digits, based on the number of place digits
            long dn = (long)((value - (double)(n)) * place * 10);
            if (dn == 0)
            {
                stringbuilder.Append('0');
                return this;
            }
            addZeros = true;
            while (place > 0)
            {
                if (dn >= place)
                {
                    long modulator = place * 10;
                    long val = dn % modulator;
                    long dc = val / place;
                    stringbuilder.Append((char)(dc + 48));
                    if (val - dc * place == 0) // && trimEndZeros  aectetic
                    {
                        return this;
                    }
                }
                else
                    if (addZeros) { stringbuilder.Append('0'); }

                place = (long)(place * .1);
            }
            return this;
        }

        public MgStringBuilder AppendTrim(float value, int digits)
        {
            // basics
            bool addZeros = false;
            int n = (int)(value);
            int place = 100000000;
            if (value < 0)
            {
                // Negative.
                stringbuilder.Append(minus);
                value = -value;
                n = -n;
            }
            if (value == 0)
            {
                stringbuilder.Append('0');
                return this;
            }
            // fix march 18-17
            // values not zero value is at least a integer
            if (value <= -1f || value >= 1f)
            {

                place = 100000000;
                if (value >= place * 10)
                {
                    // just append it, if its this big its a edge case.
                    stringbuilder.Append(value);
                    return this;
                }
                // part 1 pull integer digits
                // int n =  // moved
                addZeros = false;
                while (place > 0)
                {
                    if (n >= place)
                    {
                        addZeros = true;
                        int modulator = place * 10;
                        int val = n % modulator;
                        int dc = val / place;
                        stringbuilder.Append((char)(dc + 48));
                    }
                    else
                    {
                        if (addZeros) { stringbuilder.Append('0'); }
                    }
                    place = (int)(place * .1);
                }
            }
            else
                stringbuilder.Append('0');

            stringbuilder.Append(decimalseperator);

            // part 2 

            // floating point part now it can have about 28 digits but uh ya.. nooo lol

            // nov 20 2018 added the amount of digits to trim.
            //place = 100;
            if (digits < 1)
                place = 0;
            else
            {
                place = 1;
                for (int i = 0; i < digits - 1; i++)
                    place *= 10;
            }

            // pull decimal to integer digits, based on the number of place digits
            int dn = (int)((value - (float)(n)) * place * 10);
            // ... march 17 testing... cut out extra zeros case 1
            if (dn == 0)
            {
                stringbuilder.Append('0');
                return this;
            }
            addZeros = true;
            while (place > 0)
            {
                if (dn >= place)
                {
                    //addzeros = true;
                    int modulator = place * 10;
                    int val = dn % modulator;
                    int dc = val / place;
                    stringbuilder.Append((char)(dc + 48));
                    if (val - dc * place == 0) // && trimEndZeros this would be a acstetic
                    {
                        return this;
                    }
                }
                else
                {
                    if (addZeros) { stringbuilder.Append('0'); }
                }
                place = (int)(place * .1);
            }
            return this;
        }
        public MgStringBuilder AppendTrim(double value, int digits)
        {
            // basics
            bool addZeros = false;
            long n = (long)(value);
            long place = 10000000000000000L;
            if (value < 0) // is Negative.
            {
                stringbuilder.Append(minus);
                value = -value;
                n = -n;
            }
            if (value == 0) // is Zero
            {
                stringbuilder.Append('0');
                return this;
            }
            if (value <= -1d || value >= 1d) // is a Integer
            {
                if (value >= place * 10)
                {
                    stringbuilder.Append(value); // is big, just append its a edge case.
                    return this;
                }
                // part 1 pull integer digits
                addZeros = false;
                while (place > 0)
                {
                    if (n >= place)
                    {
                        addZeros = true;
                        long modulator = place * 10;
                        long val = n % modulator;
                        long dc = val / place;
                        stringbuilder.Append((char)(dc + 48));
                    }
                    else
                        if (addZeros) { stringbuilder.Append('0'); }

                    place = (long)(place * .1);
                }
            }
            else
                stringbuilder.Append('0');

            stringbuilder.Append(decimalseperator);

            // part 2 
            // floating point part now it can have about 28 digits but uh ya.. nooo lol
            // nov 20 2018 added the amount of digits to trim.
            //place = 100L;
            if (digits < 1)
                place = 0;
            else
            {
                place = 1;
                for (int i = 0; i < digits - 1; i++)
                    place *= 10;
            }

            // pull decimal to integer digits, based on the number of place digits
            long dn = (long)((value - (double)(n)) * place * 10);
            if (dn == 0)
            {
                stringbuilder.Append('0');
                return this;
            }
            addZeros = true;
            while (place > 0)
            {
                if (dn >= place)
                {
                    long modulator = place * 10;
                    long val = dn % modulator;
                    long dc = val / place;
                    stringbuilder.Append((char)(dc + 48));
                    if (val - dc * place == 0) // && trimEndZeros  aectetic
                    {
                        return this;
                    }
                }
                else
                    if (addZeros) { stringbuilder.Append('0'); }

                place = (long)(place * .1);
            }
            return this;
        }

        public MgStringBuilder AppendTrim(Vector2 value)
        {
            Append("(");
            AppendTrim(value.X);
            Append(", ");
            AppendTrim(value.Y);
            Append(")");
            return this;
        }
        public MgStringBuilder AppendTrim(Vector3 value)
        {
            Append("(");
            AppendTrim(value.X);
            Append(", ");
            AppendTrim(value.Y);
            Append(", ");
            AppendTrim(value.Z);
            Append(")");
            return this;
        }
        public MgStringBuilder AppendTrim(Vector4 value)
        {
            Append("(");
            AppendTrim(value.X);
            Append(", ");
            AppendTrim(value.Y);
            Append(", ");
            AppendTrim(value.Z);
            Append(", ");
            AppendTrim(value.W);
            Append(")");
            return this;
        }
        public MgStringBuilder Append(Rectangle value)
        {
            Append("(");
            AppendTrim(value.X);
            Append(", ");
            AppendTrim(value.Y);
            Append(", ");
            AppendTrim(value.Width);
            Append(", ");
            AppendTrim(value.Height);
            Append(")");
            return this;
        }

        public MgStringBuilder AppendLine(StringBuilder s)
        {
            Append(s);
            stringbuilder.AppendLine();
            return this;
        }
        public MgStringBuilder AppendLine(string s)
        {
            stringbuilder.Append(s);
            stringbuilder.AppendLine();
            return this;
        }
        public MgStringBuilder AppendLine()
        {
            stringbuilder.AppendLine();
            return this;
        }

        /// <summary>
        /// Functions just like a indexer.
        /// </summary>
        public void OverWriteAt(int index, Char s)
        {
            CheckOverWriteCapacityAndLength(index, 1);
            this.StringBuilder[index] = (char)(s);
        }
        /// <summary>
        /// Functions to overwrite data at the index on
        /// </summary>
        public void OverWriteAt(int index, StringBuilder s)
        {
            CheckOverWriteCapacityAndLength(index, s.Length);
            for (int i = 0; i < s.Length; i++)
                this.StringBuilder[i + index] = (char)(s[i]);
        }
        /// <summary>
        /// Functions to overwrite data at the index on
        /// </summary>
        public void OverWriteAt(int index, String s)
        {
            CheckAppendCapacityAndLength(index, s.Length);
            for (int i = 0; i < s.Length; i++)
                this.StringBuilder[i + index] = (char)(s[i]);
        }

        /// <summary>
        /// This uses AppendAt to get around problems with garbage collections.
        /// </summary>
        public MgStringBuilder Insert(int index, char c)
        {
            AppendAt(index, c);
            return this;
        }
        /// <summary>
        /// This uses AppendAt to get around problems with garbage collections.
        /// </summary>
        public MgStringBuilder Insert(int index, StringBuilder s)
        {
            AppendAt(index, s);
            return this;
        }
        /// <summary>
        /// This uses AppendAt to get around problems with garbage collections.
        /// </summary>
        public MgStringBuilder Insert(int index, string s)
        {
            AppendAt(index, s);
            return this;
        }

        /// <summary>
        /// Functions as a insert, existing text will be moved over.
        /// </summary>
        public void AppendAt(int index, Char s)
        {
            CheckAppendCapacityAndLength(index, 1);
            for (int j = StringBuilder.Length - 1; j >= index + 1; j--)
                stringbuilder[j] = stringbuilder[j - 1];
            for (int i = 0; i < 1; i++)
                stringbuilder[i + index] = (char)(s);
        }
        /// <summary>
        /// Functions as a insert, existing text will be moved over.
        /// </summary>
        public void AppendAt(int index, StringBuilder s)
        {
            CheckAppendCapacityAndLength(index, s.Length);
            int insertedsbLength = s.Length;
            for (int j = stringbuilder.Length - 1; j >= index + insertedsbLength; j--)
                stringbuilder[j] = stringbuilder[j - insertedsbLength];
            for (int i = 0; i < insertedsbLength; i++)
            {
                stringbuilder[index + i] = s[i];
            }
        }
        /// <summary>
        /// Functions as a insert, existing text will be moved over. Notes are left in this method overload.
        /// </summary>
        public void AppendAt(int index, String s)
        {
            CheckAppendCapacityAndLength(index, s.Length);
            // Now we will wind from back to front the current characters in the stringbuilder to make room for this append.
            // Yes this will be a expensive operation however this must be done if we want a proper AppendAt.
            // Chunks or no chunks stringbuilders insert is piss poor worse it creates garbage.
            int insertedsbLength = s.Length;
            for (int j = stringbuilder.Length - 1; j >= index + insertedsbLength; j--)
                stringbuilder[j] = stringbuilder[j - insertedsbLength];
            // perform the append
            for (int i = 0; i < insertedsbLength; i++)
            {
                stringbuilder[index + i] = s[i];
            }
        }

        public MgStringBuilder Remove(int index, int length)
        {
            stringbuilder.Remove(index, length);
            //Delete(index, length);
            return this;
        }

        public MgStringBuilder Delete(int index, int length)
        {
            int len = stringbuilder.Length - length;
            int j = length;
            for (int i = index; i < len; i++)
            {
                if (i + j < len)
                    stringbuilder[i] = stringbuilder[i + j];
                else
                    stringbuilder[i] = ' ';
            }
            stringbuilder.Length = len;
            return this;
        }

        private void CheckAppendCapacityAndLength(int index, int lengthOfAddition)
        {
            int newLength = lengthOfAddition + stringbuilder.Length;
            int reqcapacity = (newLength + 1) - (stringbuilder.Capacity);
            if (reqcapacity >= 0)
                stringbuilder.Capacity = (stringbuilder.Capacity + reqcapacity + 64);
            stringbuilder.Length = newLength;
        }
        private void CheckOverWriteCapacityAndLength(int index, int lengthOfOverWrite)
        {
            int dist = (index + lengthOfOverWrite);
            if (dist > stringbuilder.Length)
            {
                int newLength = index + lengthOfOverWrite;
                int reqcapacity = (newLength + 1) - (stringbuilder.Capacity);
                if (reqcapacity >= 0)
                    stringbuilder.Capacity = (stringbuilder.Capacity + reqcapacity + 64);
                stringbuilder.Length = newLength;
            }
        }

        /// <summary>
        /// Use with caution this solves a rare edge case.
        /// Be careful using this you should understand c# references before doing so. 
        /// This creates a direct secondary reference to the internal stringbuilder via out.
        /// Declare a StringBuilder reference such as StringBuilder sb; don't call new on it,  then pass sb to this function.
        /// When you are done with it unlink it by declaring new on it like so, sb = new StringBuilder();
        /// This allows a way to link a reference to the internal stringbuilder without creating deallocation garbage.
        /// </summary>
        public void LinkReferenceToTheInnerStringBuilder(out StringBuilder rsb)
        {
            rsb = stringbuilder;
        }

        public char[] ToCharArray()
        {
            char[] a = new char[stringbuilder.Length];
            stringbuilder.CopyTo(0, a, 0, stringbuilder.Length);
            return a;
        }

        public override string ToString()
        {
            return stringbuilder.ToString();
        }
    }
}
