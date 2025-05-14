using System.Text;
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
        Task<int> SubmitSurvey(List<SurveyAnswer> answers);
        Task<List<SurveyBody>> GetSurveyAnswer();
        Task<SurveyBody> GetSurveysById(int surveyId);
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
                    var query = "SELECT id AS 'Id', question AS 'Question' FROM SurveyQuestion";
                    var surveys = (await connection.QueryAsync<SurveyQuestion>(query)).ToList();
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

        public async Task<int> SubmitSurvey(List<SurveyAnswer> answers)
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

        public async Task<List<SurveyBody>> GetSurveyAnswer()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = "SELECT id AS 'Id', submit_date_time AS 'SubmitDateTime' FROM Surveys";
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
    }

}
