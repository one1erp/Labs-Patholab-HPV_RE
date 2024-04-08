using System.ComponentModel;

//using MessageBox = System.Windows.Controls.MessageBox;


namespace HPV_RE
{
    public class RiskType : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _checked;
        public bool Checked
        {
            get { return _checked; }
            set
            {
                this._checked = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged("Checked");


            }
        }

        private string _riskVal;
        public string RiskVal
        {
            get { return _riskVal; }
            set
            {
                _riskVal = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged("RiskVal");
            }
        }

        // Create the OnPropertyChanged method to raise the event
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

    }
}



