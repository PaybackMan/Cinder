using Microsoft.WindowsAzure.Mobile.Service;

namespace Cinder.Platform.Services.DataObjects
{
    public class TodoItem : EntityData
    {
        public string Text { get; set; }

        public bool Complete { get; set; }
    }
}