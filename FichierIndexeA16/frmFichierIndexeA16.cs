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

        public frmFichierIndexeA16()
        {
            InitializeComponent();
            btnModifier.Enabled = false;
            btnSupprimer.Enabled = false;
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
            m_FSE.Close();
            FichierIndex.Close();
            bw.Close();
        }

        private void btnAjouter_Click(object sender, EventArgs e)
        {
            int NoEmploye;
            string NomEmploye;
            double SalEmploye;
            SIndex Ind = new SIndex();
            bool ConversionNo, ConversionSal;

            m_BWE = new BinaryWriter(m_FSE);

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
            int i;
            long pos = GetEmploye(NoEmploye, out i);
            bool flag = pos != -1;
            if (i == -1)
            {
                MessageBox.Show("Vous ne pouvez pas ajouter cet employé puisqu'il est déjà existant.", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //Association a la structure
            SEmploye Employe = new SEmploye();
            Employe.NoEmp = NoEmploye;
            Employe.Nom = NomEmploye;
            Employe.Salaire = SalEmploye;

            m_FSE.Seek(0, SeekOrigin.End);
            long Pointer = m_FSE.Position; // Savoir la position initiale de la struct
            Employe.Ecrire(m_FSE, m_BWE);

            //Associate Index
            m_NbreEnrg++;
            m_Index[m_NbreEnrg - 1].Position = Pointer;
            m_Index[m_NbreEnrg - 1].Cle = NoEmploye;
            m_Index[m_NbreEnrg - 1].ADetruire = false;


            MessageBox.Show("Votre nouvel employé a été enregistré avec succès.", "Félicitations !",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnRechercher_Click(object sender, EventArgs e)
        {
            int i = 0, NoEmploye;
            m_BRE = new BinaryReader(m_FSE);
            SEmploye PEmp = new SEmploye();
            bool ConversionNo = Int32.TryParse(txtNumero.Text, out NoEmploye);
            if (!ConversionNo)//Savoir si on a pu trouver le dossier associé au numéro.
            {
                MessageBox.Show("Vous devez entrer un numéro d'employé valide pour le rechercher.", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            while (i < m_NbreEnrg && m_Index[i].Cle != NoEmploye)
                i++;
            if (i == m_NbreEnrg && m_Index[i].Cle != NoEmploye)
            {
                MessageBox.Show("L'employé que vous essayez de trouver n'existe pas.", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnModifier.Enabled = false;
                btnSupprimer.Enabled = false;
                txtNom.Text = "";
                txtSalaire.Text = "";
                return;
            }
            else
            {
                long Position = m_Index[i].Position;
                PEmp.Lire(Position, m_FSE, m_BRE);
                Affichage(PEmp);
                btnModifier.Enabled = true;
                btnSupprimer.Enabled = true;
            }
        }

        private void btnSupprimer_Click(object sender, EventArgs e)
        {
            int NoEmploye, Index;
            bool ConversionNo;

            //Conversion des textbox dans les variables
            ConversionNo = Int32.TryParse(txtNumero.Text, out NoEmploye);

            /// Validate
            if (!ConversionNo)
            {
                MessageBox.Show("Vous devez entrer un numéro d'employé valide pour l'effacer", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            long Position = GetEmploye(NoEmploye, out Index);
            if (Index == -1)
            {
                MessageBox.Show("L'employé que vous essayez d'effacer n'existe pas.", "Erreur",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            //Déterminer que la structure à cet indice est à détruire.
            m_Index[Index].ADetruire = true;
            MessageBox.Show("Vous devrez compresser le fichier pour finaliser la suppression.", "Avertissement",
                    MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private void btnModifier_Click(object sender, EventArgs e)
        {
            SEmploye emp = default(SEmploye);
            int NoEmploye, i = 0;
            double SalEmploye;
            bool Conversion;
            //Conversion UI en variables
            Conversion = Int32.TryParse(txtNumero.Text, out NoEmploye);
            Conversion = Double.TryParse(txtSalaire.Text, out SalEmploye);

            /// Validate
            if (!Conversion)
            {
                MessageBox.Show("Vous devez entrer des informations valides pour faire des modifications", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            while (NoEmploye != m_Index[i].Cle)
            {
                i++;
                if (i == m_NbreEnrg)
                {
                    MessageBox.Show("L'employé que vous essayez d'effacer n'existe pas.", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (NoEmploye == m_Index[i].Cle && m_Index[i].ADetruire == true)
                {
                    MessageBox.Show("Cet employé a été supprimé.", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            emp.NoEmp = NoEmploye;
            emp.Nom = txtNom.Text;
            emp.Salaire = SalEmploye;

            //Écrire le nouvel employé à la fin du fichier
            m_FSE.Seek(0, SeekOrigin.End);
            emp.Ecrire(m_FSE, m_BWE);

            //Ajouter au tableau d'indice après
            m_Index[i].Position = i;
            m_Index[i].Cle = NoEmploye;
            m_Index[i].ADetruire = false;

            MessageBox.Show("Vos changements ont été effectués avec succès.", "Félicitations !",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            return;
        }

        private void btnCompresser_Click(object sender, EventArgs e)
        {
            Save_(sender, e);
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
            SEmploye emp = default(SEmploye);
            int pos = 0;
            m_FSE.Seek(0, SeekOrigin.Begin);
            for (int i = 0; pos < m_FSE.Length; i++)
            {
                emp.Lire(pos, m_FSE, m_BRE);
                s += "Numéro d'employé : " + m_Index[i].Cle.ToString() + "\tNom : " + (emp.Nom).ToString() + "\tSalaire : " + (emp.Salaire).ToString() + "\n";
                pos = (int)this.m_FSE.Position;
            }
            MessageBox.Show(s);
        }
        #endregion

        #region Methodes
        private void Affichage(SEmploye Employe)
        {
            txtNumero.Text = Employe.NoEmp.ToString();
            txtNom.Text = Employe.Nom;
            txtSalaire.Text = Employe.Salaire.ToString();
        }

        private long GetEmploye(int NoEmploye,out int Index)
        {
            Index = 0;
            while (Index < m_NbreEnrg && NoEmploye != m_Index[Index].Cle)
            {
                Index++;
            }
            bool FinFichier = Index == m_NbreEnrg;
            long retour;
            if (FinFichier)
            {
                retour = -1;
            }
            else
            {
                retour = m_Index[Index].Position;
            }
            return retour;
        }

        private void Save_(object sender, EventArgs e)
        {
            int NbEmploye = 0;
            SEmploye emp = default(SEmploye);
            FileStream Temporaire = new FileStream(Directory.GetCurrentDirectory() + @"\Replacement.tmp",
                FileMode.Create, FileAccess.Write, FileShare.None);
            BinaryWriter BWE = new BinaryWriter(Temporaire);
            for (int i = 0; i < m_NbreEnrg; i++) // Transférer tout le fichier d'index dans le nouveau fichier temporaire
            {
                bool Flag = !m_Index[i].ADetruire;
                if (Flag)
                {
                    emp.Lire(m_Index[i].Position, m_FSE, m_BRE);
                    m_Index[i].Position = Temporaire.Position;
                    emp.Ecrire(Temporaire, BWE);
                    NbEmploye++;
                }
            }
            //On ferme tout
            m_BRE.Close();
            m_FSE.Close();
            m_BWE.Close();
            BWE.Close();
            Temporaire.Close();

            File.Replace("Replacement.tmp", "Employes.don", "Employe.bak");
            //On réécrit les bonnes valeurs qui doivent êtres présentes dans le fichier
            FileStream FichierIndex = new FileStream(Directory.GetCurrentDirectory() + @"\Index.ndx", FileMode.Truncate, FileAccess.Write, FileShare.None);
            BinaryWriter bwe = new BinaryWriter(FichierIndex);
            string Signature = "Index Employés";
            FichierIndex.Seek(0, SeekOrigin.Begin);

            bwe.Write(Signature);
            bwe.Write(NbEmploye);

            //Réécriture
            for (int i = 0; i < m_NbreEnrg; i++) // Transférer tout le fichier d'index dans le nouveau fichier temporaire
            {
                bool Flag = !m_Index[i].ADetruire;
                if (Flag)
                {
                    bwe.Write(m_Index[i].ADetruire);
                    bwe.Write(m_Index[i].Cle);
                    bwe.Write(m_Index[i].Position);
                }
            }
            FichierIndex.Close();
            bwe.Close();
            this.Form1_Load(sender, e);
        }

        private bool SameKeyValidation(int Indice)
        {
            for (int i = 0; i < m_NbreEnrg; i++)
            {
                if (m_Index[i].Cle == Indice)
                {
                    return false;
                }
            }
            return true;
        }

        private bool ToSuppressValidation(int Indice)
        {
            for (int i = 0; i < m_NbreEnrg; i++)
            {
                if (m_Index[i].ADetruire == true)
                {
                    return false;
                }
            }
            return true;
        }
        #endregion
    }
}
