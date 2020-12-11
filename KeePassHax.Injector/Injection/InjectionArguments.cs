namespace KeePassHax.Injector.Injection
{
    internal struct InjectionArguments
    {
        public string Path;
        public string Namespace;
        public string Type;
        public string Method;
        public string Argument;

        public string TypeFull => Namespace is null ? Type : Namespace + "." + Type;
    }
}