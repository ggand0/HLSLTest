using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace HLSLTest
{
    public class GlassEffect
    {
        public static Game1 game;
		public static Level level;

        private Object glassMesh;
        private Effect glassEffect;
        private ContentManager content;
        private GraphicsDevice graphics;
        private RenderTarget2D reflectionTarget, refractionTarget;
        private bool hasSaved;
        private PrelightingRenderer renderer;
        private List<Object> models;

        public List<IRenderable> Objects = new List<IRenderable>();


        public void Initialize()
        {
            // Objectsのうち、Objectクラスのみのリストを作っておく
            // 厳密に言うと「Prelighting及びShadowMappingを使うエフェクトで描画するオブジェクト」。
            models = new List<Object>();
            foreach (IRenderable renderable in Objects) {
                if (renderable is Object) {
                    models.Add(renderable as Object);
                }
            }
        }
        /// <param name="camera">main camera instance</param>
        public void renderReflection(ArcBallCamera camera)
        {
            // Reflect the camera's properties across the water plane
            Vector3 reflectedCameraPosition = camera.Position;
            reflectedCameraPosition.Y = -reflectedCameraPosition.Y + glassMesh.Position.Y * 2;
            Vector3 reflectedCameraTarget = camera.ChasePosition;
            reflectedCameraTarget.Y = -reflectedCameraTarget.Y - camera.LookAtOffset.Y/**/ + glassMesh.Position.Y * 2;

            // Create a temporary camera to render the reflected scene
            // ArcBallCameraのLookAtOffsetの関係でずれたりするから、出来れば同じ型のクラスのほうがいいだろう
            TargetCamera reflectionCamera = new TargetCamera(reflectedCameraPosition, reflectedCameraTarget, graphics);
            reflectionCamera.Update();// 上方ベクトルは-Yになってた
            //ArcBallCamera reflectionCamera = new ArcBallCamera(reflectedCameraPosition, reflectedCameraTarget, Vector3.Down);
            // reflectionのviewがおかしいのか...??

            // これ、ネットから拾ってきたmatrixだったのを忘れてた
            /*Matrix reflectionMatrix = Matrix.Identity
                //Matrix.CreateTranslation(0f, -waterMesh.Position.Y, 0f) * 
                //Matrix.CreateScale(1f, -1f, 1f)	 
                //Matrix.CreateTranslation(0f, -waterMesh.Position.Y, 0f)
                ;
            Matrix reflectedViewMatrix = reflectionMatrix * camera.View;*/


            // Set the reflection camera's view matrix to the water effect
            glassEffect.Parameters["ReflectedView"].SetValue(reflectionCamera.View);
            //waterEffect.Parameters["ReflectedView"].SetValue(reflectedViewMatrix);

            // Create the clip plane
            Vector4 clipPlane = new Vector4(0, 1, 0, -glassMesh.Position.Y);// w成分は法線方向を表す？


            // lt, stを使うmodelsのために初期化:light map, shadow mapを作り直す。（めんどくさい）
            //models[1].RotationMatrix = Matrix.Identity * Matrix.CreateRotationZ(MathHelper.ToRadians(90))* Matrix.CreateRotationX(MathHelper.ToRadians(-90));
            // あー分かった、モデルの回転が異常なのではなくて、BlendStateなどがおかしい！（だから透過して見えるので、表面が見えて回転しているようにみえる）
            /*var depthNormal = renderer.drawDepthNormalMap(models
                //, reflectedViewMatrix, reflectionCamera.Projection, reflectionCamera.Position);
                        , reflectionCamera.View, reflectionCamera.Projection, reflectionCamera.Position);
            RenderTarget2D lt = renderer.drawLightMap(models, depthNormal.dt, depthNormal.nt
                , reflectionCamera.View, reflectionCamera.Projection, reflectionCamera.Position);
            //, reflectedViewMatrix, reflectionCamera.Projection, reflectionCamera.Position);
            //RenderTarget2D st = renderer.drawShadowDepthMap(models);
            renderer.drawShadowDepthMap();
            renderer.prepareMainPass(models, lt);*/

            // Set the render target
            graphics.SetRenderTarget(reflectionTarget);
            graphics.Clear(Color.Black);

            // Draw all objects with clip plane
            // ここを弄ると面白い演出が出来るかも
            foreach (IRenderable renderable in Objects) {
                renderable.SetClipPlane(clipPlane);
                string cullState = graphics.RasterizerState.ToString();
                if (renderable is Object) {
                    (renderable as Object).Update(new GameTime());
                }
                // ここでreflectionCameraを設定しているはずなのだが... 
                //renderable.Draw(reflectedViewMatrix, reflectionCamera.Projection, reflectedCameraPosition);
                //renderable.Draw(camera.View, camera.Projection, camera.CameraPosition);// ここでreflectionCameraを設定しているはずなのだが... 
                renderable.Draw(reflectionCamera.View, reflectionCamera.Projection, reflectedCameraPosition);
                renderable.SetClipPlane(null);
            }
            graphics.SetRenderTarget(null);
            graphics.RasterizerState = RasterizerState.CullCounterClockwise;


            /*// Set the render target
            //graphics.SetRenderTarget(reflectionTarg);
            graphics.Clear(Color.Black);
            foreach (IRenderable renderable in Objects) {
                renderable.SetClipPlane(clipPlane);
                if (renderable is Object) {
                    (renderable as Object).Update(new GameTime());
                    //continue;
                }
                // ここでreflectionCameraを設定しているはずなのだが... 
                //renderable.Draw(reflectedViewMatrix, reflectionCamera.Projection, reflectedCameraPosition);
                //renderable.Draw(camera.View, camera.Projection, camera.CameraPosition);// ここでreflectionCameraを設定しているはずなのだが... 
                renderable.Draw(reflectionCamera.View, reflectionCamera.Projection, reflectedCameraPosition);

                renderable.SetClipPlane(null);
            }*/

            /*if (!hasSaved) {
                using (Stream stream = File.OpenWrite("reflection_map.png")) {
                    reflectionTarg.SaveAsPng(stream, reflectionTarg.Width, reflectionTarg.Height);
                    stream.Position = 0;
                    //MediaLibrary media = new MediaLibrary();
                    //media.SavePicture("shadowDepth.jpg", stream);
                    hasSaved = true; // 下でfalseに
                }
            }*/
            // Set the reflected scene to its effect parameter in
            // the water effect
            glassEffect.Parameters["ReflectionMap"].SetValue(reflectionTarget);
        }
        public void RenderRefraction(ArcBallCamera camera)
        {
             // Reflect the camera's properties across the water plane
            /*Vector3 reflectedCameraPosition = camera.Position;
            reflectedCameraPosition.Y = -reflectedCameraPosition.Y + glassMesh.Position.Y * 2;
            Vector3 reflectedCameraTarget = camera.ChasePosition;
            reflectedCameraTarget.Y = -reflectedCameraTarget.Y - camera.LookAtOffset.Y + glassMesh.Position.Y * 2;
            // Create a temporary camera to render the reflected scene
            TargetCamera reflectionCamera
                = new TargetCamera(reflectedCameraPosition, reflectedCameraTarget, graphics);
            reflectionCamera.Update();
            // Set the reflection camera's view matrix to the water effect
            glassEffect.Parameters["ReflectedView"].SetValue(reflectionCamera.View);*/

            // Create the clip plane
			Vector3 planeNormalDirection = Vector3.Normalize(camera.CameraPosition - glassMesh.Position);
			//Vector4 clipPlane = new Vector4(planeNormalDirection, 1.0f);
			//Vector4 clipPlane = new Vector4(planeNormalDirection, (camera.CameraPosition - glassMesh.Position).Length());// 手前にあるものは屈折しないのでクリップ
			Vector4 clipPlane = new Vector4(planeNormalDirection, 20000);

            /*var depthNormal = renderer.drawDepthNormalMap(models
                        , camera.View, camera.Projection, camera.Position);
            RenderTarget2D lt = renderer.drawLightMap(models, depthNormal.dt, depthNormal.nt
                , camera.View, camera.Projection, camera.Position);
            renderer.drawShadowDepthMap();
            renderer.prepareMainPass(models, lt);*/

            // Set the render target
            graphics.SetRenderTarget(refractionTarget);
            graphics.Clear(Color.Black);

            // Draw all objects with clip plane
            foreach (IRenderable renderable in Objects) {
                //renderable.SetClipPlane(clipPlane);
                string cullState = graphics.RasterizerState.ToString();
                if (renderable is Object) {
                    (renderable as Object).Update(new GameTime());
                }
                renderable.Draw(camera.View, camera.Projection, camera.CameraPosition);
                //renderable.SetClipPlane(null);
            }
            graphics.SetRenderTarget(null);
            graphics.RasterizerState = RasterizerState.CullCounterClockwise;


            // デバッグ用に直接画面に描画する
            /*graphics.Clear(Color.Black);
            foreach (IRenderable renderable in Objects) {
                renderable.SetClipPlane(clipPlane);
                if (renderable is Object) {
                    (renderable as Object).Update(new GameTime());
                }
                // ここでreflectionCameraを設定しているはずなのだが... 
                renderable.Draw(camera.View, camera.Projection, camera.CameraPosition);
                renderable.SetClipPlane(null);
            }*/


            //if (KeyInput.IsOnKeyDown(Keys.Z)) {
            /*if (!hasSaved) {//if (JoyStick.IsOnKeyDown(1)) {
                using (Stream stream = File.OpenWrite("refraction_map.png")) {
                    refractionTarget.SaveAsPng(stream, refractionTarget.Width, refractionTarget.Height);
                    stream.Position = 0;
                    //MediaLibrary media = new MediaLibrary();
                    //media.SavePicture("shadowDepth.jpg", stream);
                    hasSaved = true; // 下でfalseに
                }
            }*/

            // Set the reflected scene to its effect parameter in the water effect
            //glassEffect.Parameters["RefractionMap"].SetValue(refractionTarget);// 屈折マップ生成
			//glassEffect.Parameters["RefractionMap"].SetValue(game.Sky.TextureCube);// 屈折マップ生成

            //Update();

			//glassEffect.Parameters["RefractionMap"].SetValue(level.EnvironmentalMap);// シーンが映り込んだマップを使う場合
        }

        public void Update(GameTime gameTime)
        {
            if (JoyStick.onStickDirectionChanged) {
                hasSaved = false;
            }
            renderer.Update(gameTime);
        }
        public void PreDraw(ArcBallCamera camera, GameTime gameTime)
        {
            //renderReflection(camera);
            RenderRefraction(camera);
            //glassEffect.Parameters["Time"].SetValue((float)gameTime.TotalGameTime.TotalSeconds);
        }
        /// <summary>
        /// カスタムエフェクトを使用したDraw
        /// </summary>
        public void Draw(Matrix View, Matrix Projection, Vector3 CameraPosition)
        {
            glassEffect.Parameters["vecEye"].SetValue(new Vector4(CameraPosition, 1));

            graphics.BlendState = BlendState.AlphaBlend;
            //graphics.DepthStencilState = DepthStencilState.None;
            // 本文にはDraw関数の記述は無かったが、恐らくSkySphere.Drawと同様だろう
            glassMesh.World = Matrix.CreateScale(glassMesh.Scale)
                * glassMesh.RotationMatrix * Matrix.CreateTranslation(glassMesh.Position);// スケールにベクトルを使用していることに注意
            graphics.RasterizerState = RasterizerState.CullClockwise;
            glassMesh.Draw(View, Projection, CameraPosition);
            //graphics.RasterizerState = RasterizerState.CullCounterClockwise;
            //glassMesh.Draw(View, Projection, CameraPosition);

            graphics.BlendState = BlendState.Opaque;
            graphics.DepthStencilState = DepthStencilState.Default;
        }
        
        // Constructor
        public GlassEffect(ContentManager content, GraphicsDevice graphics, Vector3 position, float Scale)
        {
            this.content = content;
            this.graphics = graphics;
            //waterMesh = new Object(content.Load<Model>("plane"), position, Vector3.Zero, new Vector3(size.X, 1, size.Y), graphics);
            glassMesh = new Object(position, "Models\\SkySphereMesh");
            glassMesh.Scale = Scale;
            //glassMesh.ScaleVector = new Vector3(size.X, size.Y, 1);// 元々Blenderで作成した素材なので傾いている。それに合わせて、rescaleする成分を調整
            /*glassMesh.RotationMatrix = Matrix.Identity * Matrix.CreateRotationZ(MathHelper.ToRadians(90))
                * Matrix.CreateRotationX(MathHelper.ToRadians(-90));*/

            glassEffect = content.Load<Effect>("Lights\\GlassEffectV2");
            glassMesh.SetModelEffect(glassEffect, false);
            //glassEffect.Parameters["viewportWidth"].SetValue(graphics.Viewport.Width);
            //glassEffect.Parameters["viewportHeight"].SetValue(graphics.Viewport.Height);

            //reflectionTarg = new RenderTarget2D(graphics, graphics.Viewport.Width, graphics.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24);
            reflectionTarget = new RenderTarget2D(graphics, graphics.Viewport.Width, graphics.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24);
            refractionTarget = new RenderTarget2D(graphics, graphics.Viewport.Width, graphics.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24);
            //PresentationParameters pp = graphics.PresentationParameters;
            //reflectionTarg = new RenderTarget2D(graphics, pp.BackBufferWidth, pp.BackBufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth24);
            //reflectionTarg = new RenderTarget2D(graphics, 2048, 2048, false, SurfaceFormat.Color, DepthFormat.Depth24);

            //glassEffect.Parameters["WaterNormalMap"].SetValue(content.Load<Texture2D>("waterbump"));
            //glassEffect.Parameters["Mask"].SetValue(content.Load<Texture2D>("Textures\\cloud_mask2"));


            if (renderer == null) {
                renderer = new PrelightingRenderer(graphics, content);
            }
            renderer = new PrelightingRenderer(graphics, content);
            renderer.Models = level.Models;
            renderer.Lights = new List<PointLight>() {
				new PointLight(new Vector3(0, 200, 0), Color.White * .85f,//ew Vector3(0, 100, -100),
				20000),
				new PointLight(new Vector3(0, -200, 0), Color.White * .85f,//ew Vector3(0, 100, -100),
				20000)
			};
            // setup shadows
            renderer.ShadowLightPosition = new Vector3(500, 500, 0);//new Vector3(1500, 1500, 2000);
            renderer.ShadowLightTarget = new Vector3(0, 300, 0);//new Vector3(0, 150, 0)


            renderer.DoShadowMapping = true;
            renderer.ShadowMult = 0.3f;//0.01f;//0.3f;
        }
        /*public GlassEffect(ContentManager content, GraphicsDevice graphics,
            Vector3 position, Vector2 size, PrelightingRenderer renderer)
            : this(content, graphics, position, size)
        {
            this.renderer = renderer;
        }*/
    }

}
