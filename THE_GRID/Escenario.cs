﻿using AlumnoEjemplos.THE_GRID.Shaders;
using AlumnoEjemplos.THE_GRID;
using Microsoft.DirectX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using AlumnoEjemplos.THE_GRID.Colisiones;
using System.Drawing;
using AlumnoEjemplos.THE_GRID.Helpers;

namespace AlumnoEjemplos.THE_GRID
{
    class Escenario
    {
        #region Atributos
        public ManagerLaser laserManager = new ManagerLaser(100);
        public ManagerAsteroide asteroidManager; 
        public Dibujable principal;
        public enum TipoModo { THE_OPENING, IMPULSE_DRIVE, WELCOME_HOME, VACUUM, MISION, WELCOME_HOME_FINAL };
        public TipoModo escenarioActual = TipoModo.VACUUM;
        public TipoModo escenarioElegido = TipoModo.VACUUM;
        //Objetos
        public Dibujable sol;
        public float distanciaSol = 9200;
        public Dibujable planet;
        private List<Dibujable> cuerposGravitacionales = new List<Dibujable>();
        public List<Dibujable> CuerposGravitacionales { get{ return cuerposGravitacionales; } }
        //public bool ingresoMision;
        float valorDeTransicionCapitulo1 = 50000;
        Vector3 posicionInicialDeConteo;
        public bool sePuedeSalirDelCapitulo1 = false;
        public bool mensaje1a = false;
        public bool mensaje1b = false;
        public bool mensaje2a = false;
        public bool mensaje2b = false;
        public bool mensaje3 = false;
        float valorDeTransicionCapitulo2 = 30;
        public bool sePuedeSalirDelCapitulo2 = false;
        float valorDeTransicionCapitulo3 = 40000;
        public bool sePuedeSalirDelCapitulo3 = false;
        float acumTime;
        float distancia;



        #endregion

        #region Constructor y Destructor
        public Escenario(Dibujable ppal) 
        {
            principal = ppal;
            asteroidManager = new ManagerAsteroide(2000);
            crearSol();
            crearParticulas();
            crearPlaneta(); 
        }

        private static void crearParticulas()
        {
            EjemploAlumno.workspace().estrellas = new List<TgcMesh>();
            EjemploAlumno.workspace().estrellasNo = new List<TgcMesh>();
            EjemploAlumno.workspace().estrellaControl = new Estrella();
            EjemploAlumno.workspace().estrellaControl.recur = false;
            EjemploAlumno.workspace().estrellaControl.cuadrante = 1;

            for (int i = 0; i < 100; i++)
            {
                TgcTriangle protoestrella = new TgcTriangle();
                protoestrella.A = new Vector3(-1f, 0, 20);
                protoestrella.B = new Vector3(0, 2, 20);
                protoestrella.C = new Vector3(1f, 0, 20);
                protoestrella.Color = Color.White;
                TgcMesh estrella;
                estrella = protoestrella.toMesh("asd");
                EjemploAlumno.workspace().estrellas.Add(estrella);
                EjemploAlumno.workspace().objetosBrillantes.Add(estrella);
            }
        }

        private void crearSol()
        {
            TgcMesh mesh_Sol = Factory.cargarMesh(@"Sol\sol-TgcScene.xml");
            sol = new Dibujable();
            sol.setObject(mesh_Sol, 0, 20, new Vector3(2F, 2F, 2F));
            sol.giro = -1;
            sol.ubicarEnUnaPosicion(new Vector3(0, 0, distanciaSol));
            sol.activar();
            EjemploAlumno.workspace().sol = sol;
        }
        private void crearPlaneta()
        {
            TgcMesh mesh_Planet = Factory.cargarMesh(@"Asteroide\theplanet-TgcScene.xml");
            planet = new Dibujable();
            planet.setObject(mesh_Planet, 0, 10, new Vector3(10F, 10F, 10F));
            planet.setFisica(0, 0, 0, 500000000);
            planet.giro = -1;
            planet.ubicarEnUnaPosicion(new Vector3(0, 0, 0));
            planet.desactivar();
            TgcBoundingSphere bounding_asteroide = new TgcBoundingSphere(new Vector3(0, 0, 0), 15000);
            planet.setColision(new ColisionAsteroide());
            planet.getColision().setBoundingBox(bounding_asteroide);
            EjemploAlumno.workspace().objectosNoMeshesCollection.Add(planet.objeto);
        }
        public void dispose()
        {
            sol.dispose();
        }
        #endregion

        #region Carga de escenario
        //-------------------------------------------------------------------------------------------CHAPTER-1
        public void loadChapter1()
        {
            disposeOld();
            asteroidManager.fabricarMapaAsteroides(principal.getPosicion(), 50, 7000);
            cuerposGravitacionales = asteroidManager.Controlados();
            escenarioActual = TipoModo.THE_OPENING;
        }
        //-------------------------------------------------------------------------------------------CHAPTER-2
        public void loadChapter2() 
        {
            disposeOld();
            escenarioActual = TipoModo.IMPULSE_DRIVE;
        }
        //-------------------------------------------------------------------------------------------CHAPTER-3
        public void loadChapter3() 
        {
            disposeOld();
            escenarioActual = TipoModo.WELCOME_HOME;
            EjemploAlumno.workspace().nave.fisica.velocidadInstantanea = 10;
            Dibujable ppal = EjemploAlumno.workspace().ObjetoPrincipal;
            List<int> opciones = new List<int>() { -8000, 8000 };
            Vector3 posicion = ppal.getPosicion();
            posicion.Add(Vector3.Multiply(ppal.getDireccion(),21000));
            posicion.Add(Vector3.Multiply(ppal.getDireccion_X(), Factory.elementoRandom<int>(opciones)));
            posicion.Add(Vector3.Multiply(ppal.getDireccion_Y(), (new Random()).Next(-3000,3000)));
            planet.ubicarEnUnaPosicion(posicion);
            planet.activar();
            cuerposGravitacionales = new List<Dibujable>() { planet };
        }
        //-------------------------------------------------------------------------------------------MISION
        public void loadMision()
        {
            disposeOld();
            posicionInicialDeConteo = principal.getPosicion();
            loadChapter1();
        }
        public void loadVacuum() 
        {
            disposeOld();
            escenarioActual = TipoModo.VACUUM;
        }

        private void disposeOld()
        {
            asteroidManager.desactivarTodos();
            planet.desactivar();
            cuerposGravitacionales = new List<Dibujable>();
            EjemploAlumno.workspace().Shader.motionBlurActivado = false;
            // Aca se deben borran todas las cosas para un reinicializado.
        }
        #endregion
        
        #region Update segun escenario
        internal void dispararLaser()
        {
            laserManager.cargarDisparo(principal.getEjes(), principal.getPosicion());
            EjemploAlumno.workspace().music.playLazer();
        }
        internal void dispararLaserAzul(float tiempo)
        {
            if (tiempo == 0) return; //Si no cargo nada, no hay lazer que disparar
            laserManager.cargarSuperDisparo(principal.getEjes(), principal.getPosicion(), tiempo);
        }

        internal void refrescar(float elapsedTime)
        {
            //Movemos el sol en funcion a la nave
            sol.rotarPorTiempo(elapsedTime, new List<Dibujable>());
            sol.ubicarEnUnaPosicion(Vector3.Add(principal.getPosicion(), new Vector3(0, 0, distanciaSol)));

            switch (escenarioActual)
            {
                case TipoModo.THE_OPENING:
                    laserManager.operar(elapsedTime);
                    asteroidManager.reinsertarSiEsNecesario();
                    asteroidManager.operar(elapsedTime);
                    asteroidManager.chocoNave(principal);
                    laserManager.chocoAsteroide();
                    asteroidManager.colisionEntreAsteroides(0);
                    break;
                case TipoModo.IMPULSE_DRIVE:
                    if (EjemploAlumno.workspace().Shader.motionBlurActivado)
                        EjemploAlumno.workspace().estrellaControl.insertarEstrellas(EjemploAlumno.workspace().estrellas, EjemploAlumno.workspace().estrellasNo, EjemploAlumno.workspace().nave.getPosicion(), EjemploAlumno.workspace().nave.getDireccion(), EjemploAlumno.workspace().nave.ultimaTraslacion, EjemploAlumno.workspace().nave.getEjes().mRotor, elapsedTime);
                    break;
                case TipoModo.WELCOME_HOME:
                    colisionNavePlaneta(EjemploAlumno.workspace().ObjetoPrincipal);
                    planet.rotarPorTiempo(elapsedTime, new List<Dibujable>());
                    break;
            }
            switch (escenarioElegido)
            {
                case TipoModo.MISION:
                    distancia = Vector3.Subtract(principal.getPosicion(), posicionInicialDeConteo).Length();
                    if (sePuedeSalirDelCapitulo1)
                    {
                        if (distancia > valorDeTransicionCapitulo1)
                        {
                            loadChapter2();
                            sePuedeSalirDelCapitulo1 = false;
                            sePuedeSalirDelCapitulo2 = true;
                            mensaje1b = false;
                            mensaje2a = true;
                        }
                        else 
                        {
                            if (distancia > valorDeTransicionCapitulo1 / 2)
                            {
                                if (!mensaje1b) { mensaje1a = false; mensaje1b = true; }
                            }
                        }
                    }
                    if (sePuedeSalirDelCapitulo2)
                    {
                        if (EjemploAlumno.workspace().Shader.motionBlurActivado)
                        {
                            acumTime += elapsedTime;
                            if (mensaje2a) { mensaje2a = false; mensaje2b = true; }
                        }
                        if (acumTime > valorDeTransicionCapitulo2)
                        {
                            if (EjemploAlumno.workspace().Shader.motionBlurActivado)
                            {
                                EjemploAlumno.workspace().desactivarBlur();
                                loadChapter3();
                                posicionInicialDeConteo = sol.getPosicion();
                                sePuedeSalirDelCapitulo2 = false;
                                acumTime = 0;
                            }
                        }
                        else if (acumTime > valorDeTransicionCapitulo2 / 1.2f && mensaje2b)
                        {
                            mensaje2b = false;
                            EjemploAlumno.workspace().music.chequearCambio("Tron Ending");
                        }
                    }
                    break;
            }
        }
        private void colisionNavePlaneta(Dibujable nave)
        {
            Color color = Color.Yellow;
            if (nave.getColision().colisiono(((TgcBoundingSphere)planet.getColision().getBoundingBox())))
            {
                color = Color.Red;
                //--------Realiza el calculo para determinar para donde salir disparado por el choque elastico con el Planeta
                //--------Si si....elastico....es mas facil.
                int flagReintento = 0;
                Vector3 direccion = nave.getDireccion();
                Vector3 distancias = Vector3.Subtract(nave.getPosicion(), planet.getPosicion());
                distancias.Normalize();
                distancias.Multiply(1);
                direccion.X *= distancias.X;
                direccion.Y *= distancias.Y;
                direccion.Z *= distancias.Z;
                float velocidad = nave.fisica.velocidadInstantanea * 0.7f;
                float radioCuad = FastMath.Pow2(((TgcBoundingSphere)planet.getColision().getBoundingBox()).Radius) + 0;
                while (Vector3.LengthSq(Vector3.Subtract(nave.getPosicion(), planet.getPosicion())) < radioCuad)
                {
                    nave.impulsate(direccion, velocidad, 0.01f);
                }               
                if (flagReintento == 0) EjemploAlumno.workspace().music.playAsteroideColision();
                //--------
            }
            ((TgcObb)nave.getColision().getBoundingBox()).setRenderColor(color);
        }
        #endregion

        #region Chequeo de cambios
        internal void chequearCambio(string opcion) //Cambio de Capitulo
        {
            switch (opcion)
            {
                case "THE OPENING":
                    //if (escenarioElegido != TipoModo.THE_OPENING)
                        loadChapter1();
                    escenarioElegido = TipoModo.THE_OPENING;
                    sePuedeSalirDelCapitulo1 = false;
                    sePuedeSalirDelCapitulo2 = false;
                    mensaje1a = false;
                    mensaje1b = false;
                    mensaje2a = false;
                    mensaje2b = false;
                    mensaje3 = false;
                    acumTime = 0;
                    break;
                case "IMPULSE DRIVE":
                    //if (escenarioElegido != TipoModo.IMPULSE_DRIVE)
                        loadChapter2();
                    escenarioElegido = TipoModo.IMPULSE_DRIVE;
                    sePuedeSalirDelCapitulo1 = false;
                    sePuedeSalirDelCapitulo2 = false;
                    mensaje1a = false;
                    mensaje1b = false;
                    mensaje2a = false;
                    mensaje2b = false;
                    mensaje3 = false;
                    acumTime = 0;
                    break;
                case "WELCOME HOME":
                    //if (escenarioElegido != TipoModo.WELCOME_HOME)
                        loadChapter3();
                    escenarioElegido = TipoModo.WELCOME_HOME;
                    sePuedeSalirDelCapitulo1 = false;
                    sePuedeSalirDelCapitulo2 = false;
                    mensaje1a = false;
                    mensaje1b = false;
                    mensaje2a = false;
                    mensaje2b = false;
                    mensaje3 = false;
                    acumTime = 0;
                    break;
                case "VACUUM":
                    //if (escenarioElegido != TipoModo.VACUUM)
                        loadVacuum();
                    escenarioElegido = TipoModo.VACUUM;
                    sePuedeSalirDelCapitulo1 = false;
                    sePuedeSalirDelCapitulo2 = false;
                    mensaje1a = false;
                    mensaje1b = false;
                    mensaje2a = false;
                    mensaje2b = false;
                    mensaje3 = false;
                    acumTime = 0;
                    break;
                case "MISION":
                    //if (escenarioElegido != TipoModo.MISION)
                    sePuedeSalirDelCapitulo1 = false;
                    sePuedeSalirDelCapitulo2 = false;
                    mensaje1a = true;
                    mensaje1b = false;
                    mensaje2a = false;
                    mensaje2b = false;
                    mensaje3 = false;
                    acumTime = 0;
                    loadMision();
                    sePuedeSalirDelCapitulo1 = true;
                    escenarioElegido = TipoModo.MISION;
                    break;
            }
        }
        #endregion

        //Codigo funcional, pero no utilizado aun. Genera la dispersion de particulas espaciales
        #region Deprecados
        private void crearEstrellas()
        {
            //Cargamos la lista de texturas
            List<TgcTexture> texturasEstrellas = new List<TgcTexture>();
            texturasEstrellas.Add(TgcTexture.createTexture(GuiController.Instance.D3dDevice, EjemploAlumno.TG_Folder + @"Estrella\Textures\Azul.jpg"));
            texturasEstrellas.Add(TgcTexture.createTexture(GuiController.Instance.D3dDevice, EjemploAlumno.TG_Folder + @"Estrella\Textures\Blanca.jpg"));
            texturasEstrellas.Add(TgcTexture.createTexture(GuiController.Instance.D3dDevice, EjemploAlumno.TG_Folder + @"Estrella\Textures\Brillante.jpg"));
            texturasEstrellas.Add(TgcTexture.createTexture(GuiController.Instance.D3dDevice, EjemploAlumno.TG_Folder + @"Estrella\Textures\Celeste.jpg"));
            texturasEstrellas.Add(TgcTexture.createTexture(GuiController.Instance.D3dDevice, EjemploAlumno.TG_Folder + @"Estrella\Textures\Marron.jpg"));
            texturasEstrellas.Add(TgcTexture.createTexture(GuiController.Instance.D3dDevice, EjemploAlumno.TG_Folder + @"Estrella\Textures\Negra.jpg"));
            texturasEstrellas.Add(TgcTexture.createTexture(GuiController.Instance.D3dDevice, EjemploAlumno.TG_Folder + @"Estrella\Textures\Roja.jpg"));

            int i;
            for (i = 0; i < 1000; i++)
            {
                //Estrella como esfera            
                TgcSphere star = new TgcSphere();
                star.Radius = 20;
                int resto;
                Math.DivRem(i, texturasEstrellas.Count(), out resto);
                star.setTexture(texturasEstrellas[resto]);      
                star.Position = new Vector3(0, 0, 0);
                star.BasePoly = TgcSphere.eBasePoly.ICOSAHEDRON;
                star.Inflate = true;
                star.LevelOfDetail = 0;
                star.updateValues();
                TgcMesh meshTemporal = star.toMesh("Estrellita");
                //Estrella como dibujable
                Dibujable estrella;
                estrella = new Dibujable();
                meshTemporal.AutoTransformEnable = false;
                estrella.setObject(meshTemporal, 0, 200, new Vector3(1F, 1F, 1F));
                List<int> opcionesRotacion = new List<int>();
                opcionesRotacion.Add(1);
                opcionesRotacion.Add(-1);
                estrella.giro = Factory.elementoRandom<int>(opcionesRotacion);
                estrella.activar();
                //Rotamos la posicion de la estrella
                List<Vector3> opcionesPosiciones = new List<Vector3>();
                opcionesPosiciones.Add(new Vector3(9500, 0, 0));
                opcionesPosiciones.Add(new Vector3(-9500, 0, 0));
                opcionesPosiciones.Add(new Vector3(0, 9500, 0));
                opcionesPosiciones.Add(new Vector3(0, -9500, 0));
                opcionesPosiciones.Add(new Vector3(0, 0, 9500));
                opcionesPosiciones.Add(new Vector3(0, 0, -9500));

                Vector3 posicionFinal = Factory.elementoRandom<Vector3>(opcionesPosiciones);
                Vector3 rotacion = Factory.VectorRandom(0, 360);  // Aca va una rotacion random
                rotacion.X = Geometry.DegreeToRadian(rotacion.X + i);
                rotacion.Y = Geometry.DegreeToRadian(rotacion.Y * i);
                rotacion.Z = Geometry.DegreeToRadian(rotacion.Z + 3 * i);
                Matrix rotation = Matrix.RotationYawPitchRoll(rotacion.Y, rotacion.X, rotacion.Z);
                Vector4 normal4 = Vector3.Transform(posicionFinal, rotation);
                posicionFinal = new Vector3(normal4.X, normal4.Y, normal4.Z);
                //Llevamos a la estrella a su posicion final
                estrella.ubicarEnUnaPosicion(posicionFinal);
                //Añadimos la estrella a las listas
                EjemploAlumno.workspace().dibujableCollection.Add(estrella);
            }
        }
        #endregion

    }
}
