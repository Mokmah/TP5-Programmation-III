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
        #region Events
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

            //Mettre les données qui sont dans le fichier dans la struct SEmploye
            if (m_FSE.Length != 0)
            {//Lire le dernier employe dans le fichier
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

        private void btnAjouter_Click(object sender, EventArgs e)
        {
            int NoEmploye;
            string NomEmploye;
            double SalEmploye;
            bool ConversionNo, ConversionSal;

            //Conversion des textbox dans les variables
            NomEmploye = txtNom.Text;
            ConversionNo = Int32.TryParse(txtNumero.Text, out NoEmploye);
            ConversionSal = Double.TryParse(txtSalaire.Text, out SalEmploye);

            /// Validate
            if (!ConversionNo)
            {
                MessageBox.Show("Vous devez entrer un numéro d'employé valide pour l'indexer.", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!ConversionSal)
            {
                MessageBox.Show("Le salaire de l'employé que vous essayez d'enregistrer est invalide.", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (NomEmploye == "")
            {
                MessageBox.Show("Le nom de l'employé que vous essayez d'enregistrer est invalide.", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            SEmploye Employe = new SEmploye();
            Employe.NoEmp = NoEmploye;
            Employe.Nom = NomEmploye;
            Employe.Salaire = SalEmploye;
            if (!IsListed(NomEmploye))
            {
                Save_(Employe);
            }           
        }

        private void btnRechercher_Click(object sender, EventArgs e)
        {
            int NoEmploye;
            bool ConversionNo = Int32.TryParse(txtNumero.Text, out NoEmploye);
            if (!ConversionNo)//Savoir si on a pu trouver le dossier associé au numéro.
            {
                MessageBox.Show("Vous devez entrer un numéro d'employé valide pour le rechercher.", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            bool IsItFound;
            SEmploye Employe = IsFound(NoEmploye, out IsItFound);
            if (IsItFound)
            {
                //Affichage des données dans les textbox
                Affichage(Employe);
            }
            else
            {
                MessageBox.Show("L'employé que vous essayez de trouver n'existe pas.", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
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
        #endregion

        #region Methodes

        private void Save_(SEmploye Employe)
        {
            SIndex Index = new SIndex();
            Index.ADetruire = false;
            Index.Cle = Employe.NoEmp;
            Index.Position = m_NbreEnrg;//Bug
            m_Employe[m_NbreEnrg] = Employe;
            m_Index[m_NbreEnrg] = Index;
            m_NbreEnrg++;
        }

        private bool IsListed(string NomEmploye)
        {
            try
            {
                SEmploye Employe = m_Employe.First(f => f.Nom == NomEmploye);
                SIndex Index = m_Index.First(f => f.Cle == Employe.NoEmp);
            }
            catch (InvalidOperationException Nope)
            {
                return false;
            }
            return true;
        }

        private SEmploye IsFound(int NoEmploye, out bool IsFound)
        {
            SEmploye Employe = new SEmploye();
            SIndex Index;
            IsFound = false;
            try
            {
                Index = m_Index.First(f => f.Cle == NoEmploye);
            }
            catch(InvalidOperationException Nope)
            {
                return Employe;
            }
            if (!Index.ADetruire)
            {
                Employe = m_Employe.First(f => f.NoEmp == Index.Cle);
                IsFound = true;
            }
            return Employe;
        }

        private void Affichage(SEmploye Employe)
        {
            txtNumero.Text = Employe.NoEmp.ToString();
            txtNom.Text = Employe.Nom;
            txtSalaire.Text = Employe.Salaire.ToString();
        }

        #endregion

    }
}
