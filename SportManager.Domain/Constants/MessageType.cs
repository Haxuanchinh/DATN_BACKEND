namespace SportManager.Domain.Constants;

public enum MessageType
{
    Text,//0
    Image,
    File,
    System //3
}

public enum MessageStatus
{
    Sent,
    Delivered,
    Read
}