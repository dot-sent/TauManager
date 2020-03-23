namespace TauManager.ViewModels
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class ActionResponseViewModel
    {
        public bool Result { get; set; }
        public string Message { get; set; }
        public int Id { get; set; }

        public static ActionResponseViewModel Error(string message = "", int id=0)
        {
            return new ActionResponseViewModel{
                Result = false,
                Message = message,
                Id = id,
            };
        }

        public static ActionResponseViewModel Success(string message = "", int id=0)
        {
            return new ActionResponseViewModel{
                Result = true,
                Message = message,
                Id = id,
            };
        }
    }
}