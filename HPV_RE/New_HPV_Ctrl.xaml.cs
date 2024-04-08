using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Patholab_Common;

using Patholab_DAL_V1;
using System.Collections.ObjectModel;
using Patholab_XmlService;
using LSSERVICEPROVIDERLib;

namespace HPV_RE
{
    /// <summary>
    /// Interaction logic for New_HPV_Ctrl.xaml
    /// </summary>
    public partial class New_HPV_Ctrl : UserControl, HPV_Control
    {
        public List<PHRASE_ENTRY> hpvResults;
        private ResultEntryXmlHandler resultXml;
        INautilusServiceProvider sp;
        SDG sdg;

        public New_HPV_Ctrl()
        {

            InitializeComponent();

        }

        public New_HPV_Ctrl(INautilusServiceProvider serviceProvider, SDG _sdg)
        {
            sp = serviceProvider;
            sdg = _sdg;
            InitializeComponent();

        }

        #region Local fields

        public List<PHRASE_ENTRY> ListLowRisk
        {
            get;
            set;
        }
        public List<PHRASE_ENTRY> ListHighRisk
        {
            get;
            set;
        }
        public HPV_RE_Manager Controller
        {
            get;
            set;
        }
        public ObservableCollection<RiskType> ListHighRisk2
        {
            get;
            set;
        }
        public ObservableCollection<RiskType> ListLowRisk2
        {
            get;
            set;
        }

        #endregion

        #region Initilaize and clear

        public void InitialMode()
        {
            comboBoxHPV16.SelectedIndex = -1;
            comboBoxHPV18.SelectedIndex = -1;
            comboBoxHPVOHR.SelectedIndex = -1;
            txtRemark.Clear();
            datePicker.Value = null;
            checkBoxLBC.IsChecked = false;
            EditMode(false);
        }

        public void ClearScreen()
        {
            InitialMode();
        }


        public void EditMode(bool p)
        {
            comboBoxHPV16.IsEnabled = p;
            comboBoxHPV18.IsEnabled = p;
            comboBoxHPVOHR.IsEnabled = p;
            txtRemark.IsEnabled = p;
            datePicker.IsEnabled = p;
        }

        #endregion

        #region Logic

        #region Load sdg new

        public void LoadRequest(Request request)
        {
            bool isHpvPos = false;
            int index;
            EditMode("VPC".Contains(request.Status));

            foreach (WResult r in request.Results)
            {
                if (r.Value == "20")
                {
                    isHpvPos = true;
                }
                index = ListLowRisk.FindIndex(x => x.PHRASE_NAME == r.Value);
                switch (r.Name)
                {

                    case "HPV16":
                        comboBoxHPV16.SelectedIndex = index;
                        break;
                    case "HPV18":
                        comboBoxHPV18.SelectedIndex = index;
                        break;
                    case "HPVOHR":
                        comboBoxHPVOHR.SelectedIndex = index;
                        break;
                    case "HPV Remark":
                        txtRemark.Text = r.Value;
                        break;
                    case "HPV Cobas Date":
                        if (r.Value != null)
                        {
                            try
                            {
                                DateTime date = DateTime.Parse(r.Value);
                                datePicker.Value = date;
                            }
                            catch (Exception e)
                            {
                            }
                            
                        }    
                        break;
                    case "HPV TYPE":
                        try
                        {
                            if (r.Value != null && ListHighRisk.Find(x => x.PHRASE_NAME == r.Value).PHRASE_DESCRIPTION != null)
                            {
                                calculatedResult.Content = ListHighRisk.Find(x => x.PHRASE_NAME == r.Value).PHRASE_DESCRIPTION;
                            }
                            if (isHpvPos)
                            {
                                calculatedResult.Foreground = Brushes.Red;
                            }
                            else
                            {
                                calculatedResult.Foreground = Brushes.Green;
                            }
                        }
                        catch (Exception e)
                        {
                            calculatedResult.Content = "No Result";
                        }

                        break;
                }
            }
            List<TEST> tests = sdg.SAMPLEs.FirstOrDefault().ALIQUOTs.FirstOrDefault().TESTs.ToList();
            if (tests.Count >= 4)
            {
                checkBoxLBC.IsChecked = true;
                checkBoxLBC.IsEnabled = false;
            }

        }

        #endregion

        #region Save sdg new

        public void Save(Request request)
        {
            resultXml = null;
            long testId = sdg.SAMPLEs.FirstOrDefault().ALIQUOTs.FirstOrDefault().TESTs.FirstOrDefault().TEST_ID;
            PHRASE_ENTRY entry;
            foreach (WResult wr in request.Results)
            {

                wr.Status = "C";

                switch (wr.Name)
                {
                    case "HPV16":
                        entry = (PHRASE_ENTRY)comboBoxHPV16.SelectedValue;
                        wr.Value = entry != null ? entry.PHRASE_NAME : "";
                        break;
                    case "HPV18":
                        entry = (PHRASE_ENTRY)comboBoxHPV18.SelectedValue;
                        wr.Value = entry != null ? entry.PHRASE_NAME : "";
                        break;
                    case "HPVOHR":
                        entry = (PHRASE_ENTRY)comboBoxHPVOHR.SelectedValue;
                        wr.Value = entry != null ? entry.PHRASE_NAME : "";
                        break;
                    case "HPV Remark":
                        wr.Value = txtRemark.Text;
                        break;
                    case "HPV Cobas Date":
                        try
                        {
                            wr.Value = datePicker.Value.ToString();
                        }
                        catch (Exception e)
                        {
                        }
                        
                        break;
                    case "HPV TYPE":
                        break;
                }

                if (resultXml == null)
                {
                    resultXml = new ResultEntryXmlHandler(sp);
                    resultXml.CreateResultEntryXml(testId, wr.ResultId, wr.Value);
                }
                else
                {
                    if (wr.Name != "HPV TYPE")
                    {
                        resultXml.AddResultEntryElem(wr.ResultId.ToString(), wr.Value);
                    }
                }
            }
            if ((bool)checkBoxLBC.IsChecked)
            {
                FireEventXmlHandler authorizeSdg = new FireEventXmlHandler(sp, "Add LBC Test");
                long aliquotId = sdg.SAMPLEs.FirstOrDefault().ALIQUOTs.FirstOrDefault().ALIQUOT_ID;
                authorizeSdg.CreateFireEventXml("ALIQUOT", aliquotId, "Add LBC Test");
                bool res = authorizeSdg.ProcssXmlWithOutSave();

                //if (!res)
                //{
                //    MessageBox.Show(authorizeSdg.ErrorResponse);
                //    return;
                //}
                //else
                //{
                //    MessageBox.Show("Succeeded");
                //}
            }
            resultXml.ProcssXml();
            InitialMode();
        }
        //Check if I need this function, maybe there are times that we can not save.
        public bool IsValid4Save()
        {
            return true;
        }

        #endregion

        #endregion


        //Not needed for now.
        public void SetList()
        {
            comboBoxHPV16.ItemsSource = ListLowRisk;
            comboBoxHPV18.ItemsSource = ListLowRisk;
            comboBoxHPVOHR.ItemsSource = ListLowRisk;

            comboBoxHPV16.DisplayMemberPath = "PHRASE_DESCRIPTION";
            comboBoxHPV18.DisplayMemberPath = "PHRASE_DESCRIPTION";
            comboBoxHPVOHR.DisplayMemberPath = "PHRASE_DESCRIPTION";
        }

        private void ToHebrew(object sender, RoutedEventArgs e)
        {
            zLang.Hebrew();
        }


    }
}
