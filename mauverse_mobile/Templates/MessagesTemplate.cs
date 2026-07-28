using mau.Database;
using mau.DTOModels;
using mau.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mau.Templates
{
    public class MessagesTemplate : DataTemplateSelector
    {
        private readonly DataTemplate _senderMessageTemplate;
        private readonly DataTemplate _receiverMessageTemplate;

        public MessagesTemplate()
        {
            _senderMessageTemplate = new DataTemplate(typeof(SenderChatMessageTemplate));
            _receiverMessageTemplate = new DataTemplate(typeof(ReceiverChatMessageTemplate));
        }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            var message = (MessageDTO)item;
            var currentUser = ViewModel.BaseViewModel.CurrentUser;
            if (currentUser is not null && message.UserIdFrom == currentUser.UserId)
                return _receiverMessageTemplate;
            return _senderMessageTemplate;
        }
    }
}
