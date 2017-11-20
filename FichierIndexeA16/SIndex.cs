using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//**********************************************************************************************
//
//  Structure représentant l'index d'un employé qui sera convervé dans un fichier maître d'employé
//
//  organisé en accès indexé. L'index est conservé dans un fichier d'index.
//
//  Département d'informatique Cégep Lévis-Lauzon
//
//  Automne 2016
//
//********************************************************************************************** 

namespace FichierIndexeA16
{
    public struct SIndex
    {

        private bool m_ADetruire;
        private int m_Cle;
        private long m_Position;

        public int Cle
        {
            get { return m_Cle; }
            set { m_Cle = value; }
        }
        public long Position
        {
            get { return m_Position; }
            set { m_Position = value; }
        }
        public bool ADetruire
        {
            get { return m_ADetruire; }
            set { m_ADetruire = value; }
        }
    }
}
