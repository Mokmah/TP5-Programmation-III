using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
//*************************************************************************************************************
//
//
//                              William Garneau, 2017-11-20, Fichiers binaires
//
//
//*************************************************************************************************************
namespace FichierIndexeA16
{
    public partial class frmFichierIndexeA16 : Form
    {
        int m_NbreEnrg;
        SIndex[] m_Index;
        FileStream m_FSE;
        BinaryReader m_BRE;
        BinaryWriter m_BWE;
        SEmploye[] m_Employe;

        public frmFichierIndexeA16()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            FileStream FichierIndex = new FileStream(Directory.GetCurrentDirectory() + @"\Index.ndx", FileMode.OpenOrCreate, FileAccess.Read, FileShare.None);
            BinaryReader br = new BinaryReader(FichierIndex);
            if (FichierIndex.Length != 0)
            {
                string signature = br.ReadString();
                m_NbreEnrg = br.ReadInt32();
                if (signature != "Index Employés")
                {
                    MessageBox.Show("Problème d'index");
                    this.Close();
                }
                m_Index = new SIndex[m_NbreEnrg + 50];
                for (int i = 0; i < m_NbreEnrg; i++)
                {
                    m_Index[i].ADetruire = br.ReadBoolean();
                    m_Index[i].Cle = br.ReadInt32();
                    m_Index[i].Position = br.ReadInt64();
                }
            }
            else
                m_Index = new SIndex[50];
            br.Close();
            FichierIndex.Close();
            m_FSE = new FileStream(Directory.GetCurrentDirectory() + @"\Employes.don", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            m_BRE = new BinaryReader(m_FSE);
            m_BWE = new BinaryWriter(m_FSE);
            if (m_FSE.Length != 0)
            {
                m_Employe = new SEmploye[m_NbreEnrg + 50];
                for (int i = 0; i < m_NbreEnrg; i++)
                {
                    m_Employe[i].NoEmp = m_BRE.ReadInt32();
                    m_Employe[i].Nom = m_BRE.ReadString();
                    m_Employe[i].Salaire = m_BRE.ReadDouble();
                }
            }
            else
            {
                m_Employe = new SEmploye[50];
            }

        }

        private void frmFichierIndexe_FormClosed(object sender, FormClosedEventArgs e)
        {
            FileStream FichierIndex = new FileStream(Directory.GetCurrentDirectory() + @"\Index.ndx", FileMode.Truncate, FileAccess.Write, FileShare.None);
            BinaryWriter bw = new BinaryWriter(FichierIndex);
            string signature = "Index Employés";

            FichierIndex.Seek(0, SeekOrigin.Begin);
            bw.Write(signature);
            bw.Write(m_NbreEnrg);

            for (int i = 0; i < m_NbreEnrg; i++)
            {
                bw.Write(m_Index[i].ADetruire);
                bw.Write(m_Index[i].Cle);
                bw.Write(m_Index[i].Position);
            }

            FichierIndex.Close();
            bw.Close();


        }
        //******************************************************************
        //
        //  Ce bouton est là pour se déboguer
        //  à retirer pour la remise du projet
        //
        //*******************************************************************
        private void btnAfficher_Click(object sender, EventArgs e)
        {
            string s = "";

            for (int i = 0; i < m_NbreEnrg; i++)
            {
                s += m_Index[i].Cle.ToString() + " " + (m_Index[i].Position).ToString() + "\n";
            }
            MessageBox.Show(s);
        }
    }
}
