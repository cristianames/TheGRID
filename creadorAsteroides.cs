﻿using Microsoft.DirectX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TgcViewer.Utils.Modifiers;

namespace AlumnoEjemplos.TheGRID
{
    class creadorAsteroides
    {
        public TgcMesh crearAsteroide(Vector3 tamanio)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;
            string alumnoMediaFolder = GuiController.Instance.AlumnoEjemplosMediaDir; 
          
            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene scene = loader.loadSceneFromFile(GuiController.Instance.AlumnoEjemplosMediaDir + "Asteroide\\esferita-TgcScene.xml");
            TgcMesh asteroide = scene.Meshes[0];
            asteroide.AutoTransformEnable = false;
            asteroide.Transform = Matrix.Scaling(tamanio);
            return asteroide;
        }
        public void transladar(TgcMesh asteroide, Vector3 vector)
        {
            Matrix translacion = Matrix.Translation(vector);
            asteroide.Transform *= translacion;
        }

       /* public Asteroide(Vector3 tamanio)
        {
            transform.Scale(tamanio);
            Device d3dDevice = GuiController.Instance.D3dDevice;
            //Carpeta de archivos Media del alumno
            string alumnoMediaFolder = GuiController.Instance.AlumnoEjemplosMediaDir;   

            TgcSceneLoader loader = new TgcSceneLoader();
            changeDiffuseMaps(new TgcTexture[] { TgcTexture.createTexture(d3dDevice, GuiController.Instance.ExamplesDir + "Transformations\\SistemaSolar\\SunTexture.jpg") });

        }*/
    }
}
