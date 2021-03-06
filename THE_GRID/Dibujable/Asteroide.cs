﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.DirectX;
using TgcViewer.Utils.TgcGeometry;
using AlumnoEjemplos.THE_GRID.Explosiones;

namespace AlumnoEjemplos.THE_GRID
{
    public class Asteroide : Dibujable
    {
        public Asteroide() : base() {}
        public TamanioAsteroide tamanioAnterior;
        public ManagerAsteroide manager;
        private float vida;
        public float Vida { set { vida = value; } }

        public override bool soyAsteroide() { return true; }

        public override void teChoque(Dibujable colisionador, float moduloVelocidad)
        {
            //Verificacion por si el asteroide NO posee fisica asociada.
            float masa = 0.01f;
            if (colisionador.fisica != null) masa = colisionador.fisica.Masa;           
            daniate(masa, moduloVelocidad);
            if (vida <= 0) sinVida(colisionador);
        }

        private void sinVida(Dibujable colisionador)
        {
            fraccionate();
            if (!colisionador.soyAsteroide())
                EjemploAlumno.workspace().music.playAsteroideFragmentacion();
        }

        private void daniate(float masa, float moduloVelocidad)
        {
            //Perdida de vida
            vida -= (float) 5 * masa * moduloVelocidad;
        }

        private void fraccionate()
        {
            manager.desactivar(this);
            if (tamanioAnterior != TamanioAsteroide.NULO) //Chequeo de si no es el asteroide mas chico
                    manager.fabricarMiniAsteroides(3, tamanioAnterior, getPosicion(),((TgcBoundingSphere)colision.getBoundingBox()).Radius);
        }

        public static FormatoAsteroide elegirAsteroidePor(TamanioAsteroide tamanio)
        {
            switch (tamanio) //Factory de asteroides en funcion al enum
            {
                case TamanioAsteroide.MUYGRANDE: return new AsteroideMuyGrande();
                case TamanioAsteroide.GRANDE: return new AsteroideGrande();
                case TamanioAsteroide.MEDIANO: return new AsteroideMediano();
                case TamanioAsteroide.CHICO: return new AsteroideChico();
            }
            return null;
        }
    }

    public enum TamanioAsteroide { MUYGRANDE, GRANDE, MEDIANO, CHICO, NULO }

    //Extract Class que se encarga de setear la forma del asteroide
    public interface FormatoAsteroide
    {
        float getMasa(); //En toneladas
        Vector3 getVolumen(); //En realidad un factor de escalado
        float getVelocidad();
        TamanioAsteroide tamanioAnterior(); //Para saber cual es el asteroide mas chico siguiente. Si es el mas chico, es TamanioAsteroide.NULO
        float vidaInicial();
    }

    public class AsteroideMuyGrande : FormatoAsteroide
    {
        private float masa = 20000;
        private float longitud = 200;
        public float getMasa() { return masa; }
        public Vector3 getVolumen() { return new Vector3(longitud, longitud, longitud); }
        public float getVelocidad() { return 8; }
        public TamanioAsteroide tamanioAnterior() { return TamanioAsteroide.GRANDE; }
        public float vidaInicial() { return 10000; }
    }

    public class AsteroideGrande : FormatoAsteroide
    {
        private float masa = 8000;
        private float longitud = 100;
        public float getMasa() { return masa; }
        public Vector3 getVolumen() { return new Vector3(longitud, longitud, longitud); }
        public float getVelocidad() { return 13; }
        public TamanioAsteroide tamanioAnterior() { return TamanioAsteroide.MEDIANO; }
        public float vidaInicial() { return 5000; }
    }

    public class AsteroideMediano : FormatoAsteroide
    {
        private float masa = 5000;
        private float longitud = 50;
        public float getMasa() { return masa; }
        public Vector3 getVolumen() { return new Vector3(longitud, longitud, longitud); }
        public float getVelocidad() { return 18; }
        public TamanioAsteroide tamanioAnterior() { return TamanioAsteroide.CHICO; }
        public float vidaInicial() { return 2000; }
    }

    public class AsteroideChico : FormatoAsteroide
    {
        private float masa = 200;
        private float longitud = 25;
        public float getMasa() { return masa; }
        public Vector3 getVolumen() { return new Vector3(longitud, longitud, longitud); }
        public float getVelocidad() { return 24; }
        public float vidaInicial() { return 500; }
        public TamanioAsteroide tamanioAnterior() { return TamanioAsteroide.NULO; }
    }
}
