using Patholab_DAL_V1;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HPV_RE
{
    public interface HPV_Control
    {
        List<PHRASE_ENTRY> ListLowRisk
        {
            get;
            set;
        }
        List<PHRASE_ENTRY> ListHighRisk
        {
            get;
            set;
        }
        HPV_RE_Manager Controller
        {
            get;
            set;
        }
        ObservableCollection<RiskType> ListHighRisk2
        {
            get;
            set;
        }
        ObservableCollection<RiskType> ListLowRisk2
        {
            get;
            set;
        }

        #region Initilaize and clear

        void InitialMode();

        void ClearScreen();

        void EditMode(Boolean p);

        #endregion

        #region Logic

        #region Load sdg new

        void LoadRequest(Request request);

        #endregion


        #region Save sdg new

        void Save(Request request);

        bool IsValid4Save();

        #endregion

        #endregion

        void SetList();

    }
}
