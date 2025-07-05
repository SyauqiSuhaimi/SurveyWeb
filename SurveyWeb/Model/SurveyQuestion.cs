namespace SurveyWeb.Model
{
    public class SurveyQuestion
    {
        public int Id { get; set; }
        public string Question { get; set; }
        public string Validation { get; set; }
        public string ErrorMsg { get; set; }
        public int TemplateId { get; set; }
    }
}
