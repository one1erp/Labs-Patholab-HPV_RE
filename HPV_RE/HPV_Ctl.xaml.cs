using System;
using System.Collections.ObjectModel;
using System.Data.Entity;


using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Patholab_Common;

using Patholab_DAL_V1;
using CheckBox = System.Windows.Controls.CheckBox;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;

//using MessageBox = System.Windows.Controls.MessageBox;


namespace HPV_RE
{
    /// <summary>
    /// Interaction logic for WpfShipmentCtl.xaml
    /// </summary>
    public partial class HPV_Ctl : System.Windows.Controls.UserControl, HPV_Control
    {
        #region Ctor

        public HPV_Ctl()
        {
            InitializeComponent ( );
            
        }
        public HPV_Ctl(HPV_RE_Manager controller)
        {
            InitializeComponent();
            GridHPV.Visibility = Visibility.Visible;
            this.Controller = controller;
            this.DataContext = this;


            InitialMode();

        }

        #endregion

        #region CONSTANTS

        private const string MboxHeader = "הזנת תוצאות - HPV";
        private const string HpvPos = "HPV Pos";
        private const string HpvNeg = "HPV Neg";
        private const string HpvOthers = "HPV Others";
        private const string HpvRemark = "HPV Remark";


        private const string Hr = "HR";
        private const string HrType = "HR Type";
        private const string Lr = "LR";
        private const string LrType = "LR Type";
        private const string UnknownType = "Unknown Type";
        private const string ProntoName = "Pronto Name";

        #endregion

        #region Local fields

     //   public bool DEBUG;
        public List<PHRASE_ENTRY> ListLowRisk { get; set; }
        public List<PHRASE_ENTRY> ListHighRisk { get; set; }
        public HPV_RE_Manager Controller
        {
            get;
            set;
        }
        public ObservableCollection<RiskType> ListHighRisk2 { get; set; }
        public ObservableCollection<RiskType> ListLowRisk2 { get; set; }

        #endregion

        #region Initilaize and clear

        public void InitialMode()
        {
            cbUnKnown.IsChecked = false;
            cbHR.IsChecked = false;
            cbLR.IsChecked = false;
            rbNeg.IsChecked = false;
            rbPos.IsChecked = false;
            rbOther.IsChecked = false;
            cbUnKnown.IsChecked = false;
            rbNeg.IsEnabled = false;
            rbPos.IsEnabled = false;
            rbOther.IsEnabled = false;
            txtPronto.Clear();
            txtRemark.Clear();

            if (ListHighRisk2 != null) ListHighRisk2.Foreach(x => x.Checked = false);
            if (ListLowRisk2 != null) ListLowRisk2.Foreach(x => x.Checked = false);
            EditMode(false);
        }

        public void ClearScreen()
        {
            InitialMode();
        }

        public void EditMode(bool p)
        {
            rbNeg.IsEnabled = p;
            rbPos.IsEnabled = p;
            rbOther.IsEnabled = p;
            txtRemark.IsEnabled = p;
            txtPronto.IsEnabled = p;
        }

        public void SetList()
        {
            ListLowRisk2 = (from item in ListLowRisk select new RiskType() { RiskVal = item.PHRASE_NAME, Checked = false }).ToObservableCollection();
            ListHighRisk2 = (from item in ListHighRisk select new RiskType() { RiskVal = item.PHRASE_NAME, Checked = false }).ToObservableCollection();
        }

        #endregion

        #region Logic



        #region Load sdg new
        public void LoadRequest(Request request)
        {
            EditMode("VPC".Contains(request.Status));

            txtPronto.Text = request.PontoNum;

            foreach (WResult r in request.Results)
            {
                switch (r.Name)
                {

                    case HpvPos:
                        rbPos.IsChecked = (r.Value == "T" || r.Value == "True");
                        break;
                    case HpvNeg:
                        rbNeg.IsChecked = (r.Value == "T" || r.Value == "True");
                        break;
                    case HpvOthers:
                        rbOther.IsChecked = (r.Value == "T" || r.Value == "True");
                        break;
                    case HpvRemark:
                        txtRemark.Text = r.Value;
                        break;
                    case Hr:
                        cbHR.IsChecked = (r.Value == "T" || r.Value == "True");
                        break;
                    case HrType:
                        SetRiskType(ListHighRisk2, r.Value);
                        break;
                    case Lr:
                        cbLR.IsChecked = (r.Value == "T" || r.Value == "True");
                        break;
                    case LrType:
                        SetRiskType(ListLowRisk2, r.Value);
                        break;
                    case UnknownType:
                        cbUnKnown.IsChecked = (r.Value == "T" || r.Value == "True");
                        break;
                    case ProntoName:
                        txtPronto.Text = r.Value;
                        break;
                }
            }
        }
        #endregion

        #region Save sdg new



        public void Save(Request request)
        {

            foreach (WResult wr in request.Results)
            {
                wr.Status = "C";
                switch (wr.Name)
                {

                    case HpvPos:
                        wr.Value = rbPos.IsChecked == true ? "True" : "False";
                        break;
                    case HpvNeg:
                        wr.Value = rbNeg.IsChecked == true ? "True" : "False";
                        break;
                    case HpvOthers:
                        wr.Value = rbOther.IsChecked == true ? "True" : "False";
                        break;
                    case HpvRemark:
                        wr.Value = txtRemark.Text;
                        break;
                    case Hr:
                        wr.Value = cbHR.IsChecked == true ? "True" : "False";
                        break;
                    case HrType:
                        wr.Value = GetRiskType(ListHighRisk2);
                        break;
                    case Lr:
                        wr.Value = cbLR.IsChecked == true ? "True" : "False";
                        break;
                    case LrType:
                        wr.Value = GetRiskType(ListLowRisk2);

                        break;
                    case UnknownType:
                        wr.Value = cbUnKnown.IsChecked == true ? "True" : "False";
                        break;
                    case ProntoName:
                        wr.Value = txtPronto.Text;
                        break;
                }
            }
            request.PontoNum = txtPronto.Text;

            InitialMode();
        }

        public bool IsValid4Save()
        {
            if (rbNeg.IsChecked == false && rbPos.IsChecked == false && rbOther.IsChecked == false) // לא נבחרה תוצאה
                return false;

            if (rbPos.IsChecked == true) //נבחרה תוצאה חיובית
            {
                List<CheckBox> checkBoxes = new List<CheckBox>() { cbHR, cbLR, cbUnKnown };
                if (checkBoxes.All(x => x.IsChecked != true))
                {
                    //לא נבחרה סוג תוצאה חיובית
                    return false;
                }

                if (cbHR.IsChecked == true && string.IsNullOrEmpty(GetRiskType(ListHighRisk2)))
                    return false;
                if (cbLR.IsChecked == true && string.IsNullOrEmpty(GetRiskType(ListLowRisk2)))
                    return false;
            }
            return true;

        }



        #endregion

        private void SetRiskType(ObservableCollection<RiskType> wp, string val)
        {

            foreach (RiskType riskType in wp)
            {
                riskType.Checked = false;
            }
            if (val != null)
            {
                var splited = val.Split(';');
                foreach (string s in splited)
                {
                    var po = wp.FirstOrDefault(x => x.RiskVal == s);
                    if (po != null) po.Checked = true;
                }
            }
        }

        private string GetRiskType(ObservableCollection<RiskType> wp)
        {

            var selectedVal = wp.Where(x => x.Checked).Select(x => x.RiskVal);
            string res = null;
            foreach (string s in selectedVal)
            {
                res += s + ";";
            }
            return res;

        }



        #endregion

        #region UI events

        private void To_english(object sender, RoutedEventArgs e)
        {
            zLang.English();
        }

        private void ToHebrew(object sender, RoutedEventArgs e)
        {
            zLang.Hebrew();
        }

        #endregion


    }
}



