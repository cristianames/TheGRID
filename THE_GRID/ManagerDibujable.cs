﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using TgcViewer.Utils.TgcGeometry;
using Microsoft.DirectX;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer;
using AlumnoEjemplos.THE_GRID.Shaders;
using AlumnoEjemplos.THE_GRID.Helpers;

namespace AlumnoEjemplos.THE_GRID
{
    #region Manager Base
    public abstract class ManagerDibujable
    {
        //Listas de reciclaje de dibujables
        protected List<Dibujable> controlados;
        protected List<Dibujable> inactivos;
        protected int limiteControlados;
        List<int> opciones = new List<int>() { -1, 1, 0 };

        public ManagerDibujable(int limite)
        {
            inactivos = new List<Dibujable>(limite);
            controlados = new List<Dibujable>(limite);
            limiteControlados = limite;
        }

        internal void addNew(Dibujable nuevo)
        {
            if (inactivos.Count == limiteControlados) controlados.RemoveAt(0);
            controlados.Add(nuevo);
        }

        public virtual void operar(float time)
        {
            foreach (var item in controlados)
            {
                trasladar(item, time);
                rotar(item, time);
            }
        }

        protected void trasladar(Dibujable objeto, float time)
        {
            List<Dibujable> lista = new List<Dibujable>(0);
            objeto.desplazarsePorTiempo(time, lista);
        }

        protected void rotar(Dibujable objeto, float time)
        {
            List<Dibujable> lista = new List<Dibujable>(0);
            objeto.rotarPorTiempo(time, lista);
        }
        public void eliminarInactivo(Dibujable aEliminar)
        {
            controlados.Remove(aEliminar);
            aEliminar.dispose();
        }
        public void destruirListas()
        {
            foreach (var item in controlados) { item.dispose(); }
            foreach (var item in inactivos) { item.dispose(); }
        }

        public Dibujable activar()
        {
            Dibujable objeto = inactivos[0];
            inactivos.RemoveAt(0);
            controlados.Add(objeto);
            objeto.activar();
            return objeto;
        }

        public virtual void desactivar(Dibujable objeto)
        {
            controlados.Remove(objeto);
            inactivos.Add(objeto);
            objeto.desactivar();
        }
    }
    #endregion

    #region Manager Laser
    public class ManagerLaser : ManagerDibujable //Clase Para Manejar los láseres
    {
        bool alternado;
        protected List<Dibujable> controlados_azul;
        protected List<Dibujable> inactivos_azul;

        public ManagerLaser(int limite) : base(limite) 
        {
            controlados_azul = new List<Dibujable>(limite);
            inactivos_azul = new List<Dibujable>(limite);
            for (int i = 0; i < limiteControlados; i++) inactivos.Add(Factory.crearLaserRojo());
            for (int i = 0; i < limiteControlados; i++) inactivos_azul.Add(Factory.crearLaserAzul());
        }        
        public void cargarDisparo(EjeCoordenadas ejes, Vector3 posicionNave)
        {
            Vector3 lateral = ejes.vectorX;
            Vector3 atras = ejes.vectorZ;
            lateral.Multiply(5);
            atras.Multiply(-20);
            if (alternado)      //Ubicamos de forma alternada los lasers.
            {
                posicionNave += lateral;
                alternado = false;
            }
            else
            {
                lateral.Multiply(-1);
                posicionNave += lateral;
                alternado = true;
            }
            posicionNave += atras;
            if (inactivos.Count == 0)
            {
                Dibujable dead = controlados[0];
                desactivar(dead);
            }
            Dibujable laser = activar();
            Factory.reubicarLaserAPosicion(laser, ejes, posicionNave);
        }
        public void cargarSuperDisparo(EjeCoordenadas ejes, Vector3 posicionNave, float tiempo) //El disparo azul
        {
            Vector3 atras = ejes.vectorZ;
            atras.Multiply(20);
            posicionNave += atras;
            if (inactivos.Count == 0)
            {
                Dibujable dead = controlados[0];
                desactivar_azul(dead);
            }
            Dibujable laser = activar_azul();
            if (tiempo > 5)
                tiempo = 5;
            Factory.escalarLaser(laser, new Vector3(0.3f*tiempo, 0.3f*tiempo, 0.7f));
            Factory.reubicarLaserAPosicion(laser, ejes, posicionNave);
            laser.setFisica(0, 0, 10000, 0.15f * tiempo);
            laser.impulsate(laser.getDireccion(), 600, 0.1f);
        }
        public override void desactivar(Dibujable objeto)
        {
            controlados.Remove(objeto);            
            inactivos.Add(Factory.resetearLaser(objeto));
            objeto.desactivar();
        }
        public void desactivar_azul(Dibujable objeto)
        {
            controlados_azul.Remove(objeto);
            inactivos_azul.Add(Factory.resetearLaser(objeto));
            objeto.desactivar();
        }
        public Dibujable activar_azul()
        {
            Dibujable objeto = inactivos_azul[0];
            inactivos_azul.RemoveAt(0);
            controlados_azul.Add(objeto);
            objeto.activar();
            return objeto;
        }
        public override void operar(float time)
        {
            base.operar(time);
            foreach (var item in controlados_azul)
            {
                trasladar(item, time);
                rotar(item, time);
            }
        }
        public void chocoAsteroide() //Chequear por Fuerza bruta si los láseres chocaron algo
        {
            foreach (Dibujable laser in controlados)
                if(laser.objeto.Enabled) EjemploAlumno.workspace().Escenario.asteroidManager.chocoLaser(laser);
            foreach (Dibujable laser in controlados_azul)
                if (laser.objeto.Enabled) EjemploAlumno.workspace().Escenario.asteroidManager.chocoLaser(laser);
        }
    }
    #endregion

    #region Manager Asteroide
    public class ManagerAsteroide : ManagerDibujable //Clase Para Manejar los láseres
    {
        int cantidadEnMapa = 0;
        public ManagerAsteroide(int limite) : base(limite) 
        {
            Factory fabrica = new Factory();
            for(int i=0;i< limite;i++)
            {                                                                                                       
                 Asteroide asteroide = fabrica.crearAsteroide(TamanioAsteroide.CHICO, new Vector3(0, 0, 0), this);
                asteroide.desactivar();
                inactivos.Add(asteroide);
            }
        }

        public void explotaAlPrimero(){
            Dibujable colisionador = controlados[1];
            controlados.First().teChoque(colisionador,50);
        }

        public override void operar(float time)
        {
            foreach (var item in controlados)
            {
                trasladar(item, time);
                rotar(item, time);
            }

            reciclajeAsteroidesFueraDelSky();            
        }

        public void reinsertarSiEsNecesario(){
            if (controlados.Count < cantidadEnMapa) reinsertar(cantidadEnMapa - controlados.Count);
        }

        private void reciclajeAsteroidesFueraDelSky()
        {
            SkySphere skysphere = EjemploAlumno.workspace().SkySphere;
            bool breakForzoso = true;
            while (breakForzoso && controlados.Count > 0)
            {
                breakForzoso = false;
                foreach (var asteroide in controlados)
                    if (!asteroide.getColision().colisiono(skysphere.bordeSky))
                    {
                        desactivar(asteroide);
                        breakForzoso = true;
                        break;
                    }
            }
        }

        private void reinsertar(int cantidadAReinsertar)
        {
            List<TamanioAsteroide> opciones = new List<TamanioAsteroide>() { TamanioAsteroide.CHICO, TamanioAsteroide.MEDIANO, TamanioAsteroide.GRANDE, TamanioAsteroide.MUYGRANDE };
            Random random = new Random();
            Formato setearFormato = new Formato();
            Vector3 posicion = EjemploAlumno.workspace().ObjetoPrincipal.getPosicion();
            Vector3 ejeZ = EjemploAlumno.workspace().ObjetoPrincipal.getDireccion();
            Vector3 ejeX = EjemploAlumno.workspace().ObjetoPrincipal.getDireccion_X();
            Vector3 ejeY = EjemploAlumno.workspace().ObjetoPrincipal.getDireccion_Y();
            for (int i = 0; i < cantidadAReinsertar; i++)
            {                
                setearFormato.tamanio = Factory.elementoRandom<TamanioAsteroide>(opciones);
                //Seteamos posicion de reinsercion
                setearFormato.posicion = Vector3.Add(Vector3.Add(Vector3.Add(posicion, Vector3.Multiply(ejeZ, 9000)), Vector3.Multiply(ejeX, random.Next(-3000, 3000))), Vector3.Multiply(ejeY, random.Next(-3000, 3000)));
                //Pasaje de asteroides con formato
                activarAsteroide(setearFormato);
            }
        }
        public void desactivarTodos()
        {
            while (controlados.Count() > 0)
            {
                desactivar(controlados.First());
            }
        }
        public void activarAsteroide(Formato formato)     
        {
            if(inactivos.Count > 0)
            {      
                Dibujable asteroide = inactivos[0];
                inactivos.RemoveAt(0);

                //Darle el formato al asteroide
                formato.actualizarAsteroide(asteroide);
                controlados.Add(asteroide);
                
                Vector3 direccionImpulso = Factory.VectorRandom(-200, 200);
                float velocidadImpulso = new Random().Next(150, 750);     //IMPORTANTE: Sin importar la velocidad cargada en el formato, aca le indicamos una nueva velocidad.
                asteroide.fisica.impulsar(direccionImpulso, velocidadImpulso, 0.01f);                
                asteroide.activar();                                   
            }
        }
        public override void desactivar(Dibujable objeto)
        {
            controlados.Remove(objeto);
            inactivos.Add(Factory.resetearAsteroide((Asteroide) objeto));
            objeto.desactivar();
        }
        public void fabricarMiniAsteroides(int cuantos, TamanioAsteroide tam, Vector3 pos, float radio) //Crear los fragmentos de asteroides
        {
            Formato format = new Formato();
            format.tamanio = tam;
            Vector3 correccion;
            for (int i = 0; i < cuantos; i++)
            {
                correccion = Factory.VectorRandom(-500, 500);
                correccion.Normalize();
                correccion.Multiply(radio / 2);
                format.posicion = Vector3.Add(pos,correccion);
                activarAsteroide(format);
            }
        }
        public void fabricarMapaAsteroides(Vector3 pos_base, int cantidadAsteroides, int radioAdmisible) //Crear la disposicion de los asteroides
        {
            foreach (var asteroide in controlados)  
            {                                      
                desactivar(asteroide);
            }
            float max_x =  radioAdmisible + pos_base.X;
            float max_y =  radioAdmisible + pos_base.Y;
            float max_z =  radioAdmisible + pos_base.Z;
            float min_x = -radioAdmisible + pos_base.X;
            float min_y = -radioAdmisible + pos_base.Y;
            float min_z = -radioAdmisible + pos_base.Z;
            Formato setearFormato = new Formato();
            Random random = new Random();
            List<TamanioAsteroide> opciones = new List<TamanioAsteroide>(){TamanioAsteroide.CHICO, TamanioAsteroide.MEDIANO, TamanioAsteroide.GRANDE, TamanioAsteroide.MUYGRANDE};
            for (int i = 0; i < cantidadAsteroides; i++)
            {
                setearFormato.tamanio = Factory.elementoRandom<TamanioAsteroide>(opciones);
                setearFormato.posicion = new Vector3(random.Next((int)min_x, (int)max_x), random.Next((int)min_y, (int)max_y), random.Next((int)min_z, (int)max_z));
                //Pasaje de asteroides con formato
                activarAsteroide(setearFormato);
            }
            cantidadEnMapa = cantidadAsteroides;
        }

        public void chocoNave(Dibujable nave) //Chequeo contra la nave de colision
        {
            bool naveColision = false;
            foreach (Dibujable asteroide in controlados)
            {
                Color color = Color.Yellow;
                if (nave.getColision().colisiono(((TgcBoundingSphere)asteroide.getColision().getBoundingBox())))
                {
                    color = Color.Red;
                    ((TgcObb)nave.getColision().getBoundingBox()).setRenderColor(color);
                    naveColision = true;
                    //--------
                    int flagReintento = 0;
                    Dupla<Vector3> velocidades = Fisica.CalcularChoqueElastico(nave, asteroide);
                    Dupla <float> modulos = new Dupla<float>(asteroide.fisica.velocidadInstantanea * 0.9f, nave.fisica.velocidadInstantanea * 0.15f);
                    asteroide.impulsate(velocidades.fst, modulos.fst, 0.01f);
                    nave.impulsate(velocidades.snd, modulos.snd, 0.01f);
                    if (flagReintento == 0) EjemploAlumno.workspace().music.playAsteroideColision();
                    //--------
                    asteroide.teChoque(nave, nave.velocidadActual());
                    break;
                }
                ((TgcBoundingSphere)asteroide.getColision().getBoundingBox()).setRenderColor(color);
            }
            if (!naveColision) ((TgcObb)nave.getColision().getBoundingBox()).setRenderColor(Color.Yellow);
        }

        public void chocoLaser(Dibujable laser) //Chequeo contra el laser por colision
        {
            foreach (Asteroide asteroide in controlados)
            {
                if (laser.getColision().colisiono(((TgcBoundingSphere)asteroide.getColision().getBoundingBox())))
                {
                    ((TgcObb)laser.getColision().getBoundingBox()).setRenderColor(Color.Blue);
                    ((TgcBoundingSphere)asteroide.getColision().getBoundingBox()).setRenderColor(Color.Blue);
                    asteroide.teChoque(laser,laser.velocidadActual());
                    EjemploAlumno.workspace().music.playAsteroideImpacto();
                    laser.desactivar();
                    break;
                }
            }
        }

        public void colisionEntreAsteroides(int i) 
        {
            int pos = i;
            int cant = controlados.Count();
            if (cant<2) return;
            if (i + 1 == cant) //osea es el ultimo
            {
                //no hace nada
            }
            else
            {
                for (++i; i < controlados.Count(); i++)
                {
                    Dupla<Vector3> velocidades;
                    Dupla<float> modulos;
                    if (controlados[pos].getColision().colisiono(((TgcBoundingSphere)controlados[i].getColision().getBoundingBox()))) 
                    {
                        int flagReintento = 0;
                        ((TgcBoundingSphere)controlados[pos].getColision().getBoundingBox()).setRenderColor(Color.DarkGreen);
                        ((TgcBoundingSphere)controlados[i].getColision().getBoundingBox()).setRenderColor(Color.DarkGreen);
                        velocidades = Fisica.CalcularChoqueElastico(controlados[i], controlados[pos]);
                        modulos = new Dupla<float>(controlados[pos].fisica.velocidadInstantanea * 0.9f, controlados[i].fisica.velocidadInstantanea * 0.9f);
                        controlados[pos].impulsate(velocidades.fst, modulos.fst, 0.01f);
                        controlados[i].impulsate(velocidades.snd, modulos.snd, 0.01f);
                        //Lo de abajo es necesario para que no se quede eternamente reacalculando en algunos casos, pero hace que los asteroides desaparezcan.
                        while (!distanciaSegura((Asteroide)controlados[i], (Asteroide)controlados[pos]))
                        {
                            controlados[pos].impulsate(Vector3.Multiply(controlados[pos].ultimaTraslacion, -1), modulos.fst, 0.5f);
                            controlados[i].impulsate(Vector3.Multiply(controlados[i].ultimaTraslacion, -1), modulos.snd, 0.5f);
                            if (flagReintento > 1000) { desactivar(controlados[i]); break; }
                            flagReintento++;
                        }
                        if (flagReintento == 0) EjemploAlumno.workspace().music.playAsteroideColision();
                    }
                }
                if (pos < controlados.Count)colisionEntreAsteroides(++pos);
            }
        }

        private bool distanciaSegura(Asteroide colisionado, Asteroide colisionador)
        {
            float suma_radios = ((TgcBoundingSphere)colisionado.getColision().getBoundingBox()).Radius;
            suma_radios += ((TgcBoundingSphere)colisionador.getColision().getBoundingBox()).Radius;
            float distancia = Vector3.Subtract(colisionado.getPosicion(), colisionador.getPosicion()).Length();
            if (distancia > suma_radios) return true;
            else return false;
        }

        internal List<Dibujable> Controlados()
        {
            return controlados;
        }
    }
    #endregion

    #region Formato
    public class Formato
    {
        //Clase que sirve para actualizar los asteroides y reciclarlo
        public TamanioAsteroide tamanio;
        public Vector3 posicion;
        List<int> opciones = new List<int>() { -1, 1 };

        public void actualizarAsteroide(Dibujable asteroide)
        {
            FormatoAsteroide formatoAUsar = Asteroide.elegirAsteroidePor(tamanio);

            asteroide.escalarSinBB(formatoAUsar.getVolumen());
            asteroide.setFisica(0, 0, 500, formatoAUsar.getMasa());     //IMPORTANTE: Aca se setea la velocidad maxima que puede alcanzar un asteroide.
            asteroide.velocidad = formatoAUsar.getVelocidad();
            ((Asteroide)asteroide).tamanioAnterior = formatoAUsar.tamanioAnterior();
            ((Asteroide)asteroide).Vida = formatoAUsar.vidaInicial();

            asteroide.traslacion = 1;
            asteroide.rotacion = Factory.elementoRandom<int>(opciones);
            asteroide.giro = Factory.elementoRandom<int>(opciones);
            asteroide.inclinacion = Factory.elementoRandom<int>(opciones);

            float radioMalla3DsMax = 10.633f;
            TgcBoundingSphere bounding = (TgcBoundingSphere) asteroide.getColision().getBoundingBox();
            bounding.setValues(bounding.Center, radioMalla3DsMax * formatoAUsar.getVolumen().X);
            asteroide.ubicarEnUnaPosicion(posicion);
        }
    }
    #endregion
}
