﻿using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using TgcViewer;
using TgcViewer.Utils;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;

namespace AlumnoEjemplos.THE_GRID.Shaders
{
    class MotionBlur : IShader
    {
        private string ShaderDirectory = EjemploAlumno.TG_Folder + "Shaders\\MotionBlur.fx";
        private SuperRender mainShader;
        private Effect effect;
        private VertexBuffer g_pVBV3D;
        private Surface g_pDepthStencil;     // Depth-stencil buffer 
        private Texture g_pRenderTarget;    //Textura
        private Texture g_pVel1, g_pVel2;   // velocidad
        private Matrix antMatView;

        public MotionBlur(SuperRender main)
        {
            mainShader = main;
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Cargar Shader del motionBlur
            string compilationErrors;
            effect = Effect.FromFile(GuiController.Instance.D3dDevice, ShaderDirectory,
                null, null, ShaderFlags.PreferFlowControl, null, out compilationErrors);
            if (effect == null)
            {
                throw new Exception("Error al cargar shader. Errores: " + compilationErrors);
            }
            //Configurar Technique Default dentro del shader
            effect.Technique = "DefaultTechnique";
            // stencil
            g_pDepthStencil = d3dDevice.CreateDepthStencilSurface(d3dDevice.PresentationParameters.BackBufferWidth,
                                                                         d3dDevice.PresentationParameters.BackBufferHeight,
                                                                         DepthFormat.D24S8,
                                                                         MultiSampleType.None,
                                                                         0,
                                                                         true);
            // Inicializo las Texturas de Velocidad
            g_pVel1 = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth
                    , d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget,
                        Format.A16B16G16R16F, Pool.Default);
            g_pVel2 = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth
                    , d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget,
                        Format.A16B16G16R16F, Pool.Default);
            // Resolucion de pantalla
            effect.SetValue("screen_dx", d3dDevice.PresentationParameters.BackBufferWidth);
            effect.SetValue("screen_dy", d3dDevice.PresentationParameters.BackBufferHeight);

            CustomVertex.PositionTextured[] vertices = new CustomVertex.PositionTextured[]
		    {
    			new CustomVertex.PositionTextured( -1, 1, 1, 0,0), 
			    new CustomVertex.PositionTextured(1,  1, 1, 1,0),
			    new CustomVertex.PositionTextured(-1, -1, 1, 0,1),
			    new CustomVertex.PositionTextured(1,-1, 1, 1,1)
    		};
            //vertex buffer de los triangulos
            g_pVBV3D = new VertexBuffer(typeof(CustomVertex.PositionTextured),
                    4, d3dDevice, Usage.Dynamic | Usage.WriteOnly,
                        CustomVertex.PositionTextured.Format, Pool.Default);
            g_pVBV3D.SetData(vertices, 0, LockFlags.None);
            //Inicializo la matriz de vision
            antMatView = d3dDevice.Transform.View;
        }

        public Texture renderPostProccess(EstructuraRender parametros)
        {
            Device device = GuiController.Instance.D3dDevice;
            
            //Pedimos que nos renderizen la pantalla default
            g_pRenderTarget = mainShader.renderAnterior(parametros, tipoShader());

            //Hacemos el post Procesado
            device.BeginScene();
            effect.Technique = "PostProcessMotionBlur";
            device.VertexFormat = CustomVertex.PositionTextured.Format;
            device.SetStreamSource(0, g_pVBV3D, 0);
            effect.SetValue("g_RenderTarget", g_pRenderTarget);
            effect.SetValue("texVelocityMap", g_pVel1);
            effect.SetValue("texVelocityMapAnt", g_pVel2);
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            effect.Begin(FX.None);
                effect.BeginPass(0);
                    device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
                effect.EndPass();
            effect.End();
            device.EndScene();
            return null;
        }

        public Texture renderEffect(EstructuraRender parametros)
        {
            Device device = GuiController.Instance.D3dDevice;
            antMatView = device.Transform.View;
            if (mainShader.motionBlurActivado)
            {
                dibujarVelocidad(parametros);
                renderPostProccess(parametros);
            }
            else
                renderDefault(parametros);
            return null;
        }
        
        public Texture renderDefault(EstructuraRender parametros)
        {
            Device device = GuiController.Instance.D3dDevice;

            //Obtenemos la textura dspues de todos los efectos anteriores
            g_pRenderTarget = mainShader.renderAnterior(parametros, tipoShader());

            //La renderizamos sobre un TriangleStrip
            device.BeginScene();
                effect.Technique = "OnlyTexture";
                device.VertexFormat = CustomVertex.PositionTextured.Format;
                device.SetStreamSource(0, g_pVBV3D, 0);
                effect.SetValue("texOnly", g_pRenderTarget);
                device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                effect.Begin(FX.None);
                    effect.BeginPass(0);
                        device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
                    effect.EndPass();
                effect.End();
            device.EndScene();
            return null;
        }

        private void dibujarVelocidad(EstructuraRender parametros)
        {
            Device device = GuiController.Instance.D3dDevice;
            float pixel_blur_variable;

            //Calcula el porcentual de aplicacion sobre el blur.
            pixel_blur_variable = 0.2f * (EjemploAlumno.workspace().nave.velocidadActual() / 300000); 
            effect.SetValue("PixelBlurConst", pixel_blur_variable);

            //Cambio el render target por la textura de velocidad
            Surface pOldRT = device.GetRenderTarget(0);
            Surface pSurf = g_pVel1.GetSurfaceLevel(0);
            device.SetRenderTarget(0, pSurf);
            //Cambio el depthbuffer por uno sin multisampling
            Surface pOldDS = device.DepthStencilSurface;
            device.DepthStencilSurface = g_pDepthStencil;

            //PASADA DE MAPA DE VELOCIDAD
            effect.Technique = "VelocityMap";
            //Mando las matrices de vision
            effect.SetValue("matView", device.Transform.View);
            effect.SetValue("matViewAnt", antMatView);
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            device.BeginScene();
                //Renderizo los objetos
                renderScene(parametros.meshes, "VelocityMap");
                if (!EjemploAlumno.workspace().camara.soyFPS())
                    renderScene(parametros.nave, "VelocityMap");
                renderScene(parametros.elementosRenderizables);
            device.EndScene();
            pSurf.Dispose();

            //Vuelvo a Setear el depthbuffer y el Render target originales
            device.SetRenderTarget(0, pOldRT);
            device.DepthStencilSurface = pOldDS;

            //Actualizo los valores para el proximo frame
            Texture aux = g_pVel2;
            g_pVel2 = g_pVel1;
            g_pVel1 = aux;

        }

        #region RenderScene
        /// <summary>
        /// Renderizar la mesh de la nave segun una technique
        /// </summary>
        public void renderScene(Nave nave, string technique)
        {
            ((TgcMesh)nave.objeto).Effect = effect;
            ((TgcMesh)nave.objeto).Technique = technique;
            nave.render();
        }
        /// <summary>
        /// Renderizar un dibujable segun una technique
        /// </summary>
        public void renderScene(Dibujable dibujable, string technique)
        {
            if (dibujable.soyAsteroide() && mainShader.fueraFrustrum(dibujable)) return;
            ((TgcMesh)dibujable.objeto).Effect = effect;
            ((TgcMesh)dibujable.objeto).Technique = technique;
            dibujable.render();
        }
        /// <summary>
        /// Renderizar una lista de dibujables segun una technique
        /// </summary>
        public void renderScene(List<Dibujable> dibujables, string technique)
        {
            foreach (Dibujable dibujable in dibujables)
            {
                renderScene(dibujable, technique);
            }
        }
        /// <summary>
        /// Renderizar objetos sin efectos
        /// </summary>
        public void renderScene(List<IRenderObject> elementosRenderizables)
        {
            foreach (IRenderObject elemento in elementosRenderizables)
            {
                elemento.render();
            }
        }

        #endregion

        #region Metodos Auxiliares

        public SuperRender.tipo tipoShader()
        {
            return SuperRender.tipo.MOTION; ;
        }

        public void close()
        {
            effect.Dispose();
            g_pDepthStencil.Dispose();
            g_pVBV3D.Dispose();
            g_pVel1.Dispose();
            g_pVel2.Dispose();
        }

        #endregion
      
    }
}
