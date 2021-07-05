namespace Nokdu.Net
{
    public interface ISender
    {
        int SendTo(byte[] buffer, int offset, int length);
    }

    public interface IReceiver
    {
        int ReceiveFrom(byte[] buffer, int offset, int length);
    }
}