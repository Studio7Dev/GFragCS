using System.Text;
using System.Text.Json;
using CommunityToolkit.Maui.Views;

namespace GFrag
{


    public partial class UnShareModal : Popup
    {

        public List<string> UsersList { get; set; }
        public Picker UserPicker => userPicker; // Expose nameEntry

        public event EventHandler OnSubmit;

        public UnShareModal()
        {
            InitializeComponent();
        }

        private async void OnSubmitClicked(object sender, EventArgs e)
        {
            // Trigger the OnSubmit event when the button is clicked
            OnSubmit?.Invoke(this, EventArgs.Empty);
        }
    }
}