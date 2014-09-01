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
using TgcViewer.Utils.TgcGeometry;

namespace AlumnoEjemplos.TheGRID
{
    class Factory
    {
        public Dibujable crearAsteroide(Vector3 tamanio)
        {
            //Carguemos el DirectX y la carpeta de media
            Device d3dDevice = GuiController.Instance.D3dDevice;
            string alumnoMediaFolder = GuiController.Instance.AlumnoEjemplosMediaDir; 

            //Creemos la mesh
            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene scene = loader.loadSceneFromFile(GuiController.Instance.AlumnoEjemplosMediaDir + "Asteroide\\esferita-TgcScene.xml");
            TgcMesh mesh_asteroide = scene.Meshes[0];
            mesh_asteroide.AutoTransformEnable = false;
            mesh_asteroide.Transform = Matrix.Scaling(tamanio);
            //Creamos su bounding Sphere
            TgcBoundingSphere bounding_asteroide = new TgcBoundingSphere(mesh_asteroide.BoundingBox.calculateBoxCenter(), mesh_asteroide.BoundingBox.calculateBoxRadius());
           
            //Cargamos las cosas en el dibujable
            Dibujable asteroide = new Dibujable();
            asteroide.objeto = mesh_asteroide;
            asteroide.setBoundingBox(bounding_asteroide);

            return asteroide;
        }
        public void trasladar(Dibujable asteroide, Vector3 vector)
        {
            Matrix traslacion = Matrix.Translation(vector);
            ((TgcMesh)asteroide.objeto).Transform *= traslacion;
            ((TgcBoundingSphere)asteroide.getBoundingBox()).setCenter(((TgcMesh)asteroide.objeto).BoundingBox.calculateBoxCenter() + vector);
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