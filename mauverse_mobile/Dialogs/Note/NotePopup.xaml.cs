using CommunityToolkit.Maui.Views;

using mau.Database;
using mau.Utils.Services.Interface;
using mau.ViewModel.Dialogs;

namespace mau.Dialogs.Note;

public partial class NotePopup : Popup
{
    public NotePopup(
        DbConnect context,
        int scheduleId,
        IAppNavigationService navigation,
        bool isPreview = false,
        bool isCreate = false,
        bool isEdit = false,
        bool isDelete = false)
    {
        InitializeComponent();
        global::mau.Dialogs.ResponsivePopupSize.Apply(this, 360, 440);
        BindingContext = new NoteViewModel(
            this,
            context,
            navigation,
            scheduleId,
            isPreview,
            isCreate,
            isEdit,
            isDelete);
    }
}
