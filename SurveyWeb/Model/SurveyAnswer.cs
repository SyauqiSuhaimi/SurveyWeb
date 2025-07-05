namespace SurveyWeb.Model
{
    public class SurveyAnswer
    {
        public int Id { get; set; }
        public string Answer { get; set; }
        public int QuestionId { get; set; }
    }

    public class SurveyBody
    {
        public int Id { get; set; }
        public string SubmitDateTime { get; set; }
        public List<SurveyAnswer> Answers { get; set; }
        public int TemplateId { get; set; }
    }
}
