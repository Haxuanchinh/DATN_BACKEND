namespace SportManager.Application.Common.Exception;

public static class ErrorCode
{
    #region COMMON
    public const string EXCHANGE_RATE_NOT_FOUND = "EXCHANGE_RATE_NOT_FOUND";
    public const string FIELD_REQUIRED = "FIELD_REQUIRED";
    public const string NOT_FOUND = "NOT_FOUND";
    public const string INVALID_TYPE = "INVALID_TYPE";
    public const string INVALID_DATA = "INVALID_DATA";
    public const string DUPLICATE_KEY = "DUPLICATE_KEY";
    public const string MAX_LENGTH_200 = "MAX_LENGTH_200";
    public const string MAX_LENGTH_500 = "MAX_LENGTH_500";
    public const string IS_EXITS = "IS_EXITS";
    #endregion COMMON

    #region Port
    public static class Ports
    {
        public const string CodeIsRequired = "CODE_IS_REQUIRED";
        public const string NameIsRequired = "NAME_IS_REQUIRED";
        public const string TypeIsRequired = "TYPE_IS_REQUIRED";
        public const string PortNameMaxLength500 = "PORT_NAME_MAX_LENGTH_500";
        public const string PortCodeMaxLength100 = "PORT_CODE_MAX_LENGTH_100";
        public const string PortIsExits = "PORT_IS_EXIST";
        public const string PortNotFound = "PORT_NOT_FOUND";

        public const string Notfound = "NOT_FOUND";
    }
    #endregion Port

}
