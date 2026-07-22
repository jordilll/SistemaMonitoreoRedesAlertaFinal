namespace SistemaMonitoreoRedes.Helpers
{
    public static class AuthHelper
    {
        public const string SessionKey = "UsuarioLogueado";
        public const string Usuario = "admin";
        public const string Password = "admin123";

        public static bool ValidarCredenciales(string usuario, string password)
            => usuario == Usuario && password == Password;

        public static bool EstaLogueado(ISession session)
            => session.GetString(SessionKey) == "true";

        public static void IniciarSesion(ISession session)
            => session.SetString(SessionKey, "true");

        public static void CerrarSesion(ISession session)
            => session.Remove(SessionKey);
    }
}
