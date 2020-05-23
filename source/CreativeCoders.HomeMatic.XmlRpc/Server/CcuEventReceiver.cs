using System;
using CreativeCoders.Core.Messaging;
using CreativeCoders.HomeMatic.XmlRpc.Server.Messages;
using CreativeCoders.Reactive.Messaging;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.XmlRpc.Server
{
    [PublicAPI]
    public class CcuEventReceiver : IDisposable
    {
        private readonly IMessenger _messenger;
        
        public CcuEventReceiver()
        {
            _messenger = Messenger.Default;

            _messenger.Register<HomeMaticEventMessage>(this, OnEvent);
            _messenger.Register<HomeMaticNewDevicesMessage>(this, OnNewDevices);
            _messenger.Register<HomeMaticDeleteDevicesMessage>(this, OnDeleteDevices);
            _messenger.Register<HomeMaticUpdateDeviceMessage>(this, OnUpdateDevice);

            EventMessageTopic = new MessageTopic<HomeMaticEventMessage>();
            NewDevicesMessageTopic = new MessageTopic<HomeMaticNewDevicesMessage>();
            DeleteDevicesMessageTopic = new MessageTopic<HomeMaticDeleteDevicesMessage>();
            UpdateDeviceMessageTopic = new MessageTopic<HomeMaticUpdateDeviceMessage>();
        }

        private void OnUpdateDevice(HomeMaticUpdateDeviceMessage message)
        {
            UpdateDeviceMessageTopic.Publish(message);
        }

        private void OnDeleteDevices(HomeMaticDeleteDevicesMessage message)
        {
            DeleteDevicesMessageTopic.Publish(message);
        }

        private void OnNewDevices(HomeMaticNewDevicesMessage message)
        {
            NewDevicesMessageTopic.Publish(message);
        }

        private void OnEvent(HomeMaticEventMessage message)
        {
            EventMessageTopic.Publish(message);
        }

        public void Dispose()
        {
            _messenger.Unregister<HomeMaticEventMessage>(this);
            _messenger.Unregister<HomeMaticNewDevicesMessage>(this);
            _messenger.Unregister<HomeMaticDeleteDevicesMessage>(this);
            _messenger.Unregister<HomeMaticUpdateDeviceMessage>(this);
        }

        public IMessageTopic<HomeMaticEventMessage> EventMessageTopic { get; }
        
        public MessageTopic<HomeMaticNewDevicesMessage> NewDevicesMessageTopic { get; }
        
        public MessageTopic<HomeMaticUpdateDeviceMessage> UpdateDeviceMessageTopic { get; }

        public MessageTopic<HomeMaticDeleteDevicesMessage> DeleteDevicesMessageTopic { get; }
    }
}