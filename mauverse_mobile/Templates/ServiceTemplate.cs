using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mau.Templates
{
    public class ServiceTemplate : DataTemplateSelector
    {
        private readonly DataTemplate _serviceFormTemplate;

        public ServiceTemplate()
        {
            _serviceFormTemplate = new DataTemplate(typeof(SenderChatMessageTemplate));
        }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            return _serviceFormTemplate;
        }
    }
}
