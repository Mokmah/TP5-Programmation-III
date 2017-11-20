using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
//**********************************************************************************************
//
//  Structure représentant un employé qui sera convervé dans un fichier maître d'employé
//
//  organisé en accès indexé. L'index est conservé dans un fichier d'index
//
//  Département d'informatique Cégep Lévis-Lauzon
//
//  Automne 2016
//
//********************************************************************************************** 

namespace FichierIndexeA16
{
    public struct SEmploye
    {
        private int m_NoEmp;
        private string m_Nom;
        private double m_Salaire;

        public int NoEmp
        {
            get { return m_NoEmp; }

            set
            {
                if (value > 0)
                    m_NoEmp = value;
            }
        }
        public string Nom
        {
            get { return m_Nom; }
            set { m_Nom = value; }
        }
        public double Salaire
        {
            get { return m_Salaire; }
            set
            {
                if (value > 0)
                    m_Salaire = value;
            }
        }
        public void Lire(long Position, FileStream fs, BinaryReader br)
        {
            fs.Seek(Position, SeekOrigin.Begin);
            NoEmp = br.ReadInt32();
            Nom = br.ReadString();
            Salaire = br.ReadDouble();
        }
        public void Ecrire(FileStream fs, BinaryWriter bw)
        {
            bw.Write(this.NoEmp);
            bw.Write(this.Nom);
            bw.Write(this.Salaire);
        }
    }
}
