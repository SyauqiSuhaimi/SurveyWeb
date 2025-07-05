using System.Text;
using System.Text.RegularExpressions;
using System.Transactions;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SurveyWeb.Model;

namespace SurveyWeb.Services
{
    public interface ISurveyServices
    {
        Task<List<SurveyQuestion>> GetSurveysAsync();
        Task<int> CreateQuestion(SurveyQuestion question);
        Task<int> SubmitSurveynew(List<SurveyAnswer> answers);
        Task<List<SurveyBody>> GetSurveyAnswer();
        Task<SurveyBody> GetSurveysById(int surveyId);
        Task<Response> SubmitSurvey(List<SurveyAnswer> answers, int templateid);
        Task<List<SurveyTemplate>> GetTemplate();
        Task<List<SurveyQuestion>> GetQuestionByTemplate(int id);
        Task<int> CreateTemplate(SurveyTemplate template);
        Task<List<SurveyQuestion>> GetQuestionByTemplateId(int templateId);
    }
    public class SurveyServices : ISurveyServices
    {
        private readonly string _connectionString;

        public SurveyServices(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public async Task<List<SurveyQuestion>> GetSurveysAsync()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = "SELECT id AS 'Id', question AS 'Question', validation as 'Validation', errormsg as 'ErrorMsg' FROM SurveyQuestion";
                    var surveys = (await connection.QueryAsync<SurveyQuestion>(query)).ToList();
                    return surveys;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching survey questions.", ex);
            }
        }

        public async Task<SurveyQuestion> GetQuestionById(int id)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = "SELECT id AS 'Id', question AS 'Question', validation as 'Validation', errormsg as 'ErrorMsg' FROM SurveyQuestion where id = @id";
                    var surveys = await connection.QueryFirstOrDefaultAsync<SurveyQuestion>(query, new {id});
                    return surveys;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching survey questions.", ex);
            }
        }

        public async Task<List<SurveyQuestion>> GetQuestionByTemplateId(int templateId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = "SELECT id AS 'Id', question AS 'Question', validation as 'Validation', errormsg as 'ErrorMsg' FROM SurveyQuestion where template_id = @templateId";
                    var surveys = (await connection.QueryAsync<SurveyQuestion>(query, new { templateId })).ToList();
                    return surveys;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching survey questions.", ex);
            }
        }
        public async Task<int> CreateQuestion(SurveyQuestion question)
        {
            try
            {
                if (question == null)
                {
                    throw new Exception("Question is null");
                }
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var insertSurveyQuery = @"
                            INSERT INTO SurveyQuestion (question)
                            VALUES (@question);
                            SELECT CAST(SCOPE_IDENTITY() AS int);";

                    var surveyParams = new { question = question.Question };
                    var newId = await connection.ExecuteScalarAsync<int>(insertSurveyQuery, surveyParams);

                    return newId;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while submitting survey.", ex);
            }
        }

        public async Task<int> SubmitSurveynew(List<SurveyAnswer> answers)
        {
            try
            {
                if(answers.Count == 0 || answers == null)
                {
                    throw new Exception("Answer list is empty");
                }
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            var currentDateTime = DateTime.Now;
                            var insertSurveyQuery = @"
                            INSERT INTO Surveys (submit_date_time)
                            VALUES (@SubmitDateTime);
                            SELECT CAST(SCOPE_IDENTITY() AS int);";

                            var surveyParams = new { SubmitDateTime = currentDateTime };
                            var newId = await connection.ExecuteScalarAsync<int>(
                                insertSurveyQuery, surveyParams, transaction
                            );

                            foreach (var answer in answers)
                            {
                                var question = await GetQuestionById(answer.QuestionId);

                                if (!Regex.IsMatch(answer.Answer, question.Validation))
                                {
                                    throw new Exception(question.ErrorMsg);
                                }
                                var insertAnswerQuery = @"
                                INSERT INTO SurveysAnswer (answer, question_id, surveys_id)
                                VALUES (@Answer, @QuestionId, @SurveyId);";

                                var answerParams = new
                                {
                                    Answer = answer.Answer,
                                    QuestionId = answer.QuestionId,
                                    SurveyId = newId
                                };

                                await connection.ExecuteAsync(insertAnswerQuery, answerParams, transaction);
                            }

                            transaction.Commit();
                            return newId;
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while submitting survey.", ex);
            }
        }

        public async Task<Response> SubmitSurvey(List<SurveyAnswer> answers, int templateid)
        {
            try
            {
                Response response = new Response();
                if (answers.Count == 0 || answers == null)
                {
                    throw new Exception("Answer list is empty");
                }
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            var currentDateTime = DateTime.Now;
                            var insertSurveyQuery = @"
                            INSERT INTO Surveys (submit_date_time, template_id)
                            VALUES (@SubmitDateTime, @templateid);
                            SELECT CAST(SCOPE_IDENTITY() AS int);";

                            var surveyParams = new { SubmitDateTime = currentDateTime, templateid };
                            var newId = await connection.ExecuteScalarAsync<int>(
                                insertSurveyQuery, surveyParams, transaction
                            );

                            List<string> errormsg = [];

                            foreach (var answer in answers)
                            {
                                var question = await GetQuestionById(answer.QuestionId);

                                if (!Regex.IsMatch(answer.Answer, question.Validation))
                                {

                                    errormsg.Add(question.ErrorMsg);

                                }
                                else
                                {
                                    var insertAnswerQuery = @"
                                INSERT INTO SurveysAnswer (answer, question_id, surveys_id)
                                VALUES (@Answer, @QuestionId, @SurveyId);";

                                    var answerParams = new
                                    {
                                        Answer = answer.Answer,
                                        QuestionId = answer.QuestionId,
                                        SurveyId = newId
                                    };

                                    await connection.ExecuteAsync(insertAnswerQuery, answerParams, transaction);

                                }

                                
                                    
                            }
                            if (errormsg.Count() > 0)
                            {
                                response.Message = string.Join(",", errormsg);
                            }

                            if (string.IsNullOrEmpty(response.Message))
                            {
                                transaction.Commit();
                            }
                            return response;

                           
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while submitting survey.", ex);
            }
        }



        public async Task<List<SurveyBody>> GetSurveyAnswer()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = "SELECT id AS 'Id', submit_date_time AS 'SubmitDateTime', template_id 'TemplateId' FROM Surveys";
                    var surveys = (await connection.QueryAsync<SurveyBody>(query)).ToList();
                    return surveys;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while getting surveys asnwers.", ex);
            }
        }

        public async Task<SurveyBody> GetSurveysById(int surveyId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = "SELECT id AS 'Id', submit_date_time AS 'SubmitDateTime' FROM Surveys WHERE id = @surveyId";
                    var survey = await connection.QueryFirstOrDefaultAsync<SurveyBody>(query, new { surveyId });

                    if (survey != null)
                    {
                        var answersQuery = "SELECT id AS 'Id', answer AS 'Answer', question_id AS 'QuestionId' FROM SurveysAnswer WHERE surveys_id = @surveyId";
                        var answers = (await connection.QueryAsync<SurveyAnswer>(answersQuery, new { surveyId })).ToList();
                        survey.Answers = answers;
                    }

                    return survey;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while getting survey by ID.", ex);
            }
        }

        public async Task<List<SurveyTemplate>> GetTemplate()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = "SELECT id AS 'Id', name as 'Name' FROM SurveyTemplate";
                    var surveys = (await connection.QueryAsync<SurveyTemplate>(query)).ToList();
                    return surveys;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching survey questions.", ex);
            }
        }

        public async Task<List<SurveyQuestion>> GetQuestionByTemplate(int id)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = "SELECT id AS 'Id', question AS 'Question', validation as 'Validation', errormsg as 'ErrorMsg', template_id as 'TemplateId' FROM SurveyQuestion where template_id = @id";
                    var surveys = (await connection.QueryAsync<SurveyQuestion>(query, new { id})).ToList();
                    return surveys;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching survey questions.", ex);
            }
        }

        public async Task<int> CreateTemplate(SurveyTemplate template)
        {
            try
            {
                if (template == null)
                {
                    throw new Exception("Question is null");
                }
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var insertSurveyQuery = @"
                            INSERT INTO SurveyTemplate (name)
                            VALUES (@name);
                            SELECT CAST(SCOPE_IDENTITY() AS int);";

                    var surveyParams = new { name = template.name };
                    var newId = await connection.ExecuteScalarAsync<int>(insertSurveyQuery, surveyParams);

                    return newId;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while submitting template.", ex);
            }
        }

    }
}

    
