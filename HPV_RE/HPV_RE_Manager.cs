using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows.Forms;

using Patholab_DAL_V1;
using Patholab_XmlService;
using Patholab_Common;
using LSSERVICEPROVIDERLib;


namespace HPV_RE
{





    public partial class HPV_RE_Manager : UserControl
    {
        #region Members
        public bool DEBUG = true;
        public DataLayer _dal;
        public INautilusServiceProvider _sp;
        private Request _request;
        private SDG _sdg;
        private List<RESULT> _hpvResults;
        public List<PHRASE_ENTRY> ListLowRisk { get; set; }
        public List<PHRASE_ENTRY> ListHighRisk { get; set; }
        public string GetSdgRes { get; set; }
        HPV_Control view;
        public static bool isHpvRegular;
        //HPV_Ctl hpvView;
        //New_HPV_Ctrl hpvViewNew;
        TEST cobasHpvTest;
        #endregion

        #region Ctor
        public HPV_RE_Manager ( )
        {
            InitializeComponent ( );
        }
        #endregion

        #region New
        public void Init(DataLayer dal, INautilusServiceProvider sp)
        {

            //hpvView = new HPV_Ctl(this);
            _dal = dal;
            _sp = sp;
            //elementHost1.Child = hpvView;
            //hpvView.ListLowRisk = _dal.GetPhraseEntries("HPV Low Risk").ToList();
            //hpvView.ListHighRisk = _dal.GetPhraseEntries("HPV High Risk").ToList();
            //hpvView.SetList();
            
        }
        
        public bool Is_HPV_request { get; set; }
        
        public bool LoadHPVrequest(SDG p)
        {
            //_dal.ReloadEntity(p);
            this._sdg = p;
            //Seperate between the old HPV and the new Hpv through the workflow name
            cobasHpvTest = null;
            foreach (SAMPLE s in p.SAMPLEs)
            {
                foreach (ALIQUOT a in s.ALIQUOTs)
                {
                    foreach (TEST t in a.TESTs)
                    {
                        if (t.NAME == "Cobas HPV")
                        {
                            cobasHpvTest = t;
                        }
                    }
                    
                }
            }
            if (cobasHpvTest != null)
            {
                view = new New_HPV_Ctrl(_sp, _sdg);
                elementHost1.Child = (New_HPV_Ctrl)view;
                view.ListLowRisk = _dal.GetPhraseEntries("HPV Cobas").ToList();
                view.ListHighRisk = _dal.GetPhraseEntries("HPV Final Res 2 Cortex Code").ToList();
                isHpvRegular = false;
            }
            else
            {
                view = new HPV_Ctl(this);
                elementHost1.Child = (HPV_Ctl)view;
                view.ListLowRisk = _dal.GetPhraseEntries("HPV Low Risk").ToList();
                view.ListHighRisk = _dal.GetPhraseEntries("HPV High Risk").ToList();
                isHpvRegular = true;
            }
            view.SetList();
            Is_HPV_request = false;
            _request = null;
            
            var hpvTest = (from aliq in _dal.FindBy<TEST>(al => al.ALIQUOT.SAMPLE.SDG_ID == _sdg.SDG_ID
                                                              && (al.NAME == "HPV" || al.NAME == "Cobas HPV")
                                                              && "ACVP".Contains(al.STATUS))
                                          .Include(a => a.ALIQUOT.ALIQUOT_USER)
                           select new
                           {
                               PontoNum = aliq.ALIQUOT.ALIQUOT_USER.U_EXTERNAL_LAB_NUM,
                               aliquotId = aliq.ALIQUOT_ID.Value
                           })
                            .FirstOrDefault();
            if (hpvTest != null)
            {
                this._request = new Request
                {

                    Status = _sdg.STATUS,
                    Results = GetResults(_sdg.SDG_ID,isHpvRegular),
                    PontoNum = hpvTest.PontoNum,
                    AliquotId = hpvTest.aliquotId
                };

                view.LoadRequest(_request);
            }
            Is_HPV_request = hpvTest != null;
            return Is_HPV_request;
        }
       
        public void ClearScreen()
        {
            if (view != null)
            {
                view.ClearScreen();
            }
            
        }
        #endregion
        #region LOGIC

        private List<WResult> GetResults(long sdgId, bool flag)
        {
            if (flag)
            {
                _hpvResults = (from rl in _dal.FindBy<RESULT>
                                        (x => x.TEST.ALIQUOT.SAMPLE.SDG_ID == sdgId)
                               where rl.TEST.NAME == "HPV"
                               select rl).ToList();
            }
            else
            {
                _hpvResults = (from rl in _dal.FindBy<RESULT>
                                        (x => x.TEST.ALIQUOT.SAMPLE.SDG_ID == sdgId)
                               where rl.TEST.NAME == "Cobas HPV"
                               select rl).ToList();
            }
            
            var res = _hpvResults.Select(x => new WResult { Name = x.NAME,ResultId=x.RESULT_ID, Value = x.FORMATTED_RESULT }).ToList();


            return res;

        }
        public bool CanSave()
        {
            return view.IsValid4Save();
        }
        public string Save()
        {

            try
            {
                view.Save(_request);
                foreach (WResult wResult in this._request.Results)
                {
                    RESULT r = this._hpvResults.FirstOrDefault(x => x.NAME == wResult.Name);
                    r.FORMATTED_RESULT = wResult.Value;

                    //Set original result
                    if (wResult.Value == "True")
                        r.ORIGINAL_RESULT = "T";
                    else if (wResult.Value == "False")
                        r.ORIGINAL_RESULT = "F";
                    else
                        r.ORIGINAL_RESULT = wResult.Value;

                    r.STATUS = "C";
                }
                _hpvResults.First().TEST.STATUS = "C";
                _hpvResults.First().TEST.ALIQUOT.ALIQUOT_USER.U_EXTERNAL_LAB_NUM = _request.PontoNum;




                //    _dal.SaveChanges();
                return "";
            }
            catch (Exception ex)
            {

                return ex.Message;
            }
        }

        public void EnableControls(bool flag)
        {
            if (view != null)
                view.EditMode(flag);
        }
        #endregion



    }


}



